using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
/*
    //https://www.world-timezone.com/index-of-time-zones-and-abbreviations/
    //http://www.xiirus.net/articles/article-_net-convert-datetime-from-one-timezone-to-another-7e44y.aspx
    //https://support.microsoft.com/en-ie/help/973627/microsoft-time-zone-index-values
    //https://pubs.vmware.com/vsphere-50/index.jsp?topic=%2Fcom.vmware.vsphere.vco_use_plugins.doc_42%2FGUID-83EBD74D-B5EC-4A0A-B54E-8E45BE2928C2.html
    public static class TimeZones
    {
        private static Dictionary<string, TimeZoneInfo> tzs = new Dictionary<string, TimeZoneInfo>();
        private static Dictionary<TimeSpan, TimeZoneInfo> tzo = new Dictionary<TimeSpan, TimeZoneInfo>();

        public static bool IsDaylightSavings { get; } = DateTime.Now.IsDaylightSavingTime();

        public static TimeZoneInfo FromAbbrev(string abbrev)
        {
            return tzs.TryGetValue(abbrev, out TimeZoneInfo tz) ? tz : null;
        }

        public static TimeZoneInfo FromUTCOffset(string offset)
        {
            return tzs.TryGetValue(abbrev, out TimeZoneInfo tz) ? tz : null;
        }

        public static TimeZoneInfo FromOffset(string offset)
        {

        }

        public static TimeZoneInfo FromOffset(TimeSpan offset)
        {

        }

        public static TimeSpan ConvertOffset(string offset)
        {
            return tzs.TryGetValue(abbrev, out TimeZoneInfo tz) ? tz : null;
        }

        private static void A(string id, TimeSpan utcOffset, string abbrev = null)
        {
            TimeZoneInfo tzi;
            try {
                tzi = TimeZoneInfo.FindSystemTimeZoneById(id);
                if (abbrev != null)
                    tzs.Add(abbrev, tzi);
            }
            catch {
                tzi = TimeZoneInfo.CreateCustomTimeZone(id, utcOffset, id, id);
            }
            if (!tzo.ContainsKey(utcOffset))
                tzo.Add(utcOffset, tzi);
        }

        private static void A(string id, TimeSpan utcOffset, string abbrev, int dst = 0)
        {
            TimeZoneInfo tzi;
            try {
                tzi = TimeZoneInfo.FindSystemTimeZoneById(id);
                if (abbrev != null)
                    tzs.Add(abbrev, tzi);
            }
            catch {
                tzi = TimeZoneInfo.CreateCustomTimeZone(id, utcOffset, id, id, id, Array.Empty<TimeZoneInfo.AdjustmentRule>(), dst == 0);
            }
            if (!tzo.ContainsKey(utcOffset))
                tzo.Add(utcOffset, tzi);
        }

        //Daylight Savings Time adds 1 hour
        //-1 means daylight savings or summer time, 1 means standard time
        static TimeZones()
        {
            //Discontinued
            //A("Armenia Summer Time", new TimeSpan(5,0 ,0), "AMST", true); //discontinued 2012

            //Other
            A("Australian Central Daylight Time", new TimeSpan(10, 30, 0), "ACDT", -1); //UTC+10:30
            A("Alpha Time Zone", new TimeSpan(1, 0, 0), "A"); //UTC+1
            A("Atlantic Daylight Time", new TimeSpan(-3, 0, 0), "ADT", -1); //UTC-3
            A("Alma-Ata Time", new TimeSpan(6, 0, 0), "ALMT"); //UTC+6
            A("Amazon Summer Time", new TimeSpan(-3, 0, 0), "AMST", -1); //UTC-3
            A("Amazon Time", new TimeSpan(-4, 0, 0), "AMT", 1); //UTC-4
            A("Alaskan Daylight Time", new TimeSpan(-8, 0, 0), "AKDT", -1); //UTC-8
            A("Azores Summer Time", new TimeSpan(0, 0, 0), "AZOST", -1); //UTC+0
            A("Chatham Island Daylight Time", new TimeSpan(13, 45, 0), "CHADT", -1); //UTC+13:45
            A("Central European Summer Time", new TimeSpan(2, 0, 0), "CEST"); //UTC+2
            A("Bravo Time Zone", new TimeSpan(2, 0, 0), "B"); // UTC+2
            A("Brunei Darussalam Time", new TimeSpan(8, 0, 0), "BNT"); //UTC+8
            A("Bolivia Time", new TimeSpan(-4, 0, 0), "BOT"); //UTC-4
            A("Brasilia Summer Time", new TimeSpan(-2, 0, 0), "BRST", -1); //UTC-2
            A("Brasília Time", new TimeSpan(-3, 0, 0), "BRT", 1); //UTC-3
            A("British Summer Time", new TimeSpan(1, 0, 0), "BST", -1); //UTC+1
            A("Bhutan Time", new TimeSpan(6, 0, 0), "BTT"); //UTC+6
            A("Charlie Time Zone", new TimeSpan(3, 0, 0), "C"); //UTC+3
            A("Casey Time", new TimeSpan(8, 0, 0), "CAST"); //UTC+8

            //Windows .NET Built-in
            A("Australian Eastern Daylight Time", new TimeSpan(11, 0, 0), "AEDT", -1); //UTC+11
            A("A.U.S. Central Standard Time", new TimeSpan(1, 0, 0), "ACST", 1); //UTC+9:30
            A("A.U.S. Eastern Standard Time", new TimeSpan(10, 0, 0), "AEST", 1); //UTC+10
            A("Afghanistan Standard Time", new TimeSpan(4, 30, 0), "AFT"); //UTC+4:30
            A("Alaskan Standard Time", new TimeSpan(-9, 0, 0), "AKST", 1); //UTC-9
            A("Aleutian Standard Time", new TimeSpan(-10), "HAST", 1); //UTC–10, Hawaii
            //A("Altai Standard Time", new TimeSpan(7, 0, 0), null, false); //UTC+7, Russia/Siberia 
            //A("Arab Standard Time", new TimeSpan(3,0,0), null); //UTC+3
            A("Arabian Standard Time", new TimeSpan(3, 0, 0), "AST", 1); //UTC+3
            //A("Arabic Standard Time", new TimeSpan(3, 0, 0), null, 1); //UTC+3
            A("Argentina Standard Time", new TimeSpan(-3, 0, 0), ""); //UTC-3
            A("Armenian Standard Time", new TimeSpan(4, 0, 0), "AMT"); //UTC+4 
            //A("Astrakhan Standard Time", new TimeSpan(4, 0, 0), null, false); //UTC+4, MSK+1, Russia (started February 15, 2016)
            A("Atlantic Standard Time", new TimeSpan(-4, 0, 0), "AST", true); //UTC-4
            A("AUS Central Standard Time", new TimeSpan(9, 30, 0), "ACST", true); //UTC+9:30
            A("Aus Central W. Standard Time", new TimeSpan(8, 45, 0), "ACWST", true); //UTC+8:45
            A("AUS Eastern Standard Time", new TimeSpan(10, 0, 0), "AEST", true); //UTC+10
            A("Azerbaijan Standard Time", new TimeSpan(4, 0, 0), "AZT", true); //UTC+4
            A("Azores Standard Time", new TimeSpan(-1, 0, 0), "AZOT", true); //UTC-1, Portugal
            //A("Bahia Standard Time", new TimeSpan(-3,0,0), null, false); //UTC-3, Brazil
            A("Bangladesh Standard Time", new TimeSpan(6, 0, 0), "BST", false); //UTC+6
            //A("Belarus Standard Time", new TimeSpan(3, 0, 0), null); //UTC+3
            A("Bougainville Standard Time", new TimeSpan(11, 0, 0), "BST"); //UTC+11
            //A("Canada Central Standard Time", new TimeSpan(-6, 0, 0), null); //UTC-6??
            A("Cape Verde Standard Time", new TimeSpan(-1, 0, 0), "CVT", false); //UTC-1
            //A("Caucasus Standard Time", new TimeSpan(4,0,0), null); //UTC+4
            //A("Cen. Australia Standard Time", new TimeSpan(9, 30, 0), "ACST"); //UTC+9:30
            A("Central America Standard Time", new TimeSpan(-6, 0, 0), "CST"); //UTC-6
            //A("Central Asia Standard Time", new TimeSpan(6, 0, 0), null); //UTC+6
            A("Central Brazilian Standard Time", new TimeSpan(-3, 0, 0), "BRT"); //UTC-3
            A("Central European Standard Time", new TimeSpan(1, 0, 0), "CET"); //UTC+1
            //A("Central Europe Standard Time", new TimeSpan(1, 0, 0), "CET"); //UTC+1
            A("Central Pacific Standard Time", new TimeSpan(-8, 0, 0), "PST"); //UTC-8
            //A("Central Standard Time (Mexico)", new TimeSpan(-8, 0, 0), "PST"); //UTC-8
            A("Central Standard Time", new TimeSpan(-6, 0, 0), "CST"); //UTC-6
            A("Chatham Islands Standard Time", new TimeSpan(12, 45, 0), "CHAST", true); //UTC+12:45.
            A("China Standard Time", new TimeSpan(8), "CST"); //UTC+8
            A("Cuba Standard Time", new TimeSpan(-5, 0, 0), "CST"); //UTC-5
            A("Dateline Standard Time", new TimeSpan(-12, 0, 0), null); //UTC-12
            A("E. Africa Standard Time", new TimeSpan(3, 0, 0), "EAT"); //UTC+3
            A("E. Australia Standard Time", "", "");
            A("E. Europe Standard Time", new TimeSpan(2, 0, 0), "EET", true); //UTC+2
            A("E. South America Standard Time", "", "");
            A("Easter Island Standard Time", "", "");
            A("Eastern Standard Time (Mexico)", new TimeSpan(-5,0,0), "EST", true);
            A("Eastern Standard Time", new TimeSpan(-5,0,0), "EST", true);
            A("Egypt Standard Time", "", "");
            A("Ekaterinburg Standard Time", "", "");
            A("Fiji Islands Standard Time", "", "");
            A("Fiji Standard Time", "", "");
            A("FLE Standard Time", "", "");
            A("Georgian Standard Time", "", "");
            A("GMT Standard Time", "", "");
            A("Greenland Standard Time", "", "");
            A("Greenwich Standard Time", "", "");
            A("GTB Standard Time", "", "");
            A("Haiti Standard Time", "", "");
            A("Hawaiian Standard Time", new TimeSpan(-10, 0, 0), "HAST"); //UTC-10
            A("India Standard Time", "", "");
            A("Iran Standard Time", "", "");
            A("Israel Standard Time", "", "");
            A("Jordan Standard Time", "", "");
            A("Kaliningrad Standard Time", "", "");
            A("Kamchatka Standard Time", "", "");
            A("Korea Standard Time", "", "");
            A("Libya Standard Time", "", "");
            A("Line Islands Standard Time", "", "");
            A("Lord Howe Standard Time", "", "");
            A("Magadan Standard Time", "", "");
            A("Magallanes Standard Time", "", "");
            A("Marquesas Standard Time", "", "");
            A("Mauritius Standard Time", "", "");
            A("Mexico Standard Time 2", "", "");
            A("Mexico Standard Time", "", "");
            A("Mid-Atlantic Standard Time", "", "");
            A("Middle East Standard Time", "", "");
            A("Montevideo Standard Time", "", "");
            A("Morocco Standard Time", "", "");
            A("Mountain Standard Time (Mexico)", new TimeSpan(-7, 0, 0), "MST", true);
            A("Mountain Standard Time", new TimeSpan(-7, 0, 0), "MST", true);
            A("Myanmar Standard Time", "", "");
            A("N. Central Asia Standard Time", "", "");
            A("Namibia Standard Time", "", "");
            A("Nepal Standard Time", "", "");
            A("New Zealand Standard Time", "", "");
            A("Newfoundland and Labrador Standard Time", "", "");
            A("Newfoundland Standard Time", "", "");
            A("Norfolk Standard Time", "", "");
            A("North Asia East Standard Time", "", "");
            A("North Asia Standard Time", "", "");
            A("North Korea Standard Time", "", "");
            A("Omsk Standard Time", "", "");
            A("Pacific S.A. Standard Time", new TimeSpan(-5, 0, 0), null); //UTC-5
            A("Pacific SA Standard Time", new TimeSpan(-5, 0, 0), null); //UTC-5
            A("Pacific Standard Time (Mexico)", new TimeSpan(-8, 0, 0), "PST", true);
            A("Pacific Standard Time", new TimeSpan(-8, 0, 0), "PST", true);
            A("Pakistan Standard Time", "", "");
            A("Paraguay Standard Time", "", "");
            A("Romance Standard Time", "", "");
            A("Russia Time Zone 10", "", "");
            A("Russia Time Zone 11", "", "");
            A("Russia Time Zone 3", "", "");
            A("Russian Standard Time", "", "");
            A("S.A. Eastern Standard Time", "", "");
            A("S.A. Pacific Standard Time", "", "");
            A("S.A. Western Standard Time", "", "");
            A("S.E. Asia Standard Time", "", "");
            A("SA Eastern Standard Time", "", "");
            A("SA Pacific Standard Time", "", "");
            A("SA Western Standard Time", "", "");
            A("Saint Pierre Standard Time", "", "");
            A("Sakhalin Standard Time", "", "");
            A("Samoa Standard Time", new TimeSpan(-11, 0, 0), "SST"); //UTC-11
            A("Sao Tome Standard Time", "", "");
            A("Saratov Standard Time", "", "");
            A("SE Asia Standard Time", "", "");
            A("Singapore Standard Time", "", "");
            A("South Africa Standard Time", new TimeSpan(2, 0, 0), "SAST", false); //UTC+2
            A("Sri Lanka Standard Time", "", "");
            A("Sudan Standard Time", "", "");
            A("Syria Standard Time", "", "");
            A("Taipei Standard Time", "", "");
            A("Tasmania Standard Time", "", "");
            A("Tocantins Standard Time", "", "");
            A("Tokyo Standard Time", "", "");
            A("Tomsk Standard Time", "", "");
            A("Tonga Standard Time", "", "");
            A("Transbaikal Standard Time", "", "");
            A("Transitional Islamic State of Afghanistan Standard Time", "", "");
            A("Turkey Standard Time", "", "");
            A("Turks And Caicos Standard Time", "", "");
            A("U.S. Eastern Standard Time", "", "");
            A("U.S. Mountain Standard Time", "", "");
            A("Ulaanbaatar Standard Time", "", "");
            A("US Eastern Standard Time", "", "");
            A("US Mountain Standard Time", "", "");
            A("UTC", "", "");
            A("UTC+12", "", "");
            A("UTC+13", "", "");
            A("UTC-02", "", "");
            A("UTC-08", "", "");
            A("UTC-09", "", "");
            A("UTC-11", "", "");
            A("Venezuela Standard Time", "", "");
            A("Vladivostok Standard Time", "", "");
            A("W. Australia Standard Time", "", "");
            A("W. Central Africa Standard Time", "", ""); //UTC+1
            A("W. Europe Standard Time", "", "");
            A("W. Mongolia Standard Time", "", "");
            A("West Asia Standard Time", "", "");
            A("West Bank Standard Time", "", "");
            A("West Pacific Standard Time", "", "");
            A("Yakutsk Standard Time", "", "");
        }
    }
    */
}
