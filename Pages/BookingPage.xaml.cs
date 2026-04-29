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
    /// Логика взаимодействия для BookingPage.xaml
    /// </summary>
    public partial class BookingPage : Page
    {
        private Clients _selectedClient;
        private List<dynamic> _selectedServices = new List<dynamic>();
        public BookingPage()
        {
            InitializeComponent();

            LoadApartments();
            LoadServiceCategories();
            FillTimeCombos();
            LoadActiveBookings();
        }

        private void LoadApartments()
        {
            var apts = Connection.db.Apartments
                .Where(a => (bool)a.IsActive)
                .Select(a => new { a.ApartmentID, a.Name, a.Type })
                .ToList();

            cmbApartment.ItemsSource = apts;
            cmbApartment.DisplayMemberPath = "Name";
            cmbApartment.SelectedValuePath = "ApartmentID";
            if (apts.Any()) cmbApartment.SelectedIndex = 0;
        }

        private void LoadServiceCategories()
        {
            var categories = Connection.db.ServiceCategories.ToList();
            cmbServiceCategory.ItemsSource = categories;
            cmbServiceCategory.DisplayMemberPath = "CategoryName";
            cmbServiceCategory.SelectedValuePath = "CategoryID";
            if (categories.Any()) cmbServiceCategory.SelectedIndex = 0;
        }

        private void FillTimeCombos()
        {
            var times = new List<string>();
            for (int h = 0; h < 24; h++)
            {
                times.Add($"{h:D2}:00");
                times.Add($"{h:D2}:30");
            }
            cmbStartTime.ItemsSource = times;
            cmbEndTime.ItemsSource = times;
            cmbStartTime.SelectedItem = "10:00";
            cmbEndTime.SelectedItem = "12:00";
        }

        private void btnSelectClient_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClientSelectDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();

            if (dialog.IsSelected && dialog.SelectedClient != null)
            {
                _selectedClient = dialog.SelectedClient;
                txtClientName.Text = _selectedClient.FullName;
                lblClientInfo.Content = $"Тел: {_selectedClient.Phone} | Скидка: {_selectedClient.DiscountPercent}%";
                CalculateTotal();
            }
        }

        private void CalculateTotal(object sender = null, SelectionChangedEventArgs e = null)
        {
            if (cmbApartment.SelectedValue == null || !dpVisitDate.SelectedDate.HasValue) return;

            int aptId = (int)cmbApartment.SelectedValue;
            var date = dpVisitDate.SelectedDate.Value;
            bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
            string dayType = isWeekend ? "Weekend" : "Weekday";

            var pricing = Connection.db.ApartmentPricing
                .FirstOrDefault(p => p.ApartmentID == aptId && p.DayType == dayType);
            decimal pricePerHour = pricing?.PricePerHour ?? 0;
            lblPricePerHour.Content = $"{pricePerHour:N0} р/час";

            // Расчет часов
            TimeSpan startTs, endTs;
            if (TimeSpan.TryParse(cmbStartTime.SelectedItem?.ToString() ?? "0:00", out startTs) &&
                TimeSpan.TryParse(cmbEndTime.SelectedItem?.ToString() ?? "0:00", out endTs))
            {
                double totalHours = (endTs - startTs).TotalHours;
                if (totalHours <= 0) totalHours = 1;

                decimal apartmentCost = pricePerHour * (decimal)totalHours;
                lblApartmentCost.Content = $"{apartmentCost:N0} ";

                // Доп услуги
                decimal servicesTotal = 0;
                foreach (var service in _selectedServices)
                {
                    servicesTotal += (decimal)(service.Total ?? 0);
                }

                decimal subtotal = apartmentCost + servicesTotal;
                decimal discountAmount = 0;
                decimal finalAmount = subtotal;

                if (_selectedClient != null && _selectedClient.DiscountPercent > 0)
                {
                    discountAmount = (decimal)(subtotal * (_selectedClient.DiscountPercent / 100m));
                    finalAmount = subtotal - discountAmount;
                    lblDiscountLabel.Content = $"Скидка ({_selectedClient.DiscountPercent}%):";
                    lblDiscount.Content = $"-{discountAmount:N0} ";
                }
                else
                {
                    lblDiscountLabel.Content = "Скидка:";
                    lblDiscount.Content = "0 ";
                }

                lblSubtotalApartment.Content = $"{apartmentCost:N0} ";
                lblSubtotalServices.Content = $"{servicesTotal:N0} ";
                lblFinalAmount.Content = $"{finalAmount:N0} ";
            }
        }

        private void UpdateDayType()
        {
            // Проверяем, что контрол существует и дата выбрана
            if (lblDayType == null || !dpVisitDate.SelectedDate.HasValue)
                return;

            var date = dpVisitDate.SelectedDate.Value;
            bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

            lblDayType.Content = isWeekend ? "Выходной день (повышенный тариф)" : "Будний день";

            // Исправляем цвета на нормальные кисти
            lblDayType.Foreground = isWeekend
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC0, 0x60, 0x40))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6B, 0x8E, 0x5A));
        }

        private void cmbApartment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDayType();
            CalculateTotal();
        }

        private void dpVisitDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDayType();
            CalculateTotal();
        }

        private void cmbStartTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cmbServiceCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbServiceCategory.SelectedValue is int catId)
            {
                var services = Connection.db.Services
                    .Where(s => s.CategoryID == catId)
                    .Select(s => new { s.ServiceID, s.ServiceName, s.Price })
                    .ToList();

                cmbService.ItemsSource = services;
                cmbService.DisplayMemberPath = "ServiceName";
                cmbService.SelectedValuePath = "ServiceID";
            }
        }

        private void btnAddService_Click(object sender, RoutedEventArgs e)
        {
            dynamic selectedService = cmbService.SelectedItem;
            if (selectedService == null)
            {
                MessageBox.Show("Выберите услугу.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string name = selectedService.ServiceName;
            decimal price = selectedService.Price;

            // Проверяем, есть ли уже такая услуга
            var existing = _selectedServices.FirstOrDefault(s => s.ServiceName == name);
            if (existing != null)
            {
                existing.Quantity++;
                existing.Total = existing.Quantity * price;
            }
            else
            {
                _selectedServices.Add(new
                {
                    ServiceID = selectedService.ServiceID,
                    ServiceName = name,
                    Price = price,
                    Quantity = 1,
                    Total = price
                });
            }

            dgSelectedServices.ItemsSource = null;
            dgSelectedServices.ItemsSource = _selectedServices;
            CalculateTotal();
        }

        private void btnRemoveService_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                string name = btn.Tag.ToString();
                var item = _selectedServices.FirstOrDefault(s => s.ServiceName == name);
                if (item != null)
                {
                    _selectedServices.Remove(item);
                    dgSelectedServices.ItemsSource = null;
                    dgSelectedServices.ItemsSource = _selectedServices;
                    CalculateTotal();
                }
            }
        }

        private void btnCreateBooking_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (_selectedClient == null)
            {
                MessageBox.Show("Выберите клиента!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cmbApartment.SelectedValue == null)
            {
                MessageBox.Show("Выберите апартамент!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!dpVisitDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var date = dpVisitDate.SelectedDate.Value;
                var startTime = TimeSpan.Parse(cmbStartTime.SelectedItem.ToString());
                var endTime = TimeSpan.Parse(cmbEndTime.SelectedItem.ToString());

                var booking = new Bookings
                {
                    ClientID = _selectedClient.ClientID,
                    ApartmentID = (int)cmbApartment.SelectedValue,
                    BookingDate = DateTime.Now,
                    StartTime = date.Add(startTime),
                    EndTime = date.Add(endTime),
                    Status = "Active",
                    CreatedByUserID = UserSession.UserID,
                    DiscountApplied = _selectedClient.DiscountPercent
                };

                // Расчет финальной суммы
                decimal finalAmount = decimal.Parse(lblFinalAmount.Content.ToString().Replace(" р", "").Replace(" ", ""));
                booking.FinalAmount = finalAmount;
                booking.PaymentMethod = ((ComboBoxItem)cmbPaymentMethod.SelectedItem).Content.ToString();

                Connection.db.Bookings.Add(booking);
                Connection.db.SaveChanges();

                // Сохраняем доп. услуги
                foreach (var s in _selectedServices)
                {
                    Connection.db.BookingServices.Add(new BookingServices
                    {
                        BookingID = booking.BookingID,
                        ServiceID = s.ServiceID,
                        Quantity = s.Quantity,
                        PricePerUnit = s.Price
                    });
                }
                Connection.db.SaveChanges();

                // Меняем статус апартамента на "Занято"
                var busyStatus = Connection.db.ApartmentStatuses.FirstOrDefault(st => st.StatusName == "Занято");
                if (busyStatus != null)
                {
                    Connection.db.ApartmentStatusLog.Add(new ApartmentStatusLog
                    {
                        ApartmentID = booking.ApartmentID,
                        StatusID = busyStatus.StatusID,
                        SetByUserID = UserSession.UserID,
                        SetTime = DateTime.Now
                    });
                    Connection.db.SaveChanges();
                }

                MessageBox.Show($"Бронь #{booking.BookingID} создана!\nСумма: {finalAmount:N0} ",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистка
                _selectedClient = null;
                _selectedServices.Clear();
                txtClientName.Text = "Выберите клиента...";
                lblClientInfo.Content = "";
                dgSelectedServices.ItemsSource = null;
                LoadActiveBookings();
                CalculateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания брони: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadActiveBookings(string filter = "Active")
        {
            try
            {
                var query = Connection.db.Bookings.AsQueryable();

                if (filter == "Active")
                    query = query.Where(b => b.Status == "Active");
                else if (filter == "Finished")
                    query = query.Where(b => b.Status == "Finished");

                var bookings = (from b in query
                                join c in Connection.db.Clients on b.ClientID equals c.ClientID
                                join a in Connection.db.Apartments on b.ApartmentID equals a.ApartmentID
                                orderby b.StartTime descending
                                select new
                                {
                                    b.BookingID,
                                    ClientName = c.FullName,
                                    ApartmentName = a.Name,
                                    b.StartTime,
                                    b.EndTime,
                                    b.FinalAmount,
                                    b.Status,
                                    b.ApartmentID
                                }).Take(50).ToList();

                dgActiveBookings.ItemsSource = bookings;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRefreshBookings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFilterAll_Click(object sender, RoutedEventArgs e) => LoadActiveBookings(null);


        private void btnFilterActive_Click(object sender, RoutedEventArgs e) => LoadActiveBookings("Active");


        private void btnFilterFinished_Click(object sender, RoutedEventArgs e) => LoadActiveBookings("Finished");


        private void dgActiveBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgActiveBookings.SelectedItem != null)
            {
                dynamic selected = dgActiveBookings.SelectedItem;
                lblSelectedBookingInfo.Content = $"Бронь #{selected.BookingID} | {selected.ClientName} | {selected.ApartmentName}";
                bookingControlPanel.Visibility = Visibility.Visible;
            }
            else
            {
                bookingControlPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeApartmentStatus(string statusName)
        {
            if (dgActiveBookings.SelectedItem == null) return;
            dynamic selected = dgActiveBookings.SelectedItem;
            int aptId = selected.ApartmentID;

            var status = Connection.db.ApartmentStatuses.FirstOrDefault(s => s.StatusName == statusName);
            if (status != null)
            {
                Connection.db.ApartmentStatusLog.Add(new ApartmentStatusLog
                {
                    ApartmentID = aptId,
                    StatusID = status.StatusID,
                    SetByUserID = UserSession.UserID,
                    SetTime = DateTime.Now
                });
                Connection.db.SaveChanges();
                LoadActiveBookings();
            }
        }

        private void btnStatusBusy_Click(object sender, RoutedEventArgs e) => ChangeApartmentStatus("Занято");


        private void btnStatusCleaning_Click(object sender, RoutedEventArgs e) => ChangeApartmentStatus("Уборка");


        private void btnStatusFree_Click(object sender, RoutedEventArgs e) => ChangeApartmentStatus("Свободно");


        private void btnFinishBooking_Click(object sender, RoutedEventArgs e)
        {
            if (dgActiveBookings.SelectedItem == null) return;
            dynamic selected = dgActiveBookings.SelectedItem;
            int bookingId = selected.BookingID;

            var booking = Connection.db.Bookings.Find(bookingId);
            if (booking == null) return;

            var result = MessageBox.Show(
                $"Завершить визит #{bookingId}?\nСумма к оплате: {booking.FinalAmount:N0} р\nСпособ: {booking.PaymentMethod}",
                "Завершение визита", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                booking.Status = "Finished";
                booking.ActualEndTime = DateTime.Now;
                Connection.db.SaveChanges();
                ChangeApartmentStatus("Уборка");
                LoadActiveBookings();
                MessageBox.Show("Визит завершен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
