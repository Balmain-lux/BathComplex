using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
    /// Логика взаимодействия для FinancePage.xaml
    /// </summary>
    public partial class FinancePage : Page
    {
        private DateTime _startDate;
        private DateTime _endDate;
        public FinancePage()
        {
            InitializeComponent();
            SetDefaultPeriod();
            LoadSummary();
            LoadIncome();
        }

        private void SetDefaultPeriod()
        {
            _startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _endDate = DateTime.Today;
            dpStartDate.SelectedDate = _startDate;
            dpEndDate.SelectedDate = _endDate;
        }

        private void btnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (dpStartDate.SelectedDate.HasValue && dpEndDate.SelectedDate.HasValue)
            {
                _startDate = dpStartDate.SelectedDate.Value;
                _endDate = dpEndDate.SelectedDate.Value;
                RefreshAll();
            }
        }

        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            _startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _endDate = DateTime.Today;
            dpStartDate.SelectedDate = _startDate;
            dpEndDate.SelectedDate = _endDate;
            RefreshAll();
        }

        private void btnThisYear_Click(object sender, RoutedEventArgs e)
        {
            _startDate = new DateTime(DateTime.Today.Year, 1, 1);
            _endDate = DateTime.Today;
            dpStartDate.SelectedDate = _startDate;
            dpEndDate.SelectedDate = _endDate;
            RefreshAll();
        }

        private void RefreshAll()
        {
            LoadSummary();
            if (dgIncome.Visibility == Visibility.Visible)
                LoadIncome();
            else
                LoadExpenses();
        }

        private void LoadSummary()
        {
            try
            {
                // Используем _startDate и _endDate (поля класса, начинаются с _)
                var endDateInclusive = _endDate.AddDays(1);

                // Доходы
                var income = Connection.db.Bookings
                    .Where(b => b.Status == "Finished"
                             && b.StartTime >= _startDate
                             && b.StartTime < endDateInclusive)
                    .Sum(b => (decimal?)b.FinalAmount) ?? 0;

                lblTotalIncome.Content = $"{income:N0} ";

                // Расходы
                var expenses = Connection.db.Expenses
                    .Where(e => e.ExpenseDate >= _startDate && e.ExpenseDate <= _endDate)
                    .Sum(e => (decimal?)e.Amount) ?? 0;

                lblTotalExpenses.Content = $"{expenses:N0} ";

                // Прибыль
                decimal profit = income - expenses;
                lblProfit.Content = $"{profit:N0} ";

                if (profit >= 0)
                {
                    lblProfit.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0x6B, 0x8E, 0x5A));
                    lblProfit.Content = $"+{profit:N0} ";
                }
                else
                {
                    lblProfit.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0xB8, 0x54, 0x50));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки итогов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadIncome()
        {
            try
            {
                var endDateInclusive = _endDate.AddDays(1); // <-- ДО запроса!

                var income = (from b in Connection.db.Bookings
                              join c in Connection.db.Clients on b.ClientID equals c.ClientID
                              join a in Connection.db.Apartments on b.ApartmentID equals a.ApartmentID
                              where b.Status == "Finished"
                                 && b.StartTime >= _startDate
                                 && b.StartTime < endDateInclusive  // <-- переменная, не метод
                              orderby b.StartTime descending
                              select new
                              {
                                  b.BookingID,
                                  Date = b.StartTime,
                                  ClientName = c.FullName,
                                  ApartmentName = a.Name,
                                  Amount = b.FinalAmount ?? 0,
                                  b.PaymentMethod,
                                  b.Status
                              }).ToList();

                dgIncome.ItemsSource = income;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки доходов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadExpenses()
        {
            try
            {
                var expenses = (from e in Connection.db.Expenses
                                join cat in Connection.db.ExpenseCategories
                                    on e.CategoryID equals cat.CategoryID
                                where e.ExpenseDate >= _startDate && e.ExpenseDate <= _endDate
                                orderby e.ExpenseDate descending
                                select new
                                {
                                    e.ExpenseID,
                                    e.ExpenseDate,
                                    e.Description,
                                    CategoryName = cat.CategoryName,
                                    e.Amount
                                }).ToList();

                dgExpenses.ItemsSource = expenses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки расходов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnTabIncome_Click(object sender, RoutedEventArgs e)
        {
            dgIncome.Visibility = Visibility.Visible;
            dgExpenses.Visibility = Visibility.Collapsed;
            LoadIncome();
            HighlightTab(true);
        }

        private void btnTabExpenses_Click(object sender, RoutedEventArgs e)
        {
            dgIncome.Visibility = Visibility.Collapsed;
            dgExpenses.Visibility = Visibility.Visible;
            LoadExpenses();
            HighlightTab(false);
        }

        private void HighlightTab(bool isIncome)
        {
            if (isIncome)
            {
                btnTabIncome.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x3C, 0x3C, 0x3C));
                btnTabExpenses.Background = System.Windows.Media.Brushes.Transparent;
            }
            else
            {
                btnTabExpenses.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x3C, 0x3C, 0x3C));
                btnTabIncome.Background = System.Windows.Media.Brushes.Transparent;
            }
        }

        private void btnAddExpense_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ExpenseDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                try
                {
                    var expense = new Expenses
                    {
                        Description = dialog.Description,
                        Amount = dialog.Amount,
                        CategoryID = dialog.CategoryID,
                        ExpenseDate = dialog.ExpenseDate,
                        CreatedByUserID = UserSession.UserID
                    };
                    Connection.db.Expenses.Add(expense);
                    Connection.db.SaveChanges();

                    LoadSummary();
                    LoadExpenses();
                    MessageBox.Show("Расход добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnEditExpense_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int expenseId = Convert.ToInt32(btn.Tag);
                var expense = Connection.db.Expenses.Find(expenseId);
                if (expense == null) return;

                var dialog = new ExpenseDialog();
                dialog.Owner = Application.Current.MainWindow;
                dialog.Title = "Редактирование расхода";
                dialog.LoadExpenseData(expense.Description, expense.Amount,
                    expense.CategoryID ?? 1, expense.ExpenseDate);
                dialog.ShowDialog();

                if (dialog.IsSaved)
                {
                    try
                    {
                        expense.Description = dialog.Description;
                        expense.Amount = dialog.Amount;
                        expense.CategoryID = dialog.CategoryID;
                        expense.ExpenseDate = dialog.ExpenseDate;
                        Connection.db.SaveChanges();

                        LoadSummary();
                        LoadExpenses();
                        MessageBox.Show("Расход обновлен!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnDeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int expenseId = Convert.ToInt32(btn.Tag);
                var expense = Connection.db.Expenses.Find(expenseId);
                if (expense == null) return;

                var result = MessageBox.Show($"Удалить расход \"{expense.Description}\" на сумму {expense.Amount:N0} ?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Connection.db.Expenses.Remove(expense);
                        Connection.db.SaveChanges();

                        LoadSummary();
                        LoadExpenses();
                        MessageBox.Show("Расход удален.", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
