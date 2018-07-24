using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    /// Time class by Wes Rollings.
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// Checks if DateTime is between two other DateTimes
        /// </summary>
        /// <param name="compare">Time to be checked</param>
        /// <param name="d0">Start time</param>
        /// <param name="d1">End time</param>
        /// <returns></returns>
        public static bool IsBetween(this DateTime compare, DateTime d0, DateTime d1)
        {
            return (compare.Ticks >= d0.Ticks && compare.Ticks <= d1.Ticks) ? true : false;
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
            DateTime now = DateTime.Now;
            return now.AddDays(-((int) now.DayOfWeek) + 1 - 7);
        }

        public static DateTime StartOfPreviousWorkWeek(out int week)
        {
            DateTime now = DateTime.Now;
            DateTime prevMonday = now.AddDays(-((int) now.DayOfWeek) + 1 - 7);
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

        /// <summary>
        /// Get current month's start
        /// </summary>
        /// <returns></returns>
        public static DateTime MonthStart(DateTime day)
        {
            return new DateTime(day.Year, day.Month, 1);
        }

        /// <summary>
        /// Get current month's end
        /// </summary>
        /// <returns></returns>
        public static DateTime MonthEnd(DateTime? startingpoint)
        {
            DateTime setup = startingpoint == null ? DateTime.Now : (DateTime) startingpoint;

            return new DateTime(setup.Year, setup.Month, 1).AddMonths(1).AddDays(-1);

        }

        /// <summary>
        /// Get the start of given day
        /// </summary>
        /// <param name="day">DateTime to process</param>
        /// <returns></returns>
        public static DateTime DayStart(this DateTime day)
        {
            return day.Date;
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

        /// <summary>
        /// Finds the DateTime of the previous day of the week specified
        /// </summary>
        /// <param name="day">Enum of the weekday</param>
        /// <returns></returns>
        public static DateTime PreviousWeekday(DayOfWeek day)
        {
            DateTime today = DateTime.Now;
            return today.AddDays(-6 - ((int) today.DayOfWeek)).Date;
        }

        /// <summary>
        /// Finds the DateTime of the next day of the week specified
        /// </summary>
        /// <param name="day">Enum of the weekday</param>
        /// <returns></returns>
        public static DateTime NextWeekday(DayOfWeek day)
        {
            DateTime d = DateTime.Today;
            while (d.DayOfWeek != day) {
                d = d.AddDays(1);
            }
            return d;
        }

        /// <summary>
        /// Looks for the previous day number specified going back
        /// </summary>
        /// <param name="daynumber">Numbered day to get last month</param>
        /// <returns></returns>
        public static DateTime PreviousDayNumber(int daynumber, DateTime? startingpoint)
        {
            DateTime setup = startingpoint == null ? DateTime.Now : (DateTime) startingpoint;
            if (setup.Day > daynumber) {
                return new DateTime(setup.Year, setup.Month, daynumber);
            }
            else {
                return new DateTime(setup.Year, setup.Month, daynumber).AddMonths(-1);
            }
        }

        /// <summary>
        /// Looks for the next day number specified going forward
        /// </summary>
        /// <param name="daynumber">Numbered day to get next month</param>
        /// <returns></returns>
        public static DateTime NextDayNumber(int daynumber, DateTime? startingpoint)
        {
            DateTime setup = startingpoint == null ? DateTime.Now : (DateTime) startingpoint;
            if (setup.Day > daynumber) {
                return new DateTime(setup.Year, setup.Month, daynumber).AddMonths(1);
            }
            else {
                return new DateTime(setup.Year, setup.Month, daynumber);
            }
        }

        /// <summary>
        /// Counts the number of weekdays between the two DateTimes
        /// </summary>
        /// <param name="start">Start date range</param>
        /// <param name="end">End date range</param>
        /// <param name="excludeHolidays">Option to include or exclude holidays to calculate for workable days</param>
        /// <returns></returns>
        public static int CountWeekDays(DateTime start, DateTime end, bool excludeHolidays = false)
        {
            int result = -1;

            int ndays = 1 + Convert.ToInt32((end - start).TotalDays);
            int nsaturdays = (ndays + Convert.ToInt32(start.DayOfWeek)) / 7;

            result = (ndays - 2 * nsaturdays
                   - (start.DayOfWeek == DayOfWeek.Sunday ? 1 : 0)
                   + (end.DayOfWeek == DayOfWeek.Saturday ? 1 : 0)
                   );

            return result;
        }

        /// <summary>
        /// Get an IEnumerable of all individual days for DateTimes between the start and end
        /// </summary>
        /// <param name="start">Start date range</param>
        /// <param name="end">End date range</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> DaysBetween(DateTime start, DateTime end)
        {
            var current = start;
            if (current != current.Date) //handle the case where the date isn't already midnight
                current = current.AddDays(1).Date;
            while (current < end) {
                yield return current;
                current = current.AddDays(1);
            }
        }

        /// <summary>
        /// Get a count of all individual days for DateTimes between the start and end
        /// </summary>
        /// <param name="start">Start date range</param>
        /// <param name="end">End date range</param>
        /// <returns></returns>
        public static int DaysBetweenCount(DateTime start, DateTime end)
        {
            IEnumerable<DateTime> dates = Time.DaysBetween(start, end);
            return dates.Count();
        }

        /// <summary>
        /// Get an IEnumerable of all datetime holidays between the start and end
        /// </summary>
        /// <param name="start">Start date range</param>
        /// <param name="end">End date range</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> HolidaysBetween(DateTime start, DateTime end)
        {
            for (var day = start.Date; day <= end; day = day.AddDays(1)) {
                if (day.IsHoliday()) {
                    //Trace.WriteLine(day.ToString("MM/dd/yyyy"));
                    yield return day;
                }
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

            return WorkDaysBetweenCount(startdate, enddate);
        }

        /// <summary>
        /// Get an IEnumerable of all individual days for DateTimes between the start and end, excluding holidays
        /// </summary>
        /// <param name="start">Start date range</param>
        /// <param name="end">End date range</param>
        /// <returns></returns>
        public static IEnumerable<DateTime> WorkDaysBetween(DateTime start, DateTime end)
        {
            return DaysBetween(start, end)
                .Where(date => IsWorkDay(date));
        }

        /// <summary>
        /// Get a count of all individual days for DateTimes between the start and end, excluding holidays
        /// </summary>
        /// <param name="start">Start date range</param>
        /// <param name="end">End date range</param>
        /// <returns></returns>
        public static int WorkDaysBetweenCount(DateTime start, DateTime end)
        {
            return WorkDaysBetween(start, end).Count();
        }

        /// <summary>
        /// Checks if DateTime is considered a holiday
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static bool IsWorkDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday
                            && date.DayOfWeek != DayOfWeek.Sunday
                            && !date.IsHoliday();
        }

        /// <summary>
        /// Returns DateTime Collection of all days this year
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<DateTime> AllDatesThisYear()
        {
            DateTime start = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime end = new DateTime(DateTime.Now.Year + 1, 1, 1).AddDays(-1);

            return DaysBetween(start, end);
        }

        /// <summary>
        /// Determines if this date is a federal holiday.
        /// </summary>
        /// <param name="date">This date</param>
        /// <returns>True if this date is a federal holiday</returns>
        public static bool IsHoliday(this DateTime date)
        {
            // to ease typing
            int nthWeekDay = (int) (Math.Ceiling((double) date.Day / 7.0d));
            DayOfWeek dayName = date.DayOfWeek;
            bool isThursday = dayName == DayOfWeek.Thursday;
            bool isFriday = dayName == DayOfWeek.Friday;
            bool isMonday = dayName == DayOfWeek.Monday;
            bool isWeekend = dayName == DayOfWeek.Saturday || dayName == DayOfWeek.Sunday;

            // New Years Day (Jan 1, or preceding Friday/following Monday if weekend)
            if ((date.Month == 12 && date.Day == 31 && isFriday) ||
                (date.Month == 1 && date.Day == 1 && !isWeekend) ||
                (date.Month == 1 && date.Day == 2 && isMonday))
                return true;

            // MLK day (3rd monday in January)
            if (date.Month == 1 && isMonday && nthWeekDay == 3)
                return true;

            //// President’s Day (3rd Monday in February)
            //if (date.Month == 2 && isMonday && nthWeekDay == 3) return true;

            // Memorial Day (Last Monday in May)
            if (date.Month == 5 && isMonday && date.AddDays(7).Month == 6)
                return true;

            // Independence Day (July 4, or preceding Friday/following Monday if weekend)
            if ((date.Month == 7 && date.Day == 3 && isFriday) ||
                (date.Month == 7 && date.Day == 4 && !isWeekend) ||
                (date.Month == 7 && date.Day == 5 && isMonday))
                return true;

            // Labor Day (1st Monday in September)
            if (date.Month == 9 && isMonday && nthWeekDay == 1)
                return true;

            //// Columbus Day (2nd Monday in October)
            //if (date.Month == 10 && isMonday && nthWeekDay == 2) return true;

            //// Veteran’s Day (November 11, or preceding Friday/following Monday if weekend))
            //if ((date.Month == 11 && date.Day == 10 && isFriday) ||
            //    (date.Month == 11 && date.Day == 11 && !isWeekend) ||
            //    (date.Month == 11 && date.Day == 12 && isMonday)) return true;

            // Thanksgiving Day (4th Thursday in November)
            if (date.Month == 11 && isThursday && nthWeekDay == 4)
                return true;

            // Day After Thanksgiving Day (4th Friday in November)
            if (date.Month == 11 && isFriday && nthWeekDay == 4)
                return true;

            // Christmas Day (December 25, or preceding Friday/following Monday if weekend))
            if ((date.Month == 12 && date.Day == 24 && isFriday) ||
                (date.Month == 12 && date.Day == 25 && !isWeekend) ||
                (date.Month == 12 && date.Day == 26 && isMonday))
                return true;

            return false;
        }
    }
}