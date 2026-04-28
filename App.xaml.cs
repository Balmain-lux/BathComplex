using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BathComplex.DataBase;




namespace BathComplex
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Меняем режим завершения — только по явному вызову Shutdown()
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Проверка подключения к БД
            try
            {
                Connection.db.Database.Connection.Open();
                Connection.db.Database.Connection.Close();
            }
            catch
            {
                MessageBox.Show("Нет подключения к базе данных.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // Окно входа
            Windows.LoginWindow loginWindow = new Windows.LoginWindow();
            loginWindow.ShowDialog();

            if (!UserSession.IsAuthenticated)
            {
                Shutdown();
                return;
            }

            // Возвращаем стандартный режим и запускаем главное окно
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

            MainWindow mainWindow = new MainWindow();
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }

    }
}
