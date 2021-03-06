﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OfficeOpenXml;

namespace Utilities.Excel
{
	/// <summary>
	/// Wrapper for the EPPlus implementation of <see cref="ExcelWorksheet"/>.
	/// </summary>
	public class Worksheet
	{
		/// <summary>
		/// The number of rows to check in a <see cref="DateTime"/> column before determining whether to remove the Time component.
		/// </summary>
		private const int MaxDateOnlyRows = 25;

		/// <summary>
		/// The currency symbols to accept when reading a potential decimal value of a cell.
		/// </summary>
		private const string CurrencySymbols = "$£¥€¢";
		////private const string currencySymbols = "$¥₤€£฿₿₵¢₡₫₲₱₽₮₩₸₳ℳ₹؋₼﷼₪₭₴";

		/// <summary>
		/// The <see cref="ExcelWorksheet"/> implementation of the <see cref="Worksheet"/>.
		/// </summary>
		public ExcelWorksheet Data { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Worksheet"/> class.
		/// </summary>
		/// <param name="worksheet">The EPPlus <see cref="ExcelWorksheet"/> to represent.</param>
		public Worksheet(ExcelWorksheet worksheet)
		{
			Data = worksheet;
		}

		/// <summary>
		/// Freezes the top row (locks it for visibility).
		/// </summary>
		public void FreezePanes()
		{
			Data.View.FreezePanes(2, 1);
		}

		/// <summary>
		/// Freezes the panes (locks them for visibility).
		/// </summary>
		/// <param name="rows">The rows to freeze (0-Rows).</param>
		/// <param name="cols">The number of columns to freeze (0-Columns).</param>
		public void FreezePanes(int rows, int cols)
		{
			Data.View.FreezePanes(rows + 1, cols + 1);
		}

		/// <summary>
		/// Moves the <see cref="Worksheet"/> to the start of the <see cref="Spreadsheet"/>.
		/// </summary>
		public void MoveToStart()
		{
			Data.Workbook.Worksheets.MoveToStart(Data.Name);
		}

		/// <summary>
		/// Moves the <see cref="Worksheet"/> to the end of the <see cref="Spreadsheet"/>.
		/// </summary>
		public void MoveToEnd()
		{
			Data.Workbook.Worksheets.MoveToEnd(Data.Name);
		}

		/// <summary>
		/// Moves the <see cref="Worksheet"/> after the one with the given name.
		/// </summary>
		/// <param name="worksheetName">The name of the other <see cref="Worksheet"/>.</param>
		public void MoveAfter(string worksheetName)
		{
			Data.Workbook.Worksheets.MoveAfter(Data.Name, worksheetName);
		}

		/// <summary>
		/// Moves the <see cref="Worksheet"/> after the one with the given index.
		/// </summary>
		/// <param name="worksheetIndex">The index of the other <see cref="Worksheet"/>.</param>
		public void MoveAfter(int worksheetIndex)
		{
			Data.Workbook.Worksheets.MoveAfter(Data.Index, worksheetIndex);
		}

		/// <summary>
		/// Moves the <see cref="Worksheet"/> before the one with the given name.
		/// </summary>
		/// <param name="worksheetName">The name of the other <see cref="Worksheet"/>.</param>
		public void MoveBefore(string worksheetName)
		{
			Data.Workbook.Worksheets.MoveBefore(Data.Name, worksheetName);
		}

		/// <summary>
		/// Moves the <see cref="Worksheet"/> before the one with the given index.
		/// </summary>
		/// <param name="worksheetIndex">The index of the other <see cref="Worksheet"/>.</param>
		public void MoveBefore(int worksheetIndex)
		{
			Data.Workbook.Worksheets.MoveBefore(Data.Index, worksheetIndex);
		}

		/// <summary>
		/// Loads a <see cref="IEnumerable{T}"/> into a <see cref="Worksheet"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of objects in the <see cref="IEnumerable{T}"/>.</typeparam>
		/// <param name="list">The <see cref="IEnumerable{T}"/> to load.</param>
		/// <param name="printHeaders">Determines if the property names of the <see cref="Type"/> should be written to the first row.</param>
		public void Load<T>(IEnumerable<T> list, bool printHeaders = true)
		{
			Data.Cells.LoadFromCollection<T>(list, printHeaders);
			FixColumnTypes(Util.GetPropertyTypes(typeof(T)));
		}

		/// <summary>
		/// Loads a <see cref="DataTable"/> into a <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to load.</param>
		/// <param name="printHeaders">Determines if the column names of the <see cref="DataTable"/> should be written to the first row.</param>
		public void Load(DataTable table, bool printHeaders = true)
		{
			if (table.Rows.Count > 0) {
				Data.Cells.LoadFromDataTable(table, printHeaders);
				FixColumnTypes(table.Columns.Cast<DataColumn>().AsEnumerable().Select(col => col.DataType));
			}
		}

		/// <summary>
		/// Fixes the data in <see cref="DateTime"/> and <see cref="TimeSpan"/> columns.
		/// </summary>
		/// <param name="types">The <see cref="Type"/> of each column.</param>
		private void FixColumnTypes(IEnumerable<Type> types)
		{
			int col = 1;
			foreach (Type ty in types) {
				if (ty == typeof(DateTime) || ty == typeof(DateTime?))
					FixDateColumn(col);
				else if (ty == typeof(TimeSpan) || ty == typeof(TimeSpan?)) {
					Data.Column(col).Style.Numberformat.Format = "h:mm:ss";
					Data.Cells[1, col, Data.Dimension.Rows, col].Style.Numberformat.Format = "h:mm:ss";
				}
				col++;
			}
		}

		/// <summary>
		/// Fixes the data in a <see cref="DateTime"/> column.
		/// </summary>
		/// <param name="columnIndex">The index of the <see cref="DateTime"/> column.</param>
		private void FixDateColumn(int columnIndex)
		{
			ExcelColumn column = Data.Column(columnIndex);
			int rowCheck = Math.Min(Data.Dimension.Rows, MaxDateOnlyRows);
			for (int row = Data.Dimension.Rows > 1 ? 2 : 1; row < rowCheck; row++) {
				DateTime datetime = Data.Cells[row, columnIndex].GetValue<DateTime>();
				if (datetime.TimeOfDay != TimeSpan.Zero) {
					column.Style.Numberformat.Format = "M/d/yyyy h:mm:ss AM/PM";
					Data.Cells[1, columnIndex, Data.Dimension.Rows + 1, columnIndex].Style.Numberformat.Format = "M/d/yyyy h:mm:ss AM/PM";
					return;
				}
			}
			column.Style.Numberformat.Format = "M/d/yyyy";
			////column.StyleID = 14; //Short Date
		}

		/// <summary>
		/// Loads an <see cref="IDataReader"/> into the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="reader">The <see cref="IDataReader"/> to load.</param>
		/// <param name="printHeaders">Determines if the first line should contain headers.</param>
		public void Load(IDataReader reader, bool printHeaders = true)
		{
			Data.Cells.LoadFromDataReader(reader, printHeaders);
			List<Type> types = new List<Type>();
			for (int i = 0; i < reader.FieldCount; i++) {
				types.Add(reader.GetFieldType(i));
			}
			FixColumnTypes(types);
		}

		/// <summary>
		/// Loads an <see cref="IEnumerable{T}"/> into a <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="list">The <see cref="IEnumerable{T}"/> to load.</param>
		public void Load(IEnumerable<object[]> list)
		{
			Data.Cells.LoadFromArrays(list);
			if (!list.Any())
				return;
			List<Type> types = new List<Type>();
			object[] first = list.First();
			for (int i = 0; i < first.Length; i++) {
				types.Add(first[i]?.GetType() ?? typeof(string));
			}
			FixColumnTypes(types);
		}

		/// <summary>
		/// Loads comma-separated text into a <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="csvText">The comma-separated text to load.</param>
		public void Load(string csvText)
		{
			Data.Cells.LoadFromText(csvText);
		}

		/// <summary>
		/// Deletes the <see cref="Worksheet"/>.
		/// </summary>
		public void Delete()
		{
			Data.Workbook.Worksheets.Delete(Data);
			Data = null;
		}

		/// <summary>
		/// Clears the <see cref="Worksheet"/>.
		/// </summary>
		public void Clear()
		{
			Data.Cells.Clear();
		}

		/// <summary>
		/// Gets a cell from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="row">The row index of the cell (between 1 to <see cref="Rows"/>).</param>
		/// <param name="col">The column index of the cell (between 1 to <see cref="Columns"/>).</param>
		/// <returns>The cell.</returns>
		public ExcelRange this[int row, int col] => Data.Cells[row, col];

		/// <summary>
		/// Returns an <see cref="ExcelRange"/> from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="address">The address (A1) or address range (A1:Z5) of the cells.</param>
		/// <returns>The <see cref="ExcelRange"/>.</returns>
		public ExcelRange this[string address] => Data.Cells[address];

		/// <summary>
		/// Gets an <see cref="ExcelRange"/> from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="rowA">The starting row index of the cell range (between 1 to <see cref="Rows"/>).</param>
		/// <param name="colA">The starting column index of the cell range (between 1 to <see cref="Columns"/>).</param>
		/// <param name="rowB">The last row index of the cell range (between 1 to <see cref="Rows"/>).</param>
		/// <param name="colB">The last column index of the cell range (between 1 to <see cref="Columns"/>).</param>
		/// <returns>The <see cref="ExcelRange"/>.</returns>
		public ExcelRange this[int rowA, int colA, int rowB, int colB] => Data.Cells[rowA, colA, rowB, colB];

		/// <summary>
		/// Returns the value at the given address.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the cell.</typeparam>
		/// <param name="address">The address of the cell (e.g. A1).</param>
		/// <returns>The value at the given address.</returns>
		public T Cell<T>(string address)
		{
			return Data.Cells[address].GetValue<T>();
		}

		/// <summary>
		/// Returns the value at the given address.
		/// </summary>
		/// <param name="address">The address of the cell (e.g. A1).</param>
		/// <returns>The value at the given address.</returns>
		public object Cell(string address)
		{
			return Data.Cells[address].Value;
		}

		/// <summary>
		/// Returns the value at the given row and column.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of the cell.</typeparam>
		/// <param name="row">The row index of the cell (between 1 to <see cref="Rows"/>).</param>
		/// <param name="col">The column index of the cell (between 1 to <see cref="Columns"/>).</param>
		/// <returns>The value at the given row and column.</returns>
		public T Cell<T>(int row, int col)
		{
			return Data.Cells[row, col].GetValue<T>();
		}

		/// <summary>
		/// Returns the value at the given row and column.
		/// </summary>
		/// <param name="row">The row index of the cell (between 1 to <see cref="Rows"/>).</param>
		/// <param name="col">The column index of the cell (between 1 to <see cref="Columns"/>).</param>
		/// <returns>The value at the given row and column.</returns>
		public object Cell(int row, int col)
		{
			return Data.Cells[row, col].Value;
		}

		/// <summary>
		/// Returns the values at the given addresse range.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to cast the cell values to.</typeparam>
		/// <param name="addresses">The address address range (A1:Z5) of the cells.</param>
		/// <returns>The values at the given address range.</returns>
		public T[,] Cells<T>(string addresses)
		{
			return Data.Cells[addresses].GetValue<T[,]>();
		}

		/// <summary>
		/// Returns the values at the given addresse range.
		/// </summary>
		/// <param name="addresses">The address address range (A1:Z5) of the cells.</param>
		/// <returns>The values at the given address range.</returns>
		public object[,] Cells(string addresses)
		{
			return Data.Cells[addresses].GetValue<object[,]>();
		}

		/// <summary>
		/// Returns the values at the given addresse range.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to cast the cell values to.</typeparam>
		/// <param name="rowA">The starting row index of the cell range (between 1 to <see cref="Rows"/>).</param>
		/// <param name="colA">The starting column index of the cell range (between 1 to <see cref="Columns"/>).</param>
		/// <param name="rowB">The last row index of the cell range (between 1 to <see cref="Rows"/>).</param>
		/// <param name="colB">The last column index of the cell range (between 1 to <see cref="Columns"/>).</param>
		/// <returns>The values at the given address range.</returns>
		public T[,] Cells<T>(int rowA, int colA, int rowB, int colB)
		{
			return Data.Cells[rowA, colB, rowB, colB].GetValue<T[,]>();
		}

		/// <summary>
		/// Returns the values at the given addresse range.
		/// </summary>
		/// <param name="rowA">The starting row index of the cell range (between 1 to <see cref="Rows"/>).</param>
		/// <param name="colA">The starting column index of the cell range (between 1 to <see cref="Columns"/>).</param>
		/// <param name="rowB">The last row index of the cell range (between 1 to <see cref="Rows"/>).</param>
		/// <param name="colB">The last column index of the cell range (between 1 to <see cref="Columns"/>).</param>
		/// <returns>The values at the given address range.</returns>
		public object[,] Cells(int rowA, int colA, int rowB, int colB)
		{
			return Data.Cells[rowA, colB, rowB, colB].GetValue<object[,]>();
		}

		/// <summary>
		/// Returns the <see cref="ExcelRow"/> at the given index.
		/// </summary>
		/// <param name="index">The index of the <see cref="ExcelRow"/> (between 1 and <see cref="Rows"/>).</param>
		/// <returns>The <see cref="ExcelRow"/> at the given index.</returns>
		public ExcelRow Row(int index)
		{
			return Data.Row(index);
		}

		/// <summary>
		/// Returns the <see cref="ExcelColumn"/> at the given index.
		/// </summary>
		/// <param name="index">The index of the <see cref="ExcelColumn"/> (between 1 and <see cref="Columns"/>).</param>
		/// <returns>The <see cref="ExcelColumn"/> at the given index.</returns>
		public ExcelColumn Column(int index)
		{
			return Data.Column(index);
		}

		/// <summary>
		/// Returns the index of the <see cref="ExcelColumn"/> with the given header, or -1 if it doesn't exist.
		/// </summary>
		/// <param name="colName">The column header to find.</param>
		/// <returns>The index of the <see cref="ExcelColumn"/> the given header, or -1 if it doesn't exist.</returns>
		public int Column(string colName)
		{
			for (int col = 1; col <= Data.Dimension.Columns; col++) {
				if (Data.Cells[1, col].GetValue<string>() == colName)
					return col;
			}
			return -1;
		}

		/// <summary>
		/// The index of the <see cref="Worksheet"/>.
		/// </summary>
		public int Index => Data.Index;

		/// <summary>
		/// The name of the <see cref="Worksheet"/>.
		/// </summary>
		public string Name {
			get => Data.Name;
			set => Data.Name = value;
		}

		/// <summary>
		/// The number of rows in the <see cref="Worksheet"/>.
		/// </summary>
		public int Rows => Data.Dimension.Rows;

		/// <summary>
		/// The number of columns in the <see cref="Worksheet"/>.
		/// </summary>
		public int Columns => Data.Dimension.Columns;

		/// <summary>
		/// Removes a column from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="index">The index of the <see cref="ExcelColumn"/> to remove.</param>
		public void RemoveColumn(int index)
		{
			Data.DeleteColumn(index);
		}

		/// <summary>
		/// Removes an <see cref="ExcelColumn"/> from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="name">The name of the <see cref="ExcelColumn"/> to remove.</param>
		public void RemoveColumn(string name)
		{
			for (int col = 1; col <= Data.Dimension.Columns; col++) {
				if (Data.Cells[1, col].Value.ToString() == name) {
					Data.DeleteColumn(col);
					return;
				}
			}
		}

		/// <summary>
		/// Removes an <see cref="ExcelRow"/> from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="index">The index of the <see cref="ExcelRow"/> to remove.</param>
		public void RemoveRow(int index)
		{
			Data.DeleteRow(index);
		}

		/// <summary>
		/// Resizes the columns to fit the data. Cells with wrapped text and formulas are not counted for this.
		/// </summary>
		/// <param name="minimumWidth">The minimum width of all columns.</param>
		public void AutoFit(double minimumWidth = 0, double? maximumWidth = null)
		{
			int cols = Data.Dimension?.Columns ?? 0;
			for (int col = 1; col <= cols; col++) {
				if (maximumWidth == null) {
					Data.Column(col).AutoFit(minimumWidth);
				}
				else {
					Data.Column(col).AutoFit(minimumWidth, (double) maximumWidth);
				}
			}
			Data.Cells.AutoFitColumns(minimumWidth);
		}

		/// <summary>
		/// The last <see cref="ExcelCellAddress"/> in the <see cref="Worksheet"/>.
		/// </summary>
		public ExcelCellAddress End => Data.Dimension.End;

		/// <summary>
		/// The first <see cref="ExcelCellAddress"/> in the <see cref="Worksheet"/>. This should always be A1.
		/// </summary>
		public ExcelCellAddress Start => Data.Dimension.Start;

		/// <summary>
		/// Auto-filtering for all columns in the <see cref="Worksheet"/>. This allows sorting and filtering of data.
		/// </summary>
		public bool AutoFilter {
			get => Data.Cells[Data.Dimension.Address].AutoFilter;
			set => Data.Cells[Data.Dimension.Address].AutoFilter = value;
		}

		/// <summary>
		/// Text wrapping to all cells in the <see cref="Worksheet"/>. This should be used with <see cref="AutoFit(double)"/> for best results.
		/// </summary>
		public bool WrapText {
			get => Data.Cells[Data.Dimension.Address].Style.WrapText;
			set => Data.Cells[Data.Dimension.Address].Style.WrapText = value;
		}

		/// <summary>
		/// Determines if the <see cref="Worksheet"/> is hidden.
		/// </summary>
		public bool Hidden {
			get => Data.Hidden != eWorkSheetHidden.Visible;
			set => Data.Hidden = value ? eWorkSheetHidden.Visible : eWorkSheetHidden.Hidden;
		}

		/// <summary>
		/// The color of the <see cref="Worksheet"/> tab.
		/// </summary>
		public System.Drawing.Color TabColor {
			get => Data.TabColor;
			set => Data.TabColor = value;
		}

		/// <summary>
		/// Trims empty rows from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="allEmptyRows">Determines if only empty rows at the bottom of the worksheet should be trimmed.</param>
		public void Trim(bool allEmptyRows = false)
		{
			int colCount = Data.Dimension.Columns;
			for (int row = Rows; row > 0; row--) {
				bool deleteRow = true;
				for (int col = 1; col <= colCount; col++) {
					ExcelRange cell = Data.Cells[row, col];
					if (cell.Value != null && cell.Text.Length > 0) {
						if (!allEmptyRows)
							return;
						deleteRow = false;
						break;
					}
				}
				if (deleteRow)
					Data.DeleteRow(row);
			}
		}

		/// <summary>
		/// Returns the index of the last <see cref="ExcelRow"/> with data.
		/// </summary>
		private int LastRowWithData {
			get {
				int colCount = Data.Dimension.Columns;
				for (int row = Rows; row > 0; row--) {
					for (int col = 1; col <= colCount; col++) {
						ExcelRange cell = Data.Cells[row, col];
						if (cell.Value != null && cell.Value.ToString().Length > 0)
							return row;
					}
				}
				return 0;
			}
		}

		/// <summary>
		/// Trims empty columns from the <see cref="Worksheet"/>.
		/// </summary>
		/// <param name="allEmptyColumns">Determines if only empty rows on the right side of the worksheet should be trimmed.</param>
		public void TrimColumns(bool allEmptyColumns = false)
		{
			int rowCount = Data.Dimension.Rows;
			int colCount = Data.Dimension.Columns;
			for (int col = colCount; col >= 1; col--) {
				bool deleteCol = true;
				for (int row = 1; row <= rowCount; row++) {
					ExcelRange cell = Data.Cells[row, col];
					if (cell.Value != null && cell.Value.ToString().Length > 0) {
						if (!allEmptyColumns)
							return;
						deleteCol = false;
						break;
					}
				}
				if (deleteCol)
					Data.DeleteColumn(col);
			}
		}

		/*ID  Format Code
		9   0%
		10  0.00%
		11  0.00E+00
		12  # ?/?
		13  # ??/??
		14  d/m/yyyy
		15  d-mmm-yy
		16  d-mmm
		17  mmm-yy
		18  h:mm tt
		19  h:mm:ss tt
		20  H:mm
		21  H:mm:ss
		22  m/d/yyyy H:mm
		37  #,##0 ;(#,##0)
		38  #,##0 ;[Red](#,##0)
		39  #,##0.00;(#,##0.00)
		40  #,##0.00;[Red](#,##0.00)
		45  mm:ss
		46  [h]:mm:ss
		47  mmss.0
		48  ##0.0E+0
		49  @
		*/

		/// <summary>
		/// Returns the <see cref="Type"/> of data stored in the <see cref="ExcelColumn"/>.
		/// </summary>
		/// <param name="columnIndex">The <see cref="ExcelColumn"/> index.</param>
		/// <returns>The <see cref="Type"/> of data in the <see cref="ExcelColumn"/>.</returns>
		/// <see cref="https://support.office.com/en-us/article/number-format-codes-5026bbd6-04bc-48cd-bf33-80f18b4eae68"/>
		public Type ColumnType(int columnIndex)
		{
			ExcelColumn col = Column(columnIndex);

			if (col.Style.Numberformat == null)
				return typeof(string);
			string numfmt = col.Style.Numberformat.Format;
			if (string.IsNullOrEmpty(numfmt)) {
				////if (!col.Style.Numberformat.BuildIn)
				////    return typeof(string);
				int numfmtID = col.Style.Numberformat.NumFmtID;
				if (numfmtID == 0 || numfmtID == 49)
					return typeof(string);
				else if ((numfmtID >= 14 && numfmtID <= 17) || numfmtID == 22)
					return typeof(DateTime);
				else if ((numfmtID >= 18 && numfmtID <= 21) || (numfmtID >= 45 && numfmtID <= 47))
					return typeof(TimeSpan);
				else if (numfmtID == 9)
					return typeof(int);
				else if (numfmtID >= 9 && numfmtID <= 48)
					return typeof(double);
			}
			else {
				char ch;
				bool isNum = false;
				bool isTime = false;
				bool isDouble = false;
				bool isDecimal = false;
				for (int i = 0; i < numfmt.Length; i++) {
					ch = numfmt[i];
					if (ch == '0' || ch == '#' || ch == '?')
						isNum = true;
					else if (ch == 'd' || ch == 'M')
						return typeof(DateTime);
					else if (ch == '.')
						isDouble = true;
					else if (ch == 'y') {
						if (i < numfmt.Length - 1 && numfmt[i + 1] == 'y')
							return typeof(DateTime);
					}
					else if (ch == 'h' || ch == 'H' || ch == 's' || ch == 'm')
						isTime = true;
					else if (ch == '*' || ch == '\\')
						i++; // skip next
					else if (ch == '"') {
						i++;
						while (i < numfmt.Length && numfmt[i] != '"')
							i++;
					}
					else if (ch == '[') {
						i++;
						while (i < numfmt.Length && numfmt[i] != ']')
							i++;
					}
					else if (ch == '%' || CurrencySymbols.Contains(ch))
						isDecimal = true;
					////else if (ch == '@')
					////    return typeof(string);
				}
				if (isTime)
					return typeof(TimeSpan);
				else if (isDecimal)
					return typeof(decimal);
				else if (isDouble)
					return typeof(double);
				else if (isNum)
					return typeof(int);
			}
			return typeof(string);
		}

		/// <summary>
		/// Converts the <see cref="Worksheet"/> to a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to fill with data.</param>
		/// <param name="hasHeaders">Determines if the <see cref="Worksheet"/> has headers.</param>
		/// <returns>The <see cref="DataTable"/>.</returns>
		public DataTable ToDataTable(DataTable table, bool hasHeaders = true)
		{
			int maxRow = LastRowWithData;
			int maxCol = Columns;
			if (table.Columns.Count == 0) {
				for (int col = 1; col <= maxCol; col++) {
					Type ty = ColumnType(col);
					string header = hasHeaders ? Data.Cells[1, col].Value?.ToString() : "Column" + col;
					if (header == null)
						header = new string(' ', col);
					if (table.Columns.Contains(header)) {
						int index = 1;
						if (header == "Column" + col)
							header = "Column";
						while (table.Columns.Contains(header + index)) {
							index++;
						}
						header = header + index;
					}
					table.Columns.Add(header ?? new string(' ', col), ty);
				}
			}
			else if (table.Columns.Count != Columns)
				throw new InvalidProgramException(string.Format("DataTable column count ({0}) does not match expected excel column count ({1}).",
					table.Columns.Count,
					maxCol));

			List<Type> colTypes = table.Columns.Cast<DataColumn>().Select(col => col.DataType).ToList();
			Func<object, object>[] converters = new Func<object, object>[colTypes.Count];
			for (int i = 0; i < converters.Length; i++) {
				converters[i] = Converters.Converters.GetConverter(typeof(object), colTypes[i]);
			}
			for (int row = hasHeaders ? 2 : 1; row <= maxRow; row++) {
				DataRow newRow = table.NewRow();
				for (int col = 1; col <= maxCol; col++) {
					ExcelRangeBase cell = Data.Cells[row, col];
					if (cell.Text != "") {
						object value = cell.Value;
						if (colTypes[col - 1] == typeof(TimeSpan)) {
							if (cell.Value is double d)
								value = DateTime.FromOADate(d).TimeOfDay;
							else if (cell.Value is string str)
								value = TimeSpan.Parse(str);
						}
						else if (colTypes[col - 1] == typeof(DateTime)) {
							if (cell.Value is double d)
								value = DateTime.FromOADate(d);
							else if (cell.Value is string str)
								value = DateTime.Parse(str);
						}
						newRow[col - 1] = converters[col - 1](value);
					}
				}
				table.Rows.Add(newRow);
			}
			return table.Trim();
		}

		/// <summary>
		/// Converts the <see cref="Worksheet"/> to a <see cref="DataTable"/>.
		/// </summary>
		/// <param name="hasHeaders">Determines if the <see cref="Worksheet"/> has headers.</param>
		/// <returns>The <see cref="DataTable"/>.</returns>
		public DataTable ToDataTable(bool hasHeaders = true)
		{
			return ToDataTable(new DataTable(), hasHeaders);
		}

		/// <summary>
		/// Enumerates the rows in the <see cref="Worksheet"/>.
		/// </summary>
		/// <returns>An <see cref="IEnumerable{T}"/> of <see langword="string"/>[] representing the rows in the <see cref="Worksheet"/>.</returns>
		public IEnumerable<string[]> AsEnumerable()
		{
			int columns = Columns;
			int rows = LastRowWithData;
			for (int row = 1; row <= rows; row++) {
				string[] vals = new string[columns];
				for (int col = 0; col < columns; col++) {
					vals[col] = Data.Cells[row, col + 1].Value?.ToString();
				}
				yield return vals;
			}
		}

		/// <summary>
		/// Enumerates the rows in the <see cref="Worksheet"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to convert the rows to.</typeparam>
		/// <param name="hasHeaders">Determines if the first row should be skipped.</param>
		/// <returns>An <see cref="IEnumerable{T}"/> of objects representing the rows in the <see cref="Worksheet"/>.</returns>
		public IEnumerable<T> AsEnumerable<T>(bool hasHeaders = true)
		{
			Func<string[], T> converter = hasHeaders
				? Converters.Converters.ListToObject<string, T>(Data.Cells[1, 1, 1, Columns].Select(cell => cell.Value?.ToString()).ToList())
				: Converters.Converters.ListToObject<string, T>();

			Type[] colTypes = new Type[Columns];
			for (int i = 0; i < colTypes.Length; i++) {
				colTypes[i] = ColumnType(i + 1);
			}
			int rows = LastRowWithData;
			for (int row = hasHeaders ? 2 : 1; row <= rows; row++) {
				string[] line = new string[Columns];
				for (int col = 1; col <= Columns; col++) {
					// Excel stores all numbers as double including int
					ExcelRange cell = Data.Cells[row, col];
					string value = null;
					if (cell.Text != "") {
						if (colTypes[col - 1] == typeof(TimeSpan))
							value = DateTime.FromOADate(cell.GetValue<double>()).ToString("h:mm:ss");
						else if (colTypes[col - 1] == typeof(DateTime) && cell.Value is double d)
							value = DateTime.FromOADate(d).ToString();
						else
							value = cell.GetValue<string>();
						if (value != null && value.Length == 0)
							value = null;
					}
					line[col - 1] = value;
				}
				yield return converter(line);
			}
		}

		/// <summary>
		/// Converts a <see cref="Worksheet"/> to a <see cref="List{T}"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to convert the rows to.</typeparam>
		/// <param name="hasHeaders">Determines if the first row should be skipped.</param>
		/// <returns>The <see cref="List"/>.</returns>
		public List<T> ToList<T>(bool hasHeaders = true)
		{
			return AsEnumerable<T>(hasHeaders).ToList();
		}

		/// <summary>
		/// Adds the <see cref="Worksheet"/> data to an <see cref="ICollection{T}"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to convert the rows to.</typeparam>
		/// <param name="list">The <see cref="ICollection{T}"/> to add data to.</param>
		/// <param name="hasHeaders">Determines if the first row should be skipped.</param>
		/// <returns>The <see cref="ICollection{T}"/>.</returns>
		public ICollection<T> ToList<T>(ICollection<T> list, bool hasHeaders = true)
		{
			foreach (T obj in AsEnumerable<T>(hasHeaders)) {
				list.Add(obj);
			}
			return list;
		}

		/// <summary>
		/// Automatically formats string values to numbers, dates, timespans, currency, percentages, etc.
		/// </summary>
		public void AutoFormat()
		{
			int rows = Data.Dimension.Rows;
			int cols = Data.Dimension.Columns;
			for (int row = 1; row <= rows; row++) {
				for (int col = 1; col <= cols; col++) {
					ExcelRange cell = Data.Cells[row, col];
					if (cell.Value is string str)
						cell.Value = Parse(cell, str);
				}
			}
		}

		/// <summary>
		/// Parses an <see cref="ExcelRange"/> containing a string into an object.
		/// </summary>
		/// <param name="cell">The <see cref="ExcelRange"/> to parse.</param>
		/// <param name="str">The <see cref="string"/> value of the cell.</param>
		/// <returns>The result of the <see cref="ExcelRange"/> being parsed.</returns>
		private static object Parse(ExcelRange cell, string str)
		{
			string str2 = str.Trim();
			if (str2.Length == 0)
				return str;

			char c = str2[0];
			if (char.IsDigit(c) || c == '-') {
				char last = str2.Last();
				if (last == '%' || CurrencySymbols.Contains(last)) {
					str2 = str2.Substring(0, str.Length - 1);
					if (str2.Length <= 29) {
						if (decimal.TryParse(str2, out decimal dec)) {
							cell.Style.Numberformat.Format = "0.00" + last;
							return dec / 100m;
						}
					}
					else if (double.TryParse(str2, out double doub)) {
						cell.Style.Numberformat.Format = "0.00" + last;
						return doub / 100.0;
					}
					return str;
				}
				if (str2.Length <= 19 && long.TryParse(str2, out long lval)) {
					cell.Style.Numberformat.Format = "0";
					return lval;
				}
				else if (str2.Length <= 29 && decimal.TryParse(str2, out decimal d)) {
					cell.Style.Numberformat.Format = "0.0#####";
					return d;
				}
				else if (double.TryParse(str2, out double doub)) {
					cell.Style.Numberformat.Format = "0.0#####";
					return doub;
				}
				else if (TryParseFraction(str2, out d)) {
					cell.Style.Numberformat.Format = "#/######";
					return d;
				}
				else if (TimeSpan.TryParse(str2, out TimeSpan ts)) {
					cell.Style.Numberformat.Format = "h:mm:ss";
					return ts;
				}
			}
			else if (str2.Equals("false", StringComparison.OrdinalIgnoreCase))
				return false;
			else if (str2.Equals("true", StringComparison.OrdinalIgnoreCase))
				return true;
			else if (str2.Equals("null", StringComparison.OrdinalIgnoreCase))
				return null;
			else if (CurrencySymbols.Contains(c)) {
				str2 = str2.Substring(1);
				if (str2.Length <= 29) {
					if (decimal.TryParse(str2, out decimal dec)) {
						cell.Style.Numberformat.Format = c + "0.00";
						return dec;
					}
				}
				else if (double.TryParse(str2, out double doub)) {
					cell.Style.Numberformat.Format = c + "0.00";
					return doub;
				}
				return str;
			}
			if (DateTime.TryParse(str2, out DateTime dt)) {
				if (dt.TimeOfDay == TimeSpan.Zero)
					cell.Style.Numberformat.Format = "M/d/yyyy";
				else
					cell.Style.Numberformat.Format = "M/d/yyyy H:mm:ss AM/PM";
				return dt;
			}
			return str;
		}

		private static bool TryParseFraction(string str, out decimal d)
		{
			d = 0;
			string[] parts = str.Split('/');
			if (parts.Length != 2 || !int.TryParse(parts[0], out int numerator) || !int.TryParse(parts[1], out int denominator)
				|| denominator == 0)
				return false;
			if (numerator != 0)
				d = ((decimal) numerator) / denominator;
			return true;
		}

		public void SetValue(int row, int col, object value)
		{
			Data.SetValue(row, col, value);
		}

		public void SetValue(string address, object value)
		{
			Data.SetValue(address, value);
		}

		public int AddRow(params object[] values)
		{
			int row = Data.Dimension?.Rows + 1 ?? 1;
			for (int col = 0; col < values.Length; col++) {
				Data.SetValue(row, col + 1, values[col]);
			}
			return row;
		}

		public void InsertRow(int index, int count = 1)
		{
			Data.InsertRow(index, count);
		}

		public int AddColumn()
		{
			int col = Data.Dimension?.Columns + 1 ?? 1;
			Data.InsertColumn(col, 1);
			return col;
		}

		public void InsertColumn(int index, int count = 1)
		{
			Data.InsertColumn(index, count);
		}

		public void SetFormula(int row, int col, string formula, bool allowCircularReferences = false)
		{
			Data.Cells[row, col].Formula = formula;
			ExcelRange x = Data.Cells[row, col];
			OfficeOpenXml.FormulaParsing.ExcelCalculationOption option = new OfficeOpenXml.FormulaParsing.ExcelCalculationOption
			{
				AllowCirculareReferences = allowCircularReferences
			};
			x.Calculate(option);
		}

		public void Calculate()
		{
			Data.Cells[0, 1].Calculate();
			Data.Calculate();
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="Worksheet"/> class.
		/// </summary>
		~Worksheet()
		{
			Data = null;
		}
	}
}
