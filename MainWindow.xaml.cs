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
using BathComplex.Pages;

namespace BathComplex
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public MainWindow()
        {
            InitializeComponent();
           

            LoadUserInfo();
            ApplyRoleRestrictions();
            btnApartments_Click(null, null);
        }

        private void LoadUserInfo()
        {
            lblUserName.Content = UserSession.FullName;
            lblUserRole.Content = UserSession.UserRole;
        }

        private void ApplyRoleRestrictions()
        {
            
        }

        private void HighlightMenuButton(string active)
        {
            // Сбрасываем все
            ResetButtonStyle(btnApartments);
            ResetButtonStyle(btnClients);
            ResetButtonStyle(btnBooking);
            ResetButtonStyle(btnFinance);
            ResetButtonStyle(btnAnalytics);

            // Подсвечиваем активную
            switch (active)
            {
                case "Apartments": SetActiveButton(btnApartments); break;
                case "Clients": SetActiveButton(btnClients); break;
                case "Booking": SetActiveButton(btnBooking); break;
                case "Finance": SetActiveButton(btnFinance); break;
                case "Analytics": SetActiveButton(btnAnalytics); break;
            }
        }

        private void ResetButtonStyle(Button btn)
        {
            btn.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xF5, 0xF0, 0xE8));
            btn.Background = System.Windows.Media.Brushes.Transparent;
            btn.FontWeight = FontWeights.Normal;
        }

        private void SetActiveButton(Button btn)
        {
            btn.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0xC5, 0xA4, 0x6B));
            btn.FontWeight = FontWeights.Bold;
        }

        private void btnApartments_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ApartmentsPage());
            HighlightMenuButton("Apartments");
        }

        private void btnClients_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClientsPage());
            HighlightMenuButton("Clients");
        }

        private void btnBooking_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BookingPage());
            HighlightMenuButton("Booking");
        }

        private void btnFinance_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.UserRole == "Администратор смены")
            {
                MessageBox.Show("У вас нет доступа к финансовой информации.\nОбратитесь к Владельцу.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MainFrame.Navigate(new FinancePage());
            HighlightMenuButton("Finance");
        }

        private void btnAnalytics_Click(object sender, RoutedEventArgs e)
        {
            if (UserSession.UserRole == "Администратор смены")
            {
                MessageBox.Show("У вас нет доступа к аналитике.\nОбратитесь к Владельцу.",
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MainFrame.Navigate(new AnalyticsPage());
            HighlightMenuButton("Analytics");
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из системы?",
                 "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
