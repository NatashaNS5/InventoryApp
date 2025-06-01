using InventoryApp.Models;
using InventoryApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InventoryApp
{
    /// <summary>
    /// Логика взаимодействия для ProductionWindow.xaml
    /// </summary>
    public partial class ProductionWindow : Window
    {
        private ObservableCollection<Product> _products;
        private ObservableCollection<Material> _materials;
        private readonly DatabaseService _dbService;

        public ProductionWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _products = new ObservableCollection<Product>(_dbService.GetProducts());
            _materials = new ObservableCollection<Material>(_dbService.GetMaterials());
            ProductComboBox.ItemsSource = _products;
            MaterialComboBox.ItemsSource = _materials;
            QuantityTextBox.Text = "1"; 
            ActualMaterialUsageTextBox.Text = "0"; 
            ValidateInputs(null, null);
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateExcessMessage();
            UpdateCost();
            ValidateInputs(null, null);
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateExcessMessage();
            UpdateCost();
            ValidateInputs(null, null);
        }

        private void ActualMaterialUsageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateExcessMessage();
            UpdateCost();
            ValidateInputs(null, null);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var text = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(text, @"^[0-9]*\.?[0-9]*$") && !Regex.IsMatch(text, @"^[0-9]*,?[0-9]*$");
        }

        private void ValidateInputs(object sender, RoutedEventArgs e)
        {
            SaveButton.IsEnabled = ProductComboBox.SelectedItem != null &&
                                  MaterialComboBox.SelectedItem != null &&
                                  int.TryParse(QuantityTextBox.Text, out int quantity) && quantity > 0 &&
                                  double.TryParse(ActualMaterialUsageTextBox.Text, out double actualMaterialUsage) && actualMaterialUsage >= 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0 ||
                    !double.TryParse(ActualMaterialUsageTextBox.Text, out double actualMaterialUsage) || actualMaterialUsage < 0)
                {
                    MessageBox.Show("Введите корректные значения для количества и использованного материала.", "Ошибка");
                    return;
                }

                var selectedProduct = (Product)ProductComboBox.SelectedItem;
                var selectedMaterial = (Material)MaterialComboBox.SelectedItem;
                var plannedUsage = selectedProduct.PlannedMaterialUsage * quantity;

                var record = new ProductionRecord
                {
                    ProductId = selectedProduct.Id,
                    MaterialId = selectedMaterial.Id,
                    Quantity = quantity,
                    ActualMaterialUsage = actualMaterialUsage,
                    ProductionDate = DateTime.Now,
                    ScrapAmount = plannedUsage - actualMaterialUsage
                };

                _dbService.SaveProduction(record);
                MessageBox.Show("Данные сохранены!", "Успех");
                QuantityTextBox.Text = "1";
                ActualMaterialUsageTextBox.Text = "0";
                ExcessMessageTextBlock.Text = "";
                CostTextBlock.Text = "Себестоимость: 0";
                ProductComboBox.SelectedItem = null;
                MaterialComboBox.SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateExcessMessage()
        {
            if (ProductComboBox.SelectedItem == null ||
                !int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0 ||
                !double.TryParse(ActualMaterialUsageTextBox.Text, out double actualMaterialUsage))
            {
                ExcessMessageTextBlock.Text = "";
                return;
            }

            var selectedProduct = (Product)ProductComboBox.SelectedItem;
            var plannedUsage = selectedProduct.PlannedMaterialUsage * quantity;
            var excess = (actualMaterialUsage - plannedUsage) / plannedUsage * 100;
            ExcessMessageTextBlock.Text = excess > 15 ? $"Превышение на {excess:F2}% (>15%)!" : "";
        }

        private void UpdateCost()
        {
            if (MaterialComboBox.SelectedItem == null ||
                !double.TryParse(ActualMaterialUsageTextBox.Text, out double actualMaterialUsage))
            {
                CostTextBlock.Text = "Себестоимость: 0";
                return;
            }

            var selectedMaterial = (Material)MaterialComboBox.SelectedItem;
            var cost = actualMaterialUsage * selectedMaterial.PurchasePrice;
            CostTextBlock.Text = $"Себестоимость: {cost:F2}";
        }
    }
}