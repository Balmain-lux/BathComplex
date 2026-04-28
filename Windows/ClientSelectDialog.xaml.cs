using System;
using System.Collections.Generic;
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
using BathComplex.DataBase;

namespace BathComplex.Windows
{
    /// <summary>
    /// Логика взаимодействия для ClientSelectDialog.xaml
    /// </summary>
    public partial class ClientSelectDialog : Window
    {
        public Clients SelectedClient { get; private set; }
        public bool IsSelected { get; private set; }
        public ClientSelectDialog()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients(string search = null)
        {
            var query = Connection.db.Clients.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.FullName.Contains(search) || c.Phone.Contains(search));
            }

            var clients = query.OrderBy(c => c.FullName).ToList();
            dgClients.ItemsSource = clients;
        }

        private void SelectClient()
        {
            SelectedClient = dgClients.SelectedItem as Clients;
            if (SelectedClient == null)
            {
                MessageBox.Show("Выберите клиента из списка.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            IsSelected = true;
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadClients(txtSearch.Text.Trim());
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadClients(txtSearch.Text.Trim());
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsSelected = false;
            this.Close();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectClient();
        }

        private void dgClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectClient();
        }
    }
}
