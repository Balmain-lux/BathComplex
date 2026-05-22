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
        // Режим работы: true = редактирование, false = добавление
        private bool _isEditMode;
        private int? _apartmentId;

        // Свойства для возврата данных
        public string ApartmentName { get; private set; }
        public string ApartmentType { get; private set; }
        public decimal WeekdayPrice { get; private set; }
        public decimal WeekendPrice { get; private set; }
        public bool IsSaved { get; private set; }

        public ApartmentDialog()
        {
            InitializeComponent();
            _isEditMode = false;
        }

        public void SetAddMode()
        {
            _isEditMode = false;
            _apartmentId = null;
            lblTitle.Content = "➕ Добавить новый апартамент";

            // Очищаем поля
            txtName.Text = "Апартамент №";
            cmbType.SelectedIndex = 0;
            txtWeekdayPrice.Text = "4500";
            txtWeekendPrice.Text = "5500";
        }

        public void SetEditMode(int apartmentId, string name, string type, decimal weekdayPrice, decimal weekendPrice)
        {
            _isEditMode = true;
            _apartmentId = apartmentId;
            lblTitle.Content = "✏️ Редактирование апартамента";

            // Заполняем поля
            txtName.Text = name;

            // Выбираем тип в ComboBox
            foreach (ComboBoxItem item in cmbType.Items)
            {
                if (item.Content.ToString() == type)
                {
                    cmbType.SelectedItem = item;
                    break;
                }
            }

            txtWeekdayPrice.Text = weekdayPrice.ToString();
            txtWeekendPrice.Text = weekendPrice.ToString();
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
            ApartmentType = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString();

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
