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
    /// Логика взаимодействия для ApartmentDialog.xaml
    /// </summary>
    public partial class ApartmentDialog : Window
    {
        public string ApartmentName { get; private set; }
        public string ApartmentType { get; private set; }
        public decimal WeekdayPrice { get; private set; }
        public decimal WeekendPrice { get; private set; }
        public bool IsSaved { get; private set; }
        public ApartmentDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация названия
            ApartmentName = txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(ApartmentName))
            {
                MessageBox.Show("Введите название апартамента.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            // Тип
            ApartmentType = ((System.Windows.Controls.ComboBoxItem)cmbType.SelectedItem).Content.ToString();

            // Валидация цены будни
            if (!decimal.TryParse(txtWeekdayPrice.Text, out decimal wdPrice) || wdPrice <= 0)
            {
                MessageBox.Show("Введите корректную цену для будних дней.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtWeekdayPrice.Focus();
                return;
            }
            WeekdayPrice = wdPrice;

            // Валидация цены выходные
            if (!decimal.TryParse(txtWeekendPrice.Text, out decimal wePrice) || wePrice <= 0)
            {
                MessageBox.Show("Введите корректную цену для выходных дней.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtWeekendPrice.Focus();
                return;
            }
            WeekendPrice = wePrice;

            IsSaved = true;
            this.Close();
        }
    }
}
