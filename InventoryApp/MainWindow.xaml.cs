using InventoryApp.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventoryApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenInventory_Click(object sender, RoutedEventArgs e)
        {
            new InventoryWindow().Show();
        }

        private void OpenOrders_Click(object sender, RoutedEventArgs e)
        {
            new OrdersWindow().Show();
        }

        private void OpenOrderForm_Click(object sender, RoutedEventArgs e)
        {
            new OrderFormWindow().Show();
        }

        private void OpenProduction_Click(object sender, RoutedEventArgs e)
        {
            new ProductionWindow().Show();
        }

        private void OpenReports_Click(object sender, RoutedEventArgs e)
        {
            new ReportsWindow().Show();
        }
    }
}