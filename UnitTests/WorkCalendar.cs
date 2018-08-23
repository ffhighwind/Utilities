using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class WorkCalendarTests
    {
        private const DayOfWeek WEEK_START = DayOfWeek.Monday;

        [TestMethod]
        public void PreviousWeek()
        {
            //PreviousWeek
            for (int year = 2; year <= 9999; year++) {
                for (int month = 1; month <= 12; month++) {
                    for (int week = 1; week <= WorkCalendar.WeeksInMonth(year, month); week++) {
                        DateTime date1 = WorkCalendar.WeekStart(year, month, week);
                        int week1 = (date1.Day - 1) / 7 + 1;
                        if (week1 != week)
                            throw new Exception();
                    }
                }
            }
        }

        [TestMethod]
        public void WeeksInMonth()
        {
            for (int year = 1; year <= 9998; year++) {
                var holidays = WorkCalendar.Holidays(year);
                for (int month = 1; month <= 12; month++) {
                    int weeks1 = WorkCalendar.WeeksInMonth(year, month);
                    int weeks2 = WorkCalendar.FirstDay(year, month, WEEK_START).AddDays(7 * 4).Month == month ? 5 : 4;
                    if (weeks1 != weeks2)
                        throw new Exception();
                }
            }
        }

        [TestMethod]
        public void IsHoliday()
        {
            for (int year = 1; year <= 9998; year++) {
                var holidays = WorkCalendar.Holidays(year);
                for (int month = 1; month <= 12; month++) {
                    int days = DateTime.DaysInMonth(year, month);
                    for (int day = 1; day <= days; day++) {
                        DateTime date = new DateTime(year, month, day);
                        bool a = WorkCalendar.IsHoliday(date);
                        bool b = holidays.Contains(date);
                        if (a != b)
                            throw new Exception();
                    }
                }
            }
        }

        [TestMethod]
        public void WeekStart()
        {
            for (int year = 1; year <= 9998; year++) {
                int weekCount = 0;
                for (int month = 1; month <= 12; month++) {
                    DateTime week1 = WorkCalendar.FirstDay(year, month, DayOfWeek.Monday);
                    int weeks = WorkCalendar.WeeksInMonth(year, month);
                    weekCount += weeks;
                    for (int week = 0; week < weeks; week++) {
                        DateTime date1 = week1.AddDays(7 * week);
                        DateTime date2 = WorkCalendar.WeekStart(year, month, week + 1);
                        if (date1 != date2)
                            throw new Exception();
                    }
                }
                if (weekCount < 52 || weekCount > 53)
                    throw new Exception();
                weekCount = 0;
            }
        }

        [TestMethod]
        public void FirstDay()
        {
            for (int year = 1; year <= 9999; year++) {
                for (int month = 1; month <= 12; month++) {
                    for (int day = 1; day <= 7; day++) {
                        DateTime date = new DateTime(year, month, day);
                        if (WorkCalendar.FirstDay(year, month, date.DayOfWeek) != date)
                            throw new Exception();
                    }
                }
            }
        }

        [TestMethod]
        public void PreviousDay()
        {
            for (int year = 2; year <= 9999; year++) {
                for (int month = 1; month <= 12; month++) {
                    int days = DateTime.DaysInMonth(year, month);
                    for (int day = 1; day <= days; day++) {
                        DateTime date = new DateTime(year, month, day);
                        for (int i = 0; i <= 6; i++) {
                            DateTime date2 = WorkCalendar.PreviousDay(date, (DayOfWeek) i);
                            if (date2.DayOfWeek != (DayOfWeek) i)
                                throw new Exception();
                            double totalDays = (date - date2).TotalDays;
                            if (totalDays > 7 || totalDays <= 0)
                                throw new Exception();
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void NextDay()
        {
            for (int year = 1; year <= 9998; year++) {
                for (int month = 1; month <= 12; month++) {
                    int days = DateTime.DaysInMonth(year, month);
                    for (int day = 1; day <= days; day++) {
                        DateTime date = new DateTime(year, month, day);
                        for (int i = 0; i <= 6; i++) {
                            DateTime date2 = WorkCalendar.NextDay(date, (DayOfWeek) i);
                            if (date2.DayOfWeek != (DayOfWeek) i)
                                throw new Exception();
                            double totalDays = (date2 - date).TotalDays;
                            if (totalDays > 7 || totalDays <= 0)
                                throw new Exception();
                        }
                    }
                }
            }

        }
    }
}
