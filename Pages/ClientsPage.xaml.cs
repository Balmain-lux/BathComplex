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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BathComplex.DataBase;
using BathComplex.Windows;

namespace BathComplex.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        private List<dynamic> _allClients;
        private int? _selectedClientId;
        public ClientsPage()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients(string searchText = null)
        {
            try
            {
                var query = Connection.db.Clients.AsQueryable();

                // Поиск по тексту
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(c => c.FullName.Contains(searchText) ||
                                             c.Phone.Contains(searchText));
                }

                _allClients = query.Select(c => new
                {
                    c.ClientID,
                    c.FullName,
                    c.Phone,
                    c.DiscountPercent,
                    c.RegistrationDate,
                    VisitCount = Connection.db.Bookings.Count(b => b.ClientID == c.ClientID && b.Status == "Finished"),
                    TotalSpent = Connection.db.Bookings
                        .Where(b => b.ClientID == c.ClientID && b.Status == "Finished")
                        .Sum(b => (decimal?)b.FinalAmount) ?? 0
                }).OrderByDescending(c => c.TotalSpent).ToList<dynamic>();

                dgClients.ItemsSource = _allClients;
                lblCount.Content = $"Всего клиентов: {_allClients.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadClientHistory(int clientId)
        {
            try
            {
                var client = Connection.db.Clients.Find(clientId);
                if (client == null) return;

                var history = (from b in Connection.db.Bookings
                               join a in Connection.db.Apartments on b.ApartmentID equals a.ApartmentID
                               where b.ClientID == clientId
                               orderby b.StartTime descending
                               select new
                               {
                                   VisitDate = b.StartTime,
                                   ApartmentName = a.Name,
                                   Amount = b.FinalAmount ?? 0,
                                   PaymentMethod = b.PaymentMethod ?? "Не указан",
                                   Status = b.Status
                               }).ToList();

                dgClientHistory.ItemsSource = history;
                lblHistoryTitle.Content = $"История визитов: {client.FullName}";
                historyPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadClients(txtSearch.Text.Trim());
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            LoadClients();
        }

        private void btnAddClient_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClientDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.SetAddMode();
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                try
                {
                    // Проверка на уникальность телефона
                    var existing = Connection.db.Clients
                        .FirstOrDefault(c => c.Phone == dialog.Phone);
                    if (existing != null)
                    {
                        MessageBox.Show($"Клиент с телефоном {dialog.Phone} уже существует:\n{existing.FullName}",
                            "Дубликат", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newClient = new Clients
                    {
                        FullName = dialog.FullName,
                        Phone = dialog.Phone,
                        DiscountPercent = dialog.DiscountPercent,
                        RegistrationDate = dialog.RegistrationDate
                    };
                    Connection.db.Clients.Add(newClient);
                    Connection.db.SaveChanges();
                    LoadClients();

                    MessageBox.Show($"Клиент \"{dialog.FullName}\" успешно добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadClients();
        }

        

        private void btnEditClient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int clientId = Convert.ToInt32(btn.Tag);
                var client = Connection.db.Clients.Find(clientId);
                if (client == null) return;

                var dialog = new ClientDialog();
                dialog.Owner = Application.Current.MainWindow;
                dialog.SetEditMode(           // <-- Режим редактирования
                    client.FullName,
                    client.Phone,
                    (decimal)client.DiscountPercent,
                    client.RegistrationDate ?? DateTime.Today
                );
                dialog.ShowDialog();

                if (dialog.IsSaved)
                {
                    try
                    {
                        // Проверка телефона (кроме текущего клиента)
                        var duplicate = Connection.db.Clients
                            .FirstOrDefault(c => c.Phone == dialog.Phone && c.ClientID != clientId);
                        if (duplicate != null)
                        {
                            MessageBox.Show($"Телефон {dialog.Phone} уже используется клиентом:\n{duplicate.FullName}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        client.FullName = dialog.FullName;
                        client.Phone = dialog.Phone;
                        client.DiscountPercent = dialog.DiscountPercent;
                        client.RegistrationDate = dialog.RegistrationDate;
                        Connection.db.SaveChanges();
                        LoadClients();

                        MessageBox.Show("Данные клиента обновлены!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnClientHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int clientId = Convert.ToInt32(btn.Tag);
                _selectedClientId = clientId;
                LoadClientHistory(clientId);
            }
        }

        private void btnDeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int clientId = Convert.ToInt32(btn.Tag);
                var client = Connection.db.Clients.Find(clientId);
                if (client == null) return;

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить клиента \"{client.FullName}\"?\n\nВся история визитов будет сохранена.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Connection.db.Clients.Remove(client);
                        Connection.db.SaveChanges();
                        LoadClients();
                        MessageBox.Show("Клиент удален.", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}\n\nВозможно, у клиента есть связанные записи.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnCloseHistory_Click(object sender, RoutedEventArgs e)
        {
            historyPanel.Visibility = Visibility.Collapsed;
            dgClientHistory.ItemsSource = null;
            _selectedClientId = null;
        }
    }
}
