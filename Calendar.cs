using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public static class Calendar
    {
        public static bool IsBetween(this DateTime time, DateTime start, DateTime end)
        {
            return time >= start && time <= end;
        }

        public static int WeeksInMonth(int year, int month)
        {
            DateTime date = StartOfWorkWeek(year, month, 1);
            int lastDayInMonth = new System.Globalization.GregorianCalendar().GetDaysInMonth(year, month);
            int dayOfMonth = date.Day + 7 * 4;
            if (dayOfMonth <= lastDayInMonth)
                return 5;
            return 4;
        }

        public static DateTime StartOfPreviousWorkWeek()
        {
            DateTime now = DateTime.Now.AddDays(-7);
            return now.AddDays((int) now.DayOfWeek + 1);
        }

        public static DateTime StartOfPreviousWorkWeek(out int week)
        {
            DateTime weekago = DateTime.Now.AddDays(-7);
            DateTime prevMonday = weekago.AddDays((int) weekago.DayOfWeek + 1);
            int dayDiff = prevMonday.DayOfYear - new DateTime(prevMonday.Year, prevMonday.Month, 1).DayOfYear;
            week = 1 + (dayDiff / 7);
            return prevMonday;
        }

        public static DateTime StartOfWorkWeek(int year, int month, int week)
        {
            DateTime date = new DateTime(year, month, 1);
            if (date.DayOfWeek != DayOfWeek.Monday) {
                int firstMonday = (-(int) (date.DayOfWeek)) + 2;
                if (firstMonday < 1)
                    firstMonday += 7;
                date = new DateTime(year, month, firstMonday + (week - 1) * 7);
                if (date.DayOfWeek != DayOfWeek.Monday)
                    throw new Exception(string.Format("Error! Failed to calculate FirstDayOfWeek for week:{0} month:{1} year:{2}. Got {3} instead of Monday.", week, month, year, date.DayOfWeek.ToString()));
            }
            return date;
        }

        public static DateTime MonthStart()
        {
            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, 1);
        }

        public static DateTime MonthStart(DateTime day)
        {
            return new DateTime(day.Year, day.Month, 1);
        }

        public static DateTime MonthEnd()
        {
            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, 1).AddMonths(1).AddMinutes(-1);
        }

        public static DateTime MonthEnd(DateTime datetime)
        {
            return new DateTime(datetime.Year, datetime.Month, 1).AddMonths(1).AddMinutes(-1);
        }

        /// <summary>
        /// Get the end of given day
        /// </summary>
        /// <param name="day">DateTime to process</param>
        /// /// <returns></returns>
        public static DateTime DayEnd(this DateTime day)
        {
            return day.Date.AddDays(1).AddMinutes(-1);
        }

        public static DateTime PreviousWeekday(DayOfWeek day)
        {
            DateTime today = DateTime.Today;
            int daydiff = (int) day - (int) today.DayOfWeek;
            return today.AddDays(daydiff > 0 ? daydiff : daydiff - 7);
        }

        public static DateTime NextWeekday(DayOfWeek day)
        {
            DateTime today = DateTime.Today;
            int daydiff = today.DayOfWeek - day;
            return today.AddDays(-7 + (int) day).Date;
        }

        /// <summary>
        /// Get an IEnumerable of all individual days for DateTimes between the start and end
        /// </summary>
        /// <param name="start">Start date range</param>
        /// <param name="end">End date range</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> DaysBetween(DateTime start, DateTime end)
        {
            for (DateTime day = start.Date; day <= end; day = day.AddDays(1))
                yield return day;
        }

        /// Get an IEnumerable of all datetime holidays between the start and end
        /// </summary>
        /// <param name="start">Start date range</param>
        /// <param name="end">End date range</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> HolidaysBetween(DateTime start, DateTime end)
        {
            for (var day = start.Date; day <= end; day = day.AddDays(1)) {
                if (day.IsHoliday())
                    yield return day;
            }
        }

        /// <summary>
        /// Get count of how many work days are in the given month on a given year
        /// </summary>
        /// <param name="month">Month to count work days in</param>
        /// <param name="year">Year because holidays can be on weekends some years</param>
        /// <returns></returns>
        public static int WorkDaysInMonth(int month, int year)
        {
            DateTime startdate = new DateTime(year, month, 1);
            DateTime enddate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            return WorkDaysBetween(startdate, enddate).Count();
        }

        /// <summary>
        /// Get an IEnumerable of all individual days for DateTimes between the start and end, excluding holidays
        /// </summary>
        /// <param name="start">Start date range</param>
        /// <param name="end">End date range</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> WorkDaysBetween(DateTime start, DateTime end)
        {
            return DaysBetween(start, end).Where(date => IsWorkDay(date));
        }

        /// <summary>
        /// Checks if DateTime is considered a workday. A workday is a weekday that isn't a holiday.
        /// </summary>
        /// <param name="date">The day to determine if it's a weekday.</param>
        /// <returns>True if the date is a weekday. False otherwise.</returns>
        public static bool IsWorkDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday
                && date.DayOfWeek != DayOfWeek.Sunday
                && !date.IsHoliday();
        }

        /// <summary>
        /// Returns DateTime Collection of all days this year
        /// </summary>
        /// <returns>A list of days in this year.</returns>
        public static IEnumerable<DateTime> DaysThisYear()
        {
            DateTime start = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime end = new DateTime(DateTime.Now.Year + 1, 1, 1).AddDays(-1);
            return DaysBetween(start, end);
        }

        /// <summary>
        /// Determines if this date is a paid holiday:
        /// These include: New Years, MLK Jr, Washington's Bday, Memorial Day, Independence Day, Labor Day, Columbus Day, Veterans Day, Thanksgiving, Christmas
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>True if this date is a paid holiday.</returns>
        public static bool IsHoliday(this DateTime date)
        {
            int weekOfMonth = (date.Day - 1) / 7 + 1;
            int day = date.Day;
            DayOfWeek dow = date.DayOfWeek;

            // https://en.wikipedia.org/wiki/Public_holidays_in_the_United_States#Holiday_listing_as_paid_time_off
            // New Years (January 1st, or preceding Friday/following Monday if weekend)
            // Christmas (December 25th, or preceding Friday/following Monday if weekend)
            // Martin Luther King Jr (3rd Monday of January, between 15th-21st)
            // Memorial Day (Last Monday in May)
            // Labor Day (First Monday in September)
            // Independence Day (July 4, or preceding Friday/following Monday if weekend)

            switch (date.Month) {
                case 1:
                    // New Years
                    if (day == 1)
                        return dow != DayOfWeek.Saturday || dow != DayOfWeek.Sunday;
                    if (day == 2)
                        return dow == DayOfWeek.Monday;
                    // MLK Jr Day
                    return weekOfMonth == 3 && dow == DayOfWeek.Monday;
                // case 2:
                //// President’s Day (3rd Monday in February)
                //if (date.Month == 2 && isMonday && nthWeekDay == 3) return true;
                // case 4:
                //// Veteran’s Day (November 11, or preceding Friday/following Monday if weekend))
                //if ((date.Month == 11 && date.Day == 10 && isFriday) ||
                //    (date.Month == 11 && date.Day == 11 && !isWeekend) ||
                //    (date.Month == 11 && date.Day == 12 && isMonday)) return true;
                case 5:
                    // Memorial Day 
                    return dow == DayOfWeek.Monday && date.AddDays(7).Month == 6;
                case 7:
                    // Independence Day
                    return (date.Day == 3 && dow == DayOfWeek.Friday)
                        || (date.Day == 4 && dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday)
                        || (date.Day == 5 && dow == DayOfWeek.Monday);
                case 9:
                    // Labor Day
                    return dow == DayOfWeek.Monday && weekOfMonth == 1; //Labor Day
                                                                        // case 10:
                                                                        //// Columbus Day (2nd Monday in October)
                                                                        //if (date.Month == 10 && isMonday && nthWeekDay == 2) return true;
                case 11:
                    //Thanksgiving + day after Thanksgiving
                    return weekOfMonth == 4 && (dow == DayOfWeek.Thursday || dow == DayOfWeek.Friday);
                case 12:
                    // New Years
                    if (day == 31)
                        return dow == DayOfWeek.Friday;
                    // Christmas
                    if (day == 24)
                        return dow == DayOfWeek.Friday;
                    if (day == 25)
                        return dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday;
                    if (day == 26)
                        return dow == DayOfWeek.Monday;
                    break;
            }
            return false;
        }
    }
}