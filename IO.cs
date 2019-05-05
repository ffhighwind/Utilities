using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using Utilities.Excel;

namespace Utilities
{
	/// <summary>
	/// IO utilities class.
	/// </summary>
	public static class IO
	{
		private const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		#region Any
		/// <summary>
		/// Reads a file into a <see cref="DataSet"/>.
		/// </summary>
		/// <param name="dataset">The <see cref="DataSet"/> to fill.</param>
		/// <param name="path">The path of the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>A <see cref="DataSet"/> with contents from the file.</returns>
		public static DataSet Read(this DataSet dataset, string path, bool hasHeaders = true)
		{
			string ext = Path.GetExtension(path)?.ToLower();
			if (ext == ".xlsx")
				return dataset.ReadXlsx(path, hasHeaders);
			else if (ext == ".csv" || ext == ".tsv") {
				DataTable table = new DataTable();
				table.ReadCsv(path, hasHeaders, true, ext == ".csv" ? "," : "\t");
				dataset.Tables.Add(table);
				return dataset;
			}
			else if (ext == ".xls")
				return dataset.ReadXls(path, hasHeaders);
			throw new NotSupportedException("File extension type not supported: '" + ext + "'");
		}

		/// <summary>
		/// Reads a file into a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to fill.</param>
		/// <param name="path">The path of the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="sheetName">The name of the worksheet to iterate if there are any.
		/// If this is <see langword="null"/> then the first sheet will be read.</param>
		/// <returns>A <see cref="DataTable"/> with contents from the file.</returns>
		public static DataTable Read(this DataTable table, string path, bool hasHeaders = true, string sheetName = null)
		{
			string ext = Path.GetExtension(path)?.ToLower();
			if (ext == ".xlsx")
				return table.ReadXlsx(path, sheetName, hasHeaders);
			if (ext == ".csv" || ext == ".tsv")
				return table.ReadCsv(path, hasHeaders, true, ext == ".csv" ? "," : "\t");
			if (ext == ".xls")
				return table.ReadXls(path, sheetName, hasHeaders);
			throw new NotSupportedException("File extension type not supported: '" + ext + "'");
		}

		/// <summary>
		/// Reads a file into a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to fill.</param>
		/// <param name="path">The path of the file.</param>
		/// <param name="sheetName">The name of the worksheet to iterate if there are any.
		/// If this is <see langword="null"/> then the first sheet will be read.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>A <see cref="DataTable"/> with contents from the file.</returns>
		public static DataTable Read(this DataTable table, string path, string sheetName, bool hasHeaders = true)
		{
			return table.Read(path, hasHeaders, sheetName);
		}

		/// <summary>
		/// Reads a file into an <see cref="ICollection{T}"/>.
		/// </summary>
		/// <typeparam name="T">The Type of object to return.</typeparam>
		/// <param name="list">The <see cref="ICollection{T}"/> to fill.</param>
		/// <param name="path">The path of the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="sheetName">The name of the worksheet to iterate if there are any.
		/// If this is <see langword="null"/> then the first sheet will be read.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<T> Read<T>(this ICollection<T> list, string path, bool hasHeaders = true, string sheetName = null)
		{
			string ext = Path.GetExtension(path)?.ToLower();
			if (ext == ".xlsx")
				return list.ReadXlsx<T>(path, sheetName, hasHeaders);
			if (ext == ".csv" || ext == ".tsv")
				return list.ReadCsv(path, hasHeaders, true, ext == ".csv" ? "," : "\t");
			if (ext == ".xls")
				return list.ReadXls(path, sheetName, hasHeaders);
			throw new NotSupportedException("File extension type not supported: '" + ext + "'");
		}

		/// <summary>
		/// Reads a file into an <see cref="ICollection{T}"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="list">The <see cref="ICollection{T}"/> to fill.</param>
		/// <param name="path">The path of the file.</param>
		/// <param name="sheetName">The name of the worksheet to iterate if there are any.
		/// If this is <see langword="null"/> then the first sheet will be read.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<T> Read<T>(this ICollection<T> list, string path, string sheetName, bool hasHeaders = true)
		{
			return list.Read(path, hasHeaders, sheetName);
		}

		/// <summary>
		/// Reads a file into a <see cref="List{T}"/>.
		/// </summary>
		/// <typeparam name="T">The Type of object to return.</typeparam>
		/// <param name="path">The path of the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="sheetName">The name of the worksheet to iterate if there are any.
		/// If this is <see langword="null"/> then the first sheet will be read.</param>
		/// <returns>A List with contents from the file.</returns>
		public static List<T> Read<T>(string path, bool hasHeaders = true, string sheetName = null)
		{
			List<T> list = new List<T>();
			list.Read<T>(path, hasHeaders, sheetName);
			return list;
		}

		/// <summary>
		/// Reads a file into a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="path">The path of the file.</param>
		/// <param name="hasHeaders">Determines if the first line of the file should be skipped.</param>
		/// <returns>A <see cref="DataTable"/> with contents from the file, or <see langword="null"/> on error.</returns>
		public static DataTable Read(string path, bool hasHeaders = true)
		{
			return new DataTable().Read(path, hasHeaders);
		}

		/// <summary>
		/// Enumerates each row in a file.
		/// </summary>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="hasHeaders">Determines if the first line of the file should be skipped.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<string[]> Foreach(string path, bool hasHeaders = true)
		{
			string ext = Path.GetExtension(path)?.ToLower();
			if (ext == ".xlsx")
				return XlsxForeach(path, null, hasHeaders);
			if (ext == ".csv" || ext == ".tsv")
				return CsvForeach(path, hasHeaders, true, ext == ".csv" ? "," : "\t");
			if (ext == ".xls")
				return XlsForeach(path, null, hasHeaders);
			throw new NotSupportedException("File extension type not supported: '" + ext + "'");
		}

		/// <summary>
		/// Enumerates each row in a file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="constructor">A function that constructs an object from a <see langword="string"/>[].</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="sheetName">The name of the worksheet to iterate if there are any.
		/// If this is <see langword="null"/> then the first sheet will be read.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> Foreach<T>(string path, Func<string[], T> constructor, bool hasHeaders = true, string sheetName = null)
		{
			string ext = Path.GetExtension(path)?.ToLower();
			if (ext == ".xlsx")
				return XlsxForeach<T>(path, constructor, sheetName, hasHeaders);
			if (ext == ".csv" || ext == ".tsv")
				return CsvForeach<T>(path, constructor, hasHeaders, true, ext == ".csv" ? "," : "\t");
			if (ext == ".xls")
				return XlsForeach<T>(path, constructor, sheetName, hasHeaders);
			throw new NotSupportedException("File extension type not supported: '" + ext + "'");
		}

		/// <summary>
		/// Enumerates each row in a file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="sheetName">The name of the worksheet to iterate if there are any.
		/// If this is <see langword="null"/> then the first sheet will be read.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> Foreach<T>(string path, bool hasHeaders = true, string sheetName = null)
		{
			string ext = Path.GetExtension(path)?.ToLower();
			if (ext == ".xlsx")
				return XlsxForeach<T>(path, sheetName, hasHeaders);
			if (ext == ".csv" || ext == ".tsv")
				return CsvForeach<T>(path, hasHeaders, true, ext == ".csv" ? "," : "\t");
			if (ext == ".xls")
				return XlsForeach<T>(path, sheetName, hasHeaders);
			throw new NotSupportedException("File extension type not supported: '" + ext + "'");
		}

		/// <summary>
		/// Enumerates each row in a file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="sheetName">The name of the worksheet to iterate if there are any.
		/// If this is <see langword="null"/> then the first sheet will be read.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> Foreach<T>(string path, string sheetName, bool hasHeaders = true)
		{
			return Foreach<T>(path, hasHeaders, sheetName);
		}
		#endregion
		
		#region Delimited
		/// <summary>
		/// Enumerates each row in a delimited file.
		/// </summary>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="delim">The separator for the columns in the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>The Enumerable rows in the file.</returns>
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
		/// Enumerates each row in delimited text.
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/> of the delimited file.</param>
		/// <param name="delim">The separator for the columns in the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>The Enumerable rows in the file.</returns>
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
		/// Enumerates each row in delimited text.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="constructor">The constructor for the objects returned.</param>
		/// <param name="delim">The separator for the columns in the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>The Enumerable rows in the file.</returns>
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
		/// Enumerates each row in delimited text.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="delim">The separator for the columns in the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> FileForeach<T>(string path, char delim = ',', bool hasHeaders = true)
		{
			using (TextReader reader = Util.TextReader(new FileInfo(path))) {
				Func<IReadOnlyList<string>, T> constructor = hasHeaders
					? Converters.Converters.ListToObject<string, T>(reader.ReadLine().Split(delim))
					: Converters.Converters.ListToObject<string, T>();
				return FileForeach<T>(reader, constructor, delim, false);
			}
		}

		/// <summary>
		/// Enumerates each row in delimited text.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="reader">The TextReader of the delimited file.</param>
		/// <param name="constructor">The constructor for the objects returned.</param>
		/// <param name="delim">The separator for the columns in the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>The Enumerable rows in the file.</returns>
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
		/// Enumerates each row in delimited text.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="reader">The TextReader of the delimited file.</param>
		/// <param name="delim">The separator for the columns in the file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> FileForeach<T>(TextReader reader, char delim = ',', bool hasHeaders = true)
		{
			Func<IReadOnlyList<string>, T> constructor = hasHeaders
				? Converters.Converters.ListToObject<string, T>(reader.ReadLine().Split(delim))
				: Converters.Converters.ListToObject<string, T>();
			string line;
			while ((line = reader.ReadLine()) != null) {
				yield return constructor(line.Split(delim));
			}
		}

		/// <summary>
		/// Reads a delimited file into an <see cref="ICollection{T}"/> of <see langword="string"/>[].
		/// </summary>
		/// <param name="list">The <see cref="ICollection{T}"/> to modify.</param>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="delim">The character separating each column of each line.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<string[]> ReadFile(this ICollection<string[]> list, string path, char delim = ',', bool hasHeaders = true)
		{
			using (TextReader reader = Util.TextReader(new FileInfo(path))) {
				IEnumerable<string[]> lines = FileForeach(reader, delim, false);
				if (hasHeaders)
					lines = lines.Skip(1);
				foreach (string[] line in lines) {
					list.Add(line);
				}
			}
			return list;
		}

		/// <summary>
		/// Reads a delimited file into an <see cref="ICollection{T}"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="list">The <see cref="ICollection{T}"/> to modify.</param>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="delim">The character separating each column of each line.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<T> ReadFile<T>(this ICollection<T> list, string path, char delim = ',', bool hasHeaders = true)
		{
			IEnumerable<T> lines = FileForeach<T>(path, delim, hasHeaders);
			foreach (T line in lines) {
				list.Add(line);
			}
			return list;
		}

		/// <summary>
		/// Reads a delimited file into a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to modify.</param>
		/// <param name="path">The path of the delimited file.</param>
		/// <param name="delim">The character separating each column of each line.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>A <see cref="DataTable"/> with data from the file.</returns>
		public static DataTable ReadFile(this DataTable table, string path, char delim = ',', bool hasHeaders = true)
		{
			using (TextReader reader = Util.TextReader(new FileInfo(path))) {
				IEnumerable<string[]> lines = FileForeach(reader, delim, hasHeaders);
				// add columns
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
				// add rows
				foreach (string[] line in lines) {
					for (int i = table.Columns.Count; i < line.Length; i++) {
						table.Columns.Add(new string(' ', i + 1));
					}
					table.Rows.Add(line);
				}
				return table.Trim();
			}
		}
		#endregion Delimited


		#region CSV
		/// <summary>
		/// Writes an <see cref="IEnumerable{T}"/> to a CSV file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="list">The <see cref="IEnumerable{T}"/> to export.</param>
		/// <param name="path">The path of the CSV file.</param>
		/// <param name="printHeaders">Determines if the property names should be output as headers.</param>
		/// <param name="delim">The delimiter for the text.</param>
		public static void WriteCsv<T>(this IEnumerable<T> list, string path, bool printHeaders = true, string delim = ",")
		{
			using (TextWriter writer = new StreamWriter(path, false))
			using (CsvWriter csv = new CsvWriter(writer)) {
				csv.Configuration.ShouldQuote = (str, b) => true;
				csv.Configuration.Delimiter = delim;
				PropertyInfo[] props = typeof(T).GetProperties(DefaultBindingFlags);
				if (printHeaders) {
					foreach (string header in props.Select(prop => prop.Name)) {
						csv.WriteField(header);
					}
					csv.NextRecord();
				}
				foreach (T row in list) {
					for (int i = 0; i < props.Length; i++) {
						csv.WriteField(props[i].GetValue(row));
					}
					csv.NextRecord();
				}
			}
		}

		/// <summary>
		/// Writes a <see cref="DataTable"/> to a CSV file.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to export.</param>
		/// <param name="path">The path of the CSV file.</param>
		/// <param name="hasHeaders">Determines if the column names should be output.</param>
		/// <param name="delim">The delimiter for the text.</param>
		public static void WriteCsv(this DataTable table, string path, bool hasHeaders = true, string delim = ",")
		{
			using (StreamWriter sr = new StreamWriter(path, false))
			using (CsvWriter csv = new CsvWriter(sr)) {
				csv.Configuration.Delimiter = delim;
				csv.Configuration.ShouldQuote = (str, b) => true;
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
		}

		/// <summary>
		/// Reads a CSV file into an <see cref="ICollection{T}"/> of <see langword="string"/>[].
		/// </summary>
		/// <param name="list">The <see cref="ICollection{T}"/> to modify.</param>
		/// <param name="path">The path of the CSV file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<string[]> ReadCsv(this ICollection<string[]> list, string path, bool hasHeaders = true, string delim = ",")
		{
			foreach (string[] line in CsvForeach(path, hasHeaders, true, delim)) {
				list.Add(line);
			}
			return list;
		}

		/// <summary>
		/// Reads a CSV file into an <see cref="ICollection{T}"/> of <see langword="string"/>[].
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects to return.</typeparam>
		/// <param name="list">The <see cref="ICollection{T}"/> to modify.</param>
		/// <param name="path">The path of the CSV file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<T> ReadCsv<T>(this ICollection<T> list, string path, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
		{
			using (TextReader reader = Util.TextReader(path)) {
				return ReadCsv(list, reader, hasHeaders, ignoreBlankLines, delim);
			}
		}

		/// <summary>
		/// Reads a CSV file into an <see cref="ICollection{T}"/> of <see langword="string"/>[].
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects to return.</typeparam>
		/// <param name="list">The <see cref="ICollection{T}"/> to modify.</param>
		/// <param name="reader">The <see cref="TextReader"/> of the CSV file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<T> ReadCsv<T>(this ICollection<T> list, TextReader reader, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
		{
			using (CsvParser csv = new CsvParser(reader)) {
				csv.Configuration.TrimOptions = TrimOptions.None;
				csv.Configuration.IgnoreBlankLines = ignoreBlankLines;
				csv.Configuration.Delimiter = delim;
				string[] line = csv.Read();
				if (line != null) {
					Func<IReadOnlyList<string>, T> constructor = hasHeaders
						? Converters.Converters.ListToObject<string, T>(line)
						: Converters.Converters.ListToObject<string, T>();
					while ((line = csv.Read()) != null)
						list.Add(constructor(line));
				}
			}
			return list;
		}

		/// <summary>
		/// Reads a CSV file into an <see cref="ICollection{T}"/> of <see langword="string"/>[].
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects to return.</typeparam>
		/// <param name="list">The <see cref="ICollection{T}"/> to modify.</param>
		/// <param name="reader">The <see cref="TextReader"/> of the CSV file.</param>
		/// <param name="constructor">The constructor for the returned objects.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<T> ReadCsv<T>(this ICollection<T> list, TextReader reader, Func<string[], T> constructor, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
		{
			using (CsvParser csv = new CsvParser(reader)) {
				csv.Configuration.TrimOptions = TrimOptions.None;
				csv.Configuration.IgnoreBlankLines = ignoreBlankLines;
				csv.Configuration.Delimiter = delim;
				string[] line;
				if (hasHeaders)
					csv.Read();
				while ((line = csv.Read()) != null) {
					list.Add(constructor(line));
				}
			}
			return list;
		}

		/// <summary>
		/// Reads a CSV file into a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to modify.</param>
		/// <param name="path">The path of the CSV file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>A <see cref="DataTable"/> with data from the file.</returns>
		public static DataTable ReadCsv(this DataTable table, string path, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
		{
			IEnumerable<string[]> lines = CsvForeach(path, false, ignoreBlankLines, delim);
			if (lines.Any()) {
				string[] headers = lines.First();
				lines = lines.Skip(1);
				if (headers != null && table.Columns.Count == 0) {
					for (int i = 0; i < headers.Length; i++) {
						string columnName = headers[i].Trim();
						if (hasHeaders) {
							table.Columns.Add(columnName.Length == 0 ? new string(' ', i) : columnName);
						}
						else
							table.Columns.Add(new string(' ', i + 1));
					}
				}
				foreach (string[] line in lines) {
					for (int i = table.Columns.Count; i < line.Length; i++) {
						table.Columns.Add(new string(' ', i + 1));
					}
					table.Rows.Add(line);
				}
				table.Trim();
			}
			return table;
		}

		/// <summary>
		/// Enumerates each row in a CSV file.
		/// </summary>
		/// <param name="path">The path of the CSV file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<string[]> CsvForeach(string path, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
		{
			using (TextReader reader = Util.TextReader(new FileInfo(path))) {
				foreach (string[] line in CsvForeach(reader, hasHeaders, ignoreBlankLines, delim)) {
					yield return line;
				}
			}
		}

		/// <summary>
		/// Enumerates each row in a CSV file.
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/> of the CSV file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>The Enumerable rows in the file.</returns>
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
		/// Enumerates each row in a CSV file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects to return.</typeparam>
		/// <param name="path">The path of the CSV file.</param>
		/// <param name="constructor">The constructor for the returned objects.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> CsvForeach<T>(string path, Func<string[], T> constructor, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
		{
			using (TextReader reader = Util.TextReader(path)) {
				foreach (string[] strs in CsvForeach(reader, hasHeaders, ignoreBlankLines, delim)) {
					yield return constructor(strs);
				}
			}
		}

		/// <summary>
		/// Enumerates each row in a CSV file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects to return.</typeparam>
		/// <param name="path">The path of the CSV file.</param>
		/// <param name="constructor">The constructor for the returned objects.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> CsvForeach<T>(string path, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
		{
			using (TextReader reader = Util.TextReader(path)) {
				foreach (T obj in CsvForeach<T>(reader, hasHeaders, ignoreBlankLines, delim)) {
					yield return obj;
				}
			}
		}

		/// <summary>
		/// Enumerates each row in a CSV file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects to return.</typeparam>
		/// <param name="path">The path of the CSV file.</param>
		/// <param name="constructor">The constructor for the returned objects.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> CsvForeach<T>(TextReader reader, Func<string[], T> constructor, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
		{
			foreach (string[] strs in CsvForeach(reader, hasHeaders, ignoreBlankLines, delim)) {
				yield return constructor(strs);
			}
		}

		/// <summary>
		/// Enumerates each row in a CSV file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects to return.</typeparam>
		/// <param name="reader">The <see cref="TextReader"/> for the CSV file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <param name="ignoreBlankLines">Determines if blank lines should be skipped.</param>
		/// <param name="delim">The delimiter of the file.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> CsvForeach<T>(TextReader reader, bool hasHeaders = true, bool ignoreBlankLines = true, string delim = ",")
		{
			using (CsvParser csv = new CsvParser(reader)) {
				string[] line = csv.Read();
				if (line != null) {
					Func<IReadOnlyList<string>, T> constructor = hasHeaders
						? Converters.Converters.ListToObject<string, T>(line)
						: Converters.Converters.ListToObject<string, T>();
					csv.Configuration.TrimOptions = TrimOptions.None;
					csv.Configuration.IgnoreBlankLines = ignoreBlankLines;
					csv.Configuration.Delimiter = delim;
					while ((line = csv.Read()) != null) {
						yield return constructor(line);
					}
				}
			}
		}
		#endregion CSV

		#region XLS
		/// <summary>
		/// Enumerates each row in an XLS file.
		/// </summary>
		/// <param name="path">The path of the XLS file.</param>
		/// <param name="sheetName">The name of the worksheet to iterate.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<string[]> XlsForeach(string path, string sheetName = null, bool hasHeaders = true)
		{
			IEnumerable<string[]> func(OleDbCommand cmd, DataTable schema)
			{
				if (sheetName == null)
					sheetName = schema.Rows[0]["TABLE_NAME"].ToString();
				cmd.CommandText = "SELECT * FROM [" + sheetName + "]";
				OleDbDataReader reader = cmd.ExecuteReader();
				if (hasHeaders)
					reader.Read();
				while (reader.Read()) {
					int count = reader.VisibleFieldCount;
					string[] cellValues = new string[count];
					for (int i = 0; i < count; i++) {
						cellValues[i] = reader.GetString(i);
					}
					yield return cellValues;
				}
				reader.Close();
			}
			foreach (string[] line in GetXlsData(path, func)) {
				yield return line;
			}
		}

		/// <summary>
		/// Enumerates each row in an XLS file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects to return.</typeparam>
		/// <param name="path">The path of the XLS file.</param>
		/// <param name="sheetName">The name of the worksheet to iterate.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> XlsForeach<T>(string path, string sheetName = null, bool hasHeaders = true)
		{
			IEnumerable<string[]> lines = XlsForeach(path, sheetName, false);
			if (lines.Any()) {
				Func<IReadOnlyList<string>, T> constructor;
				if (hasHeaders) {
					constructor = Converters.Converters.ListToObject<string, T>(lines.First());
					lines = lines.Skip(1);
				}
				else
					constructor = Converters.Converters.ListToObject<string, T>();
				foreach (string[] line in lines) {
					yield return constructor(line);
				}
			}
		}

		/// <summary>
		/// Enumerates each row in an XLS file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects to return.</typeparam>
		/// <param name="path">The path of the XLS file.</param>
		/// <param name="constructor">The constructor for the returned objects.</param>
		/// <param name="sheetName">The name of the worksheet to iterate.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> XlsForeach<T>(string path, Func<string[], T> constructor, string sheetName = null, bool hasHeaders = true)
		{
			foreach (string[] line in XlsForeach(path, sheetName, hasHeaders)) {
				yield return constructor(line);
			}
		}

		/// <summary>
		/// Reads an XLS Excel file into an <see cref="ICollection{T}"/>.
		/// This assumes that the type T has public properties with identical names of the columns in the <see cref="DataTable"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to convert the <see cref="DataRow"/>s into.</typeparam>
		/// <param name="list">The <see cref="ICollection{T}"/> to append data to.</param>
		/// <param name="path">The path of the Excel file.</param>
		/// <param name="sheetName">The name of the worksheet to iterate.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<T> ReadXls<T>(this ICollection<T> list, string path, string sheetName = null, bool hasHeaders = true)
		{
			foreach (T obj in XlsForeach<T>(path, sheetName, hasHeaders)) {
				list.Add(obj);
			}
			return list;
		}

		/// <summary>
		/// Reads the first sheet of an XLS Excel file into a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to append data to.</param>
		/// <param name="path">The path of the Excel file.</param>
		/// <param name="sheetName">The name of the worksheet to iterate.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>A <see cref="DataTable"/> with data from the file.</returns>
		public static DataTable ReadXls(this DataTable table, string path, string sheetName = null, bool hasHeaders = true)
		{
			using (OleDbDataAdapter oda = new OleDbDataAdapter()) {
				IEnumerable<DataTable> func(OleDbCommand cmd, DataTable schema)
				{
					oda.SelectCommand = cmd;
					if (sheetName == null)
						sheetName = schema.Rows[0]["TABLE_NAME"].ToString();
					FillXlsDataTable(table, oda, sheetName, hasHeaders);
					yield return table;
				}
				return GetXlsData(path, func).First();
			}
		}

		/// <summary>
		/// Reads an XLS Excel file into a <see cref="DataSet"/> where each sheet is a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="dataset">The <see cref="DataSet"/> to append data to.</param>
		/// <param name="path">The path of the XLS Excel file.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		/// <returns>A <see cref="DataSet"/> with data from the file.</returns>
		public static DataSet ReadXls(this DataSet dataset, string path, bool hasHeaders = true)
		{
			using (OleDbDataAdapter oda = new OleDbDataAdapter()) {
				IEnumerable<DataTable> func(OleDbCommand cmd, DataTable schema)
				{
					oda.SelectCommand = cmd;
					for (int i = 0; i < schema.Rows.Count; i++) {
						string sheetName = schema.Rows[i]["TABLE_NAME"].ToString();
						DataTable table = new DataTable();
						FillXlsDataTable(table, oda, sheetName, hasHeaders);
						yield return table;
					}
				}
				foreach (DataTable table in GetXlsData(path, func))
					dataset.Tables.Add(table);
				return dataset;
			}
		}

		/// <summary>
		/// Opens an XLS file and performs an action on it.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="path">The path of the XLS Excel file.</param>
		/// <param name="sheetName">The name of the worksheet to read.</param>
		/// <param name="action">The action to perform on the file.
		/// The first parameter is a command object which allows reading of the file.
		/// The second parameter is the schema of the file which allows the user to get the names of the worksheets.</param>
		/// <returns>The return value of the action.</returns>
		private static IEnumerable<T> GetXlsData<T>(string path, Func<OleDbCommand, DataTable, IEnumerable<T>> action)
		{
			// parses as the correct type
			////string connStr = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + "; Jet OLEDB:Engine Type = 5; Extended Properties =\"Excel 8.0;\"";
			// handles column data of multiple types better
			string[] connStrs = new string[]
			{
				"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path
					+ @";Extended Properties=""Excel 8.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text""",
				"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path
					+ @";Extended Properties=""Excel 12.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text"""
			};
			for (int i = 0; i < connStrs.Length; i++) {
				string connStr = connStrs[i];
				try {
					using (OleDbConnection conn = new OleDbConnection(connStr))
					using (OleDbCommand cmd = new OleDbCommand()) {
						cmd.Connection = conn;
						conn.Open();
						DataTable schema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
						if (schema.Rows.Count == 0)
							throw new InvalidDataException(path);
						List<T> results = new List<T>();
						foreach (T obj in action(cmd, schema)) {
							results.Add(obj);
						}
						return results;
					}
				}
				catch {
					if (i == connStrs.Length - 1) {
						throw;
					}
				}
			}
			throw new InvalidOperationException("Unreachable");
		}

		/// <summary>
		/// Fills a <see cref="DataTable"/> with data from an XLS file based on the worksheet name.
		/// </summary>
		/// <param name="table">The DataTable to append data to.</param>
		/// <param name="oda">The <see cref="OleDbDataAdapter"/> that reads data from the file.</param>
		/// <param name="sheetName">The name of the worksheet to read.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the columns are named based on the first line.</param>
		private static void FillXlsDataTable(DataTable table, OleDbDataAdapter oda, string sheetName, bool hasHeaders)
		{
			oda.SelectCommand.CommandText = "SELECT * FROM [" + sheetName + "]";
			oda.Fill(table);
			table.TableName = sheetName.Substring(1, sheetName.Length - 3);
			if (hasHeaders && table.Rows.Count > 0) {
				DataRow row = table.Rows[0];
				for (int c = 0; c < table.Columns.Count; c++) {
					string colName = row[c].ToString();
					if (string.IsNullOrWhiteSpace(colName)) {
						table.Columns[c].ColumnName = new string(' ', c + 1);
					}
					else
						table.Columns[c].ColumnName = row[c].ToString();
				}
				table.Rows.RemoveAt(0);
			}
			table.Trim();
			table.TrimColumns();
		}
		#endregion

		#region XLSX
		/// <summary>
		/// Enumerates each row in an XLSX file.
		/// </summary>
		/// <param name="path">The path of the XLSX file.</param>
		/// <param name="sheetName">The name of the sheet in the file to iterate.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="includeHeaders">Determines if the file has headers should be included.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<string[]> XlsxForeach(string path, string sheetName = null, bool includeHeaders = true)
		{
			if (!File.Exists(path))
				throw new IOException("File does not exist: " + path);
			using (Spreadsheet ss = new Spreadsheet(path)) {
				Worksheet worksheet = sheetName == null ? ss[0] : ss[sheetName];
				IEnumerable<string[]> rows = worksheet.AsEnumerable();
				if (includeHeaders)
					rows = rows.Skip(1);
				foreach (string[] row in rows) {
					yield return row;
				}
			}
		}

		/// <summary>
		/// Enumerates each row in an XLSX file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="path">The path of the XLSX file.</param>
		/// <param name="sheetName">The name of the sheet in the file to iterate.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> XlsxForeach<T>(string path, string sheetName = null, bool hasHeaders = true)
		{
			if (!File.Exists(path))
				throw new IOException("File does not exist: " + path);
			using (Spreadsheet ss = new Spreadsheet(path)) {
				Worksheet worksheet = sheetName == null ? ss[0] : ss[sheetName];
				foreach (T obj in worksheet.AsEnumerable<T>(hasHeaders)) {
					yield return obj;
				}
			}
		}

		/// <summary>
		/// Enumerates each row in an XLSX file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object to return.</typeparam>
		/// <param name="path">The path of the XLSX file.</param>
		/// <param name="constructor">The constructor for the objects returned.</param>
		/// <param name="sheetName">The name of the sheet in the file to iterate.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="hasHeaders">Determines if the file has headers.
		/// If this is true then the first row will be ignored.</param>
		/// <returns>The Enumerable rows in the file.</returns>
		public static IEnumerable<T> XlsxForeach<T>(string path, Func<string[], T> constructor, string sheetName = null, bool hasHeaders = true)
		{
			if (!File.Exists(path))
				throw new IOException("File does not exist: " + path);
			using (Spreadsheet ss = new Spreadsheet(path)) {
				Worksheet worksheet = sheetName == null ? ss[0] : ss[sheetName];
				IEnumerable<string[]> rows = worksheet.AsEnumerable();
				if (hasHeaders)
					rows = rows.Skip(1);
				foreach (string[] row in rows) {
					yield return constructor(row);
				}
			}
		}

		/// <summary>
		/// Reads an Xlsx Excel file into an <see cref="ICollection{T}"/>.
		/// </summary>
		/// <param name="list">The <see cref="ICollection{T}"/> to store the data in.</param>
		/// <param name="path">Name of file to be written.</param>
		/// <param name="sheetName">The name of the sheet in the file to read.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="includeHeaders">Determines if the columns should be included.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<string[]> ReadXlsx(this ICollection<string[]> list, string path, string sheetName = null, bool includeHeaders = true)
		{
			IEnumerable<string[]> lines = XlsxForeach(path, sheetName, includeHeaders);
			foreach (string[] line in lines) {
				list.Add(line);
			}
			return list;
		}

		/// <summary>
		/// Reads an Xlsx Excel file into an <see cref="ICollection{T}"/>.
		/// </summary>
		/// <param name="list">The <see cref="ICollection{T}"/> to store the data in.</param>
		/// <param name="path">Name of file to be written.</param>
		/// <param name="sheetName">The name of the sheet in the file to read.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="includeHeaders">Determines if the columns should be included.</param>
		/// <returns>An <see cref="ICollection{T}"/> with data from the file.</returns>
		public static ICollection<T> ReadXlsx<T>(this ICollection<T> list, string path, string sheetName = null, bool includeHeaders = true)
		{
			if (!File.Exists(path))
				throw new IOException("File does not exist: " + path);
			using (Spreadsheet ss = new Spreadsheet(path)) {
				if (!ss.IsOpen)
					throw new IOException(path);
				Worksheet worksheet = sheetName == null ? ss[0] : ss[sheetName];
				return worksheet.ToList(list, includeHeaders);
			}
		}

		/// <summary>
		/// Reads an Xlsx Excel file into a <see cref="DataSet"/>.
		/// </summary>
		/// <param name="dataset">The <see cref="DataSet"/> to store the data in.</param>
		/// <param name="path">Name of file to be written.</param>
		/// <param name="hasHeaders">Determines if the columns should be written.</param>
		/// <returns>A <see cref="DataSet"/> with data from the file.</returns>
		public static DataSet ReadXlsx(this DataSet dataset, string path, bool hasHeaders = true)
		{
			if (!File.Exists(path))
				throw new IOException("File does not exist: " + path);
			using (Spreadsheet ss = new Spreadsheet(path)) {
				if (!ss.IsOpen)
					throw new IOException(path);
				return ss.ToDataSet(dataset, hasHeaders);
			}
		}

		/// <summary>
		/// Reads the first sheet of an Xlsx Excel file into a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to store the data in.</param>
		/// <param name="path">Name of file to be written.</param>
		/// <param name="sheetName">The name of the worksheet to iterate.
		/// If this is <see langword="null"/> then the first sheet will be iterated.</param>
		/// <param name="hasHeaders">Determines if the columns should be written.</param>
		/// <returns>A <see cref="DataTable"/> with data from the file.</returns>
		public static DataTable ReadXlsx(this DataTable table, string path, string sheetName = null, bool hasHeaders = true)
		{
			if (!File.Exists(path))
				throw new IOException("File does not exist: " + path);
			using (Spreadsheet ss = new Spreadsheet(path)) {
				if (!ss.IsOpen)
					throw new IOException(path);
				Worksheet worksheet = sheetName == null ? ss[0] : ss[sheetName];
				return worksheet.ToDataTable(table, hasHeaders);
			}
		}

		/// <summary>
		/// Writes a <see cref="DataSet"/> to a file.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of object in the list.</typeparam>
		/// <param name="list">The <see cref="IEnumerable{T}"/> to write to the Excel file.</param>
		/// <param name="path">Name of file to be written.</param>
		/// <param name="hasHeaders">Determines if the column headers should be written.</param>
		/// <param name="autoFilter">Determines if auto-filtering will be enabled. Filters allow sorting and filtering by column.</param>
		/// <param name="autoFormat">Determines if auto-formatting will be enabled.
		/// Strings values will be converted into basic types in order to remove the green arrow in Excel.</param>
		public static void WriteXlsx<T>(this IEnumerable<T> list, string path, bool hasHeaders = true, bool autoFilter = true, bool autoFormat = false)
		{
			WriteXlsx((ss) => ss[0].Load(list), path, hasHeaders, autoFilter, autoFormat);
		}

		/// <summary>
		/// Writes a <see cref="DataTable"/> to a file.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> containing the data to be written to the Excel file.</param>
		/// <param name="path">Name of file to be written.</param>
		/// <param name="hasHeaders">Determines if the column headers should be written.</param>
		/// <param name="autoFilter">Determines if auto-filtering will be enabled. Filters allow sorting and filtering by column.</param>
		/// <param name="autoFormat">Determines if auto-formatting will be enabled.
		/// Strings values will be converted into basic types in order to remove the green arrow in Excel.</param>
		public static void WriteXlsx(this DataTable table, string path, bool hasHeaders = true, bool autoFilter = true, bool autoFormat = false)
		{
			WriteXlsx((ss) => ss[0].Load(table, hasHeaders), path, hasHeaders, autoFilter, autoFormat);
		}

		/// <summary>
		/// Writes a <see cref="DataSet"/> to a file.
		/// </summary>
		/// <param name="dataset">The <see cref="DataSet"/> containing the data to be written to the Excel.</param>
		/// <param name="path">Name of file to be written.</param>
		/// <param name="hasHeaders">Determines if the column headers should be written.</param>
		/// <param name="autoFilter">Determines if auto-filtering will be enabled. Filters allow sorting and filtering by column.</param>
		/// <param name="autoFormat">Determines if auto-formatting will be enabled.
		/// Strings values will be converted into basic types in order to remove the green arrow in Excel.</param>
		public static void WriteXlsx(this DataSet dataset, string path, bool hasHeaders = true, bool autoFilter = true, bool autoFormat = false)
		{
			WriteXlsx((ss) => ss.Load(dataset), path, hasHeaders, autoFilter, autoFormat);
		}

		/// <summary>
		/// Performs an action on a <see cref="Spreadsheet"/>.
		/// </summary>
		/// <param name="action">An <see cref="Action{Spreadsheet}"/> that loads the data into the spreadsheet.</param>
		/// <param name="path">Name of file to be written.</param>
		/// <param name="hasHeaders">Determines if the column headers should be written.</param>
		/// <param name="autoFilter">Determines if auto-filtering will be enabled. Filters allow sorting and filtering by column.</param>
		/// <param name="autoFormat">Determines if auto-formatting will be enabled.
		/// Strings values will be converted into basic types in order to remove the green arrow in Excel.</param>
		private static void WriteXlsx(Action<Spreadsheet> action, string path, bool hasHeaders, bool autoFilter, bool autoFormat)
		{
			FileInfo fi = new FileInfo(path);
			if (fi.Exists)
				fi.Delete();
			using (Spreadsheet ss = new Spreadsheet(path)) {
				if (!ss.IsOpen)
					throw new IOException(path);
				action(ss);
				if (autoFilter)
					ss.AutoFilter = true;
				if (autoFormat)
					ss.AutoFormat();
				if (hasHeaders)
					ss.FreezePanes();
				ss.AutoFit();
				ss.Save();
			}
		}
		#endregion

		#region File/Directory/Path
		/// <summary>
		/// Moves a file from the input path to the output path.
		/// </summary>
		/// <param name="inpath">The path of the input file.</param>
		/// <param name="outpath">The path to move the file to.</param>
		/// <param name="overwrite">Determines if the file should be overwritten if it already exists.</param>
		public static void Move(string inpath, string outpath, bool overwrite = true)
		{
			FileInfo fi = new FileInfo(inpath);
			if (!fi.Exists)
				throw new FileNotFoundException(inpath);
			FileInfo fo = new FileInfo(outpath);
			if (fo.FullName == fi.FullName || (!overwrite && fo.Exists))
				return;
			File.Move(inpath, outpath);
		}

		/// <summary>
		/// Determines if a file is available for reading/writing.
		/// </summary>
		/// <param name="path">The path of the file to check.</param>
		/// <param name="access">The <see cref="FileAccess"/> to test.</param>
		/// <returns>True if the file is available with the given <see cref="FileAccess"/>. False otherwise.</returns>
		public static bool IsAvailable(string path, FileAccess access = FileAccess.Read)
		{
			return IsAvailable(new FileInfo(path), access);
		}

		/// <summary>
		/// Determines if a file is available for reading/writing.
		/// </summary>
		/// <param name="path">The path of the file to check.</param>
		/// <param name="access">The <see cref="FileAccess"/> to test.</param>
		/// <returns>True if the file is available with the given <see cref="FileAccess"/>. False otherwise.</returns>
		public static bool IsAvailable(FileInfo path, FileAccess access = FileAccess.Read)
		{
			try {
				using (FileStream stream = path.Open(FileMode.Open, access, FileShare.None)) {
					// do nothing
				}
			}
			catch (IOException) {
				return false; // not accessible
			}
			return true;
		}

		/// <summary>
		/// Finds all <see cref="FileInfo"/> objects matching a search pattern.
		/// </summary>
		/// <param name="path">The path and search pattern for the files.
		/// This can include wildcards (* ?) and environment variables (%var%).</param>
		/// <param name="recursive">Determines if subdirectories should be searched.</param>
		/// <param name="expandEnvVars">Determines if environment variables should be expanded (%var)%.</param>
		/// <returns>The <see cref="FileInfo"/> objects that match the search pattern.</returns>
		public static FileInfo[] GetFiles(string path, bool recursive = false, bool expandEnvVars = true)
		{
			string envExpanded = expandEnvVars ? Environment.ExpandEnvironmentVariables(path) : path;
			string directory = Path.GetDirectoryName(envExpanded);
			string pattern = Path.GetFileName(envExpanded);
			return GetFiles(directory, pattern, recursive);
		}

		/// <summary>
		/// Finds all <see cref="FileInfo"/> objects matching a given search pattern within a given directory.
		/// </summary>
		/// <param name="directory">The directory to search.</param>
		/// <param name="pattern">The search pattern for the files. This can include wildcards (* ?).</param>
		/// <param name="recursive">Determines if subdirectories should be searched with the same search pattern.</param>
		/// <returns>The <see cref="FileInfo"/> objects that match the search pattern.</returns>
		public static FileInfo[] GetFiles(string directory, string pattern, bool recursive = false)
		{
			SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			string dir = directory.Length == 0 ? Directory.GetCurrentDirectory() : directory;
			return new DirectoryInfo(dir).GetFiles(pattern, option);
		}

		/// <summary>
		/// Finds all <see cref="DirectoryInfo"/> objects matching a search pattern.
		/// </summary>
		/// <param name="path">The path and search pattern for the directories.
		/// This can include wildcards (* ?) and environment variables (%var%).</param>
		/// <param name="recursive">Determines if subdirectories should be searched.</param>
		/// <param name="expandEnvVars">Determines if environment variables should be expanded (%var)%.</param>
		/// <returns>The <see cref="DirectoryInfo"/> objects that match the search pattern.</returns>
		public static DirectoryInfo[] GetDirectories(string path, bool recursive = false, bool expandEnvVars = true)
		{
			string envExpanded = expandEnvVars ? Environment.ExpandEnvironmentVariables(path) : path;
			string directory = Path.GetDirectoryName(envExpanded);
			string pattern = Path.GetFileName(envExpanded);
			return GetDirectories(directory, pattern, recursive);
		}

		/// <summary>
		/// Finds all <see cref="DirectoryInfo"/> objects matching a given search pattern within a given directory.
		/// </summary>
		/// <param name="directory">The directory to search.</param>
		/// <param name="pattern">The search pattern for the files. This can include wildcards (* ?).</param>
		/// <param name="recursive">Determines if subdirectories should be searched with the same search pattern.</param>
		/// <returns>The <see cref="DirectoryInfo"/> objects that match the search pattern.</returns>
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