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

namespace BathComplex.Windows
{
    /// <summary>
    /// Логика взаимодействия для ClientDialog.xaml
    /// </summary>
    public partial class ClientDialog : Window
    {
        public string FullName { get; private set; }
        public string Phone { get; private set; }
        public decimal DiscountPercent { get; private set; }
        public DateTime RegistrationDate { get; private set; }
        public bool IsSaved { get; private set; }
        public ClientDialog()
        {
            InitializeComponent();
        }

        public void LoadClientData(string fullName, string phone, decimal discount, DateTime regDate)
        {
            txtFullName.Text = fullName;
            txtPhone.Text = phone;
            txtDiscount.Text = discount.ToString();
            dpRegistrationDate.SelectedDate = regDate;
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация ФИО
            FullName = txtFullName.Text.Trim();
            if (string.IsNullOrWhiteSpace(FullName))
            {
                MessageBox.Show("Введите ФИО клиента.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return;
            }
            if (FullName.Length < 5)
            {
                MessageBox.Show("ФИО должно содержать минимум 5 символов.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return;
            }

            // Валидация телефона
            Phone = txtPhone.Text.Trim();
            if (string.IsNullOrWhiteSpace(Phone) || Phone.Length < 10)
            {
                MessageBox.Show("Введите корректный номер телефона (минимум 10 цифр).", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return;
            }

            // Валидация скидки
            if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка должна быть числом от 0 до 100.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDiscount.Focus();
                return;
            }
            DiscountPercent = discount;

            // Дата регистрации
            RegistrationDate = dpRegistrationDate.SelectedDate ?? DateTime.Today;

            IsSaved = true;
            this.Close();
        }
        }
}
