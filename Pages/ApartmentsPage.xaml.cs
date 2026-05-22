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
    /// Логика взаимодействия для ApartmentsPage.xaml
    /// </summary>
    public partial class ApartmentsPage : Page
    {
        public ApartmentsPage()
        {
            InitializeComponent();
            LoadApartments();
        }

        private void LoadApartments()
        {
            try
            {
                // Сначала загружаем данные в память
                var apartments = (from a in Connection.db.Apartments
                                  join pwd in Connection.db.ApartmentPricing on a.ApartmentID equals pwd.ApartmentID
                                  join pwe in Connection.db.ApartmentPricing on a.ApartmentID equals pwe.ApartmentID
                                  where pwd.DayType == "Weekday" && pwe.DayType == "Weekend" && a.IsActive == true
                                  select new
                                  {
                                      a.ApartmentID,
                                      a.Name,
                                      a.Type,
                                      WeekdayPrice = pwd.PricePerHour,
                                      WeekendPrice = pwe.PricePerHour
                                  }).ToList(); // <-- вызываем ToList() здесь

                // Теперь в памяти добавляем статус
                var result = apartments.Select(a => new
                {
                    a.ApartmentID,
                    a.Name,
                    a.Type,
                    a.WeekdayPrice,
                    a.WeekendPrice,
                    CurrentStatus = GetCurrentStatus(a.ApartmentID)
                }).ToList();

                dgApartments.ItemsSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetCurrentStatus(int apartmentId)
        {
            var lastStatus = Connection.db.ApartmentStatusLog
                .Where(s => s.ApartmentID == apartmentId)
                .OrderByDescending(s => s.SetTime)
                .FirstOrDefault();

            if (lastStatus == null) return "Свободно";

            return Connection.db.ApartmentStatuses
                .FirstOrDefault(s => s.StatusID == lastStatus.StatusID)?.StatusName ?? "Свободно";
        }

        private void btnAddApartment_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ApartmentDialog();
            dialog.Owner = Application.Current.MainWindow;
            dialog.SetAddMode();  // <-- Устанавливаем режим добавления
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                try
                {
                    // Создаем новый апартамент
                    var newApt = new Apartments
                    {
                        Name = dialog.ApartmentName,
                        Type = dialog.ApartmentType,
                        IsActive = true
                    };
                    Connection.db.Apartments.Add(newApt);
                    Connection.db.SaveChanges();

                    // Добавляем цены
                    Connection.db.ApartmentPricing.Add(new ApartmentPricing
                    {
                        ApartmentID = newApt.ApartmentID,
                        DayType = "Weekday",
                        PricePerHour = dialog.WeekdayPrice
                    });
                    Connection.db.ApartmentPricing.Add(new ApartmentPricing
                    {
                        ApartmentID = newApt.ApartmentID,
                        DayType = "Weekend",
                        PricePerHour = dialog.WeekendPrice
                    });
                    Connection.db.SaveChanges();

                    LoadApartments();
                    MessageBox.Show("Апартамент успешно добавлен!", "Успех",
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
            LoadApartments();
        }

        private void btnEditApartment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int aptId = Convert.ToInt32(btn.Tag);

                // Загружаем данные апартамента
                var apartment = Connection.db.Apartments.Find(aptId);
                if (apartment == null)
                {
                    MessageBox.Show("Апартамент не найден.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var pricingWeekday = Connection.db.ApartmentPricing
                    .FirstOrDefault(p => p.ApartmentID == aptId && p.DayType == "Weekday");
                var pricingWeekend = Connection.db.ApartmentPricing
                    .FirstOrDefault(p => p.ApartmentID == aptId && p.DayType == "Weekend");

                if (pricingWeekday == null || pricingWeekend == null)
                {
                    MessageBox.Show("Цены для апартамента не найдены.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Открываем диалог в режиме редактирования
                var dialog = new ApartmentDialog();
                dialog.Owner = Application.Current.MainWindow;
                dialog.SetEditMode(
                    aptId,
                    apartment.Name,
                    apartment.Type,
                    pricingWeekday.PricePerHour,
                    pricingWeekend.PricePerHour
                );
                dialog.ShowDialog();

                if (dialog.IsSaved)
                {
                    try
                    {
                        // Обновляем данные
                        apartment.Name = dialog.ApartmentName;
                        apartment.Type = dialog.ApartmentType;

                        pricingWeekday.PricePerHour = dialog.WeekdayPrice;
                        pricingWeekend.PricePerHour = dialog.WeekendPrice;

                        Connection.db.SaveChanges();
                        LoadApartments();

                        MessageBox.Show("Апартамент успешно обновлен!", "Успех",
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

        private void btnDeleteApartment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int aptId = Convert.ToInt32(btn.Tag);
                var result = MessageBox.Show($"Удалить апартамент ID: {aptId}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var apt = Connection.db.Apartments.Find(aptId);
                        if (apt != null)
                        {
                            apt.IsActive = false; // Мягкое удаление
                            Connection.db.SaveChanges();
                            LoadApartments();
                        }
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
