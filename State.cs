using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class State
    {
        private static bool isdst = DateTime.Now.IsDaylightSavingTime();

        public static bool IsDaylightSavings { get { return isdst; } }

        private State(string fullname, string name, int mstdiff)
        {
            Fullname = fullname;
            Name = name;
            MSTDiff = mstdiff;
            Timezone = GetTimezone(mstdiff);
        }

        public State(string fullname, string name, string timezone)
        {
            Fullname = fullname;
            Name = name;
            MSTDiff = GetMSTDiff(timezone);
            Timezone = timezone;
        }

        public static double GetMSTDiff(string timezone)
        {
            double diff = 0;
            if (timezone == "PST")
                diff = 1;
            else if (timezone == "CST")
                diff = -1;
            else if (timezone == "EST" || timezone == "AST")
                diff = -2;
            else if (timezone == "HST")
                diff = 4;
            else if (timezone.StartsWith("UTC") && double.TryParse(timezone.Substring(3), out diff))
                diff = -(diff + 6 + (IsDaylightSavings ? 0 : 1));

            else
                diff = 0;
            //else if (zone.Equals("MST"))
            //    diff = 0;
            return diff;
        }

        public static string GetTimezone(int mstdiff)
        {
            string timezone;
            switch (mstdiff) {
                case 0:
                    timezone = "MST";
                    break;
                case 1:
                    timezone = "PST";
                    break;
                case -1:
                    timezone = "CST";
                    break;
                case -2:
                    timezone = "EST";
                    break;
                case -3:
                    timezone = "HST";
                    break;
                default:
                    timezone = null;
                    break;
            }
            return timezone;
        }

        public string Fullname { get; private set; }
        public string Name { get; private set; }
        public double MSTDiff { get; private set; }
        public string Timezone { get; private set; }

        public static State GetState(string abbrev)
        {
            if (states.TryGetValue(abbrev, out State state))
                return state;
            return null;
        }

        //https://simple.wikipedia.org/wiki/List_of_U.S._states_by_time_zone
        public static readonly State Unknown = new State("Unknown", " ", " ");
        public static readonly State AL = new State("Alabama", "AL", "CST");
        public static readonly State AK = new State("Alaska", "AK", "HST");
        public static readonly State AS = new State("American Samoa", "AS", "UTC-11");
        public static readonly State AZ = new State("Arizona", "AZ", "MST"); //The Navajo Nation uses Daylight Saving Time (DST), the rest of the state does not
        public static readonly State AR = new State("Arkansas", "AR", "CST");
        public static readonly State CA = new State("California", "CA", "PST");
        public static readonly State CO = new State("Colorado", "CO", "MST");
        public static readonly State CT = new State("Connecticut", "CT", "EST");
        public static readonly State DE = new State("Delaware", "DE", "EST");
        public static readonly State DC = new State("District of Columbia", "DC", "EST");
        public static readonly State FL = new State("Florida", "FL", "EST"); //West of the Apalachicola River: Central Standard Time (CST)
        public static readonly State GA = new State("Georgia", "GA", "EST");
        public static readonly State GU = new State("Guam", "GU", "UTC+10");
        public static readonly State HI = new State("Hawaii", "HI", "HST"); //Hawaii does not use Daylight Saving Time (DST)
        public static readonly State ID = new State("Idaho", "ID", "MST"); //North of the Salmon River: Pacific Standard Time (PST)
        public static readonly State IL = new State("Illinois", "IL", "CST");
        public static readonly State IN = new State("Indiana", "IN", "EST"); //Northwest and southwest corners: Central Standard Time (CST)
        public static readonly State IA = new State("Iowa", "IA", "CST");
        public static readonly State KS = new State("Kansas", "KS", "CST"); //Greeley, Hamilton, Sherman and Wallace counties: Mountain Standard Time (MST)
        public static readonly State KY = new State("Kentucky", "KY", "CST"); //Eastern half of the state: Eastern Standard Time (EST)
        public static readonly State LA = new State("Louisiana", "LA", "CST");
        public static readonly State ME = new State("Maine", "ME", "EST");
        public static readonly State MD = new State("Maryland", "MD", "EST");
        public static readonly State MH = new State("Marshall Islands", "MH", "UTC+12");
        public static readonly State MA = new State("Massachusetts", "MA", "EST");
        public static readonly State MI = new State("Michigan", "MI", "EST"); //Counties that share a border with Wisconsin: Central Standard Time (CST)
        public static readonly State FM = new State("Micronesia", "FM", "UTC+11");
        public static readonly State MN = new State("Minnesota", "MN", "CST");
        public static readonly State MS = new State("Mississippi", "MS", "CST");
        public static readonly State MO = new State("Missouri", "MO", "CST");
        public static readonly State MT = new State("Montana", "MT", "MST");
        public static readonly State NE = new State("Nebraska", "NE", "CST"); //Western part of the state: Mountain Standard Time (MST)
        public static readonly State NV = new State("Nevada", "NV", "PST"); //Jackpot and West Wendover: Mountain Standard Time (MST)
        public static readonly State NH = new State("New Hampshire", "NH", "EST");
        public static readonly State NJ = new State("New Jersey", "NJ", "EST");
        public static readonly State NM = new State("New Mexico", "NM", "MST");
        public static readonly State NY = new State("New York", "NY", "EST");
        public static readonly State NC = new State("North Carolina", "NC", "EST");
        public static readonly State ND = new State("North Dakota", "ND", "CST"); //Southwestern part of the state: Mountain Standard Time (MST)
        public static readonly State MP = new State("Northern Marianas", "MP", "UTC+10");
        public static readonly State OH = new State("Ohio", "OH", "EST");
        public static readonly State OK = new State("Oklahoma", "OK", "CST");
        public static readonly State OR = new State("Oregon", "OR", "PST"); //Part of Malheur County: Mountain Standard Time (MST)
        public static readonly State PW = new State("Palau", "PW", "UTC+9");
        public static readonly State PA = new State("Pennsylvania", "PA", "EST");
        public static readonly State PR = new State("Puerto Rico", "PR", "UTC-4");
        public static readonly State RI = new State("Rhode Island", "RI", "EST");
        public static readonly State SC = new State("South Carolina", "SC", "EST");
        public static readonly State SD = new State("South Dakota", "SD", "CST"); //Western half of the state: Mountain Standard Time (MST)
        public static readonly State TN = new State("Tennessee", "TN", "CST"); //East Tennessee, except Marion County: Eastern Standard Time (EST)
        public static readonly State TX = new State("Texas", "TX", "CST"); //El Paso and Hudspeth counties and part of Culberson County: Mountain Standard Time (MST)
        public static readonly State UT = new State("Utah", "UT", "MST");
        public static readonly State VT = new State("Vermont", "VT", "EST");
        public static readonly State VA = new State("Virginia", "VA", "EST");
        public static readonly State VI = new State("Virgin Islands", "VI", "UTC-4");
        public static readonly State WA = new State("Washington", "WA", "PST");
        public static readonly State WV = new State("West Virginia", "WV", "EST");
        public static readonly State WI = new State("Wisconsin", "WI", "CST");
        public static readonly State WY = new State("Wyoming", "WY", "MST");
        //Canada
        public static readonly State AB = new State("Alberta", "AB", "MST");
        public static readonly State BC = new State("British Columbia", "BC", "PST"); //Parts are under MST and some don't experience daylight savings
        public static readonly State MB = new State("Manitoba", "MB", "CST");
        public static readonly State NB = new State("New Brunswick", "NB", "EST");
        public static readonly State NL = new State("Newfoundland and Labrador", "NL", "UTC-2.5");
        public static readonly State NS = new State("Nova Scotia", "NS", "AST");
        public static readonly State NT = new State("Northwest Territories", "NT", "MST");
        public static readonly State NU = new State("Nunavut", "NU", "EST");
        public static readonly State ON = new State("Ontario", "ON", "EST"); //western third is CST
        public static readonly State PE = new State("Prince Edward Island", "PE", "AST");
        public static readonly State QC = new State("Quebec", "QC", ""); //small parts do not observe daylight savings
        public static readonly State SK = new State("Saskatchewan", "SK", "UTC-6");
        public static readonly State YT = new State("Yukon", "YT", "PST");

        private static readonly Dictionary<string, State> states = new Dictionary<string, State>() {
            { "AL", AL },
            { "AK", AK },
            { "AS", AS },
            { "AZ", AZ }, //The Navajo Nation uses Daylight Saving Time (DST), the rest of the state does not
            { "AR", AR },
            { "CA", CA },
            { "CO", CO },
            { "CT", CT },
            { "DE", DE },
            { "DC", DC },
            { "FL", FL }, //West of the Apalachicola River: Central Standard Time (CST)
            { "GA", GA },
            { "GU", GU },
            { "HI", HI }, //Hawaii does not use Daylight Saving Time (DST)
            { "ID", ID }, //North of the Salmon River: Pacific Standard Time (PST)
            { "IL", IL },
            { "IN", IN }, //Northwest and southwest corners: Central Standard Time (CST)
            { "IA", IA },
            { "KS", KS }, //Greeley, Hamilton, Sherman and Wallace counties: Mountain Standard Time (MST)
            { "KY", KY }, //Eastern half of the state: Eastern Standard Time (EST)
            { "LA", LA },
            { "ME", ME },
            { "MD", MD },
            { "MH", MH },
            { "MA", MA },
            { "MI", MI }, //Counties that share a border with Wisconsin: Central Standard Time (CST)
            { "FM", FM },
            { "MN", MN },
            { "MS", MS },
            { "MO", MO },
            { "MT", MT },
            { "NE", NE }, //Western part of the state: Mountain Standard Time (MST)
            { "NV", NV }, //Jackpot and West Wendover: Mountain Standard Time (MST)
            { "NH", NH },
            { "NJ", NJ },
            { "NM", NM },
            { "NY", NY },
            { "NC", NC },
            { "ND", ND }, //Southwestern part of the state: Mountain Standard Time (MST)
            { "MP", MP },
            { "OH", OH },
            { "OK", OK },
            { "OR", OR }, //Part of Malheur County: Mountain Standard Time (MST)
            { "PW", PW },
            { "PA", PA },
            { "PR", PR },
            { "RI", RI },
            { "SC", SC },
            { "SD", SD }, //Western half of the state: Mountain Standard Time (MST)
            { "TN", TN }, //East Tennessee, except Marion County: Eastern Standard Time (EST)
            { "TX", TX }, //El Paso and Hudspeth counties and part of Culberson County: Mountain Standard Time (MST)
            { "UT", UT },
            { "VT", VT },
            { "VA", VA },
            { "VI", VI },
            { "WA", WA },
            { "WV", WV },
            { "WI", WI },
            { "WY", WY },
            //Canada
            { "AB", AB },
            { "BC", BC }, //Parts are under MST and some don't experience daylight savings
            { "MB", MB },
            { "NB", NB },
            { "NL", NL },
            { "NS", NS },
            { "NT", NT },
            { "NU", NU },
            { "ON", ON }, //western third is CST
            { "PE", PE },
            { "QC", QC }, //small parts do not observe daylight savings
            { "SK", SK },
            { "YT", YT },
        };
    }
}
