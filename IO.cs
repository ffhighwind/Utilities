﻿using System;
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
        /// Reads a file into a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to fill.</param>
        /// <param name="path">The path of the file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <param name="sheetname">The name of the worksheet to iterate if there are any.
        /// If this is null then the first sheet will be read.</param>
        /// <returns>A DataTable with contents from the file.</returns>
        public static bool Read(this DataTable table, string path, bool hasHeaders = true, string sheetname = null)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".csv")
                return table.ReadCsv(path, hasHeaders, true);
            else if (ext == ".xlsx")
                return table.ReadXlsx(path, sheetname, hasHeaders);
            else if (ext == ".xls")
                return table.ReadXls(path, sheetname, hasHeaders);
            else if (ext == ".tsv")
                return table.ReadCsv(path, hasHeaders, true, "\t");
            throw new NotSupportedException("File extension type not supported: '" + ext + "'");
        }

        /// <summary>
        /// Reads a file into a List.
        /// </summary>
        /// <typeparam name="T"> The Type of object to return.</typeparam>
        /// <param name="list">The List to fill.</param>
        /// <param name="path">The path of the file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <param name="sheetname">The name of the worksheet to iterate if there are any.
        /// If this is null then the first sheet will be read.</param>
        /// <returns>True if the file was read, or null if there was an error.</returns>
        public static bool Read<T>(this ICollection<T> list, string path, bool hasHeaders = true, string sheetname = null) where T : class, new()
        {
            try {
                foreach (T obj in IO.Foreach<T>(path, hasHeaders, sheetname)) {
                    list.Add(obj);
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error Read<T>({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Reads a file into a List.
        /// </summary>
        /// <typeparam name="T"> The Type of object to return.</typeparam>
        /// <param name="path">The path of the file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <param name="sheetname">The name of the worksheet to iterate if there are any.
        /// If this is null then the first sheet will be read.</param>
        /// <returns>A List with contents from the file.</returns>
        public static List<T> Read<T>(string path, bool hasHeaders = true, string sheetname = null) where T : class, new()
        {
            List<T> list = new List<T>();
            return list.Read<T>(path, hasHeaders, sheetname) ? list : null;
        }

        /// <summary>
        /// Reads a file into a List of strings.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="hasHeaders">Determines if the first line of the file should be skipped.</param>
        /// <returns>A DataTable with contents from the file, or null on error.</returns>
        public static DataTable Read(string path, bool hasHeaders = true)
        {
            DataTable table = new DataTable();
            if (table.Read(path, hasHeaders))
                return table;
            table.Dispose();
            return null;
        }

        /// <summary>
        /// Iterates each row in a file.
        /// </summary>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="hasHeaders">Determines if the first line of the file should be skipped.</param>
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
            throw new NotSupportedException("File extension type not supported: '" + ext + "'");
        }

        /// <summary>
        /// Iterates each row in a file.
        /// </summary>
        /// <typeparam name="T"> The Type of object to return.</typeparam>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="constructor">A function that constructs an object from an array of strings.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <param name="sheetname">The name of the worksheet to iterate if there are any.
        /// If this is null then the first sheet will be read.</param>
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
            throw new NotSupportedException("File extension type not supported: '" + ext + "'");
        }

        /// <summary>
        /// Iterates each row in a file.
        /// </summary>
        /// <typeparam name="T"> The Type of object to return.</typeparam>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="constructor">A function that constructs an object from an array of strings.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <param name="sheetname">The name of the worksheet to iterate if there are any.
        /// If this is null then the first sheet will be read.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> Foreach<T>(string path, bool hasHeaders = true, string sheetname = null) where T : class, new()
        {
            string ext = Path.GetExtension(path);
            if (ext == ".csv")
                return CsvForeach<T>(path, hasHeaders, true);
            else if (ext == ".xlsx")
                return XlsxForeach<T>(path, sheetname, hasHeaders);
            else if (ext == ".xls")
                return XlsForeach<T>(path, sheetname, hasHeaders);
            else if (ext == ".tsv")
                return CsvForeach<T>(path, hasHeaders, true, "\t");
            throw new NotSupportedException("File extension type not supported: '" + ext + "'");
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
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="delim">The separator for the columns in the file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> FileForeach<T>(string path, char delim = ',', bool hasHeaders = true) where T : class, new()
        {
            using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                Func<string[], T> constructor = hasHeaders ? Util.StringsConverter<T>(reader.ReadLine().Split(delim)) : Util.StringsConverter<T>();
                return FileForeach<T>(reader, constructor, delim, false);
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
        /// Iterates each row in delimited text.
        /// </summary>
        /// <typeparam name="T">The Type of object to return.</typeparam>
        /// <param name="reader">The TextReader of the delimited file.</param>
        /// <param name="delim">The separator for the columns in the file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> FileForeach<T>(TextReader reader, char delim = ',', bool hasHeaders = true) where T : class, new()
        {
            Func<string[], T> constructor = hasHeaders ? Util.StringsConverter<T>(reader.ReadLine().Split(delim)) : Util.StringsConverter<T>();
            string line;
            while ((line = reader.ReadLine()) != null) {
                yield return constructor(line.Split(delim));
            }
        }

        /// <summary>
        /// Reads a delimited file into a Collection of strings.
        /// </summary>
        /// <param name="list">The List to modify.</param>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="delim">The character separating each column of each line.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>True if the file was read succesfully, or false otherwise.</returns>
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
        /// Reads a delimited file into a Collection of strings.
        /// </summary>
        /// <param name="list">The List to modify.</param>
        /// <param name="path">The path of the delimited file.</param>
        /// <param name="delim">The character separating each column of each line.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>True if the file was read succesfully, or false otherwise.</returns>
        public static bool ReadFile<T>(this ICollection<T> list, string path, char delim = ',', bool hasHeaders = true) where T : class, new()
        {
            try {
                IEnumerable<T> lines = FileForeach<T>(path, delim, hasHeaders);
                foreach (T line in lines) {
                    list.Add(line);
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
        /// <returns>True if the file was read successfully, or falseo therwise.</returns>
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
        /// <param name="printHeaders">Determines if the property names should be output as headers.</param>
        /// <param name="delim">The delimiter for the text.</param>
        /// <returns>True if the CSV file was written to, or false otherwise.</returns>
        public static bool WriteCsv<T>(this IEnumerable<T> list, string path, bool printHeaders = true, string delim = ",") where T : class
        {
            try {
                using (TextWriter writer = new StreamWriter(path, false))
                using (CsvWriter csv = new CsvWriter(writer)) {
                    csv.Configuration.QuoteAllFields = true;
                    csv.Configuration.Delimiter = delim;
                    PropertyInfo[] props =
                                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance);
                    if (printHeaders) {
                        foreach (string header in props.Select(prop => prop.Name)) {
                            csv.WriteField(header);
                        }
                        csv.NextRecord();
                    }
                    foreach (T row in list) {
                        for(int i = 0; i < props.Length; i++) {
                            csv.WriteField(props[i].GetValue(row));
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
        /// <param name="table">The Datatable to export.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="hasHeaders">Determines if the column names should be output.</param>
        /// <param name="delim">The delimiter for the text.</param>
        /// <returns>True if the CSV file was written to, or false otherwise.</returns>
        public static bool WriteCsv(this DataTable table, string path, bool hasHeaders = true, string delim = ",")
        {
            try {
                using (StreamWriter sr = new StreamWriter(path, false))
                using (CsvWriter csv = new CsvWriter(sr)) {
                    csv.Configuration.Delimiter = delim;
                    csv.Configuration.QuoteAllFields = true;
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
        /// Reads a CSV file into a collections of strings.
        /// </summary>
        /// <param name="list">The Collection to modify.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <param name="delim">The delimiter of the file.</param>
        /// <returns>True on success, or false otherwise.</returns>
        public static bool ReadCsv(this ICollection<string[]> list, string path, bool ignoreBlankLines = true, string delim = ",")
        {
            try {
                foreach (string[] line in CsvForeach(path, false, ignoreBlankLines, delim)) {
                    list.Add(line);
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadCsv({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Reads a CSV file into a collections of strings.
        /// </summary>
        /// <typeparam name="T">The Type of objects to return.</typeparam>
        /// <param name="list">The Collection to modify.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <param name="delim">The delimiter of the file.</param>
        /// <returns>True on success, or false otherwise.</returns>
        public static bool ReadCsv<T>(this ICollection<T> list, string path, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",") where T : class, new()
        {
            try {
                foreach (T line in CsvForeach<T>(path, hasHeaders, ignoreBlankLines, delim)) {
                    list.Add(line);
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadCsv({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Reads a CSV file into a DataTable.
        /// </summary>
        /// <param name="table">The Datatable to modify.</param>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <param name="delim">The delimter of the file.</param>
        /// <returns>True on success, or false otherwise.</returns>
        public static bool ReadCsv(this DataTable table, string path, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
        {
            try {
                IEnumerable<string[]> lines = CsvForeach(path, false, ignoreBlankLines, delim);
                string[] headers = lines.First();
                lines = lines.Skip(1);
                if (headers != null && table.Columns.Count == 0) {
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
                table.Trim();
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadCsv({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Iterates each row in a CSV file.
        /// </summary>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <param name="delim">The delimiter of the file.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<string[]> CsvForeach(string path, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
        {
            using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                foreach (string[] line in CsvForeach(reader, hasHeaders, ignoreBlankLines, delim)) {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Iterates each row in a CSV file.
        /// </summary>
        /// <param name="reader">The reader of the CSV file.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
        /// <param name="delim">The delimiter of the file.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<string[]> CsvForeach(TextReader reader, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
        {
            using (CsvParser csv = new CsvParser(reader)) {
                csv.Configuration.TrimOptions = TrimOptions.None;
                csv.Configuration.IgnoreBlankLines = ignoreBlankLines;
                csv.Configuration.Delimiter = delim;
                string[] line;
                if (hasHeaders)
                    csv.Read();
                while ((line = csv.Read()) != null) {
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
        /// <param name="delim">The delimiter of the file.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> CsvForeach<T>(string path, Func<string[], T> constructor, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
        {
            using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                foreach (string[] strs in CsvForeach(reader, hasHeaders, ignoreBlankLines, delim)) {
                    yield return constructor(strs);
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
        public static IEnumerable<T> CsvForeach<T>(string path, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",") where T : class, new()
        {
            using (TextReader reader = Util.TextReader(new FileInfo(path))) {
                return CsvForeach<T>(path, hasHeaders, ignoreBlankLines, delim);
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
        /// <param name="delim">The delimiter of the file.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> CsvForeach<T>(TextReader reader, Func<string[], T> constructor, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
        {
            foreach (string[] strs in CsvForeach(reader, hasHeaders, ignoreBlankLines, delim)) {
                yield return constructor(strs);
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
        /// <param name="delim">The delimiter of the file.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> CsvForeach<T>(TextReader reader, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",") where T : class, new()
        {
            using (CsvParser csv = new CsvParser(reader)) {
                Func<string[], T> constructor;
                string[] line = csv.Read();
                if (line != null) {
                    constructor = hasHeaders ? Util.StringsConverter<T>(line) : Util.StringsConverter<T>();
                    csv.Configuration.TrimOptions = TrimOptions.None;
                    csv.Configuration.IgnoreBlankLines = ignoreBlankLines;
                    csv.Configuration.Delimiter = delim;
                    if (hasHeaders)
                        csv.Read();
                    while ((line = csv.Read()) != null) {
                        yield return constructor(line);
                    }
                }
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
        /// <typeparam name="T">The Type of objects to return.</typeparam>
        /// <param name="path">The path of the XLS file.</param>
        /// <param name="constructor">The constructor for the returned objects.</param>
        /// <param name="sheetname">The name of the worksheet to iterate. 
        /// If this is null then the first sheet will be iterated.</param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the first row will be ignored.</param>
        /// <returns>The iterable rows in the file.</returns>
        public static IEnumerable<T> XlsForeach<T>(string path, string sheetname = null, bool hasHeaders = true) where T : class, new()
        {
            IEnumerable<string[]> lines = XlsForeach(path, sheetname, hasHeaders);
            if (lines.Any()) {
                Func<string[], T> constructor;
                if (hasHeaders) {
                    constructor = Util.StringsConverter<T>(lines.First());
                    lines = lines.Skip(1);
                }
                else
                    constructor = Util.StringsConverter<T>();
                foreach (string[] line in lines) {
                    yield return constructor(line);
                }
            }
        }

        /// <summary>
        /// Iterates each row in an XLS file.
        /// </summary>
        /// <typeparam name="T">The Type of objects to return.</typeparam>
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
        public static bool ReadXls<T>(this ICollection<T> list, string path, string sheetname = null, bool hasHeaders = true) where T : class, new()
        {
            try {
                foreach (T obj in XlsForeach<T>(path, sheetname, hasHeaders)) {
                    list.Add(obj);
                }
                return true;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadXls({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Reads an XLS Excel file into a DataSet where each sheet is a DataTable.
        /// </summary>
        /// <param name="dt">The DataTable to append data to.</param>
        /// <param name="path">The path of the Excel file.</param>
        /// <param name="sheetName"></param>
        /// <param name="hasHeaders">Determines if the file has headers. 
        /// If this is true then the columns are named based on the first line.</param>
        /// <returns>True on success, or false otherwise.</returns>
        public static bool ReadXls(this DataTable dt, string path, string sheetName = null, bool hasHeaders = true)
        {
            try {
                // parses as the correct type
                //string connStr = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + "; Jet OLEDB:Engine Type = 5; Extended Properties =\"Excel 8.0;\"";
                // handles column data of multiple types better
                string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path
                    + @";Extended Properties=""Excel 8.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text""";
                using (OleDbConnection conn = new OleDbConnection(connStr))
                using (OleDbCommand cmd = new OleDbCommand())
                using (OleDbDataAdapter oda = new OleDbDataAdapter()) {
                    if (sheetName == null) {
                        DataTable dtExcelSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        if (dtExcelSchema.Rows.Count == 0)
                            return false;
                        sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                    }
                    cmd.Connection = conn;
                    dt.TableName = sheetName;
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT * FROM [" + sheetName + "]";
                    oda.SelectCommand = cmd;
                    conn.Open();
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
                    dt.Trim();
                    dt.TrimColumns();
                    return true;
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadXls({0}): {1}", path ?? "null", ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Reads an XLS Excel file into a DataSet where each sheet is a DataTable.
        /// </summary>
        /// <param name="dataset">The DataSet to append data to.</param>
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
                using (OleDbConnection conn = new OleDbConnection(connStr))
                using (OleDbCommand cmd = new OleDbCommand())
                using (OleDbDataAdapter oda = new OleDbDataAdapter()) {
                    cmd.Connection = conn;
                    conn.Open();
                    DataTable dtExcelSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    for (int i = 0; i < dtExcelSchema.Rows.Count; i++) {
                        sheetName = dtExcelSchema.Rows[i]["TABLE_NAME"].ToString();
                        DataTable dt = new DataTable(sheetName);
                        cmd.Connection = conn;
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
                //trim useless far-right side columns if there's no headers
                if (!hasHeaders) {
                    foreach (DataTable table in dataset.Tables) {
                        table.TrimColumns();
                    }
                }
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error ReadXls({0}): {1}", path ?? "null", ex.Message);
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
        public static IEnumerable<T> XlsxForeach<T>(string path, string sheetname = null, bool hasHeaders = true) where T : class, new()
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
                    if (!ss.IsOpen)
                        return false;
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

        /// <summary>
        /// Writes a DataSet to a file.
        /// </summary>
        /// <param name="action">A function that loads the data into the spreadsheet.</param>
        /// <param name="path">Name of file to be written.</param>
        /// <param name="hasHeaders">Determines if the columns should be written.</param>
        /// <param name="autofilter">Determines if auto-filtering will be enabled.</param>
        /// <param name="autoformat">Determines if auto-formatting will be enabled.</param>
        /// <returns>True if successful, false if something went wrong.</returns>
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
                    if (autofilter)
                        ss.AutoFilter = true;
                    if (autoformat)
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
        /// <summary>
        /// Moves a file from the input path to the output path. 
        /// </summary>
        /// <param name="inpath">The path of the input file.</param>
        /// <param name="outpath">The path to move the file to.</param>
        /// <param name="overwrite">Determines if the file should be overwritten if it already exists.</param>
        /// <returns>True if the file was moved successfully, or false on error.</returns>
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