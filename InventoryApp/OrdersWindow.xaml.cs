using InventoryApp.Models;
using InventoryApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InventoryApp.ViewModels
{
    public partial class OrdersWindow : Window
    {
        private ObservableCollection<Order> _orders;
        private readonly DatabaseService _dbService;

        public OrdersWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _orders = new ObservableCollection<Order>(_dbService.GetOrders());
            OrdersGrid.ItemsSource = _orders;
        }
    }
}
