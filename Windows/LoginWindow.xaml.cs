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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public Users AuthenticatedUser { get; private set; }
        public string UserRole { get; private set; }
        public LoginWindow()
        {
            InitializeComponent();
            txtLogin.Focus();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Authenticate();
        }

        private void Authenticate()
        {
            lblError.Visibility = Visibility.Collapsed;

            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Введите логин и пароль");
                return;
            }

            try
            {
                var user = Connection.db.Users
                    .FirstOrDefault(u => u.Login == login
                                      && u.PasswordHash == password
                                      && u.IsActive == true);

                if (user == null)
                {
                    ShowError("Неверный логин или пароль");
                    return;
                }

                var role = Connection.db.Roles.FirstOrDefault(r => r.RoleID == user.RoleID);
                string roleName = role?.RoleName ?? "Неизвестно";

                // Сохраняем в сессию
                UserSession.SetUser(user, roleName);

                // Закрываем окно входа
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подключения: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            lblError.Content = message;
            lblError.Visibility = Visibility.Visible;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            UserSession.Clear();
            Application.Current.Shutdown();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            Authenticate();
        }
    }
}
