using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CsvHelper.Configuration;
using CsvHelper;
using System.Reflection;
using System.Data.OleDb;
using System.Text;

namespace Utilities
{
    public static class IO
    {
        #region Any
        /// <summary>
        /// Reads a delimited file into a List of strings.
        /// </summary>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>A list of strings from the file, or null on error.</returns>
        public static List<string[]> Read(string path, bool hasHeaders = true)
        {
            return Foreach(path, hasHeaders).ToList();
        }

        /// <summary>
        /// Iterates each row in a file.
        /// </summary>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<string[]> Foreach(string path, bool hasHeaders = true)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".csv")
                return CsvForeach(path, hasHeaders, true);
            else if (ext == ".xlsx")
                return XlsxForeach(path, null, hasHeaders);
            else if (ext == ".xls")
                return XlsForeach(path, null, hasHeaders);
            else if (ext == ".tsv")
                return FileForeach(path, '\t', hasHeaders);
            return null;
        }

        /// <summary>
        /// Iterates each row in a file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="constructor">A function that constructs an object from an array of strings.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <param name="sheetname">The name of the worksheet to iterate if there are any.
        /// If this is null then the first sheet will be iterated.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> Foreach<T>(string path, Func<string[], T> constructor, bool hasHeaders = true, string sheetname = null)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".csv")
                return CsvForeach<T>(path, constructor, hasHeaders, true);
            else if (ext == ".xlsx")
                return XlsxForeach<T>(path, constructor, sheetname, hasHeaders);
            else if (ext == ".xls")
                return XlsForeach<T>(path, constructor, sheetname, hasHeaders);
            else if (ext == ".tsv")
                return FileForeach<T>(path, constructor, '\t', hasHeaders);
            return null;
        }
        #endregion

        #region Delimited
        /// <summary>
        /// Iterates each row in a delimited file.
        /// </summary>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="delim">The separator for the columns in the file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<string[]> FileForeach(string path, char delim = ',', bool hasHeaders = true)
        {
            using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                if (hasHeaders)
                    reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null) {
                    yield return line.Split(delim);
                }
            }
        }

        /// <summary>
        /// Iterates each row in delimited text.
        /// </summary>
        /// <param name="reader">The TextReader of the delimited file.</param>
        /// <param name="delim">The separator for the columns in the file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<string[]> FileForeach(TextReader reader, char delim = ',', bool hasHeaders = true)
        {
            if (hasHeaders)
                reader.ReadLine();
            string line;
            while ((line = reader.ReadLine()) != null) {
                yield return line.Split(delim);
            }
        }

        /// <summary>
        /// Iterates each row in delimited text.
        /// </summary>
        /// <typeparam name="T">The Type of object to return.</typeparam>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="constructor">The constructor for the objects returned.</param>
        /// <param name="delim">The separator for the columns in the file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> FileForeach<T>(string path, Func<string[], T> constructor, char delim = ',', bool hasHeaders = true)
        {
            using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                if (hasHeaders)
                    reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null) {
                    yield return constructor(line.Split(delim));
                }
            }
        }

        /// <summary>
        /// Iterates each row in delimited text.
        /// </summary>
        /// <typeparam name="T">The Type of object to return.</typeparam>
        /// <param name="reader">The TextReader of the delimited file.</param>
        /// <param name="constructor">The constructor for the objects returned.</param>
        /// <param name="delim">The separator for the columns in the file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> FileForeach<T>(TextReader reader, Func<string[], T> constructor, char delim = ',', bool hasHeaders = true)
        {
            if (hasHeaders)
                reader.ReadLine();
            string line;
            while ((line = reader.ReadLine()) != null) {
                yield return constructor(line.Split(delim));
            }
        }

        /// <summary>
        /// Reads a delimited file into a List of strings.
        /// </summary>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="delim">The character separating each column of each line.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>A list of strings from the file, or null on error.</returns>
        public static List<string[]> ReadFile(string path, char delim = ',', bool hasHeaders = true)
        {
            List<string[]> list = new List<string[]>();
            if (!ReadFile(list, path, delim))
                list = null;
            return list;
        }

        /// <summary>
        /// Reads a delimited file into a Collection of strings.
        /// </summary>
        /// <param name="list">The List to modify.</param>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="delim">The character separating each column of each line.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        public static bool ReadFile(this ICollection<string[]> list, string path, char delim = ',', bool hasHeaders = true)
        {
            try {
                using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                    IEnumerable<string[]> lines = FileForeach(reader, delim, false);
                    if (hasHeaders)
                        lines = lines.Skip(1);
                    foreach (string[] line in lines) {
                        list.Add(line);
                    }
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadFile({0}): {1}", path ?? "", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Reads a delimited file into a DataTable.
        /// </summary>
        /// <param name="table">The datatable to modify.</param>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="delim">The character separating each column of each line.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        public static bool ReadFile(this DataTable table, string path, char delim = ',', bool hasHeaders = true)
        {
            try {
                using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                    IEnumerable<string[]> lines = FileForeach(reader, delim, hasHeaders);
                    //add columns
                    if (hasHeaders) {
                        string[] line = lines.First();
                        lines = lines.Skip(1);
                        for (int i = 0; i < line.Length; i++) {
                            string header = line[i];
                            if (string.IsNullOrEmpty(header))
                                header = new string(' ', i + 1);
                            table.Columns.Add(header);
                        }
                    }
                    //add rows
                    foreach (string[] line in lines) {
                        for (int i = table.Columns.Count; i < line.Length; i++) {
                            table.Columns.Add(new string(' ', i + 1));
                        }
                        table.Rows.Add(line);
                    }
                    table.Trim();
                    return true;
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadFile({0}): {1}", path ?? "", ex.Message);
            }
            return false;
        }
        #endregion //Delimited

        #region CSV
        /// <summary>
        /// Writes a list to a CSV file.
        /// </summary>
        /// <param name="list">The list to export.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="printHeaders">Determines if the first rows includes headers.</param>
        public static bool WriteCsv<T>(this IEnumerable<T> list, string path, bool printHeaders) where T : IEnumerable<string>
        {
            return WriteCsv(list, path, printHeaders, Encoding.Default);
        }

        /// <summary>
        /// Writes a list to a CSV file.
        /// </summary>
        /// <param name="list">The list to export.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="printHeaders">Determines if the first rows includes headers.</param>
        /// <param name="encoding">The encoding for the text.</param>
        public static bool WriteCsv<T>(this IEnumerable<T> list, string path, bool printHeaders, Encoding encoding) where T : IEnumerable<string>
        {
            try {
                using (StreamWriter sr = new StreamWriter(path, false, encoding))
                using (CsvWriter csv = new CsvWriter(sr)) {
                    if (printHeaders) {
                        IEnumerable<string> propertyNames =
                            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance)
                            .Select(prop => prop.Name);
                        foreach (string header in propertyNames) {
                            csv.WriteField(header);
                        }
                        csv.NextRecord();
                    }
                    foreach (T row in list) {
                        foreach (string val in row) {
                            csv.WriteField(val);
                        }
                        csv.NextRecord();
                    }
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error WriteCsv({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Writes a DataTable to a CSV file.
        /// </summary>
        /// <param name="table">The datatable to export.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="hasHeaders">Determines if the column names should be output.</param>
        /// <param name="encoding">The encoding for the text.</param>
        public static bool WriteCsv(this DataTable table, string path, bool hasHeaders = true)
        {
            return WriteCsv(table, path, hasHeaders, Encoding.Default);
        }

        /// <summary>
        /// Writes a DataTable to a CSV file.
        /// </summary>
        /// <param name="table">The Datatable to export.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="hasHeaders">Determines if the column names should be output.</param>
        /// <param name="encoding">The encoding for the text.</param>
        public static bool WriteCsv(this DataTable table, string path, bool hasHeaders, Encoding encoding)
        {
            try {
                using (StreamWriter sr = new StreamWriter(path, false, encoding))
                using (CsvWriter csv = new CsvWriter(sr)) {
                    if (hasHeaders) {
                        foreach (DataColumn column in table.Columns) {
                            csv.WriteField(column.ColumnName);
                        }
                        csv.NextRecord();
                    }
                    foreach (DataRow row in table.Rows) {
                        for (int i = 0; i < table.Columns.Count; i++) {
                            csv.WriteField(row[i].ToString());
                        }
                        csv.NextRecord();
                    }
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error WriteCsv({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Reads a CSV file into a List of strings.
        /// </summary>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <returns>A list of strings from the CSV file.</returns>
        public static List<string[]> ReadCsv(string path, bool hasHeaders = true, bool ignoreBlankLines = true)
        {
            try {
                return CsvForeach(path, hasHeaders, ignoreBlankLines).ToList();
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadCsv({0}): {1}", path ?? "null", ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Reads a CSV file into a collections of strings.
        /// </summary>
        /// <param name="list">The Collection to modify.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <returns>True on success, or false otherwise.</returns>
        public static bool ReadCsv(this ICollection<string[]> list, string path, bool ignoreBlankLines = true)
        {
            try {
                foreach (string[] line in CsvForeach(path, false, ignoreBlankLines)) {
                    list.Add(line);
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadCsv({0}): {1}", path ?? "null", ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Reads a CSV file into a DataTable.
        /// </summary>
        /// <param name="table">The Datatable to modify.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <returns>True on success, or false otherwise.</returns>
        public static bool ReadCsv(this DataTable table, string path, bool hasHeaders = true, bool ignoreBlankLines = true)
        {
            try {
                IEnumerable<string[]> lines = CsvForeach(path, false, ignoreBlankLines);
                string[] headers = lines.First();
                lines = lines.Skip(1);
                if (headers != null) {
                    for (int i = 0; i < headers.Length; i++) {
                        string columnName = headers[i];
                        if (hasHeaders) {
                            table.Columns.Add(columnName.Length == 0 ? new string(' ', i) : columnName);
                        }
                        else
                            table.Columns.Add(new string(' ', i));
                    }
                }
                foreach (string[] line in lines) {
                    for (int i = table.Columns.Count; i < line.Length; i++) {
                        table.Columns.Add(new string(' ', i));
                    }
                    table.Rows.Add(line);
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadCsv({0}): {1}", path ?? "null", ex.Message);
                return false;
            }
            table.Trim();
            return true;
        }

        /// <summary>
        /// Iterates each row in a CSV file.
        /// </summary>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<string[]> CsvForeach(string path, bool hasHeaders = true, bool ignoreBlankLines = true)
        {
            using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                foreach (string[] line in CsvForeach(reader, hasHeaders)) {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Iterates each row in a CSV file.
        /// </summary>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<string[]> CsvForeach(TextReader reader, bool hasHeaders = true, bool ignoreBlankLines = true)
        {
            using (CsvParser parser = new CsvParser(reader)) {
                parser.Configuration.TrimOptions = TrimOptions.None;
                parser.Configuration.IgnoreBlankLines = ignoreBlankLines;
                string[] line;
                if (hasHeaders)
                    parser.Read();
                while ((line = parser.Read()) != null) {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Iterates each row in a CSV file.
        /// </summary>
        /// <typeparam name="T">The Type of objects to return.</typeparam>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="constructor">The constructor for the returned objects.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> CsvForeach<T>(string path, Func<string[], T> constructor, bool hasHeaders = true, bool ignoreBlankLines = true)
        {
            using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                foreach (string[] strs in CsvForeach(reader, hasHeaders, ignoreBlankLines)) {
                    yield return constructor(strs);
                }
            }
        }

        /// <summary>
        /// Iterates each row in a CSV file.
        /// </summary>
        /// <typeparam name="T">The Type of objects to return.</typeparam>
        /// <param name="reader">The TextReader for the CSV file.</param>
        /// <param name="constructor">The constructor for the returned objects.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> CsvForeach<T>(TextReader reader, Func<string[], T> constructor, bool hasHeaders = true, bool ignoreBlankLines = true)
        {
            foreach (string[] strs in CsvForeach(reader, hasHeaders, ignoreBlankLines)) {
                yield return constructor(strs);
            }
        }
        #endregion // CSV

        #region XLS
        /// <summary>
        /// Iterates each row in an XLS file.
        /// </summary>
        /// <param name="path">The path of the XLS file.</param>
        /// <param name="sheetname">The name of the worksheet to iterate. 
        /// If this is null then the first sheet will be iterated.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<string[]> XlsForeach(string path, string sheetname = null, bool hasHeaders = true)
        {
            string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path
                + @";Extended Properties=""Excel 8.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text""";
            using (OleDbConnection connection = new OleDbConnection(connStr))
            using (OleDbCommand cmd = new OleDbCommand()) {
                if (sheetname == null) {
                    DataTable dtExcelSchema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    sheetname = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                }
                cmd.Connection = connection;
                cmd.CommandText = "SELECT * FROM [" + sheetname + "]";

                connection.Open();
                OleDbDataReader reader = cmd.ExecuteReader();

                string[] cellValues;
                while (reader.Read()) {
                    int count = reader.VisibleFieldCount;
                    cellValues = new string[count];
                    for (int i = 0; i < count; i++) {
                        cellValues[i] = reader.GetString(i);
                    }
                    yield return cellValues;
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Iterates each row in an XLS file.
        /// </summary>
        ///  <typeparam name="T">The Type of objects to return.</typeparam>
        /// <param name="path">The path of the XLS file.</param>
        /// <param name="constructor">The constructor for the returned objects.</param>
        /// <param name="sheetname">The name of the worksheet to iterate. 
        /// If this is null then the first sheet will be iterated.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> XlsForeach<T>(string path, Func<string[], T> constructor, string sheetname = null, bool hasHeaders = true)
        {
            foreach (string[] line in XlsForeach(path, sheetname, hasHeaders)) {
                yield return constructor(line);
            }
        }

        /// <summary>
        /// Reads an XLS Excel file into a List.
        /// This assumes that the type T has public properties with identical names of the columns in the DataTable.
        /// </summary>
        /// <typeparam name="T">The type of object to convert the DataRows into.</typeparam>
        /// <param name="list">The list to append data to.</param>
        /// <param name="path">The path of the Excel file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>The Dataset with data from the file.</returns>
        public static bool ReadXls<T>(this List<T> list, string path) where T : new()
        {
            DataSet dataset = new DataSet();
            if (!dataset.ReadXls(path, true))
                return false;
            dataset.Tables[0].ToList<T>(list);
            return true;
        }

        /// <summary>
        /// Reads an XLS Excel file into a DataSet where each sheet is a DataTable.
        /// </summary>
        /// <param name="path">The path of the Excel file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>The Dataset with data from the file.</returns>
        public static DataSet ReadXls(string path, bool hasHeaders = true)
        {
            DataSet dataset = new DataSet();
            if (!dataset.ReadXls(path, hasHeaders))
                dataset = null;
            return dataset;
        }

        /// <summary>
        /// Reads an XLS Excel file into a DataSet where each sheet is a DataTable.
        /// </summary>
        /// <param name="dataset">The dataset to append data to.</param>
        /// <param name="path">The path of the Excel file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>True on success, or false otherwise.</returns>
        public static bool ReadXls(this DataSet dataset, string path, bool hasHeaders = true)
        {
            string sheetName = null;
            try {
                // parses as the correct type
                //string connStr = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + "; Jet OLEDB:Engine Type = 5; Extended Properties =\"Excel 8.0;\"";
                // handles column data of multiple types better
                string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path
                    + @";Extended Properties=""Excel 8.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text""";
                using (OleDbConnection con = new OleDbConnection(connStr)) {
                    using (OleDbCommand cmd = new OleDbCommand()) {
                        using (OleDbDataAdapter oda = new OleDbDataAdapter()) {
                            cmd.Connection = con;
                            con.Open();
                            DataTable dtExcelSchema = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                            for (int i = 0; i < dtExcelSchema.Rows.Count; i++) {
                                sheetName = dtExcelSchema.Rows[i]["TABLE_NAME"].ToString();
                                DataTable dt = new DataTable(sheetName);
                                cmd.Connection = con;
                                cmd.CommandText = "SELECT * FROM [" + sheetName + "]";
                                oda.SelectCommand = cmd;
                                oda.Fill(dt);
                                dt.TableName = sheetName;
                                if (hasHeaders && dt.Rows.Count > 0) {
                                    DataRow row = dt.Rows[0];
                                    for (int c = 0; c < dt.Columns.Count; c++) {
                                        string colName = row[c].ToString();
                                        if (string.IsNullOrWhiteSpace(colName)) {
                                            dt.Columns[c].ColumnName = new string(' ', c + 1);
                                        }
                                        else
                                            dt.Columns[c].ColumnName = row[c].ToString();
                                    }
                                    dt.Rows.RemoveAt(0);
                                }
                                dataset.Tables.Add(dt);
                            }
                        }
                    }
                }
                //trim useless far-right side columns if there's no headers
                if (!hasHeaders) {
                    foreach (DataTable table in dataset.Tables) {
                        table.TrimColumns();
                    }
                }
            }
            catch (Exception ex) {
                string sheetStr = sheetName == null ? ""
                    : "sheet[" + sheetName.Substring(0, sheetName.Length - 1) + "] ";
                Console.Error.WriteLine("Error ReadXls({0}): {1}{2}", path, sheetStr, ex.Message);
                return false;
            }
            return true;
        }
        #endregion

        #region XLSX
        /// <summary>
        /// Iterates each row in an XLSX file.
        /// </summary>
        /// <param name="path">The path of the XLSX file.</param>
        /// <param name="sheetname">The name of the sheet in the file to iterate. 
        /// If this is null then the first sheet will be iterated.</param>
        /// <param name="includeHeaders">Determines if the file has headers should be included.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<string[]> XlsxForeach(string path, string sheetname = null, bool includeHeaders = true)
        {
            using (Excel.Spreadsheet ss = new Excel.Spreadsheet(path)) {
                Excel.Worksheet worksheet = sheetname == null ? ss[0] : ss[sheetname];
                if (worksheet == null)
                    throw new Exception(string.Format("Invalid Worksheet '{0}'", sheetname ?? "null"));
                IEnumerable<string[]> rows = worksheet.AsEnumerable();
                if (includeHeaders)
                    rows = rows.Skip(1);
                foreach (string[] row in rows) {
                    yield return row;
                }
            }
        }

        /// <summary>
        /// Iterates each row in an XLSX file.
        /// </summary>
        /// <param name="path">The path of the XLSX file.</param>
        /// <param name="sheetname">The name of the sheet in the file to iterate. 
        /// If this is null then the first sheet will be iterated.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> XlsxForeach<T>(string path, string sheetname = null, bool hasHeaders = true) where T : new()
        {
            using (Excel.Spreadsheet ss = new Excel.Spreadsheet(path)) {
                Excel.Worksheet worksheet = sheetname == null ? ss[0] : ss[sheetname];
                if (worksheet == null)
                    throw new Exception(string.Format("Invalid Worksheet '{0}'", sheetname ?? "null"));
                foreach (T obj in worksheet.AsEnumerable<T>(hasHeaders)) {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Iterates each row in an XLSX file.
        /// </summary>
        /// <param name="path">The path of the XLSX file.</param>
        /// <param name="constructor">The constructor for the objects returned.</param>
        /// <param name="sheetname">The name of the sheet in the file to iterate. 
        /// If this is null then the first sheet will be iterated.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> XlsxForeach<T>(string path, Func<string[], T> constructor, string sheetname = null, bool hasHeaders = true)
        {
            using (Excel.Spreadsheet ss = new Excel.Spreadsheet(path)) {
                Excel.Worksheet worksheet = sheetname == null ? ss[0] : ss[sheetname];
                if (worksheet == null)
                    throw new Exception(string.Format("Invalid Worksheet '{0}'", sheetname ?? "null"));
                IEnumerable<string[]> rows = worksheet.AsEnumerable();
                if (hasHeaders)
                    rows = rows.Skip(1);
                foreach (string[] row in rows) {
                    yield return constructor(row);
                }
            }
        }

        /// <summary>
        /// Reads an Xlsx Excel file into a List.
        /// </summary>
        /// <param name="path">Name of file to be written.</param>
        /// <param name="sheetname">The name of the sheet in the file to read.
        /// If this is null then the first sheet will be iterated.</param>
        /// <param name="includeHeaders">Determines if the columns should be included.</param>
        /// <returns>The list if successful, or null if something went wrong.</returns>
        public static List<string[]> ReadXlsx(string path, string sheetname = null, bool includeHeaders = true)
        {
            List<string[]> list = new List<string[]>();
            return ReadXlsx(list, path, sheetname, includeHeaders) ? list : null;
        }

        /// <summary>
        /// Reads an Xlsx Excel file into a List.
        /// </summary>
        /// <param name="list">The List to store the data in.</param>
        /// <param name="path">Name of file to be written.</param>
        /// <param name="sheetname">The name of the sheet in the file to read.
        /// If this is null then the first sheet will be iterated.</param>
        /// <param name="includeHeaders">Determines if the columns should be included.</param>
        /// <returns>True if successful, or false if something went wrong.</returns>
        public static bool ReadXlsx(this ICollection<string[]> list, string path, string sheetname = null, bool includeHeaders = true)
        {
            try {
                IEnumerable<string[]> lines = XlsxForeach(path, sheetname, includeHeaders);
                foreach (string[] line in lines) {
                    list.Add(line);
                }
                return true;
            }
            catch (Exception ex) {
                string sheetStr = sheetname == null ? "" : "[" + sheetname + "]";
                Console.Error.WriteLine("Error ReadXlsx({0}){1}: {2}", path ?? "null", sheetStr, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Reads an Xlsx Excel file into a DataSet.
        /// </summary>
        /// <param name="ds">The DataSet to store the data in.</param>
        /// <param name="path">Name of file to be written.</param>
        /// <param name="hasHeaders">Determines if the columns should be written.</param>
        /// <returns>True if successful, false if something went wrong.</returns>
        public static bool ReadXlsx(this DataSet ds, string path, bool hasHeaders = true)
        {
            try {
                using (Excel.Spreadsheet ss = new Excel.Spreadsheet(path)) {
                    ss.ToDataSet(ds);
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadXlsx({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Reads an Xlsx Excel file into a DataSet.
        /// </summary>
        /// <param name="dt">The DataTable to store the data in.</param>
        /// <param name="path">Name of file to be written.</param>
        /// <param name="hasHeaders">Determines if the columns should be written.</param>
        /// <returns>True if successful, false if something went wrong.</returns>
        public static bool ReadXlsx(this DataTable dt, string path, string sheetname = null, bool hasHeaders = true)
        {
            try {
                using (Excel.Spreadsheet ss = new Excel.Spreadsheet(path)) {
                    if (!ss.IsOpen)
                        return false;
                    Excel.Worksheet worksheet = sheetname == null ? ss[0] : ss[sheetname];
                    worksheet.ToDataTable(dt, hasHeaders);
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadXlsx({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Writes a DataSet to a file.
        /// </summary>
        /// <typeparam name="T">The type of object in the list.</typeparam>
        /// <param name="list">The list of objects to write to the Excel file.</param>
        /// <param name="path">Name of file to be written.</param>
        /// <param name="hasHeaders">Determines if the columns should be written.</param>
        /// <param name="autofilter">Determines if auto-filtering will be enabled.</param>
        /// <param name="autoformat">Determines if auto-formatting will be enabled.</param>
        /// <returns>True if successful, false if something went wrong.</returns>
        public static bool WriteXlsx<T>(this IEnumerable<T> list, string path, bool hasHeaders = true, bool autofilter = true, bool autoformat = true)
        {
            return WriteXlsx((ss) => ss[0].Load(list), path, hasHeaders, autofilter, autoformat);
        }

        /// <summary>
        /// Writes a DataSet to a file.
        /// </summary>
        /// <param name="dt">DataTable containing the data to be written to the Excel file.</param>
        /// <param name="path">Name of file to be written.</param>
        /// <param name="hasHeaders">Determines if the columns should be written.</param>
        /// <param name="autofilter">Determines if auto-filtering will be enabled.</param>
        /// <param name="autoformat">Determines if auto-formatting will be enabled.</param>
        /// <returns>True if successful, false if something went wrong.</returns>
        public static bool WriteXlsx(this DataTable dt, string path, bool hasHeaders = true, bool autofilter = true, bool autoformat = true)
        {
            return WriteXlsx((ss) => ss[0].Load(dt, hasHeaders), path, hasHeaders, autofilter, autoformat);
        }

        /// <summary>
        /// Writes a DataSet to a file.
        /// </summary>
        /// <param name="ds">DataSet containing the data to be written to the Excel.</param>
        /// <param name="path">Name of file to be written.</param>
        /// <param name="hasHeaders">Determines if the columns should be written.</param>
        /// <param name="autofilter">Determines if auto-filtering will be enabled.</param>
        /// <param name="autoformat">Determines if auto-formatting will be enabled.</param>
        /// <returns>True if successful, false if something went wrong.</returns>
        public static bool WriteXlsx(this DataSet ds, string path, bool hasHeaders = true, bool autofilter = true, bool autoformat = true)
        {
            return WriteXlsx((ss) => ss.Load(ds), path, hasHeaders, autofilter, autoformat);
        }

        private static bool WriteXlsx(Action<Excel.Spreadsheet> action, string path, bool hasHeaders, bool autofilter, bool autoformat)
        {
            try {
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                    fi.Delete();
                using (Excel.Spreadsheet ss = new Excel.Spreadsheet(path)) {
                    if (!ss.IsOpen)
                        return false;
                    action(ss);
                    if(autofilter)
                        ss.AutoFilter = true;
                    if(autoformat)
                        ss.AutoFormat();
                    ss.AutoFit();
                    ss.BestFit = true;
                    ss.Save();
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error WriteXlsx({0}): {1}{2}", path ?? "null", ex.Message, ex.InnerException == null ? "" : "\n " + ex.InnerException);
            }
            return false;
        }
        #endregion

        #region File/Directory/Path
        public static bool Move(string inpath, string outpath, bool overwrite = true)
        {
            try {
                FileInfo fi = new FileInfo(inpath);
                if (!fi.Exists) {
                    Console.Error.WriteLine("Error IO.Move({0}, {1}): File doesn't exist.", inpath ?? "null", outpath ?? "null");
                    return false;
                }
                if (overwrite && new FileInfo(outpath).Exists)
                    File.Delete(outpath);
                fi.MoveTo(outpath);
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error IO.Move({0}, {1}): {2}", inpath ?? "null", outpath ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Determines if a file is available for reading/writing.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <param name="access">The file access to test.</param>
        /// <returns>True if the file is available with the given file access. False otherwise.</returns>
        public static bool IsAvailable(string path, FileAccess access = FileAccess.Read)
        {
            return IsAvailable(new FileInfo(path), access);
        }

        /// <summary>
        /// Determines if a file is available for reading/writing.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <param name="access">The file access to test.</param>
        /// <returns>True if the file is available with the given file access. False otherwise.</returns>
        public static bool IsAvailable(FileInfo path, FileAccess access = FileAccess.Read)
        {
            FileStream stream = null;
            try {
                //check if File is accessible
                stream = path.Open(FileMode.Open, access, FileShare.None);
            }
            catch (IOException) {
                return false; //not accessible
            }
            finally {
                if (stream != null)
                    stream.Close();
            }
            return true; //file is available
        }

        /// <summary>
        /// Finds all files matching a search pattern.
        /// </summary>
        /// <param name="path">The path and search pattern for the files. 
        /// This can include wildcards (* ?) and environment variables (%var%).</param>
        /// <param name="recursive">Determines if subdirectories should be searched.</param>
        /// <param name="expandEnvVars">Determines if environment variables should be expanded (%var)%.</param>
        /// <returns>The files that match the search pattern.</returns>
        public static FileInfo[] GetFiles(string path, bool recursive = false, bool expandEnvVars = true)
        {
            string envExpanded = expandEnvVars ? Environment.ExpandEnvironmentVariables(path) : path;
            string directory = Path.GetDirectoryName(envExpanded);
            string pattern = Path.GetFileName(envExpanded);
            return GetFiles(directory, pattern, recursive);
        }

        /// <summary>
        /// Finds all files matching a given search pattern within a given directory.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <param name="pattern">The search pattern for the files. This can include wildcards (* ?).</param>
        /// <param name="recursive">Determines if subdirectories should be searched with the same search pattern.</param>
        /// <returns>The files that match the search pattern.</returns>
        public static FileInfo[] GetFiles(string directory, string pattern, bool recursive = false)
        {
            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string dir = directory.Length == 0 ? Directory.GetCurrentDirectory() : directory;
            return new DirectoryInfo(dir).GetFiles(pattern, option);
        }

        /// <summary>
        /// Finds all directories matching a search pattern.
        /// </summary>
        /// <param name="path">The path and search pattern for the directories. 
        /// This can include wildcards (* ?) and environment variables (%var%).</param>
        /// <param name="recursive">Determines if subdirectories should be searched.</param>
        /// <param name="expandEnvVars">Determines if environment variables should be expanded (%var)%.</param>
        /// <returns>The directories that match the search pattern.</returns>
        public static DirectoryInfo[] GetDirectories(string path, bool recursive = false, bool expandEnvVars = true)
        {
            string envExpanded = expandEnvVars ? Environment.ExpandEnvironmentVariables(path) : path;
            string directory = Path.GetDirectoryName(envExpanded);
            string pattern = Path.GetFileName(envExpanded);
            return GetDirectories(directory, pattern, recursive);
        }

        /// <summary>
        /// Finds all files matching a given search pattern within a given directory.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <param name="pattern">The search pattern for the files. This can include wildcards (* ?).</param>
        /// <param name="recursive">Determines if subdirectories should be searched with the same search pattern.</param>
        /// <returns>The files that match the search pattern.</returns>
        public static DirectoryInfo[] GetDirectories(string directory, string pattern, bool recursive = false)
        {
            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string dir = directory.Length == 0 ? Directory.GetCurrentDirectory() : directory;
            return new DirectoryInfo(dir).GetDirectories(pattern, option);
        }

        /// <summary>
        /// Generates a unique filename based in the input directory, filename, and datetime.
        /// If the file doesn't exist then the same path will be returned.
        /// </summary>
        /// <param name="directoryname">The directory that the file will be created in.</param>
        /// <param name="filename">The filename to generate from.</param>
        /// <param name="date">The date that will be appended to the end of the filename.</param>
        /// <returns>A unique filename that doesn't exist in the directory.</returns>
        public static string GenerateFilename(string directoryname, string filename, DateTime date)
        {
            string pathNoExt = Path.GetFileNameWithoutExtension(filename) + date.ToString(" MM-d-yyyy");
            string fileExt = Path.GetExtension(filename);
            return GenerateFilename(directoryname, pathNoExt + fileExt);
        }

        /// <summary>
        /// Generates a unique filename based in the input directory and filename. 
        /// If the file doesn't exist then the same path will be returned.
        /// </summary>
        /// <param name="directoryname">The directory that the file will be created in.</param>
        /// <param name="filename">The filename to generate from.</param>
        /// <returns>A unique filename that doesn't exist in the directory.</returns>
        public static string GenerateFilename(string directoryname, string filename)
        {
            string outfile = directoryname + Path.DirectorySeparatorChar + filename;
            if (File.Exists(outfile)) {
                string pathNoExt = directoryname + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename);
                string fileExt = Path.GetExtension(filename);
                int index = 1;
                do {
                    outfile = pathNoExt + " (" + index + ")" + fileExt;
                    index++;
                } while (File.Exists(outfile));
            }
            return outfile;
        }

        /// <summary>
        /// Generates a unique filename based in the input path. 
        /// If the file doesn't exist then the same path will be returned.
        /// </summary>
        /// <param name="path">The path to generate a unique filename from.</param>
        /// <returns>A unique filename that doesn't exist in the directory.</returns>
        public static string GenerateFilename(string path)
        {
            return GenerateFilename(Path.GetDirectoryName(path), Path.GetFileName(path));
        }
        #endregion
    }
}