using InventoryApp.Models;
using InventoryApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace InventoryApp
{
    public partial class InventoryWindow : Window
    {
        private ObservableCollection<Material> _materials;
        private readonly DatabaseService _dbService;

        public InventoryWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _materials = new ObservableCollection<Material>(_dbService.GetMaterials());
            MaterialsGrid.ItemsSource = _materials;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            bool hasLargeDiscrepancy = _materials.Any(m => m.DiscrepancyPercentage > 20 || m.DiscrepancyPercentage < -20);
            if (hasLargeDiscrepancy)
            {
                DiscrepancyMessageTextBlock.Text = "Расхождение более 20%! Требуется утверждение директора.";
                ApproveButton.IsEnabled = true;
                return;
            }
            _dbService.SaveInventory(_materials.ToList(), true);
            DiscrepancyMessageTextBlock.Text = "Инвентаризация сохранена.";
            ApproveButton.IsEnabled = false;
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            _dbService.SaveInventory(_materials.ToList(), true);
            DiscrepancyMessageTextBlock.Text = "Инвентаризация утверждена.";
            ApproveButton.IsEnabled = false;
        }
    }
}
