using InventoryApp.Models;
using InventoryApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventoryApp
{
    public partial class OrderFormWindow : Window
    {
        private ObservableCollection<OrderItem> _orderItems;
        private ObservableCollection<Product> _products;
        private readonly DatabaseService _dbService;

        public OrderFormWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _products = new ObservableCollection<Product>(_dbService.GetProducts());
            ProductComboBox.ItemsSource = _products;
            _orderItems = new ObservableCollection<OrderItem>();
            OrderItemsGrid.ItemsSource = _orderItems;
            UpdateTotalCost();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem == null || !int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Выберите изделие и укажите корректное количество.", "Ошибка");
                return;
            }

            var selectedProduct = (Product)ProductComboBox.SelectedItem;
            var item = new OrderItem
            {
                ProductId = selectedProduct.Id,
                ProductName = selectedProduct.Name,
                Quantity = quantity,
                UnitPrice = selectedProduct.Cost
            };
            _orderItems.Add(item);
            QuantityTextBox.Text = "";
            UpdateTotalCost();
        }

        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!_orderItems.Any())
            {
                MessageBox.Show("Добавьте хотя бы одно изделие в заказ.", "Ошибка");
                return;
            }

            var order = new Order
            {
                OrderNumber = $"ORD{DateTime.Now.Ticks}",
                OrderDate = DateTime.Now,
                Status = "Новый",
                Customer = CustomerTextBox.Text,
                TotalCost = _orderItems.Sum(i => i.TotalPrice)
            };
            _dbService.SaveOrder(order, _orderItems.ToList());
            _orderItems.Clear();
            CustomerTextBox.Text = "Клиент";
            UpdateTotalCost();
        }

        private void UpdateTotalCost()
        {
            var total = _orderItems.Sum(i => i.TotalPrice);
            TotalCostTextBlock.Text = $"Итоговая стоимость: {total}";
        }
    }
}