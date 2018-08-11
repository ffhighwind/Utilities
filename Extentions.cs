using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities
{
    public static class Extentions
    {
        #region Sort/Distinct
        /// <summary>
        /// Removes duplicate rows from a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to remove duplicates from.</param>
        /// <returns>The rows with duplicates removed.</returns>
        public static DataTable Distinct(this DataTable table)
        {
            var result = table.AsEnumerable().Distinct(DataRowEqualityComparer<DataRow>.Default).Select(row => row.ItemArray).ToList();
            table.Clear();
            foreach (object[] objs in result)
                table.Rows.Add(objs);
            return table;
        }

        /// <summary>
        /// Removes duplicate rows from a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to remove duplicates from.</param>
        /// <param name="columns">The columns to select on.</param>
        /// <returns>The datatable with duplicates removed.</returns>
        public static DataTable Distinct(this DataTable table, params string[] columns)
        {
            int[] columnIndexes = table.Columns.Cast<DataColumn>().AsEnumerable().Where(col => columns.Contains(col.ColumnName)).Select(col => col.Ordinal).ToArray();
            return Distinct(table, columnIndexes);
        }

        /// <summary>
        /// Removes duplicate rows from a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to remove duplicates from.</param>
        /// <param name="columns">The columns to select on.</param>
        /// <returns>The datatable with duplicates removed.</returns>
        public static DataTable Distinct(this DataTable table, params int[] columns)
        {
            var result = table.AsEnumerable().Distinct(DataRowEqualityComparer<DataRow>.Create(columns)).Select(row => row.ItemArray).ToList();
            table.Clear();
            foreach (object[] objs in result) {
                table.Rows.Add(objs);
            }
            return table;
        }

        /// <summary>
        /// Sorts a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to sort.</param>
        /// <returns>The sorted rows.</returns>
        public static DataTable Sort(this DataTable table)
        {
            return Sort(table, 0);
        }

        /// <summary>
        /// Sorts a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to sort.</param>
        /// <returns>The sorted rows.</returns>
        public static DataTable SortDescending(this DataTable table)
        {
            return SortDescending(table, 0);
        }

        /// <summary>
        /// Sorts rows in a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to sort.</param>
        /// <returns>The sorted rows.</returns>
        public static DataTable Sort(this DataTable table, int column, params int[] columns)
        {
            var rows = table.AsEnumerable().OrderBy(dr => dr[column]).ToList();
            for (int i = 0; i < columns.Length; i++) {
                rows = rows.OrderBy(dr => dr[columns[i]]).ToList();
            }
            IEnumerable<object[]> result = rows.Select(row => row.ItemArray).ToList();
            table.Clear();
            foreach (object[] objs in result)
                table.Rows.Add(objs);
            return table;
        }

        /// <summary>
        /// Sorts rows in a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to sort.</param>
        /// <returns>The sorted rows.</returns>
        public static DataTable Sort(this DataTable table, string column, params string[] columns)
        {
            int index = table.Columns[column].Ordinal;
            var rows = table.AsEnumerable().OrderBy(dr => dr[index]).ToList();
            for (int i = 0; i < columns.Length; i++) {
                index = table.Columns[columns[i]].Ordinal;
                rows = rows.OrderBy(dr => dr[index]).ToList();
            }
            IEnumerable<object[]> result = rows.Select(row => row.ItemArray).ToList();
            table.Clear();
            foreach (object[] objs in result)
                table.Rows.Add(objs);
            return table;
        }

        /// <summary>
        /// Sorts rows in a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to sort.</param>
        /// <returns>The sorted rows.</returns>
        public static DataTable SortDescending(this DataTable table, int column, params int[] columns)
        {
            var result = table.AsEnumerable().OrderByDescending(dr => dr[column]).ToList();
            for (int i = 0; i < columns.Length; i++) {
                result = result.OrderByDescending(dr => dr[columns[i]]).ToList();
            }
            table.Clear();
            foreach (DataRow row in result)
                table.Rows.Add(row.ItemArray);
            return table;
        }

        /// <summary>
        /// Sorts rows in a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to sort.</param>
        /// <returns>The sorted rows.</returns>
        public static DataTable SortDescending(this DataTable table, string column, params string[] columns)
        {
            int index = table.Columns[column].Ordinal;
            var result = table.AsEnumerable().OrderByDescending(dr => dr[index]).ToList();
            for (int i = 0; i < columns.Length; i++) {
                index = table.Columns[columns[i]].Ordinal;
                result = result.OrderByDescending(dr => dr[index]).ToList();
            }
            table.Clear();
            foreach (DataRow row in result)
                table.Rows.Add(row.ItemArray);
            return table;
        }
        #endregion //Sort/Distinct

        #region DataTable/List
        /// <summary>
        /// Determines if the enumerable list has at least some number of elements.
        /// </summary>
        /// <typeparam name="T">Tye type of the enumerable list.</typeparam>
        /// <param name="list">The enumerable list of objects.</param>
        /// <param name="count">The minimum number of elements in the enumerable list.</param>
        /// <returns>True if there is at least the specified number of elements in the list. False otherwise.</returns>
        public static bool CountAtLeast<T>(this IEnumerable<T> list, int count)
        {
            return count >= 1 && list.Skip(count - 1).Any();
        }

        /// <summary>
        /// Trims the rightmost empty columns from the DataTable.
        /// </summary>
        /// <param name="table">The DataTable to trim.</param>
        /// <returns>The trimmed DataTable.</returns>
        public static DataTable TrimColumns(this DataTable table)
        {
            for (int col = table.Columns.Count - 1; col >= 0; col--) {
                if (table.Columns[col].DataType == typeof(string)) {
                    for (int row = 0; row < table.Rows.Count; row++) {
                        object o = table.Rows[row][col];
                        if (o == DBNull.Value || o == null && (o as string).Length == 0) {
                            continue;
                        }
                        return table;
                    }
                }
                else {
                    for (int row = 0; row < table.Rows.Count; row++) {
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
        /// Trims the empty rows at the end of each DataTable in the DataSet.
        /// </summary>
        /// <param name="dataset">The DataSet to trim.</param>
        /// <returns>The trimmed DataSet.</returns>
        public static DataSet Trim(this DataSet dataset)
        {
            foreach (DataTable table in dataset.Tables) {
                table.Trim();
            }
            return dataset;
        }

        /// <summary>
        /// Trims the empty rows at the end of the DataTable.
        /// </summary>
        /// <param name="table">The DataTable to trim.</param>
        /// <returns>The trimmed DataTable.</returns>
        public static DataTable Trim(this DataTable table)
        {
            for (int i = table.Rows.Count - 1; i >= 0; i--) {
                DataRow row = table.Rows[i];
                for (int j = 0; j < table.Columns.Count; j++) {
                    object obj = row[j];
                    if (obj != DBNull.Value && !((obj is string) && (obj as string).Length == 0)) {
                        return table;
                    }
                }
                table.Rows.RemoveAt(i);
            }
            return table;
        }

        /// <summary>
        /// Converts the Enumerable list to a DataTable.
        /// </summary>
        /// <typeparam name="T">The Type of the collection.</typeparam>
        /// <param name="dataTable">The DataTable to modify.</param>
        /// <param name="list">The list to read data from.</param>
        /// <returns>The modified DataTable.</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list, DataTable dataTable) where T : class
        {
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo info in properties) {
                if (!dataTable.Columns.Contains(info.Name)) {
                    Type ty = Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType;
                    if (ty == typeof(char))
                        ty = typeof(string);
                    dataTable.Columns.Add(new DataColumn(info.Name, ty));
                }
            }
            foreach (T entity in list) {
                DataRow row = dataTable.NewRow();
                for (int i = 0; i < properties.Length; i++) {
                    row[properties[i].Name] = properties[i].GetValue(entity) ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        /// <summary>
        /// Converts an Enumerable list to a DataTable.
        /// </summary>
        /// <typeparam name="T">The Type of elements in the list.</typeparam>
        /// <param name="list">The list of elements to convert to a DataTable.</param>
        /// <returns>The DataTable containing the data from the list.</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list) where T : class
        {
            DataTable dt = new DataTable();
            return list.ToDataTable(dt);
        }

        /// <summary>
        /// Converts the Datatable to a List. 
        /// </summary>
        /// <param name="dataTable">The DataTable to convert to a List.</param>
        /// <returns>A list with strings from the DataTable.</returns>
        public static List<string[]> ToList(this DataTable dataTable)
        {
            List<string[]> list = new List<string[]>();
            if (!dataTable.ToList(list))
                list = null;
            return list;
        }

        /// <summary>
        /// Converts the Datatable to a List. 
        /// </summary>
        /// <param name="dataTable">The DataTable to convert to a List.</param>
        /// <returns>True if successful, or false otherwise.</returns>
        public static bool ToList(this DataTable dataTable, ICollection<string[]> list)
        {
            try {
                int columns = dataTable.Columns.Count;
                foreach (DataRow row in dataTable.Rows) {
                    string[] strs = new string[columns];
                    for (int col = 0; col < columns; col++) {
                        strs[col] = row[col] == DBNull.Value ? null : row[col].ToString();
                    }
                    list.Add(strs);
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ToList: " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Converts the Datatable to a List. 
        /// This assumes that the Type T has public properties with identical names of the columns in the DataTable.
        /// </summary>
        /// <typeparam name="T">The Type to convert the datatable to.</typeparam>
        /// <param name="dataTable">The DataTable to convert to a List.</param>
        /// <returns>A list with values from the DataTable.</returns>
        public static List<T> ToList<T>(this DataTable dataTable) where T : class, new()
        {
            List<T> list = new List<T>();
            if (!dataTable.ToList<T>(list))
                list = null;
            return list;
        }

        /// <summary>
        /// Converts the Datatable to a Collection.. 
        /// This assumes that the Type T has public properties with identical names of the columns in the DataTable.
        /// </summary>
        /// <typeparam name="T">The Type to convert the DataRows into.</typeparam>
        /// <param name="dataTable">The DataTable to convert to a Collection.</param>
        /// <param name="list">True if successful, or false otherwise.</param>
        public static bool ToList<T>(this DataTable dataTable, ICollection<T> list) where T : class, new()
        {
            try {
                var converter = DataRowConverter<T>.Create(dataTable);
                foreach (T obj in converter.Convert(dataTable)) {
                    list.Add(obj);
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ToList: " + ex.Message);
            }
            return false;
        }
        #endregion //DataTable/List

        #region Type
        /// <summary>
        /// Converts the Type to its nullable equivilent.
        /// </summary>
        /// <param name="t">The type to convert to nullable.</param>
        /// <returns>The nullable equivilent of the type.</returns>
        public static Type AsNullable(this Type t)
        {
            Type returnType = t;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
                returnType = Nullable.GetUnderlyingType(t);
            }
            return returnType;
        }

        /// <summary>
        /// Determines if the Type is nullable.
        /// </summary>
        /// <param name="type">The type to determine if it is nullable.</param>
        /// <returns>True if the type is nullable. False otherwise.</returns>
        public static bool IsNullable(this Type type)
        {
            return (type == typeof(string) ||
                    type.IsArray ||
                    (type.IsGenericType &&
                     type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(float), typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
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
        /// Determines if the Type is a floating-point numeric type.
        /// </summary>
        /// <param name="o">The Type to check.</param>
        /// <returns>True if the Type is a floating-point type, false otherwise.</returns>
        public static bool IsIntegral(this Type myType)
        {
            return IntTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);
        }

        /// <summary>
        /// Determines if the Type is a floating-point numeric type.
        /// </summary>
        /// <param name="o">The Type to check.</param>
        /// <returns>True if the Type is a floating-point type, false otherwise.</returns>
        public static bool IsFloatingPoint(this Type myType)
        {
            return FloatTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);
        }

        /// <summary>
        /// Determines if the Type is numeric.
        /// </summary>
        /// <param name="o">The Type to check.</param>
        /// <returns>True if the Type is numeric, false otherwise.</returns>
        public static bool IsNumeric(this Type myType)
        {
            return NumericTypes.Contains(Nullable.GetUnderlyingType(myType) ?? myType);
        }
        #endregion //Type

        #region String/StringBuilder
        /// <summary>
        /// Joins strings together with a separator betewen each of them.
        /// </summary>
        /// <param name="strings">The strings to join.</param>
        /// <param name="sep">The separator to join the strings with.</param>
        /// <returns>A combined string with separators between each input string.</returns>
        public static string Join(this IEnumerable<string> strings, char sep)
        {
            return String.Join(sep.ToString(), strings);
        }

        /// <summary>
        /// Joins strings together with a separator between each of them.
        /// </summary>
        /// <param name="strings">The strings to join.</param>
        /// <param name="sep">The separator to join the strings with.</param>
        /// <returns>A combined string with separators between each input string.</returns>
        public static string Join(this IEnumerable<string> strings, string sep = ",")
        {
            return String.Join(sep, strings);
        }

        /// <summary>
        /// Removes characters from the end of a StringBuilder.
        /// </summary>
        /// <param name="sb">The StringBuilder to remove characters from.</param>
        /// <param name="length">The number of characters to remove.</param>
        /// <returns>The same StringBuilder with characters removed.</returns>
        public static StringBuilder RemoveLast(this StringBuilder sb, int length = 1)
        {
            return sb.Remove(sb.Length - length, length);
        }

        /// <summary>
        /// Removes characters from the end of a string.
        /// </summary>
        /// <param name="str">The string to remove characters from.</param>
        /// <param name="length">The number of characters to remove.</param>
        /// <returns>A new string with characters removed.</returns>
        public static string RemoveLast(this string str, int length = 1)
        {
            return str.Remove(str.Length - length, length);
        }
        #endregion //String/StringBuilder

        #region Print
        /// <summary>
        /// Prints the contents of a DataTable to the console.
        /// </summary>
        /// <param name="table">The datatable to print.</param>
        /// <param name="printRowNumbers">Determines if row numbers are printed before each row.</param>
        /// <param name="columnsToPrint">Determines which columns are printed by index and their order. 
        /// Null means all columns are printed.</param>
        public static void Print(this DataTable table, bool printRowNumbers = false, params int[] columnsToPrint)
        {
            Print(table, -1, printRowNumbers, columnsToPrint);
        }

        /// <summary>
        /// Prints the contents of a DataTable to the console.
        /// </summary>
        /// <param name="table">The datatable to print.</param>
        /// <param name="maxRows">The maximum number of rows.</param>
        /// <param name="printRowNumbers">Determines if row numbers are printed before each row.</param>
        /// <param name="columnsToPrint">Determines which columns are printed by index and their order. 
        /// Null means all columns are printed.</param>
        public static void Print(this DataTable table, int maxRows, bool printRowNumbers = false, params int[] columnsToPrint)
        {
            if (table.Columns.Count == 0)
                return;
            maxRows = maxRows < 0 ? table.Rows.Count : Math.Min(maxRows, table.Rows.Count);
            if (columnsToPrint.Length != 0) {
                Console.Write(table.Columns[columnsToPrint[0]].ColumnName);
                for (int col = 1; col < columnsToPrint.Length; col++) {
                    Console.Write(',');
                    Console.Write(table.Columns[columnsToPrint[col]].ColumnName);
                }
                Console.WriteLine();
                for (int row = 0; row < maxRows; row++) {
                    if (printRowNumbers)
                        Console.Write(row + " ");
                    Console.Write(table.Rows[row][columnsToPrint[0]]);
                    for (int col = 1; col < columnsToPrint.Length; col++) {
                        Console.Write(',');
                        Console.Write(table.Rows[row][columnsToPrint[col]]);
                    }
                    Console.WriteLine();
                }
            }
            else {
                Console.WriteLine(table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).Join());
                for (int row = 0; row < table.Rows.Count; row++) {
                    if (printRowNumbers)
                        Console.Write(row + " ");
                    Console.Write(table.Rows[row][0]);
                    for (int col = 1; col < table.Columns.Count; col++) {
                        Console.Write(',');
                        Console.Write(table.Rows[row][col]);
                    }
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Prints the contents of an enumerable collection to the console.
        /// </summary>
        /// <typeparam name="T">The type of object stored in the enumerable collection.</typeparam>
        /// <param name="list">The enumerable collection to print.</param>
        /// <param name="printRowNumbers">Determines if row numbers should be printed before each row.</param>
        public static void Print<T>(this IEnumerable<T> list, bool printRowNumbers = false)
        {
            Print(list, Util.ToString, printRowNumbers);
        }

        /// <summary>
        /// Prints the contents of an enumerable collection to the console.
        /// </summary>
        /// <typeparam name="T">The type of object stored in the enumerable collection.</typeparam>
        /// <param name="list">The enumerable collection to print.</param>
        /// <param name="tostringT">A method that returns the ToString() representation for type T.</param>
        /// <param name="printRowNumbers">Determines if row numbers should be printed before each row.</param>
        public static void Print<T>(this IEnumerable<T> list, Func<T, string> tostringT, bool printRowNumbers = false)
        {
            int i = 1;

            if (list.Any()) {
                if (printRowNumbers)
                    Console.Write("1 ");
                Console.WriteLine(tostringT(list.First()));
                i++;
            }
            foreach (T item in list.Skip(1)) {
                if (printRowNumbers) {
                    Console.Write(i + " ");
                    i++;
                }
                Console.WriteLine(tostringT(item));
            }
        }
        #endregion

        /// <summary>
        /// Converts the DateTimeOffset to a DateTime.
        /// </summary>
        /// <param name="dateTimeOff">The DateTimeOffset to convert.</param>
        /// <returns>The DateTime value of the DateTimeOffset.</returns>
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

        /// <summary>
        /// Iterates a list and performs an action for each element.
        /// </summary>
        /// <typeparam name="T">The Type of object in the list.</typeparam>
        /// <param name="enumeration">The enumerable list.</param>
        /// <param name="action">The action to perform.</param>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration) {
                action(item);
            }
        }
    }
}
