﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
	/// <summary>
	/// Calendar for determining date information.
	/// </summary>
	public static class WorkCalendar
	{
		/// <summary>
		/// Start of the work week.
		/// </summary>
		private const DayOfWeek Monday = DayOfWeek.Monday;

		/// <summary>
		/// Returns the quarter (1-4) of a given month.
		/// </summary>
		/// <param name="month">The month (1-12).</param>
		/// <returns>The quarter (1-4) of a given month.</returns>
		public static int Quarter(int month)
		{
			return ((month - 1) / 3) + 1;
		}

		/// <summary>
		/// Returns the number of weeks in a given month/year. This is always 4 or 5.
		/// </summary>
		/// <param name="year">The year (1-9999).</param>
		/// <param name="month">The month (1-12).</param>
		/// <returns>The number of weeks in a given month/year (4 or 5).</returns>
		/// <exception cref="ArgumentOutOfRangeException">Month an year must fall within their represented ranges.</exception>
		public static int WeeksInMonth(int year, int month)
		{
			////return FirstDay(year, month, WEEK_START).AddDays(7 * 4).Month == month ? 5 : 4;
			// checks if the start of the 5th week is in the same month
			DateTime day1 = new DateTime(year, month, 1);
			int daysAdd = ((int) Monday - (int) day1.DayOfWeek + 7) % 7 + 7 * 4;
			////return day1.AddDays(daysAdd).Month == month ? 5 : 4;
			return daysAdd < DateTime.DaysInMonth(year, month) ? 5 : 4;
		}

		/// <summary>
		/// Returns the weekly billing period for a given date. This will be at least 4 days and at most 10 days in duration.
		/// </summary>
		/// <param name="date">A date that falls in the weekly billing period.</param>
		/// <param name="start">The start of the weekly billing period. This will be Monday or the beginning of the month.</param>
		/// <param name="end">The end of the billing period. This will be Sunday or the end of the month.</param>
		public static void WeeklyBillingPeriod(DateTime date, out DateTime start, out DateTime end)
		{
			end = WorkCalendar.NextDay(date.Date.AddDays(-1), DayOfWeek.Sunday);
			start = end.AddDays(-6);
			int endDay = end.Day;
			if (endDay <= 6) {
				// The week is not allowed to falls within two different months
				// Fix the billing period by:
				// 1. Force the week to be within the same month as the date
				// 2. Be between 4-10 days in length
				// 3. Start on a Monday or end on a Sunday 
				if (date.Month == end.Month) {
					start = end.AddDays(-endDay + 1);
					if (endDay <= 3) {
						end = end.AddDays(7);
					}
				}
				else {
					end = end.AddDays(-endDay);
					if (endDay >= 4) {
						start = start.AddDays(-7);
					}
				}
			}
			else if (start.Day <= 4) {
				// within 3 days of the start of the month
				start = start.AddDays(-start.Day + 1);
			}
			else {
				// within 3 days of the end of the month
				DateTime endAdd4 = end.AddDays(4);
				if (endAdd4.Day <= 3) {
					end = endAdd4.AddDays(-endAdd4.Day);
				}
			}
		}

		/// <summary>
		/// Returns the first day of the month of with the given <see cref="DayOfWeek"/>.
		/// </summary>
		/// <param name="year">The year (1-9999).</param>
		/// <param name="month">The month (1-12).</param>
		/// <param name="day">The <see cref="DayOfWeek"/>.</param>
		/// <returns>The first day of a month with the given <see cref="DayOfWeek"/>.</returns>
		public static DateTime FirstDayOfMonth(int year, int month, DayOfWeek day)
		{
			DateTime day1 = new DateTime(year, month, 1);
			int daysAdd = ((int) day - (int) day1.DayOfWeek + 7) % 7;
			return day1.AddDays(daysAdd).Date;
		}

		/// <summary>
		/// Returns the previous day with the given <see cref="DayOfWeek"/>.
		/// </summary>
		/// <param name="date">The <see cref="DateTime"/> to start from.</param>
		/// <param name="day">The <see cref="DayOfWeek"/> to get.</param>
		/// <returns>The previous day with the given <see cref="DayOfWeek"/>.</returns>
		public static DateTime PreviousDay(DateTime date, DayOfWeek day)
		{
			int daysAdd = ((int) day - (int) date.DayOfWeek + 7) % 7 - 7;
			return date.AddDays(daysAdd).Date;
		}

		/// <summary>
		/// Returns the previous day with the given <see cref="DayOfWeek"/>.
		/// </summary>
		/// <param name="day">The <see cref="DayOfWeek"/> to get.</param>
		/// <returns>The previous day with the given <see cref="DayOfWeek"/>.</returns>
		public static DateTime PreviousDay(DayOfWeek day)
		{
			return PreviousDay(DateTime.Now, day);
		}

		/// <summary>
		/// Returns the next day with the given <see cref="DayOfWeek"/>.
		/// </summary>
		/// <param name="date">The <see cref="DateTime"/> to start from.</param>
		/// <param name="day">The <see cref="DayOfWeek"/> to get.</param>
		/// <returns>The next day with the given <see cref="DayOfWeek"/>.</returns>
		public static DateTime NextDay(DateTime date, DayOfWeek day)
		{
			int daysAdd = ((int) day - (int) date.DayOfWeek + 6) % 7 + 1;
			return date.AddDays(daysAdd).Date;
		}

		/// <summary>
		/// Returns the next day with the given <see cref="DayOfWeek"/>.
		/// </summary>
		/// <param name="day">The <see cref="DayOfWeek"/> to get.</param>
		/// <returns>The next day with the given <see cref="DayOfWeek"/>.</returns>
		public static DateTime NextDay(DayOfWeek day)
		{
			return NextDay(DateTime.Now, day);
		}

		/// <summary>
		/// Returns the start of the previous week.
		/// </summary>
		/// <returns>The start of the previous week.</returns>
		public static DateTime PreviousWeek()
		{
			return PreviousDay(DateTime.Now.AddDays(-6), Monday).Date;
		}

		/// <summary>
		/// Returns the start of the previous week.
		/// </summary>
		/// <param name="week">The week number for the previous week.</param>
		/// <returns>The start of the previous week.</returns>
		public static DateTime PreviousWeek(out int week)
		{
			DateTime prevWeekStart = PreviousWeek();
			week = (prevWeekStart.Day - 1) / 7 + 1;
			return prevWeekStart;
		}

		/// <summary>
		/// Returns the week number for the given <see cref="DateTime"/>. This assumes the date given is the start of the week.
		/// </summary>
		/// <param name="startOfWeek">The start of the week.</param>
		/// <returns>The week number for the given date.</returns>
		public static int WeekNumber(DateTime startOfWeek)
		{
			return (startOfWeek.Day - 1) / 7 + 1;
		}

		/// <summary>
		/// Returns the start of a week with the given year, month, and week number.
		/// </summary>
		/// <param name="year">The year (1-9999).</param>
		/// <param name="month">The month (1-12).</param>
		/// <param name="week">The week (1-5).</param>
		/// <returns>The start of a week with the given year, month, and week number.</returns>
		public static DateTime WeekStart(int year, int month, int week)
		{
			DateTime day1 = new DateTime(year, month, 1);
			int daysAdd = ((int) Monday - (int) day1.DayOfWeek + 7) % 7 + 7 * (week - 1);
			return new DateTime(year, month, daysAdd + 1);
		}

		/// <summary>
		/// Returns the start of the current month.
		/// </summary>
		/// <returns>The start of the current month.</returns>
		public static DateTime MonthStart()
		{
			DateTime now = DateTime.Now;
			return new DateTime(now.Year, now.Month, 1);
		}

		/// <summary>
		/// Returns the end of the current month.
		/// </summary>
		/// <returns>The end of the current month.</returns>
		public static DateTime MonthEnd()
		{
			DateTime now = DateTime.Now;
			return new DateTime(now.Year, now.Month, 1).AddMonths(1).AddSeconds(-1);
		}

		/// <summary>
		/// Returns a list of days for between the start and end inclusively.
		/// </summary>
		/// <param name="start">The start date.</param>
		/// <param name="end">The end date.</param>
		/// <returns>A list of days between start and end inclusive.</returns>
		public static IEnumerable<DateTime> DaysBetween(DateTime start, DateTime end)
		{
			for (DateTime day = start.Date; day <= end; day = day.AddDays(1))
				yield return day;
		}

		/// <summary>
		/// Returns a list of all paid holidays between two dates.
		/// </summary>
		/// <param name="start">The start date.</param>
		/// <param name="end">The end date.</param>
		/// <returns>A list of all paid holidays between two dates.</returns>
		public static HashSet<DateTime> HolidaysBetween(DateTime start, DateTime end)
		{
			HashSet<DateTime> holidays = new HashSet<DateTime>();
			for (int year = start.Year; year <= end.Year; year++) {
				HashSet<DateTime> yearHolidays = Holidays(year);
				foreach (DateTime holiday in yearHolidays) {
					if (holiday >= start && holiday <= end)
						holidays.Add(holiday);
				}
			}
			return holidays;
		}

		/// <summary>
		/// Returns the number of work days in a given month.
		/// </summary>
		/// <param name="month">The month (1-12).</param>
		/// <returns>The number of work days in a given month.</returns>
		public static int WorkDaysInMonth(int month)
		{
			return WorkDaysInMonth(month, DateTime.Now.Year);
		}

		/// <summary>
		/// Returns the number of work days in a given month.
		/// </summary>
		/// <param name="month">The month (1-12).</param>
		/// <param name="year">The year (1-9999).</param>
		/// <returns>The number of work days in a given month.</returns>
		public static int WorkDaysInMonth(int month, int year)
		{
			DateTime startdate = new DateTime(year, month, 1);
			DateTime enddate = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
			return WorkDaysBetween(startdate, enddate).Count();
		}

		/// <summary>
		/// Returns all work days between two dates inclusively.
		/// </summary>
		/// <param name="start">The start date.</param>
		/// <param name="end">The end date.</param>
		/// <returns>All work days between two dates inclusively.</returns>
		public static IEnumerable<DateTime> WorkDaysBetween(DateTime start, DateTime end)
		{
			return DaysBetween(start, end).Where(date => IsWorkDay(date));
		}

		/// <summary>
		/// Checks if DateTime is considered a workday. A workday is a weekday that isn't a holiday.
		/// </summary>
		/// <param name="date">The day to determine if it's a work day.</param>
		/// <returns>True if the date is a work day. False otherwise.</returns>
		public static bool IsWorkDay(DateTime date)
		{
			return date.DayOfWeek != DayOfWeek.Saturday
				&& date.DayOfWeek != DayOfWeek.Sunday
				&& !date.IsHoliday();
		}

		/// <summary>
		/// Returns a list of days this year.
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
		/// These include: New Years, MLK Jr, Washington's birthday, Memorial Day, Independence Day, Labor Day, Columbus Day, Veterans Day, Thanksgiving, and Christmas.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns>True if this date is a paid holiday, or false otherwise.</returns>
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
			// Thanksgiving + day after Thanksgiving

			switch (date.Month) {
				case 1:
					// New Years
					if (day == 1)
						return dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday;
					if (day == 2)
						return dow == DayOfWeek.Monday;
					return false;
				// MLK Jr Day
				////return weekOfMonth == 3 && dow == DayOfWeek.Monday;
				//// case 2:
				//// President’s Day (3rd Monday in February)
				////if (date.Month == 2 && isMonday && nthWeekDay == 3) return true;
				//// case 4:
				//// Veteran’s Day (November 11, or preceding Friday/following Monday if weekend))
				////if ((date.Month == 11 && date.Day == 10 && isFriday) ||
				////    (date.Month == 11 && date.Day == 11 && !isWeekend) ||
				////    (date.Month == 11 && date.Day == 12 && isMonday)) return true;
				case 5:
					// Memorial Day
					return dow == DayOfWeek.Monday && date.AddDays(7).Month == 6;
				case 7:
					// Independence Day
					return (day == 3 && dow == DayOfWeek.Friday)
						|| (day == 4 && dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday)
						|| (day == 5 && dow == DayOfWeek.Monday);
				case 9:
					// Labor Day
					return dow == DayOfWeek.Monday && weekOfMonth == 1;
				//// case 10:
				//// Columbus Day (2nd Monday in October)
				////if (date.Month == 10 && isMonday && nthWeekDay == 2) return true;
				case 11:
					// Thanksgiving + day after Thanksgiving
					return (weekOfMonth == 4 && dow == DayOfWeek.Thursday)
						|| ((day - 2) / 7 == 3 && dow == DayOfWeek.Friday);
				case 12:
					// New Years
					if (day == 31)
						return dow == DayOfWeek.Friday;
					// Christmas
					if (day == 24)
						return dow == DayOfWeek.Friday;
					if (day == 25)
						return dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday;
					return day == 26 && dow == DayOfWeek.Monday;
			}
			return false;
		}

		/// <summary>
		/// Returns all holidays within a given year.
		/// </summary>
		/// <param name="year">The year (1-9999).</param>
		/// <returns>All holidays within a given year.</returns>
		/// <see ref="https://stackoverflow.com/questions/3709584/business-holiday-date-handling"/>
		public static HashSet<DateTime> Holidays(int year)
		{
			HashSet<DateTime> holidays = new HashSet<DateTime> {
				// New Years
				NearestWeekDay(new DateTime(year, 1, 1)),
				// Martin Luther King Jr Day -- 3rd Monday of January
				//FirstDay(year, 1, DayOfWeek.Monday).AddDays(7 * 2),
				// Memorial Day -- last monday in May
				PreviousDay(new DateTime(year, 6, 1), DayOfWeek.Monday),
				// Independence Day
				NearestWeekDay(new DateTime(year, 7, 4)),
				// Labor Day -- 1st Monday in September
				FirstDayOfMonth(year, 9, DayOfWeek.Monday),
				// Christmas
				NearestWeekDay(new DateTime(year, 12, 25))
			};
			// Thanksgiving -- 4th Thursday in November
			DateTime thanksgiving = FirstDayOfMonth(year, 11, DayOfWeek.Thursday).AddDays(7 * 3);
			holidays.Add(thanksgiving);
			holidays.Add(thanksgiving.AddDays(1)); // day after Thanksgiving

			// Next year's New Years during this year
			DateTime nextYearNewYearsDate = NearestWeekDay(new DateTime(year + 1, 1, 1));
			if (nextYearNewYearsDate.Year == year)
				holidays.Add(nextYearNewYearsDate);

			return holidays;
		}

		/// <summary>
		/// Returns the nearest week day from a given date.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns>The nearest week day from a given date.</returns>
		/// <source>https://stackoverflow.com/questions/3709584/business-holiday-date-handling</source>
		private static DateTime NearestWeekDay(DateTime date)
		{
			if (date.DayOfWeek == DayOfWeek.Saturday)
				return date.AddDays(-1);
			else if (date.DayOfWeek == DayOfWeek.Sunday)
				return date.AddDays(1);
			return date;
		}
	}
}
