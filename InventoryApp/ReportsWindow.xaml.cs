using InventoryApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;

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
            MessageBox.Show("Функционал печати не реализован в данном примере. Для печати можно использовать библиотеку iTextSharp или ReportViewer.", "Информация");
        }
    }
}