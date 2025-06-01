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
using TextAlignment = iText.Layout.Properties.TextAlignment;

namespace InventoryApp
{
    /// <summary>
    /// Логика взаимодействия для ReportsWindow.xaml
    /// </summary>
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
            var materials = _dbService.GetMaterialStockReport();
            _reportData.Clear();
            foreach (var material in materials)
            {
                _reportData.Add(new { MaterialName = material.Name, StockQuantity = material.StockQuantity });
            }
        }

        private void ShowMovementReport_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите даты периода.", "Ошибка");
                return;
            }

            var startDate = StartDatePicker.SelectedDate.Value;
            var endDate = EndDatePicker.SelectedDate.Value;
            var movements = _dbService.GetMaterialMovementReport(startDate, endDate);
            _reportData.Clear();
            foreach (var movement in movements)
            {
                _reportData.Add(new
                {
                    movement.MaterialName,
                    movement.InitialStock,
                    movement.Receipts,
                    movement.Issues,
                    movement.FinalStock
                });
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

                        string projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\"));
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
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
                        {
                            document.Add(new Paragraph($"Период: {StartDatePicker.SelectedDate.Value:dd.MM.yyyy} - {EndDatePicker.SelectedDate.Value:dd.MM.yyyy}")
                                .SetFont(font)
                                .SetFontSize(12)
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
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
                                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        }

                        foreach (var item in _reportData)
                        {
                            foreach (var prop in properties)
                            {
                                var value = prop.GetValue(item)?.ToString() ?? "";
                                table.AddCell(new Cell()
                                    .Add(new Paragraph(value)
                                        .SetFont(font))
                                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));
                            }
                        }

                        document.Add(table);

                        document.Add(new Paragraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .SetFont(font)
                            .SetFontSize(10)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

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