using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// Extensions utility class.
    /// </summary>
    public static class Extensions
    {
        #region DataTable Sort/Distinct
        /// <summary>
        /// Removes duplicate rows from a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to remove duplicates from.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable Distinct(this DataTable table)
        {
            List<object[]> result = table.AsEnumerable().Distinct(DataRowEqualityComparer.Default).Select(row => row.ItemArray).ToList();
            table.Clear();
            foreach (object[] objs in result)
                table.Rows.Add(objs);
            return table;
        }

        /// <summary>
        /// Removes duplicate rows from a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to remove duplicates from.</param>
        /// <param name="columns">The columns to select on.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable Distinct(this DataTable table, params string[] columns)
        {
            int[] columnIndexes = table.Columns.Cast<DataColumn>().AsEnumerable().Where(col => columns.Contains(col.ColumnName)).Select(col => col.Ordinal).ToArray();
            return Distinct(table, columnIndexes);
        }

        /// <summary>
        /// Removes duplicate rows from a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to remove duplicates from.</param>
        /// <param name="columns">The columns to select on.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable Distinct(this DataTable table, params int[] columns)
        {
            List<object[]> rows = table.AsEnumerable().Distinct(DataRowEqualityComparer.Create(columns)).Select(row => row.ItemArray).ToList();
            table.Clear();
            foreach (object[] row in rows) {
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// Sorts a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to sort.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable Sort(this DataTable table)
        {
            return Sort(table, 0);
        }

        /// <summary>
        /// Sorts a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to sort.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable SortDescending(this DataTable table)
        {
            return SortDescending(table, 0);
        }

        /// <summary>
        /// Sorts a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to sort.</param>
        /// <param name="column">The first column index to sort by.</param>
        /// <param name="columns">The next columns indexes to sort by.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable Sort(this DataTable table, int column, params int[] columns)
        {
            List<object[]> rows = table.AsEnumerable().Select(row => row.ItemArray).OrderBy(row => row[column]).ToList();
            for (int i = 0; i < columns.Length; i++) {
                rows = rows.OrderBy(row => row[columns[i]]).ToList();
            }
            table.Clear();
            foreach (object[] row in rows)
                table.Rows.Add(row);
            return table;
        }

        /// <summary>
        /// Sorts a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to sort.</param>
        /// <param name="column">The first column name to sort by.</param>
        /// <param name="columns">The next column names to sort by.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable Sort(this DataTable table, string column, params string[] columns)
        {
            int index = table.Columns[column].Ordinal;
            List<object[]> rows = table.AsEnumerable().Select(row => row.ItemArray).OrderBy(row => row[index]).ToList();
            for (int i = 0; i < columns.Length; i++) {
                index = table.Columns[columns[i]].Ordinal;
                rows = rows.OrderBy(row => row[index]).ToList();
            }
            table.Clear();
            foreach (object[] row in rows)
                table.Rows.Add(row);
            return table;
        }

        /// <summary>
        /// Sorts a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to sort.</param>
        /// <param name="column">The first column index to sort by.</param>
        /// <param name="columns">The next columns indexes to sort by.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable SortDescending(this DataTable table, int column, params int[] columns)
        {
            List<object[]> rows = table.AsEnumerable().Select(row => row.ItemArray).OrderByDescending(row => row[column]).ToList();
            for (int i = 0; i < columns.Length; i++) {
                rows = rows.OrderByDescending(row => row[columns[i]]).ToList();
            }
            table.Clear();
            foreach (object[] row in rows)
                table.Rows.Add(row);
            return table;
        }

        /// <summary>
        /// Sorts a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to sort.</param>
        /// <param name="column">The first column name to sort by.</param>
        /// <param name="columns">The next column names to sort by.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable SortDescending(this DataTable table, string column, params string[] columns)
        {
            int index = table.Columns[column].Ordinal;
            List<object[]> rows = table.AsEnumerable().Select(row => row.ItemArray).OrderByDescending(row => row[index]).ToList();
            for (int i = 0; i < columns.Length; i++) {
                index = table.Columns[columns[i]].Ordinal;
                rows = rows.OrderByDescending(row => row[index]).ToList();
            }
            table.Clear();
            foreach (object[] row in rows)
                table.Rows.Add(row);
            return table;
        }
        #endregion //Sort/Distinct

        #region DataTable/Enumerable/Collection
        /// <summary>
        /// Enumerates an <see cref="IEnumerable{T}"/> and performs an <see cref="Action"/> for each element.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of object in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="list">The <see cref="IEnumerable{T}"/>.</param>
        /// <param name="action">The <see cref="Action"/>.</param>
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (T item in list) {
                action(item);
            }
        }

        /// <summary>
        /// Determines if an <see cref="IEnumerable{T}"/> has at least a certain number of elements.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of object in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="list">The <see cref="IEnumerable{T}"/> of objects.</param>
        /// <param name="count">The minimum number of elements in the <see cref="IEnumerable{T}"/>.</param>
        /// <returns>True if there is at least the specified number of elements in the <see cref="IEnumerable{T}"/>. False otherwise.</returns>
        public static bool CountAtLeast<T>(this IEnumerable<T> list, int count)
        {
            return count >= 1 && list.Skip(count - 1).Any();
        }

        /// <summary>
        /// Trims the rightmost empty columns from a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to trim.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable TrimColumns(this DataTable table)
        {
            int rows = table.Rows.Count;
            for (int col = table.Columns.Count - 1; col >= 0; col--) {
                if (table.Columns[col].DataType == typeof(string)) {
                    for (int row = 0; row < rows; row++) {
                        object o = table.Rows[row][col];
                        if (o != DBNull.Value && (o as string).Length > 0)
                            return table;
                    }
                }
                else {
                    for (int row = 0; row < rows; row++) {
                        object o = table.Rows[row][col];
                        if (o != System.DBNull.Value)
                            return table;
                    }
                }
                table.Columns.RemoveAt(col);
            }
            return table;
        }

        /// <summary>
        /// Trims the empty rows at the end of each <see cref="DataTable"/> in a <see cref="DataSet"/>.
        /// </summary>
        /// <param name="dataset">The <see cref="DataSet"/> to trim.</param>
        /// <returns>The <see cref="DataSet"/>.</returns>
        public static DataSet Trim(this DataSet dataset)
        {
            foreach (DataTable table in dataset.Tables) {
                table.Trim();
            }
            return dataset;
        }

        /// <summary>
        /// Trims the empty rows at the end of a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to trim.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable Trim(this DataTable table)
        {
            int cols = table.Columns.Count;
            for (int i = table.Rows.Count - 1; i >= 0; i--) {
                DataRow row = table.Rows[i];
                for (int j = 0; j < cols; j++) {
                    object obj = row[j];
                    if (obj == DBNull.Value || ((obj is string str) && str.Length == 0))
                        continue;
                    return table;
                }
                table.Rows.RemoveAt(i);
            }
            return table;
        }

        /// <summary>
        /// Converts a <see cref="IEnumerable{T}"/> to a <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="list">The <see cref="IEnumerable{T}"/> to read from.</param>
        /// <param name="table">The <see cref="DataTable"/> to modify.</param>
        /// <returns>The <see cref="DataTable"/>.</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list, DataTable table) where T : class
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo info in properties) {
                if (!table.Columns.Contains(info.Name)) {
                    Type ty = Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType;
                    if (ty == typeof(char))
                        ty = typeof(string);
                    table.Columns.Add(new DataColumn(info.Name, ty));
                }
            }
            foreach (T entity in list) {
                DataRow row = table.NewRow();
                for (int i = 0; i < properties.Length; i++) {
                    row[properties[i].Name] = properties[i].GetValue(entity) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="list">The <see cref="IEnumerable{T}"/> to convert to a <see cref="DataTable"/>.</param>
        /// <returns>A <see cref="DataTable"/> with data from the <see cref="IEnumerable{T}"/>.</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list) where T : class
        {
            return list.ToDataTable(new DataTable());
        }

        /// <summary>
        /// Converts a <see cref="DataTable"/> to a <see cref="List{T}"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to convert to a <see cref="List{T}"/>.</param>
        /// <returns>A <see cref="List{T}"/> of strings from the <see cref="DataTable"/>.</returns>
        public static List<string[]> ToList(this DataTable table)
        {
            return table.ToList(new List<string[]>());
        }

        /// <summary>
        /// Converts a <see cref="DataTable"/> to an <see cref="ICollection{T}"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> in the <see cref="ICollection{T}"/>.</typeparam>
        /// <param name="table">The <see cref="DataTable"/> to convert to an <see cref="ICollection{T}"/>.</param>
        /// <param name="list">The <see cref="ICollection{T}"/> to fill with data from the <see cref="DataTable"/>.</param>
        /// <returns>An <see cref="ICollection{T}"/> with strings from the <see cref="DataTable"/>.</returns>
        public static T ToList<T>(this DataTable table, T list) where T : ICollection<string[]>
        {
            int cols = table.Columns.Count;
            foreach (DataRow row in table.Rows) {
                string[] strs = new string[cols];
                for (int col = 0; col < cols; col++) {
                    strs[col] = row[col] == DBNull.Value ? null : row[col].ToString();
                }
                list.Add(strs);
            }
            return list;
        }

        /// <summary>
        /// Converts a <see cref="DataTable"/> to a <see cref="List{T}"/>.
        /// This assumes that the <see cref="Type"/> T has public properties with identical names of the columns in the <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> in the <see cref="List{T}"/>.</typeparam>
        /// <param name="table">The <see cref="DataTable"/> to convert to a <see cref="List{T}"/>.</param>
        /// <returns>A <see cref="List{T}"/> with values from the <see cref="DataTable"/>.</returns>
        public static List<T> ToList<T>(this DataTable table) where T : class, new()
        {
            List<T> list = new List<T>();
            table.ToList<T>(list);
            return list;
        }

        /// <summary>
        /// Converts a <see cref="DataTable"/> to an <see cref="ICollection{T}"/>.
        /// This assumes that the <see cref="Type"/> T has public properties with identical names of the columns in the <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> in the <see cref="ICollection{T}"/>.</typeparam>
        /// <param name="table">The <see cref="DataTable"/> to convert to an <see cref="ICollection{T}"/>.</param>
        /// <param name="list">The <see cref="ICollection{T}"/> to fill with data from the <see cref="DataTable"/>.</param>
        /// <returns>The <see cref="ICollection{T}"/>.</returns>
        public static ICollection<T> ToList<T>(this DataTable table, ICollection<T> list) where T : class, new()
        {
            Converters.DataRowConverter<T> converter = new Converters.DataRowConverter<T>(table);
            foreach (T obj in converter.Convert(table)) {
                list.Add(obj);
            }
            return list;
        }
        #endregion //DataTable/List

        #region Type
        /// <summary>
        /// Converts a <see cref="Type"/> to its <see cref="Nullable{T}"/> equivalent.
        /// </summary>
        /// <param name="t">The <see cref="Type"/> to convert to <see cref="Nullable{T}"/>.</param>
        /// <returns>The <see cref="Nullable{T}"/> equivalent of the <see cref="Type"/>.</returns>
        public static Type AsNullable(this Type t)
        {
            Type returnType = t;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
                returnType = Nullable.GetUnderlyingType(t);
            }
            return returnType;
        }

        /// <summary>
        /// Determines if a <see cref="Type"/> is <see cref="Nullable{T}"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to determine if it is <see cref="Nullable{T}"/>.</param>
        /// <returns>True if the <see cref="Type"/> is <see cref="Nullable{T}"/>. False otherwise.</returns>
        public static bool IsNullable(this Type type)
        {
            return
                type == typeof(string)
                || type.IsArray
                || (type.IsGenericType
                    && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(float), typeof(double), typeof(decimal),
            typeof(long), typeof(short), typeof(sbyte),
            typeof(byte), typeof(ulong), typeof(ushort),
            typeof(uint), typeof(int)
        };

        private static readonly HashSet<Type> IntTypes = new HashSet<Type>
        {
            typeof(int), typeof(long), typeof(short),
            typeof(sbyte), typeof(byte), typeof(ulong),
            typeof(ushort), typeof(uint),
        };

        private static readonly List<Type> FloatTypes = new List<Type>
        {
            typeof(double), typeof(decimal), typeof(float),
        };

        /// <summary>
        /// Determines if a <see cref="Type"/> is an integral numeric type.
        /// </summary>
        /// <param name="ty">The <see cref="Type"/> to check.</param>
        /// <returns>True if the <see cref="Type"/> is an integral type. False otherwise.</returns>
        public static bool IsIntegral(this Type ty)
        {
            return IntTypes.Contains(Nullable.GetUnderlyingType(ty) ?? ty);
        }

        /// <summary>
        /// Determines if a <see cref="Type"/> is a floating-point numeric type.
        /// </summary>
        /// <param name="ty">The <see cref="Type"/> to check.</param>
        /// <returns>True if the <see cref="Type"/> is a floating-point type. False otherwise.</returns>
        public static bool IsFloatingPoint(this Type ty)
        {
            return FloatTypes.Contains(Nullable.GetUnderlyingType(ty) ?? ty);
        }

        /// <summary>
        /// Determines if a <see cref="Type"/> is numeric.
        /// </summary>
        /// <param name="ty">The <see cref="Type"/> to check.</param>
        /// <returns>True if the <see cref="Type"/> is numeric. False otherwise.</returns>
        public static bool IsNumeric(this Type ty)
        {
            return NumericTypes.Contains(Nullable.GetUnderlyingType(ty) ?? ty);
        }
        #endregion //Type

        #region String/StringBuilder
        /// <summary>
        /// Joins strings together with a separator between each of them.
        /// </summary>
        /// <param name="strings">The strings to join.</param>
        /// <param name="sep">The separator to join the strings with.</param>
        /// <returns>A combined string with separators between each input string.</returns>
        public static string Join(this IEnumerable<string> strings, char sep)
        {
            return string.Join(sep.ToString(), strings);
        }

        /// <summary>
        /// Joins strings together with a separator between each of them.
        /// </summary>
        /// <param name="strings">The strings to join.</param>
        /// <param name="sep">The separator to join the strings with.</param>
        /// <returns>A combined string with separators between each input string.</returns>
        public static string Join(this IEnumerable<string> strings, string sep = ",")
        {
            return string.Join(sep, strings);
        }

        /// <summary>
        /// Removes characters from the end of a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to remove characters from.</param>
        /// <param name="length">The number of characters to remove.</param>
        /// <returns>The <see cref="StringBuilder"/>.</returns>
        public static StringBuilder RemoveLast(this StringBuilder sb, int length = 1)
        {
            return sb.Remove(sb.Length - length, length);
        }

        /// <summary>
        /// Removes characters from the end of a <see cref="string"/>.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to remove characters from.</param>
        /// <param name="length">The number of characters to remove.</param>
        /// <returns>A new <see cref="string"/> with characters removed.</returns>
        public static string RemoveLast(this string str, int length = 1)
        {
            return str.Remove(str.Length - length, length);
        }
        #endregion //String/StringBuilder

        #region Print
        /// <summary>
        /// Prints a <see cref="DataTable"/> to <see cref="Console.Out"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to print.</param>
        /// <param name="printRowNumbers">Determines if row numbers are printed before each row.</param>
        /// <param name="columnsToPrint">Determines which columns are printed and their order. Empty means all columns are printed.</param>
        public static void Print(this DataTable table, bool printRowNumbers = false, params int[] columnsToPrint)
        {
            Print(table, -1, printRowNumbers, columnsToPrint);
        }

        /// <summary>
        /// Prints a <see cref="DataTable"/> to <see cref="Console.Out"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to print.</param>
        /// <param name="maxRows">The maximum number of rows.</param>
        /// <param name="printRowNumbers">Determines if row numbers are printed before each row.</param>
        /// <param name="columnsToPrint">Determines which columns are printed and their order. Empty means all columns are printed.</param>
        public static void Print(this DataTable table, int maxRows, bool printRowNumbers = false, params int[] columnsToPrint)
        {
            if (table.Columns.Count == 0)
                return;
            maxRows = maxRows < 0 ? table.Rows.Count : Math.Min(maxRows, table.Rows.Count);
            if (columnsToPrint.Length == 0)
                columnsToPrint = Enumerable.Range(0, table.Columns.Count).ToArray();
            if (columnsToPrint.Length != 0) {
                Console.Write(table.Columns[columnsToPrint[0]].ColumnName);
                for (int col = 1; col < columnsToPrint.Length; col++) {
                    Console.Write(",{0}", table.Columns[columnsToPrint[col]].ColumnName);
                }
                Console.WriteLine();
                for (int row = 0; row < maxRows; row++) {
                    if (printRowNumbers)
                        Console.Write("{0} ", row);
                    Console.Write(table.Rows[row][columnsToPrint[0]]);
                    for (int col = 1; col < columnsToPrint.Length; col++) {
                        Console.Write(",{0}", table.Rows[row][columnsToPrint[col]]);
                    }
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Prints an <see cref="IEnumerable{T}"/> to <see cref="Console.Out"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of object stored in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="list">The <see cref="IEnumerable{T}"/> to print.</param>
        /// <param name="printRowNumbers">Determines if row numbers should be printed before each row.</param>
        public static void Print<T>(this IEnumerable<T> list, bool printRowNumbers = false)
        {
            Print(list, Util.ToString, printRowNumbers);
        }

        /// <summary>
        /// Prints an <see cref="IEnumerable{T}"/> to <see cref="Console.Out"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of object stored in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="list">The <see cref="IEnumerable{T}"/> to print.</param>
        /// <param name="tostringT">A method that returns the <see cref="string"/> representation for <see cref="Type"/> T.</param>
        /// <param name="printRowNumbers">Determines if row numbers should be printed before each row.</param>
        public static void Print<T>(this IEnumerable<T> list, Func<T, string> tostringT, bool printRowNumbers = false)
        {
            if (printRowNumbers) {
                int i = 1;
                foreach (T item in list) {
                    Console.WriteLine("{0} {1}", i, tostringT(item));
                    i++;
                }
            }
            else {
                foreach (T item in list) {
                    Console.WriteLine(tostringT(item));
                }
            }
        }

        /// <summary>
        /// Prints an object to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="obj">The object to print.</param>
        public static void Print(this TextWriter writer, object obj)
        {
            writer.Write(Util.ToString(obj));
        }
        #endregion

        #region DateTime/DateTimeOffset
        /// <summary>
        /// Determines if a <see cref="DateTime"/> is between start/end or equal to start.
        /// Start must be less than or equal to end or the result is always false.
        /// </summary>
        /// <param name="time">The <see cref="DateTime"/> being compared.</param>
        /// <param name="start">The start <see cref="DateTime"/>.</param>
        /// <param name="end">The end <see cref="DateTime"/>.</param>
        /// <returns>True if the <see cref="DateTime"/> is between start/end or equal to start. False otherwise.</returns>
        public static bool IsBetween(this DateTime time, DateTime start, DateTime end)
        {
            return time >= start && time < end;
        }

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTimeOff">The <see cref="DateTimeOffset"/> to convert.</param>
        /// <returns>The <see cref="DateTime"/> value of the <see cref="DateTimeOffset"/>.</returns>
        public static DateTime ToDateTime(this DateTimeOffset dateTimeOff)
        {
            if (dateTimeOff.Offset.Equals(TimeSpan.Zero))
                return dateTimeOff.UtcDateTime;
            else if (dateTimeOff.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTimeOff.DateTime)))
                return DateTime.SpecifyKind(dateTimeOff.DateTime, DateTimeKind.Local);
            else
                return dateTimeOff.DateTime;
        }

        /// <summary>
        /// Converts the DateTimeOffset? to a DateTime?.
        /// </summary>
        /// <param name="dateTimeOff">The DateTimeOffset? to convert.</param>
        /// <returns>The DateTime? value of the DateTimeOffset?.</returns>
        public static DateTime? ToDateTime(this DateTimeOffset? dateTimeOff)
        {
            return dateTimeOff?.ToDateTime();
        }
        #endregion
    }
}
