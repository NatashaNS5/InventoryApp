using InventoryApp.Models;
using InventoryApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace InventoryApp
{
    /// <summary>
    /// Логика взаимодействия для ProductionWindow.xaml
    /// </summary>
    public partial class ProductionWindow : Window
    {
        private ObservableCollection<Product> _products;
        private readonly DatabaseService _dbService;

        public ProductionWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _products = new ObservableCollection<Product>(_dbService.GetProducts());
            ProductComboBox.ItemsSource = _products;
        }

        private void ActualMaterialUsageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateExcessMessage();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem == null ||
                !int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0 ||
                !double.TryParse(ActualMaterialUsageTextBox.Text, out double actualMaterialUsage) || actualMaterialUsage < 0)
            {
                MessageBox.Show("Заполните все поля корректно.", "Ошибка");
                return;
            }

            var selectedProduct = (Product)ProductComboBox.SelectedItem;
            var record = new ProductionRecord
            {
                ProductId = selectedProduct.Id,
                Quantity = quantity,
                ActualMaterialUsage = actualMaterialUsage,
                ProductionDate = DateTime.Now,
                ScrapAmount = selectedProduct.PlannedMaterialUsage - actualMaterialUsage
            };
            _dbService.SaveProduction(record);
            QuantityTextBox.Text = "";
            ActualMaterialUsageTextBox.Text = "";
            ExcessMessageTextBlock.Text = "";
        }

        private void UpdateExcessMessage()
        {
            if (ProductComboBox.SelectedItem == null || !double.TryParse(ActualMaterialUsageTextBox.Text, out double actualMaterialUsage))
            {
                ExcessMessageTextBlock.Text = "";
                return;
            }

            var selectedProduct = (Product)ProductComboBox.SelectedItem;
            double excess = (actualMaterialUsage - selectedProduct.PlannedMaterialUsage) / selectedProduct.PlannedMaterialUsage * 100;
            if (excess > 15)
            {
                ExcessMessageTextBlock.Text = $"Превышение использования материала на {excess:F2}% (>15%)!";
            }
            else
            {
                ExcessMessageTextBlock.Text = "";
            }
        }
    }
}