using InventoryApp.Services;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using Image = iText.Layout.Element.Image;

namespace InventoryApp
{
    public partial class ReportsWindow : Window
    {
        private ObservableCollection<object> _reportData;
        private readonly DatabaseService _dbService;

        public ReportsWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _reportData = new ObservableCollection<object>();
            ReportGrid.ItemsSource = _reportData;
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now;
        }

        private void ShowStockReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReportGrid.Columns.Clear();
                ReportGrid.Columns.Add(new DataGridTextColumn { Header = "Материал", Binding = new System.Windows.Data.Binding("MaterialName") });
                ReportGrid.Columns.Add(new DataGridTextColumn { Header = "Остаток", Binding = new System.Windows.Data.Binding("StockQuantity") });

                var materials = _dbService.GetMaterialStockReport();
                _reportData.Clear();
                foreach (var material in materials)
                {
                    Debug.WriteLine($"Material: {material.Name}, StockQuantity: {material.StockQuantity}");
                    _reportData.Add(new
                    {
                        MaterialName = material.Name,
                        StockQuantity = material.StockQuantity
                    });
                }
                if (_reportData.Count == 0)
                {
                    MessageBox.Show("Нет данных о запасах материалов.", "Предупреждение");
                }
                else
                {
                    Debug.WriteLine($"Загружено {_reportData.Count} записей об остатках.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке отчета: {ex.Message}", "Ошибка");
                Debug.WriteLine($"Ошибка в ShowStockReport: {ex.Message}");
            }
        }

        private void ShowMovementReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите даты периода.", "Ошибка");
                    return;
                }

                var startDate = StartDatePicker.SelectedDate.Value;
                var endDate = EndDatePicker.SelectedDate.Value;

                if (startDate > endDate)
                {
                    MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка");
                    return;
                }

                ReportGrid.Columns.Clear();
                ReportGrid.Columns.Add(new DataGridTextColumn { Header = "Материал", Binding = new System.Windows.Data.Binding("MaterialName") });
                ReportGrid.Columns.Add(new DataGridTextColumn { Header = "Изделие", Binding = new System.Windows.Data.Binding("ProductName") });
                ReportGrid.Columns.Add(new DataGridTextColumn { Header = "Начальный остаток", Binding = new System.Windows.Data.Binding("InitialStock") });
                ReportGrid.Columns.Add(new DataGridTextColumn { Header = "Поступления", Binding = new System.Windows.Data.Binding("Receipts") });
                ReportGrid.Columns.Add(new DataGridTextColumn { Header = "Расход", Binding = new System.Windows.Data.Binding("Issues") });
                ReportGrid.Columns.Add(new DataGridTextColumn { Header = "Конечный остаток", Binding = new System.Windows.Data.Binding("FinalStock") });

                var movements = _dbService.GetMaterialMovementReport(startDate, endDate);
                _reportData.Clear();
                foreach (var movement in movements)
                {
                    Debug.WriteLine($"Movement: {movement.MaterialName}, Product: {movement.ProductName}, Issues: {movement.Issues}, InitialStock: {movement.InitialStock}, FinalStock: {movement.FinalStock}");
                    _reportData.Add(new
                    {
                        MaterialName = movement.MaterialName,
                        ProductName = movement.ProductName,
                        InitialStock = movement.InitialStock,
                        Receipts = movement.Receipts,
                        Issues = movement.Issues,
                        FinalStock = movement.FinalStock
                    });
                }
                if (_reportData.Count == 0)
                {
                    MessageBox.Show($"Нет данных о движении материалов за период с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}. Проверьте ProductionRecords.", "Предупреждение");
                }
                else
                {
                    Debug.WriteLine($"Загружено {_reportData.Count} записей о движении.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке отчета: {ex.Message}", "Ошибка");
                Debug.WriteLine($"Ошибка в ShowMovementReport: {ex.Message}");
            }
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            if (_reportData.Count == 0)
            {
                MessageBox.Show("Нет данных для печати. Сначала выберите отчет.", "Ошибка");
                return;
            }

            try
            {
                string reportsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
                if (!Directory.Exists(reportsFolder))
                {
                    Directory.CreateDirectory(reportsFolder);
                }

                string outputPath = Path.Combine(reportsFolder, $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                Debug.WriteLine($"Создание PDF: {outputPath}");

                using (var writer = new PdfWriter(outputPath))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);

                        string logoPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Images", "logo-03.jpg");
                        if (File.Exists(logoPath))
                        {
                            var logo = new Image(ImageDataFactory.Create(logoPath)).ScaleToFit(100, 50);
                            var logoContainer = new Div().SetTextAlignment(TextAlignment.CENTER).Add(logo);
                            document.Add(logoContainer);
                        }
                        else
                        {
                            Debug.WriteLine($"Логотип не найден по пути: {logoPath}");
                            MessageBox.Show($"Логотип не найден по пути: {logoPath}. Отчет будет создан без логотипа.", "Предупреждение");
                        }

                        string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf");
                        if (!File.Exists(fontPath))
                        {
                            throw new FileNotFoundException($"Шрифт Times New Roman не найден по пути: {fontPath}");
                        }
                        PdfFont font = PdfFontFactory.CreateFont(fontPath, "Identity-H");

                        document.Add(new Paragraph("Управленческий отчет")
                            .SetFont(font)
                            .SetFontSize(16)
                            .SetBold()
                            .SetTextAlignment(TextAlignment.CENTER));

                        if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
                        {
                            document.Add(new Paragraph($"Период: {StartDatePicker.SelectedDate.Value:dd.MM.yyyy} - {EndDatePicker.SelectedDate.Value:dd.MM.yyyy}")
                                .SetFont(font)
                                .SetFontSize(12)
                                .SetTextAlignment(TextAlignment.CENTER));
                        }

                        document.Add(new Paragraph(" ").SetFont(font));

                        var properties = _reportData.First().GetType().GetProperties();
                        var columnCount = properties.Length;
                        Table table = new Table(columnCount);
                        table.SetWidth(UnitValue.CreatePercentValue(100));

                        foreach (var prop in properties)
                        {
                            table.AddHeaderCell(new Cell()
                                .Add(new Paragraph(GetDisplayName(prop.Name))
                                    .SetFont(font)
                                    .SetBold())
                                .SetTextAlignment(TextAlignment.CENTER));
                        }

                        foreach (var item in _reportData)
                        {
                            foreach (var prop in properties)
                            {
                                var value = prop.GetValue(item)?.ToString() ?? "";
                                table.AddCell(new Cell()
                                    .Add(new Paragraph(value)
                                        .SetFont(font))
                                    .SetTextAlignment(TextAlignment.LEFT));
                            }
                        }

                        document.Add(table);

                        document.Add(new Paragraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .SetFont(font)
                            .SetFontSize(10)
                            .SetTextAlignment(TextAlignment.RIGHT));

                        document.Close();
                    }
                }

                Process.Start(new ProcessStartInfo(outputPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка");
                Debug.WriteLine($"Ошибка: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private string GetDisplayName(string propertyName)
        {
            return propertyName switch
            {
                "MaterialName" => "Материал",
                "ProductName" => "Изделие",
                "StockQuantity" => "Остаток",
                "InitialStock" => "Начальный остаток",
                "Receipts" => "Поступления",
                "Issues" => "Расход",
                "FinalStock" => "Конечный остаток",
                _ => propertyName
            };
        }
    }
}