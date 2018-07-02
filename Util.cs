using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Utilities
{
    public static class Util
    {
        #region Converters
        /// <summary>
        /// Converters for most basic types. converterDict(outputType)(inputType) returns a converter function that takes an objet of inputType and converts it to outputType.
        /// </summary>
        private static Dictionary<Type, Func<Type, Func<object, object>>> converterDict = new Dictionary<Type, Func<Type, Func<object, object>>>() {
            { typeof(string), (input) => {
                Func<object, object> converter;
                if (input == typeof(DateTime))
                    converter = (inp) => { return ((DateTime)inp).ToString("M-d-yyyy h:mm:ss.fff AM/PM"); };
                else if (input == typeof(TimeSpan))
                    converter = (inp) => { return ((TimeSpan) inp).ToString("h:mm:ss.fff"); };
                else if (input == typeof(DateTimeOffset))
                    converter = (inp) => { return Extentions.ToDateTime((DateTimeOffset) inp).ToString("M-d-yyyy h:mm:ss.fff AM/PM"); };
                else if(input == typeof(DateTime?))
                    converter = (inp) => { return (inp as DateTime?)?.ToString("M-d-yyyy h:mm:ss.fff AM/PM"); };
                else if(input == typeof(DateTimeOffset?))
                    converter = (inp) => { return inp == null ? null : Extentions.ToDateTime((DateTimeOffset)inp).ToString("M-d-yyyy h:mm:ss.fff AM/PM"); };
                else if(input == typeof(TimeSpan?))
                    converter = (inp) => { return (inp as TimeSpan?)?.ToString("h:mm:ss.fff"); };
                else
                    converter = System.Convert.ToString;
                return converter;
            } },
            { typeof(DateTime), (input) => {
                Func<object, object> converter;
                if (input == typeof(DateTime?))
                    converter = (inp) => { return (DateTime)inp; };
                else if (input == typeof(DateTimeOffset) || input == typeof(DateTimeOffset?))
                    converter = (inp) => { return Extentions.ToDateTime((DateTimeOffset)inp); };
                else if(input == typeof(TimeSpan) || input == typeof(TimeSpan?))
                    converter = (inp) => { return new DateTime(((TimeSpan) inp).Ticks); };
                else
                    converter = (inp) => { return System.Convert.ToDateTime(inp); };
                return converter;
            } },
            { typeof(TimeSpan), (input) => {
                Func<object, object> converter;
                if (input == typeof(string))
                    converter = (inp) => { return TimeSpan.Parse(inp as string); };
                else if (input == typeof(DateTime))
                    converter = (inp) => { return ((DateTime) inp).TimeOfDay; };
                else if (input == typeof(DateTimeOffset))
                    converter = (inp) => { return Extentions.ToDateTime((DateTimeOffset) inp).TimeOfDay; };
                else if (input == typeof(DateTime?))
                    converter = (inp) => { return inp == null ? TimeSpan.MinValue : ((DateTime) inp).TimeOfDay; };
                else if (input == typeof(DateTimeOffset?))
                    converter = (inp) => { return inp == null ? TimeSpan.MinValue : Extentions.ToDateTime((DateTimeOffset) inp).TimeOfDay; };
                else
                    converter = NoConvert;
                return converter;
            } },
            { typeof(Guid), (input) => {
                Func<object, object> converter;
                if(input == typeof(string))
                    converter = (inp) => { return Guid.Parse(inp as string); };
                else
                    converter = NoConvert;
                return converter;
            } },
            { typeof(char[]), (input) => {
                Func<object, object> converter;
                if(input == typeof(byte[])) {
                    converter = (inp) => {
                        byte[] inBytes = inp as byte[];
                        char[] outChars = new char[inBytes.Length / 4];
                        return System.Convert.ToBase64CharArray(inBytes, 0, inBytes.Length, outChars, 0);
                    };
                }
                else
                    converter = NoConvert;
                return converter;
            } },
            { typeof(int), (input) => { return (inp) => { return System.Convert.ToInt32(inp); }; } },
            { typeof(short), (input) => { return (inp) => { return System.Convert.ToInt16(inp); }; } },
            { typeof(long), (input) => { return (inp) => { return System.Convert.ToInt64(inp); }; } },
            { typeof(uint), (input) => { return (inp) => { return System.Convert.ToUInt32(inp); }; } },
            { typeof(UInt16), (input) => { return (inp) => { return System.Convert.ToUInt16(inp); }; } },
            { typeof(UInt64), (input) => { return (inp) => { return System.Convert.ToUInt64(inp); }; } },
            { typeof(bool), (input) => { return (inp) => { return System.Convert.ToBoolean(inp); }; } },
            { typeof(byte), (input) => { return (inp) => { return System.Convert.ToByte(inp); }; } },
            { typeof(sbyte), (input) => { return (inp) => { return System.Convert.ToSByte(inp); }; } },
            { typeof(char), (input) => { return (inp) => { return System.Convert.ToChar(inp); }; } },
            { typeof(Single), (input) => { return (inp) => { return System.Convert.ToSingle(inp); }; } },
            { typeof(double), (input) => { return (inp) => { return System.Convert.ToDouble(inp); }; } },
            { typeof(decimal), (input) => { return (inp) => { return System.Convert.ToDecimal(inp); }; } },
            { typeof(byte[]), (input) => { return NoConvert; } },
        };

        /// <summary>
        /// A converter that does nothing.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>The same object that was input.</returns>
        private static object NoConvert(object obj) { return obj; }

        /// <summary>
        /// Returns a function that converts an object from one Type to another.
        /// </summary>
        /// <param name="input">The Type of the input object.</param>
        /// <param name="output">The Type of the output object.</param>
        /// <returns>A function that converts objects from one Type to another.</returns>
        public static Func<object, object> Converter(Type input, Type output)
        {
            Func<Type, Func<object, object>> converter;
            if (input == output || !converterDict.TryGetValue(output, out converter))
                return NoConvert;
            return converter(input);
        }

        /// <summary>
        /// Returns a function that converts an object from one Type to another.
        /// </summary>
        /// <param name="input">The Type of the input object.</param>
        /// <param name="output">The Type of the output object.</param>
        /// <returns>A function that converts objects from one Type to another.</returns>
        public static Func<string[], T> StringsConverter<T>() where T : new()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty;
            PropertyInfo[] pinfos = typeof(T).GetProperties(flags);
            Func<object, object>[] converters = new Func<object, object>[pinfos.Length];
            for (int i = 0; i < pinfos.Length; i++) {
                converters[i] = converterDict[typeof(string)](pinfos[i].PropertyType);
            }
            return (strs) => {
                T obj = new T();
                for (int i = 0; i < pinfos.Length; i++) {
                    pinfos[i].SetValue(obj, converters[i](strs[i]));
                }
                return obj;
            };
        }

        /// <summary>
        /// Returns a function that converts an object from one Type to another.
        /// </summary>
        /// <param name="input">The Type of the input object.</param>
        /// <param name="output">The Type of the output object.</param>
        /// <returns>A function that converts objects from one Type to another.</returns>
        public static Func<string[], T> StringsConverter<T>(string[] propertyNames) where T : new()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty;
            PropertyInfo[] pinfos = new PropertyInfo[propertyNames.Length];
            for(int i = 0; i < pinfos.Length; i++) {
                pinfos[i] = typeof(T).GetProperty(propertyNames[i], flags);
                if (pinfos[i] == null)
                    throw new Exception("Invalid PropertyInfo '" + propertyNames[i] ?? "" + "'");
            }
            Func<object, object>[] converters = new Func<object, object>[pinfos.Length];
            for (int i = 0; i < pinfos.Length; i++) {
                converters[i] = converterDict[pinfos[i].PropertyType](typeof(string));
            }
            return (strs) => {
                T obj = new T();
                for (int i = 0; i < pinfos.Length; i++) {
                    pinfos[i].SetValue(obj, converters[i](strs[i]));
                }
                return obj;
            };
        }
        #endregion //Converters

        #region Encoding/TextReader
        /// <summary>
        /// Detects the encoding for UTF-7, UTF-8/16/32 (bom, no bom, little & big endian), 
        /// and local default codepage, and other codepages.
        /// </summary>
        /// <param name="path">The file to detect encoding of.</param>
        /// <returns>The text of the file after it has been processed for encoding.</returns>
        /// <source>https://stackoverflow.com/questions/1025332/determine-a-strings-encoding-in-c-sharp </source>
        public static string GetEncodedText(string path)
        {
            Encoding encoding;
            return GetEncodedText(path, out encoding);
        }

        /// <summary>
        /// Detects the encoding for UTF-7, UTF-8/16/32 (bom, no bom, little & big endian), 
        /// and local default codepage, and other codepages.
        /// </summary>
        /// <param name="path">The file to detect encoding of.</param>
        /// <param name="encoding">The encoding of the file.</param>
        /// <param name="maxBytes">The number of bytes to check of the file. Higher value is slower 
        /// but more reliable (especially UTF-8 with special characters later on may appear to be ASCII initially). 
        /// If negative then the whole file is read in (maximum reliability)</param>
        /// <returns>The text of the file after it has been processed for encoding.</returns>
        /// <source>https://stackoverflow.com/questions/1025332/determine-a-strings-encoding-in-c-sharp </source>
        public static string GetEncodedText(string path, out Encoding encoding, int maxBytes = -1)
        {
            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster value is desired for UTF-8
            byte[] b;
            if (maxBytes < 0)
                b = System.IO.File.ReadAllBytes(path);
            else {
                b = new byte[maxBytes];
                using (System.IO.FileStream fs = System.IO.File.OpenRead(path)) {
                    fs.Read(b, 0, maxBytes);
                }
            }
            encoding = GetTextEncoding(b, out int index);
            return encoding.GetString(b, index, b.Length - index);
        }

        /// <summary>
        /// Creates a TextReader from the Path and attempts automatically detect the file encoding.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="maxBytesRead">The maximum number of bytes to read at once. 
        /// If the file is bigger than this size then it is read as a Stream.</param>
        /// <returns>The TextReader with the file encoding automatically detected.</returns>
        public static TextReader TextReader(FileInfo fi, int maxBytesRead = 100000000)
        {
            if (fi.Exists) {
                if (fi.Length > maxBytesRead) {
                    return new StreamReader(fi.FullName, GetEncoding(fi.FullName), true);
                }
                return new StringReader(GetEncodedText(fi.FullName));
            }
            return null;
        }

        /// <summary>
        /// Attempts to detect the Encoding of a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="maxBytes">The maximum number of bytes to read for detecting encoding.</param>
        /// <returns>The encoding of the file.</returns>
        public static Encoding GetEncoding(string path, int maxBytes = 1000)
        {
            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster value is desired for UTF-8
            Encoding encoding;
            GetEncodedText(path, out encoding, maxBytes);
            return encoding;
        }

        /// <summary>
        /// Gets the file Encoding from an array of bytes.
        /// </summary>
        /// <param name="b">The array of bytes to read for Encoding.</param>
        /// <param name="index">The start of the file. This will be after the Encoding BOM/signature if one exists.</param>
        /// <returns>The Encoding of the array of bytes.</returns>
        private static Encoding GetTextEncoding(byte[] b, out int index)
        {
            //////////////// First check the low hanging fruit by checking if a
            //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF) {
                index = 4;
                return Encoding.GetEncoding("utf-32BE"); // UTF-32, big-endian 
            }
            else if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00) {
                index = 4;
                return Encoding.UTF32; // UTF-32, little-endian
            }
            else if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) {
                index = 2;
                return Encoding.BigEndianUnicode; // UTF-16, big-endian
            }
            else if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) {
                index = 2;
                return Encoding.Unicode; // UTF-16, little-endian
            }
            else if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) {
                index = 3;
                return Encoding.UTF8;
            }
            else if (b.Length >= 3 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76) {
                index = 3;
                return Encoding.UTF7;
            }
            index = 0;

            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: https://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: https://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            int i = 0;
            bool utf8 = false;
            while (i < b.Length - 4) {
                if (b[i] <= 0x7F) { i += 1; continue; }     // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                if (b[i] >= 0xC2 && b[i] <= 0xDF && b[i + 1] >= 0x80 && b[i + 1] < 0xC0) { i += 2; utf8 = true; continue; }
                if (b[i] >= 0xE0 && b[i] <= 0xF0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0) { i += 3; utf8 = true; continue; }
                if (b[i] >= 0xF0 && b[i] <= 0xF4 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0 && b[i + 3] >= 0x80 && b[i + 3] < 0xC0) { i += 4; utf8 = true; continue; }
                utf8 = false;
                break;
            }
            if (utf8) {
                return Encoding.UTF8;
            }

            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            double threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < b.Length; n += 2)
                if (b[n] == 0)
                    count++;
            if (((double)count) / b.Length > threshold) {
                return Encoding.BigEndianUnicode;
            }
            count = 0;
            for (int n = 1; n < b.Length; n += 2) {
                if (b[n] == 0)
                    count++;
            }
            if (((double)count) / b.Length > threshold) {
                return Encoding.Unicode; // (little-endian)
            }

            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (int n = 0; n < b.Length - 9; n++) {
                if (
                    ((b[n + 0] == 'c' || b[n + 0] == 'C') && (b[n + 1] == 'h' || b[n + 1] == 'H') && (b[n + 2] == 'a' || b[n + 2] == 'A') && (b[n + 3] == 'r' || b[n + 3] == 'R') && (b[n + 4] == 's' || b[n + 4] == 'S') && (b[n + 5] == 'e' || b[n + 5] == 'E') && (b[n + 6] == 't' || b[n + 6] == 'T') && (b[n + 7] == '=')) ||
                    ((b[n + 0] == 'e' || b[n + 0] == 'E') && (b[n + 1] == 'n' || b[n + 1] == 'N') && (b[n + 2] == 'c' || b[n + 2] == 'C') && (b[n + 3] == 'o' || b[n + 3] == 'O') && (b[n + 4] == 'd' || b[n + 4] == 'D') && (b[n + 5] == 'i' || b[n + 5] == 'I') && (b[n + 6] == 'n' || b[n + 6] == 'N') && (b[n + 7] == 'g' || b[n + 7] == 'G') && (b[n + 8] == '='))
                    ) {
                    if (b[n + 0] == 'c' || b[n + 0] == 'C')
                        n += 8;
                    else
                        n += 9;
                    if (b[n] == '"' || b[n] == '\'')
                        n++;
                    int oldn = n;
                    while (n < b.Length && (b[n] == '_' || b[n] == '-' || (b[n] >= '0' && b[n] <= '9') || (b[n] >= 'a' && b[n] <= 'z') || (b[n] >= 'A' && b[n] <= 'Z'))) { n++; }
                    byte[] nb = new byte[n - oldn];
                    Array.Copy(b, oldn, nb, 0, n - oldn);
                    try {
                        string internalEnc = Encoding.ASCII.GetString(nb);
                        return Encoding.GetEncoding(internalEnc);
                    }
                    catch { break; }    // If C# doesn't recognize the name of the encoding, break.
                }
            }

            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: https://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            return Encoding.Default;
        }
        #endregion //Encoding/TextReader

        #region Properties
        /// <summary>
        /// Gets the properties of the Type.
        /// </summary>
        /// <param name="ty">The Type to get properties of.</param>
        /// <param name="flags">Filters on the properties to obtain.</param>
        /// <returns>The properties of the Type.</returns>
        public static PropertyInfo[] GetProperties(Type ty, BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance)
        {
            return ty.GetProperties(flags);
        }

        /// <summary>
        /// Gets the property names of the Type.
        /// </summary>
        /// <param name="ty">The Type to get property names of.</param>
        /// <param name="flags">Filters on the properties to obtain.</param>
        /// <returns>The names of the properties of the Type.</returns>
        public static List<string> GetPropertyNames(Type ty, BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance)
        {
            List<string> propertyNames = new List<string>();
            foreach (PropertyInfo pi in ty.GetProperties(flags)) {
                propertyNames.Add(pi.Name);
            }
            return propertyNames;
        }

        /// <summary>
        /// Gets the property values of the object.
        /// </summary>
        /// <param name="obj">The object to get property values of.</param>
        /// <param name="flags">Filters on the properties to obtain.</param>
        /// <returns>The names of the properties of the object.</returns>
        public static List<object> GetPropertyValues(object obj, BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance)
        {
            List<object> values = new List<object>();
            foreach (PropertyInfo pi in obj.GetType().GetProperties(flags)) {
                values.Add(pi.GetValue(obj));
            }
            return values;
        }

        /// <summary>
        /// Gets the property Types of the Type.
        /// </summary>
        /// <param name="ty">The Type to get property Types of.</param>
        /// <param name="flags">Filters on the properties to obtain.</param>
        /// <returns>The Types of the properties of the Type.</returns>
        public static List<Type> GetPropertyTypes(Type ty, BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance)
        {
            List<Type> types = new List<Type>();
            foreach (PropertyInfo pi in ty.GetProperties(flags)) {
                types.Add(pi.PropertyType);
            }
            return types;
        }
        #endregion //Properties

        #region ToString
        /// <summary>
        /// Gets the string representation of this DataTable.
        /// </summary>
        /// <param name="table">The datatable to stringify.</param>
        /// <param name="maxRows">The maximum number of rows.</param>
        /// <param name="printRowNumbers">Determines if row numbers are added before each row.</param>
        /// <param name="sep">The separater between fields in each row.</param>
        /// <param name="columnsToPrint">Determines which columns are added by index and their order.</param>
        public static string ToString(DataTable table, int maxRows = -1, bool includeRowNumbers = false, char sep = ',', params int[] columnsToPrint)
        {
            if (table.Columns.Count == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            int rowCount = maxRows < 0 ? table.Rows.Count : Math.Min(maxRows, table.Rows.Count);
            if (columnsToPrint.Length == 0 || columnsToPrint == null) {
                // print all columns in default order
                for (int col = 0; col < table.Columns.Count; col++)
                    sb.Append(sep).Append(table.Columns[columnsToPrint[col]]);
                sb.Remove(0, 1);
                for (int row = 0; row < rowCount; row++) {
                    if (includeRowNumbers)
                        sb.Append((row + 1)).Append(' ');
                    for (int col = 0; col < table.Columns.Count; col++)
                        sb.Append(ToString(table.Rows[row][col])).Append(sep);
                    sb.Remove(sb.Length - 1, 1).AppendLine();
                }
            }
            else {
                //print specific columns in a costum order
                for (int col = 0; col < columnsToPrint.Length; col++)
                    sb.Append(sep).Append(table.Columns[columnsToPrint[col]].ColumnName);
                sb.Remove(0, 1);
                for (int row = 0; row < rowCount; row++) {
                    if (includeRowNumbers)
                        sb.Append(row + " ");
                    for (int col = 0; col < columnsToPrint.Length; col++)
                        sb.Append(table.Rows[row][columnsToPrint[col]].ToString()).Append(sep);
                    sb.Remove(sb.Length - 1, 1).AppendLine();
                }
            }
            return sb.ToString();
        }

        private delegate string ToStringDelegate();
        /// <summary>
        /// A default ToString method.
        /// </summary>
        /// <typeparam name="T">Tye Type of the object.</typeparam>
        /// <param name="obj">The object to obtain the string representation of.</param>
        /// <returns>The string representation of a type.</returns>
        public static string ToString<T>(T obj)
        {
            return ToString((object)obj);
        }

        /// <summary>
        /// A default ToString method.
        /// </summary>
        /// <typeparam name="T">Tye Type of the object.</typeparam>
        /// <param name="obj">The object to obtain the string representation of.</param>
        /// <returns>The string representation of a type.</returns>
        public static string ToString(object obj)
        {
            return (obj != null && ((ToStringDelegate)obj.ToString).Method.DeclaringType == obj.GetType())
                ? obj.ToString()
                : JsonConvert.SerializeObject(obj, Formatting.None);
        }
        #endregion //ToString

        /// <summary>
        /// Prints the contents of an object to the console.
        /// </summary>
        /// <param name="obj">The object to print.</param>
        public static void Print(object obj)
        {
            string result = (obj != null && ((ToStringDelegate)obj.ToString).Method.DeclaringType == obj.GetType())
                ? obj.ToString()
                : JsonConvert.SerializeObject(obj, Formatting.None);
            Console.Write(result);
        }

        /// <summary>
        /// Swaps the value of two objects.
        /// </summary>
        /// <typeparam name="T">The Type of each object.</typeparam>
        /// <param name="lhs">The left hand side to compare.</param>
        /// <param name="rhs">The right hand side to compare.</param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Swaps the value of two objects if the left is greater than the right.
        /// </summary>
        /// <typeparam name="T">The Type of each object.</typeparam>
        /// <param name="lhs">The left hand side to compare.</param>
        /// <param name="rhs">The right hand side to compare.</param>
        public static void SwapIfGreater<T>(ref T lhs, ref T rhs) where T : System.IComparable<T>
        {
            if (lhs.CompareTo(rhs) > 0) {
                T temp = lhs;
                lhs = rhs;
                rhs = temp;
            }
        }

        /// <summary>
        /// Adds columns to the DataTable representing the getters/setters of a Type.
        /// </summary>
        /// <typeparam name="T">The Type to create the DataTable from.</typeparam>
        /// <param name="table">The DataTable to add columns to.</param>
        /// <returns>The modified DataTable with new columns representing the getters/setters of a Type.</returns>
        public static DataTable DataTable<T>(DataTable table)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance;
            PropertyInfo[] pinfos = typeof(T).GetProperties(flags);
            foreach (var pinfo in pinfos) {
                table.Columns.Add(pinfo.Name, Nullable.GetUnderlyingType(pinfo.PropertyType) ?? pinfo.PropertyType);
            }
            return table;
        }

        /// <summary>
        /// Creates the DataTable representing the getters/setters of a Type.
        /// </summary>
        /// <typeparam name="T">The Type to create a DataTable from.</typeparam>
        /// <returns>A DataTable with columns representing the getters/setters of a Type.</returns>
        public static DataTable DataTable<T>()
        {
            DataTable table = new DataTable();
            return Util.DataTable<T>(table);
        }
    }
}
