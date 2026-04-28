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
    /// Логика взаимодействия для ExpenseDialog.xaml
    /// </summary>
    public partial class ExpenseDialog : Window
    {
        public string Description { get; private set; }
        public decimal Amount { get; private set; }
        public int CategoryID { get; private set; }
        public DateTime ExpenseDate { get; private set; }
        public bool IsSaved { get; private set; }
        public ExpenseDialog()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            var categories = Connection.db.ExpenseCategories.ToList();
            cmbCategory.ItemsSource = categories;
            cmbCategory.DisplayMemberPath = "CategoryName";
            cmbCategory.SelectedValuePath = "CategoryID";
            if (categories.Any()) cmbCategory.SelectedIndex = 0;
        }

        public void LoadExpenseData(string description, decimal amount, int categoryId, DateTime date)
        {
            txtDescription.Text = description;
            txtAmount.Text = amount.ToString();
            cmbCategory.SelectedValue = categoryId;
            dpExpenseDate.SelectedDate = date;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация описания
            Description = txtDescription.Text.Trim();
            if (string.IsNullOrWhiteSpace(Description))
            {
                MessageBox.Show("Введите описание расхода.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDescription.Focus();
                return;
            }

            // Валидация суммы
            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму (больше 0).", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                return;
            }
            Amount = amount;

            // Категория
            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию расхода.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CategoryID = (int)cmbCategory.SelectedValue;

            // Дата
            ExpenseDate = dpExpenseDate.SelectedDate ?? DateTime.Today;

            IsSaved = true;
            this.Close();
        }
    }
}
