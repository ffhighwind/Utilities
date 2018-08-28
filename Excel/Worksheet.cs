using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
        private const int MaxDateOnlyRows = 15;

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
        /// <param name="worksheet">The EPPlus ExcelWorksheet to represent.</param>
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
        /// Loads an <see cref="IDataReader"/> into a <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IDataReader"/> to load.</param>
        /// <param name="printHeaders">Determines if the first line contains headers.</param>
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
            foreach (object obj in list) {
                types.Add(obj.GetType());
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
        /// Gets an <see cref="ExcelRange"/> from the <see cref="Worksheet"/>.
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
        /// Gets a cell value from the <see cref="Worksheet"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the cell.</typeparam>
        /// <param name="address">The address (A1) of the cell.</param>
        /// <returns>The value at the given address.</returns>
        public T Cell<T>(string address)
        {
            return Data.Cells[address].GetValue<T>();
        }

        /// <summary>
        /// Gets a cell value from the <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="address">The address (A1) of the cell.</param>
        /// <returns>The value at the given address.</returns>
        public object Cell(string address)
        {
            return Data.Cells[address].Value;
        }

        /// <summary>
        /// Gets a cell value from the <see cref="Worksheet"/>.
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
        /// Gets a cell value from the <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="row">The row index of the cell (between 1 to <see cref="Rows"/>).</param>
        /// <param name="col">The column index of the cell (between 1 to <see cref="Columns"/>).</param>
        /// <returns>The value at the given row and column.</returns>
        public object Cell(int row, int col)
        {
            return Data.Cells[row, col].Value;
        }

        /// <summary>
        /// Gets cell values from the <see cref="Worksheet"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to cast the cell values to.</typeparam>
        /// <param name="addresses">The address address range (A1:Z5) of the cells.</param>
        /// <returns>The values at the given address range.</returns>
        public T[,] Cells<T>(string addresses)
        {
            return Data.Cells[addresses].GetValue<T[,]>();
        }

        /// <summary>
        /// Gets cell values from the <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="addresses">The address address range (A1:Z5) of the cells.</param>
        /// <returns>The values at the given address range.</returns>
        public object[,] Cells(string addresses)
        {
            return Data.Cells[addresses].GetValue<object[,]>();
        }

        /// <summary>
        /// Gets cell values from the <see cref="Worksheet"/>.
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
        /// Gets cell values from the <see cref="Worksheet"/>.
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
        /// Gets an <see cref="ExcelRow"/> from the <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="index">The index of the <see cref="ExcelRow"/> (between 1 and <see cref="Rows"/>).</param>
        /// <returns>The <see cref="ExcelRow"/> at the given index.</returns>
        public ExcelRow Row(int index)
        {
            return Data.Row(index);
        }

        /// <summary>
        /// Gets an <see cref="ExcelColumn"/> from the <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="index">The index of the <see cref="ExcelColumn"/> (between 1 and <see cref="Columns"/>).</param>
        /// <returns>The <see cref="ExcelColumn"/> at the given index.</returns>
        public ExcelColumn Column(int index)
        {
            return Data.Column(index);
        }

        /// <summary>
        /// Gets the index of a column from the <see cref="Worksheet"/>.
        /// </summary>
        /// <param name="colName">The column header to find.</param>
        /// <returns>The column index with the given header, or -1 if it doesn't exist.</returns>
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
        public void AutoFit(double minimumWidth = 0)
        {
            Data.Cells.AutoFitColumns(minimumWidth);
        }

        /// <summary>
        /// The last cell address in the <see cref="Worksheet"/>.
        /// </summary>
        public ExcelCellAddress End => Data.Dimension.End;

        /// <summary>
        /// The first cell address in the <see cref="Worksheet"/>. This should always be A1.
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
        /// Color for the <see cref="Worksheet"/> tab.
        /// </summary>
        public System.Drawing.Color TabColor {
            get => Data.TabColor;
            set => Data.TabColor = value;
        }

        /// <summary>
        /// Trims the empty rows at the end of the <see cref="Worksheet"/>.
        /// </summary>
        public void Trim()
        {
            int colCount = Data.Dimension.Columns;
            for (int row = Rows; row > 0; row++) {
                for (int col = 1; col <= colCount; col++) {
                    ExcelRange cell = Data.Cells[row, col];
                    if (cell.Value != null && cell.Value.ToString().Length > 0)
                        return;
                }
                Data.DeleteRow(row);
            }
        }

        /// <summary>
        /// Trims the rightmost empty columns from the <see cref="Worksheet"/>.
        /// </summary>
        public void TrimColumns()
        {
            int rowCount = Data.Dimension.Rows;
            int colCount = Data.Dimension.Columns;
            for (int col = colCount; col >= 1; col--) {
                for (int row = 1; row <= rowCount; row++) {
                    ExcelRange cell = Data.Cells[row, col];
                    if (cell.Value != null && cell.Value.ToString().Length > 0)
                        return;
                }
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
        /// Gets the <see cref="Type"/> of data stored in the <see cref="ExcelColumn"/>.
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
            int maxRow = Rows;
            int maxCol = Columns;
            if (table.Columns.Count == 0) {
                for (int col = 1; col <= maxCol; col++) {
                    Type ty = ColumnType(col);
                    string header = hasHeaders ? Data.Cells[1, col].Value?.ToString() : "Column" + col;
                    table.Columns.Add(header ?? new string(' ', col), ty);
                }
            }
            else if (table.Columns.Count != Columns)
                throw new InvalidProgramException(string.Format("DataTable column count ({0}) does not match expected excel column count ({1}).",
                    table.Columns.Count,
                    maxCol));

            List<Type> colTypes = table.Columns.Cast<DataColumn>().Select(col => col.DataType).ToList();
            for (int row = hasHeaders ? 2 : 1; row <= maxRow; row++) {
                DataRow newRow = table.NewRow();
                for (int col = 1; col <= maxCol; col++) {
                    Type colType = colTypes[col - 1];
                    ExcelRangeBase cell = Data.Cells[row, col];
                    if (colType == typeof(string))
                        newRow[col - 1] = cell.Text;
                    else if (cell.Value != null) {
                        try {
                            if (colType.IsIntegral())
                                newRow[col - 1] = cell.GetValue<long>();
                            else if (colType.IsFloatingPoint())
                                newRow[col - 1] = cell.GetValue<decimal>();
                            else if (colType == typeof(DateTime) || colType == typeof(DateTimeOffset))
                                newRow[col - 1] = cell.GetValue<DateTime>();
                            else if (colType == typeof(TimeSpan))
                                newRow[col - 1] = cell.GetValue<TimeSpan>();
                            else
                                newRow[col - 1] = cell.Value;
                        }
                        catch { // ignore
                        }
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
        /// <returns>An <see cref="IEnumerable{T}"/> of strings representing the rows in the <see cref="Worksheet"/>.</returns>
        public IEnumerable<string[]> AsEnumerable()
        {
            int columns = Columns;
            int rows = Rows;
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
        public IEnumerable<T> AsEnumerable<T>(bool hasHeaders = true) where T : class, new()
        {
            Func<string[], T> converter = hasHeaders
                ? Converters.Converters.ListToObject<string, T>(Data.Cells[1, 1, 1, Columns].Select(cell => cell.Value?.ToString()).ToList())
                : Converters.Converters.ListToObject<string, T>();

            bool[] isTimespan = new bool[Columns];
            for (int i = 0; i < isTimespan.Length; i++) {
                isTimespan[i] = ColumnType(i + 1) == typeof(TimeSpan);
            }
            for (int row = hasHeaders ? 2 : 1; row <= Rows; row++) {
                string[] line = new string[Columns];
                for (int col = 1; col <= Columns; col++) {
                    // Excel stores all numbers as double including int
                    ExcelRange cell = Data.Cells[row, col];
                    line[col - 1] = isTimespan[col - 1] ? DateTime.FromOADate(cell.GetValue<double>()).ToString("h:mm:ss") : cell.GetValue<string>();
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
        public List<T> ToList<T>(bool hasHeaders = true) where T : class, new()
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
        public ICollection<T> ToList<T>(ICollection<T> list, bool hasHeaders = true) where T : class, new()
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
                    if (Data.Cells[row, col].Value is string) {
                        Data.Cells[row, col].Value = Parse(Data.Cells[row, col]);
                    }
                }
            }
        }

        /// <summary>
        /// Parses an <see cref="ExcelRange"/> containing a string into an object.
        /// </summary>
        /// <param name="cell">The <see cref="ExcelRange"/> to parse.</param>
        /// <returns>The result of the <see cref="ExcelRange"/> being parsed.</returns>
        private static object Parse(ExcelRange cell)
        {
            if (!(cell.Value is string str))
                return null;
            string str2 = str.Trim();
            if (str2.Length == 0)
                return str;

            char c = str2[0];
            if (char.IsDigit(c) || c == '-') {
                char last = str2.Last();
                if (last == '%') {
                    if (TryParseNumber(str2.Substring(0, str2.Length - 1), out decimal d)) {
                        cell.Style.Numberformat.Format = "0.00%";
                        return d / 100m;
                    }
                    return str;
                }
                if (CurrencySymbols.Contains(last)) {
                    if (TryParseNumber(str2.Substring(0, str2.Length - 1), out decimal d)) {
                        cell.Style.Numberformat.Format = "0.00" + last;
                        return d;
                    }
                    return str;
                }
                for (int i = 1; i < str2.Length; i++) {
                    c = str2[i];
                    if (char.IsDigit(c))
                        continue;
                    else if (c == '.') {
                        if (TimeSpan.TryParse(str2, out TimeSpan ts)) {
                            cell.Style.Numberformat.Format = "h:mm:ss";
                            return ts;
                        }
                        if (TryParseNumber(str2, out decimal d)) {
                            cell.Style.Numberformat.Format = "0.0#";
                            return d;
                        }
                    }
                    else if (c == '/' || c == '-') {
                        if (DateTime.TryParse(str2, out DateTime dt)) {
                            cell.Style.Numberformat.Format = "M/d/yyyy H:mm:ss AM/PM";
                            return dt;
                        }
                    }
                    else if (c == ':') {
                        if (TimeSpan.TryParse(str2, out TimeSpan ts)) {
                            cell.Style.Numberformat.Format = "h:mm:ss";
                            return ts;
                        }
                        if (DateTime.TryParse(str2, out DateTime dt)) {
                            cell.Style.Numberformat.Format = "M/d/yyyy H:mm:ss AM/PM";
                            return dt;
                        }
                    }
                    else if (c == '/') {
                        if (DateTime.TryParse(str2, out DateTime dt)) {
                            cell.Style.Numberformat.Format = "M/d/yyyy H:mm:ss AM/PM";
                            return dt;
                        }
                        if (TryParseFraction(str2, out decimal d, out string format)) {
                            cell.Style.Numberformat.Format = format;
                            return d;
                        }
                    }
                    return str;
                }
                int ival = int.Parse(str2);
                cell.Style.Numberformat.Format = "0";
                return ival;
            }
            else if (CurrencySymbols.Contains(c)) {
                if (TryParseNumber(str2.Substring(1), out decimal d)) {
                    cell.Style.Numberformat.Format = c + "0.00";
                    return d;
                }
            }
            else if (DateTime.TryParse(str2, out DateTime dt)) {
                cell.Style.Numberformat.Format = "M-d-yyyy H:mm:ss AM/PM";
                return dt;
            }
            else {
                string lower = str2.ToUpper();
                if (lower == "false")
                    return false;
                else if (lower == "true")
                    return true;
                else if (lower == "null")
                    return null;
            }
            return str;
        }

        private static readonly NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
        private static readonly CultureInfo culture = new CultureInfo("en-US");

        public static bool TryParseNumber(string str, out decimal d)
        {
            try {
                d = decimal.Parse(str, style, culture);
                return true;
            }
            catch { // ignore
            }
            d = 0;
            return false;
        }

        private static bool TryParseFraction(string str, out decimal d, out string format)
        {
            d = 0;
            format = "#/###";
            string[] parts = str.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int numerator) || !int.TryParse(parts[1], out int denominator)
                || denominator == 0)
                return false;
            if (numerator != 0) {
                int decimals = numerator > denominator ? 3 : Math.Min(3, denominator / numerator);
                if (decimals <= 10) {
                    format = "#/" + new string('#', decimals);
                    d = ((decimal) numerator) / denominator;
                }
            }
            return true;
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
