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

namespace BathComplex.Pages
{
    /// <summary>
    /// Логика взаимодействия для AnalyticsPage.xaml
    /// </summary>
    public partial class AnalyticsPage : Page
    {
        private static readonly string[] PieColors = new[]
        {
            "#B8943A", "#B85450", "#5A7D4A", "#C58C40",
            "#7B6B8A", "#4A7B8A", "#8A6B4A", "#5A7B6B"
        };
        public AnalyticsPage()
        {
            InitializeComponent();
            this.Loaded += (s, e) => LoadAllAnalytics();
        }

        private void LoadAllAnalytics()
        {
            LoadKeyMetrics();
            LoadIncomeByDayChart();
            LoadExpensesByCategoryChart();
            LoadFinanceSummary();
            LoadTopClients();
            LoadApartmentStats();
        }

        private void LoadKeyMetrics()
        {
            try
            {
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var today = DateTime.Today;
                var todayNext = today.AddDays(1); // <-- вынесли

                // Всего визитов
                var totalVisits = Connection.db.Bookings
                    .Count(b => b.Status == "Finished"
                             && b.StartTime >= monthStart
                             && b.StartTime < todayNext);
                lblTotalVisits.Content = totalVisits.ToString();

                // Доход за месяц
                var monthIncome = Connection.db.Bookings
                    .Where(b => b.Status == "Finished"
                             && b.StartTime >= monthStart
                             && b.StartTime < todayNext)
                    .Sum(b => (decimal?)b.FinalAmount) ?? 0;  // <-- Суммируем FinalAmount, а не StartTime!
                lblMonthIncome.Content = $"{monthIncome:N0} ";

                // Расходы за месяц
                var monthExpenses = Connection.db.Expenses
                    .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= today)
                    .Sum(e => (decimal?)e.Amount) ?? 0;
                lblMonthExpenses.Content = $"{monthExpenses:N0} ";

                // Прибыль
                decimal profit = monthIncome - monthExpenses;
                lblMonthProfit.Content = $"{profit:N0} ";
                lblMonthProfit.Foreground = profit >= 0
                    ? new SolidColorBrush(Color.FromRgb(0x5A, 0x7D, 0x4A))
                    : new SolidColorBrush(Color.FromRgb(0xB8, 0x54, 0x50));

                // Средний чек
                decimal avgCheck = totalVisits > 0 ? monthIncome / totalVisits : 0;
                lblAvgCheck.Content = $"{avgCheck:N0} ";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка ключевых показателей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadIncomeByDayChart()
        {
            try
            {
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var today = DateTime.Today;

                // Исправлено: StartTime — не nullable, обращаемся напрямую
                var todayNext = today.AddDays(1);
                var incomeByDay = Connection.db.Bookings
                    .Where(b => b.Status == "Finished"
                             && b.StartTime >= monthStart
                             && b.StartTime < todayNext)
                    .AsEnumerable()
                    .GroupBy(b => b.StartTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Total = g.Sum(x => x.FinalAmount) ?? 0
                    })
                    .ToList();

                decimal maxIncome = incomeByDay.Any() ? incomeByDay.Max(x => x.Total) : 1;
                if (maxIncome == 0) maxIncome = 1;

                var chartData = new List<dynamic>();
                for (var d = monthStart; d <= today; d = d.AddDays(1))
                {
                    var dayIncome = incomeByDay.FirstOrDefault(x => x.Date == d);
                    decimal amount = dayIncome?.Total ?? 0;
                    double barHeight = (double)(amount / maxIncome * 130);

                    chartData.Add(new
                    {
                        Key = d.ToString("dd.MM"),
                        Value = amount,
                        ValueText = amount > 0 ? $"{amount / 1000:F1}K" : "",
                        BarHeight = Math.Max(barHeight, amount > 0 ? 4 : 0)
                    });
                }

                icIncomeChart.ItemsSource = chartData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка диаграммы доходов: {ex.Message}");
            }
        }


        private void LoadExpensesByCategoryChart()
        {
            try
            {
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var today = DateTime.Today;

                var expenses = (from e in Connection.db.Expenses
                                join cat in Connection.db.ExpenseCategories
                                    on e.CategoryID equals cat.CategoryID
                                where e.ExpenseDate >= monthStart && e.ExpenseDate <= today
                                group e by cat.CategoryName into g
                                select new
                                {
                                    Category = g.Key,
                                    Total = g.Sum(x => x.Amount)
                                })
                                .AsEnumerable()
                                .Where(x => x.Total > 0)
                                .OrderByDescending(x => x.Total)
                                .ToList();

                decimal total = expenses.Sum(x => x.Total);
                if (total == 0)
                {
                    pieCanvas.Children.Clear();
                    icPieLegend.ItemsSource = new[] { new { Color = "#7A756D", Label = "Нет расходов", ValueText = "0 " } };
                    return;
                }

                pieCanvas.Children.Clear();
                double centerX = 110, centerY = 110, radius = 100;
                double startAngle = -90;

                var legendData = new List<dynamic>();

                for (int i = 0; i < expenses.Count; i++)
                {
                    double sweepAngle = (double)(expenses[i].Total / total) * 360;
                    string colorHex = PieColors[i % PieColors.Length];

                    var path = new Path
                    {
                        Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHex),
                        Stroke = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
                        StrokeThickness = 1.5
                    };

                    var figure = new PathFigure { StartPoint = new Point(centerX, centerY), IsClosed = true };
                    double angleRad1 = startAngle * Math.PI / 180;
                    figure.Segments.Add(new LineSegment(
                        new Point(centerX + radius * Math.Cos(angleRad1),
                                  centerY + radius * Math.Sin(angleRad1)), true));

                    double angleRad2 = (startAngle + sweepAngle) * Math.PI / 180;
                    bool isLargeArc = sweepAngle > 180;
                    figure.Segments.Add(new ArcSegment(
                        new Point(centerX + radius * Math.Cos(angleRad2),
                                  centerY + radius * Math.Sin(angleRad2)),
                        new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true));

                    var geometry = new PathGeometry();
                    geometry.Figures.Add(figure);
                    path.Data = geometry;
                    pieCanvas.Children.Add(path);

                    // Подпись процента
                    double midAngle = (startAngle + sweepAngle / 2) * Math.PI / 180;
                    double labelRadius = radius * 0.65;
                    double labelX = centerX + labelRadius * Math.Cos(midAngle);
                    double labelY = centerY + labelRadius * Math.Sin(midAngle);

                    if (sweepAngle > 12)
                    {
                        var textBlock = new TextBlock
                        {
                            Text = $"{expenses[i].Total / total * 100:F0}%",
                            Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
                            FontSize = 11,
                            FontWeight = FontWeights.SemiBold
                        };
                        Canvas.SetLeft(textBlock, labelX - 18);
                        Canvas.SetTop(textBlock, labelY - 10);
                        pieCanvas.Children.Add(textBlock);
                    }

                    legendData.Add(new
                    {
                        Color = colorHex,
                        Label = expenses[i].Category,
                        ValueText = $"{expenses[i].Total:N0} "
                    });

                    startAngle += sweepAngle;
                }

                icPieLegend.ItemsSource = legendData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка круговой диаграммы: {ex.Message}");
            }
        }

        private void LoadFinanceSummary()
        {
            try
            {
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var today = DateTime.Today;

                // Исправлено: работаем в памяти через AsEnumerable
                var incomeByDay = Connection.db.Bookings
                    .Where(b => b.Status == "Finished")
                    .AsEnumerable()
                    .GroupBy(b => b.StartTime.Date)
                    .Select(g => new { Date = g.Key, Income = g.Sum(x => x.FinalAmount) ?? 0 })
                    .ToList();

                var expensesByDay = Connection.db.Expenses
                    .AsEnumerable()
                    .GroupBy(e => e.ExpenseDate)
                    .Select(g => new { Date = g.Key, Expenses = g.Sum(x => x.Amount) })
                    .ToList();

                var summary = new List<dynamic>();
                for (var d = monthStart; d <= today; d = d.AddDays(1))
                {
                    var inc = incomeByDay.FirstOrDefault(x => x.Date == d)?.Income ?? 0;
                    var exp = expensesByDay.FirstOrDefault(x => x.Date == d)?.Expenses ?? 0;
                    var profit = inc - exp;

                    summary.Add(new
                    {
                        Date = d,
                        Income = inc,
                        Expenses = exp,
                        Profit = profit,
                        IsPositive = profit >= 0,
                        ResultText = profit >= 0 ? "↑ Прибыль" : "↓ Убыток"
                    });
                }

                dgFinanceSummary.ItemsSource = summary.OrderByDescending(x => x.Date).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сводного отчета: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTopClients()
        {
            try
            {
                var clients = Connection.db.Clients.ToList();
                var topList = new List<dynamic>();

                foreach (var c in clients)
                {
                    var bookings = Connection.db.Bookings
                        .Where(b => b.ClientID == c.ClientID && b.Status == "Finished")
                        .ToList();

                    if (bookings.Any())
                    {
                        topList.Add(new
                        {
                            c.FullName,
                            c.Phone,
                            VisitCount = bookings.Count,
                            TotalSpent = bookings.Sum(b => b.FinalAmount) ?? 0,
                            AvgCheck = (decimal)(bookings.Average(b => (double?)b.FinalAmount) ?? 0)
                        });
                    }
                }

                int rank = 1;
                var ranked = topList.OrderByDescending(x => x.TotalSpent).Take(15).Select(x => new
                {
                    Rank = rank++,
                    x.FullName,
                    x.Phone,
                    x.VisitCount,
                    x.TotalSpent,
                    x.AvgCheck
                }).ToList();

                dgTopClients.ItemsSource = ranked;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка ТОП клиентов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadApartmentStats()
        {
            try
            {
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var today = DateTime.Today;
                int daysInMonth = Math.Max((int)(today - monthStart).TotalDays + 1, 1);

                var apartments = Connection.db.Apartments.Where(a => (bool)a.IsActive).ToList();
                var statsList = new List<dynamic>();

                foreach (var apt in apartments)
                {
                    var todayNext = today.AddDays(1);
                    var bookings = Connection.db.Bookings
                        .Where(b => b.ApartmentID == apt.ApartmentID
                                 && b.Status == "Finished"
                                 && b.StartTime >= monthStart
                                 && b.StartTime < todayNext)
                        .ToList();

                    decimal totalHours = 0;
                    foreach (var b in bookings)
                    {
                        DateTime start;
                        DateTime end;

                        // Проверяем ActualStartTime/ActualEndTime через приведение типов
                        try
                        {
                            start = b.ActualStartTime != null ? (DateTime)b.ActualStartTime : b.StartTime;
                            end = b.ActualEndTime != null ? (DateTime)b.ActualEndTime : b.EndTime;
                        }
                        catch
                        {
                            // Если не получается — используем плановое время
                            start = b.StartTime;
                            end = b.EndTime;
                        }

                        if (end > start)
                        {
                            totalHours += (decimal)(end - start).TotalHours;
                        }
                    }

                    statsList.Add(new
                    {
                        apt.Name,
                        VisitCount = bookings.Count,
                        TotalIncome = bookings.Sum(b => b.FinalAmount) ?? 0,
                        OccupancyRate = daysInMonth > 0
                            ? Math.Min(100, Math.Round(totalHours / (daysInMonth * 24) * 100, 1))
                            : 0
                    });
                }

                icApartmentBars.ItemsSource = statsList.OrderByDescending(x => x.OccupancyRate).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка статистики апартаментов: {ex.Message}");
            }
        }


    }
}
