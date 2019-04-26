using System;
using System.Collections.Generic;

namespace Utilities
{
	/// <summary>
	/// A representation of a state from the USA.
	/// </summary>
	public class State
	{
		// https://simple.wikipedia.org/wiki/List_of_U.S._states_by_time_zone
		public static readonly State Unknown = new State("Unknown", " ", 0);
		public static readonly State AL = new State("Alabama", "AL", "CST");
		public static readonly State AK = new State("Alaska", "AK", "AKST");
		public static readonly State AS = new State("American Samoa", "AS", "UTC-11");
		public static readonly State AZ = new State("Arizona", "AZ", "MST"); // The Navajo Nation uses Daylight Saving Time (DST), the rest of the state does not
		public static readonly State AR = new State("Arkansas", "AR", "CST");
		public static readonly State CA = new State("California", "CA", "PST");
		public static readonly State CO = new State("Colorado", "CO", "MST");
		public static readonly State CT = new State("Connecticut", "CT", "EST");
		public static readonly State DE = new State("Delaware", "DE", "EST");
		public static readonly State DC = new State("District of Columbia", "DC", "EST");
		public static readonly State FL = new State("Florida", "FL", "EST"); // West of the Apalachicola River: Central Standard Time (CST)
		public static readonly State GA = new State("Georgia", "GA", "EST");
		public static readonly State GU = new State("Guam", "GU", "UTC+10");
		public static readonly State HI = new State("Hawaii", "HI", "HST"); // Hawaii does not use Daylight Saving Time (DST)
		public static readonly State ID = new State("Idaho", "ID", "MST"); // North of the Salmon River: Pacific Standard Time (PST)
		public static readonly State IL = new State("Illinois", "IL", "CST");
		public static readonly State IN = new State("Indiana", "IN", "EST"); // Northwest and southwest corners: Central Standard Time (CST)
		public static readonly State IA = new State("Iowa", "IA", "CST");
		public static readonly State KS = new State("Kansas", "KS", "CST"); // Greeley, Hamilton, Sherman and Wallace counties: Mountain Standard Time (MST)
		public static readonly State KY = new State("Kentucky", "KY", "CST"); // Eastern half of the state: Eastern Standard Time (EST)
		public static readonly State LA = new State("Louisiana", "LA", "CST");
		public static readonly State ME = new State("Maine", "ME", "EST");
		public static readonly State MD = new State("Maryland", "MD", "EST");
		public static readonly State MH = new State("Marshall Islands", "MH", "UTC+12");
		public static readonly State MA = new State("Massachusetts", "MA", "EST");
		public static readonly State MI = new State("Michigan", "MI", "EST"); // Counties that share a border with Wisconsin: Central Standard Time (CST)
		public static readonly State FM = new State("Micronesia", "FM", "UTC+11");
		public static readonly State MN = new State("Minnesota", "MN", "CST");
		public static readonly State MS = new State("Mississippi", "MS", "CST");
		public static readonly State MO = new State("Missouri", "MO", "CST");
		public static readonly State MT = new State("Montana", "MT", "MST");
		public static readonly State NE = new State("Nebraska", "NE", "CST"); // Western part of the state: Mountain Standard Time (MST)
		public static readonly State NV = new State("Nevada", "NV", "PST"); // Jackpot and West Wendover: Mountain Standard Time (MST)
		public static readonly State NH = new State("New Hampshire", "NH", "EST");
		public static readonly State NJ = new State("New Jersey", "NJ", "EST");
		public static readonly State NM = new State("New Mexico", "NM", "MST");
		public static readonly State NY = new State("New York", "NY", "EST");
		public static readonly State NC = new State("North Carolina", "NC", "EST");
		public static readonly State ND = new State("North Dakota", "ND", "CST"); // Southwestern part of the state: Mountain Standard Time (MST)
		public static readonly State MP = new State("Northern Marianas", "MP", "UTC+10");
		public static readonly State OH = new State("Ohio", "OH", "EST");
		public static readonly State OK = new State("Oklahoma", "OK", "CST");
		public static readonly State OR = new State("Oregon", "OR", "PST"); // Part of Malheur County: Mountain Standard Time (MST)
		public static readonly State PW = new State("Palau", "PW", "UTC+9");
		public static readonly State PA = new State("Pennsylvania", "PA", "EST");
		public static readonly State PR = new State("Puerto Rico", "PR", "UTC-4");
		public static readonly State RI = new State("Rhode Island", "RI", "EST");
		public static readonly State SC = new State("South Carolina", "SC", "EST");
		public static readonly State SD = new State("South Dakota", "SD", "CST"); // Western half of the state: Mountain Standard Time (MST)
		public static readonly State TN = new State("Tennessee", "TN", "CST"); // East Tennessee, except Marion County: Eastern Standard Time (EST)
		public static readonly State TX = new State("Texas", "TX", "CST"); // El Paso and Hudspeth counties and part of Culberson County: Mountain Standard Time (MST)
		public static readonly State UT = new State("Utah", "UT", "MST");
		public static readonly State VT = new State("Vermont", "VT", "EST");
		public static readonly State VA = new State("Virginia", "VA", "EST");
		public static readonly State VI = new State("Virgin Islands", "VI", "UTC-4");
		public static readonly State WA = new State("Washington", "WA", "PST");
		public static readonly State WV = new State("West Virginia", "WV", "EST");
		public static readonly State WI = new State("Wisconsin", "WI", "CST");
		public static readonly State WY = new State("Wyoming", "WY", "MST");
		// Canada
		public static readonly State AB = new State("Alberta", "AB", "MST");
		public static readonly State BC = new State("British Columbia", "BC", "PST"); // Parts are under MST and some don't experience daylight savings
		public static readonly State MB = new State("Manitoba", "MB", "CST");
		public static readonly State NB = new State("New Brunswick", "NB", "EST");
		public static readonly State NL = new State("Newfoundland and Labrador", "NL", "UTC-2.5");
		public static readonly State NS = new State("Nova Scotia", "NS", "AST");
		public static readonly State NT = new State("Northwest Territories", "NT", "MST");
		public static readonly State NU = new State("Nunavut", "NU", "EST");
		public static readonly State ON = new State("Ontario", "ON", "EST"); // western third is CST
		public static readonly State PE = new State("Prince Edward Island", "PE", "AST");
		public static readonly State QC = new State("Quebec", "QC", "EST"); // small parts do not observe daylight savings
		public static readonly State SK = new State("Saskatchewan", "SK", "UTC-6");
		public static readonly State YT = new State("Yukon", "YT", "PST");

		private static readonly IReadOnlyDictionary<string, State> States = new Dictionary<string, State>() {
			{ "AL", AL },
			{ "AK", AK },
			{ "AS", AS },
			{ "AZ", AZ }, // The Navajo Nation uses Daylight Saving Time (DST), the rest of the state does not
			{ "AR", AR },
			{ "CA", CA },
			{ "CO", CO },
			{ "CT", CT },
			{ "DE", DE },
			{ "DC", DC },
			{ "FL", FL }, // West of the Apalachicola River: Central Standard Time (CST)
			{ "GA", GA },
			{ "GU", GU },
			{ "HI", HI }, // Hawaii does not use Daylight Saving Time (DST)
			{ "ID", ID }, // North of the Salmon River: Pacific Standard Time (PST)
			{ "IL", IL },
			{ "IN", IN }, // Northwest and southwest corners: Central Standard Time (CST)
			{ "IA", IA },
			{ "KS", KS }, // Greeley, Hamilton, Sherman and Wallace counties: Mountain Standard Time (MST)
			{ "KY", KY }, // Eastern half of the state: Eastern Standard Time (EST)
			{ "LA", LA },
			{ "ME", ME },
			{ "MD", MD },
			{ "MH", MH },
			{ "MA", MA },
			{ "MI", MI }, // Counties that share a border with Wisconsin: Central Standard Time (CST)
			{ "FM", FM },
			{ "MN", MN },
			{ "MS", MS },
			{ "MO", MO },
			{ "MT", MT },
			{ "NE", NE }, // Western part of the state: Mountain Standard Time (MST)
			{ "NV", NV }, // Jackpot and West Wendover: Mountain Standard Time (MST)
			{ "NH", NH },
			{ "NJ", NJ },
			{ "NM", NM },
			{ "NY", NY },
			{ "NC", NC },
			{ "ND", ND }, // Southwestern part of the state: Mountain Standard Time (MST)
			{ "MP", MP },
			{ "OH", OH },
			{ "OK", OK },
			{ "OR", OR }, // Part of Malheur County: Mountain Standard Time (MST)
			{ "PW", PW },
			{ "PA", PA },
			{ "PR", PR },
			{ "RI", RI },
			{ "SC", SC },
			{ "SD", SD }, // Western half of the state: Mountain Standard Time (MST)
			{ "TN", TN }, // East Tennessee, except Marion County: Eastern Standard Time (EST)
			{ "TX", TX }, // El Paso and Hudspeth counties and part of Culberson County: Mountain Standard Time (MST)
			{ "UT", UT },
			{ "VT", VT },
			{ "VA", VA },
			{ "VI", VI },
			{ "WA", WA },
			{ "WV", WV },
			{ "WI", WI },
			{ "WY", WY },
			// Canada
			{ "AB", AB },
			{ "BC", BC }, // Parts are under MST and some don't experience daylight savings
			{ "MB", MB },
			{ "NB", NB },
			{ "NL", NL },
			{ "NS", NS },
			{ "NT", NT },
			{ "NU", NU },
			{ "ON", ON }, // western third is CST
			{ "PE", PE },
			{ "QC", QC }, // small parts do not observe daylight savings
			{ "SK", SK },
			{ "YT", YT },
		};

		/// <summary>
		/// For determining if daylight savings time (DST) is currently in affect.
		/// </summary>
		private static bool IsDaylightSavings => DateTime.Now.IsDaylightSavingTime();

		/// <summary>
		/// Initializes a new instance of the <see cref="State"/> class.
		/// </summary>
		/// <param name="fullname">The full name of the state.</param>
		/// <param name="name">The abbreviation of the state.</param>
		/// <param name="mstdiff">The time zone hour difference from MST.</param>
		private State(string fullname, string name, int mstdiff)
		{
			FullName = fullname;
			Name = name;
			MSTDiff = mstdiff;
			TimeZone = GetTimezone(mstdiff);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="State"/> class.
		/// </summary>
		/// <param name="fullname">The full name of the state.</param>
		/// <param name="name">The abbreviation of the state.</param>
		/// <param name="timezone">The time zone of the state.</param>
		private State(string fullname, string name, string timezone)
		{
			FullName = fullname;
			Name = name;
			MSTDiff = GetMSTDiff(timezone);
			TimeZone = timezone;
		}

		/// <summary>
		/// Gets the hour difference from the MST time zone from another timezone.
		/// </summary>
		/// <param name="timezone">The abbreviated time zone to get the hour difference from Mountain Stanard Time.</param>
		/// <returns>The hour difference from the MST time zone.</returns>
		public static double GetMSTDiff(string timezone)
		{
			switch (timezone[0]) {
				case 'M':
					if (timezone == "MST" || timezone == "MT" || timezone == "MDT") {
						return 0;
					}
					break;
				case 'P':
					if (timezone == "PST" || timezone == "PT" || timezone == "PDT") {
						return 1;
					}
					break;
				case 'C':
					if (timezone == "CST" || timezone == "CT" || timezone == "CDT") {
						return -1;
					}
					break;
				case 'E':
					if (timezone == "EST" || timezone == "ET" || timezone == "EDT") {
						return -2;
					}
					break;
				case 'A':
					if (timezone == "AST") {
						return -2;
					}
					else if (timezone == "AKST" || timezone == "AKDT") {
						return 4;
					}
					break;
				case 'H':
					if (timezone == "HST") {
						return IsDaylightSavings ? 5 : 4;
					}
					break;
				case 'U':
				case 'G':
					if (timezone.StartsWith("UTC") || timezone.StartsWith("GMT")) {
						string diffstr = timezone.Substring(3);
						if (double.TryParse(diffstr, out double diff)) {
							diff = -(diff + 6 + (IsDaylightSavings ? 1 : 0));
							return diff;
						}
					}
					break;
			}
			throw new InvalidTimeZoneException(timezone + " is not a valid timezone.");
		}

		/// <summary>
		/// Gets the abbreviated time zone for a given hour difference from Mountain Stanard Time.
		/// </summary>
		/// <param name="mstdiff">The hour difference from Mountain Stanard Time.</param>
		/// <returns>The abbreviated time zone for a given hour difference from Mountain Stanard Time.</returns>
		public static string GetTimezone(int mstdiff)
		{
			switch (mstdiff) {
				case 1:
					return "PT";
				case 0:
					return "MT";
				case -1:
					return "CT";
				case -2:
					return "ET";
				case -3:
					return IsDaylightSavings ? "AKDT" : "AKST";
				default:
					if (mstdiff < -12 || mstdiff > 24)
						throw new ArgumentOutOfRangeException(nameof(mstdiff), "Value must be between -12 and 24");
					int utcdiff = ((mstdiff + 24 + 6 + (IsDaylightSavings ? 1 : 0)) % 24) - 12;
					return string.Format("UTC{0}", utcdiff);
			}
		}

		/// <summary>
		/// Gets the full name of the state.
		/// </summary>
		/// <returns>The full name of the state.</returns>
		public string FullName { get; private set; }

		/// <summary>
		/// Gets the abbreviation for the state.
		/// </summary>
		/// <returns>The abbreviation for the state.</returns>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the hour difference from Mountain Stanard Time for the state.
		/// </summary>
		/// <returns>The hour difference from Mountain Stanard Time.</returns>
		public double MSTDiff { get; private set; }

		/// <summary>
		/// Gets the time zone for a given state.
		/// </summary>
		/// <returns>The abbreviation for the time zone of the state.</returns>
		public string TimeZone { get; private set; }

		/// <summary>
		/// Gets a state from an abbreviated name.
		/// </summary>
		/// <param name="abbrev">The abbreviation for the state.</param>
		/// <returns>The state with the abbreviated name; or null if it doesn't exist.</returns>
		public static State GetState(string abbrev)
		{
			if (States.TryGetValue(abbrev, out State state))
				return state;
			return null;
		}
	}
}
