using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace Utilities.Excel
{
    /// <summary>
    /// An Excel Worksheet wrapper for the EPPlus implementation of ExcelWorksheet.
    /// </summary>
    public class Worksheet
    {
        private const int MAX_DATEONLY_ROWS_COUNT = 15;
        private ExcelWorksheet worksheet;

        /// <summary>
        /// Constructs an Excel Worksheet from an ExcelWorksheet.
        /// </summary>
        /// <param name="worksheet">The EPPlus ExcelWorksheet to represent.</param>
        public Worksheet(ExcelWorksheet worksheet)
        {
            this.worksheet = worksheet;
        }

        /// <summary>
        /// Loads data into the Worksheet from an enumerable list.
        /// </summary>
        /// <typeparam name="T">The type of objects in the enumerable list.</typeparam>
        /// <param name="list">The enumerable list to load from.</param>
        /// <param name="printHeaders">Determines if the property names of the type should be written to the first row.</param>
        public void Load<T>(IEnumerable<T> list, bool printHeaders = true)
        {
            worksheet.Cells.LoadFromCollection<T>(list, printHeaders);
            FixColumnTypes(Util.GetPropertyTypes(typeof(T)));
        }

        /// <summary>
        /// Loads data into the Worksheet from a DataTable.
        /// </summary>
        /// <param name="table">The DataTable to load from.</param>
        /// <param name="printHeaders">Determines if the table's DataColumn names should be written to the first row.</param>
        public void Load(DataTable table, bool printHeaders = true)
        {
            worksheet.Cells.LoadFromDataTable(table, printHeaders);
            FixColumnTypes(table.Columns.Cast<DataColumn>().AsEnumerable().Select(col => col.DataType));
        }

        private void FixColumnTypes(IEnumerable<Type> types)
        {
            int col = 1;
            foreach (Type ty in types) {
                if (ty == typeof(DateTime))
                    FixDateColumn(col);
                else if (ty == typeof(TimeSpan)) {
                    worksheet.Column(col).Style.Numberformat.Format = "h:mm:ss";
                    worksheet.Cells[1, col, worksheet.Dimension.Rows, col].Style.Numberformat.Format = "h:mm:ss";
                }
                col++;
            }
        }

        private void FixDateColumn(int columnIndex)
        {
            ExcelColumn column = worksheet.Column(columnIndex);
            int rowCheck = Math.Min(worksheet.Dimension.Rows, MAX_DATEONLY_ROWS_COUNT);
            for (int row = worksheet.Dimension.Rows > 1 ? 2 : 1; row < rowCheck; row++) {
                DateTime datetime = worksheet.Cells[row, columnIndex].GetValue<DateTime>();
                if (datetime.TimeOfDay != TimeSpan.Zero) {
                    column.Style.Numberformat.Format = "M/d/yyyy h:mm:ss AM/PM";
                    worksheet.Cells[1, columnIndex, worksheet.Dimension.Rows + 1, columnIndex].Style.Numberformat.Format = "M/d/yyyy h:mm:ss AM/PM";
                    //column.Style.Numberformat.Format = "m/d/yyyy h:mm:ss AM/PM";
                    return;
                }
            }
            column.Style.Numberformat.Format = "M/d/yyyy";
            //column.StyleID = 14; //Short Date
        }

        /// <summary>
        /// Loads data into the Worksheet from an IDataReader.
        /// </summary>
        /// <param name="reader">The IDataReader to load from.</param>
        /// <param name="printHeaders">Determines if the first line contains headers.</param>
        public void Load(IDataReader reader, bool printHeaders = true)
        {
            worksheet.Cells.LoadFromDataReader(reader, printHeaders);
            List<Type> types = new List<Type>();
            for (int i = 0; i < reader.FieldCount; i++) {
                types.Add(reader.GetFieldType(i));
            }
            FixColumnTypes(types);
        }

        /// <summary>
        /// Loads data into the Worksheet from an enumerable list of objects.
        /// </summary>
        /// <param name="list">The enumerable list to load from.</param>
        public void Load(IEnumerable<object[]> list)
        {
            worksheet.Cells.LoadFromArrays(list);
            if (!list.Any())
                return;
            List<Type> types = new List<Type>();
            foreach (object obj in list) {
                types.Add(obj.GetType());
            }
            FixColumnTypes(types);
        }

        /// <summary>
        /// Loads comma-separated text into the Worksheet.
        /// </summary>
        /// <param name="csvtext">The comma-separated text to load from.</param>
        public void Load(string csvtext)
        {
            worksheet.Cells.LoadFromText(csvtext);
        }

        /// <summary>
        /// Deletes the Worksheet.
        /// </summary>
        public void Delete()
        {
            worksheet.Workbook.Worksheets.Delete(worksheet);
            worksheet = null;
        }

        /// <summary>
        /// Clears the Worksheet.
        /// </summary>
        public void Clear()
        {
            worksheet.Cells.Clear();
        }

        /// <summary>
        /// Gets a cell from the Worksheet.
        /// </summary>
        /// <param name="row">The row index of the cell (between 1 to Rows).</param>
        /// <param name="col">The column index of the cell (between 1 to Columns).</param>
        /// <returns>The cell at the given row and column.</returns>
        public ExcelRange this[int row, int col] {
            get {
                return worksheet.Cells[row, col];
            }
        }

        /// <summary>
        /// Gets a cell or cells from the Worksheet.
        /// </summary>
        /// <param name="address">The address (A1) or address range (A1:Z5) of the cells.</param>
        /// <returns>The cell or cells at the given address or address range.</returns>
        public ExcelRange this[string address] {
            get {
                return worksheet.Cells[address];
            }
        }

        /// <summary>
        /// Gets cells from the Worksheet.
        /// </summary>
        /// <param name="rowA">The starting row index of the cell range (between 1 to Rows).</param>
        /// <param name="colA">The starting column index of the cell range (between 1 to Columns).</param>
        /// <param name="rowB">The last row index of the cell range (between 1 to Rows).</param>
        /// <param name="colB">The last column index of the cell range (between 1 to Columns).</param>
        /// <returns>The cells at the given address range.</returns>
        public ExcelRange this[int rowA, int colA, int rowB, int colB] {
            get {
                return worksheet.Cells[rowA, colA, rowB, colB];
            }
        }

        /// <summary>
        /// Gets a cell value from the Worksheet.
        /// </summary>
        /// <typeparam name="T">The type to cast the value of the cell to.</typeparam>
        /// <param name="address">The address (A1) of the cell.</param>
        /// <returns>The value at the given address.</returns>
        public T Cell<T>(string address)
        {
            return worksheet.Cells[address].GetValue<T>();
        }

        /// <summary>
        /// Gets a cell value from the Worksheet.
        /// </summary>
        /// <param name="address">The address (A1) of the cell.</param>
        /// <returns>The value at the given address.</returns>
        public object Cell(string address)
        {
            return worksheet.Cells[address].Value;
        }

        /// <summary>
        /// Gets a cell value from the Worksheet.
        /// </summary>
        /// <typeparam name="T">The type to cast the value of the cell to.</typeparam>
        /// <param name="row">The row index of the cell (between 1 to Rows).</param>
        /// <param name="col">The column index of the cell (between 1 to Columns).</param>
        /// <returns>The value at the given row and column.</returns>
        public T Cell<T>(int row, int col)
        {
            return worksheet.Cells[row, col].GetValue<T>();
        }

        /// <summary>
        /// Gets a cell value from the Worksheet.
        /// </summary>
        /// <param name="row">The row index of the cell (between 1 to Rows).</param>
        /// <param name="col">The column index of the cell (between 1 to Columns).</param>
        /// <returns>The value at the given row and column.</returns>
        public object Cell(int row, int col)
        {
            return worksheet.Cells[row, col].Value;
        }

        /// <summary>
        /// Gets cell values from the Worksheet.
        /// </summary>
        /// <typeparam name="T">The type to cast the cell values to.</typeparam>
        /// <param name="addresses">The address address range (A1:Z5) of the cells.</param>
        /// <returns>The values at the given address range.</returns>
        public T[,] Cells<T>(string addresses)
        {
            return worksheet.Cells[addresses].GetValue<T[,]>();
        }

        /// <summary>
        /// Gets cell values from the Worksheet.
        /// </summary>
        /// <param name="addresses">The address address range (A1:Z5) of the cells.</param>
        /// <returns>The values at the given address range.</returns>
        public object[,] Cells(string addresses)
        {
            return worksheet.Cells[addresses].GetValue<object[,]>();
        }

        /// <summary>
        /// Gets cell values from the Worksheet.
        /// </summary>
        /// <typeparam name="T">The type to cast the cell values to.</typeparam>
        /// <param name="rowA">The starting row index of the cell range (between 1 to Rows).</param>
        /// <param name="colA">The starting column index of the cell range (between 1 to Columns).</param>
        /// <param name="rowB">The last row index of the cell range (between 1 to Rows).</param>
        /// <param name="colB">The last column index of the cell range (between 1 to Columns).</param>
        /// <returns>The values at the given address range.</returns>
        public T[,] Cells<T>(int rowA, int colA, int rowB, int colB)
        {
            return worksheet.Cells[rowA, colB, rowB, colB].GetValue<T[,]>();
        }

        /// <summary>
        /// Gets cell values from the Worksheet.
        /// </summary>
        /// <param name="rowA">The starting row index of the cell range (between 1 to Rows).</param>
        /// <param name="colA">The starting column index of the cell range (between 1 to Columns).</param>
        /// <param name="rowB">The last row index of the cell range (between 1 to Rows).</param>
        /// <param name="colB">The last column index of the cell range (between 1 to Columns).</param>
        /// <returns>The values at the given address range.</returns>
        public object[,] Cells(int rowA, int colA, int rowB, int colB)
        {
            return worksheet.Cells[rowA, colB, rowB, colB].GetValue<object[,]>();
        }

        /// <summary>
        /// Gets a row from the Worksheet.
        /// </summary>
        /// <param name="index">The index of the row (between 1 and Rows).</param>
        /// <returns>The row at the given index.</returns>
        public ExcelRow Row(int index)
        {
            return worksheet.Row(index);
        }

        /// <summary>
        /// Gets a column from the Worksheet.
        /// </summary>
        /// <param name="index">The index of the row (between 1 and Rows).</param>
        /// <returns>The column at the given index.</returns>
        public ExcelColumn Column(int index)
        {
            return worksheet.Column(index);
        }

        /// <summary>
        /// The index of the Worksheet.
        /// </summary>
        public int Index {
            get {
                return worksheet.Index;
            }
        }

        /// <summary>
        /// The name of the Worksheet.
        /// </summary>
        public string Name {
            get {
                return worksheet.Name;
            }
            set {
                worksheet.Name = value;
            }
        }

        /// <summary>
        /// The number of rows in the Worksheet.
        /// </summary>
        public int Rows {
            get {
                return worksheet.Dimension.Rows;
            }
        }

        /// <summary>
        /// The number of columns in the Worksheet.
        /// </summary>
        public int Columns {
            get {
                return worksheet.Dimension.Columns;
            }
        }

        /// <summary>
        /// Removes a column from the Worksheet.
        /// </summary>
        /// <param name="index">The index of the column to remove.</param>
        public void RemoveColumn(int index)
        {
            worksheet.DeleteColumn(index);
        }

        /// <summary>
        /// Removes a row from the Worksheet.
        /// </summary>
        /// <param name="index">The index of the row to remove.</param>
        public void RemoveRow(int index)
        {
            worksheet.DeleteRow(index);
        }

        /// <summary>
        /// Resizes the columns to fit the data. Cells with wrapped text and formulas are not counted for this.
        /// </summary>
        public void AutoFit()
        {
            //worksheet.Cells.AutoFitColumns(0);
            for (int col = 1; col <= worksheet.Dimension.Columns; col++) {
                var column = worksheet.Column(col);
                column.AutoFit();
            }
        }

        /// <summary>
        /// Hint to Excel to constantly resize the columns so that they fit the data.
        /// </summary>
        public bool BestFit {
            set {
                for (int col = 1; col <= worksheet.Dimension.Columns; col++) {
                    worksheet.Column(col).BestFit = value;
                }
            }
        }

        /// <summary>
        /// The last cell address in the Worksheet.
        /// </summary>
        public ExcelCellAddress End {
            get {
                return worksheet.Dimension.End;
            }
        }

        /// <summary>
        /// The first cell address in the Worksheet. This should always be A1.
        /// </summary>
        public ExcelCellAddress Start {
            get {
                return worksheet.Dimension.Start;
            }
        }

        /// <summary>
        /// Auto-filtering for all columns in the Worksheet. This allows sorting and filtering of data.
        /// </summary>
        public bool AutoFilter {
            get {
                return worksheet.Cells[worksheet.Dimension.Address].AutoFilter;
            }
            set {
                worksheet.Cells[worksheet.Dimension.Address].AutoFilter = value;
            }
        }

        /// <summary>
        /// Text wrapping to all cells in the Worksheet. This can be used with AutoFit for best results.
        /// </summary>
        public bool WrapText {
            get {
                return worksheet.Cells[worksheet.Dimension.Address].Style.WrapText;
            }
            set {
                worksheet.Cells[worksheet.Dimension.Address].Style.WrapText = value;
            }
        }

        /// <summary>
        /// Determines if the Worksheet is hidden.
        /// </summary>
        public bool Hidden {
            get {
                return worksheet.Hidden != eWorkSheetHidden.Visible;
            }
            set {
                worksheet.Hidden = value ? eWorkSheetHidden.Visible : eWorkSheetHidden.Hidden;
            }
        }

        /// <summary>
        /// Color for the Worksheet tab.
        /// </summary>
        public System.Drawing.Color TabColor {
            get {
                return worksheet.TabColor;
            }
            set {
                worksheet.TabColor = value;
            }
        }

        /// <summary>
        /// The EPPlus implementation of the Worksheet (ExcelWorksheet).
        /// </summary>
        public ExcelWorksheet Data {
            get {
                return worksheet;
            }
        }

        /// <summary>
        /// Trims the empty rows at the end of the Worksheet.
        /// </summary>
        public void Trim()
        {
            int colCount = worksheet.Dimension.Columns;
            for (int row = Rows; row > 0; row++) {
                for (int col = 1; col <= colCount; col++) {
                    ExcelRange cell = worksheet.Cells[row, col];
                    if (cell.Value != null && cell.Value.ToString().Length > 0)
                        return;
                }
                worksheet.DeleteRow(row);
            }
        }

        /// <summary>
        /// Trims the rightmost empty columns from the Worksheet.
        /// </summary>
        public void TrimColumns()
        {
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            for (int col = colCount; col >= 1; col--) {
                for (int row = 1; row <= rowCount; row++) {
                    ExcelRange cell = worksheet.Cells[row, col];
                    if (cell.Value != null && cell.Value.ToString().Length > 0)
                        return;
                }
                worksheet.DeleteColumn(col);
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
        private static readonly Regex currencyRegex = new Regex(@"^\p{Sc}|[^*]\p{Sc}");

        /// <summary>
        /// Gets the Type of data stored in the column.
        /// </summary>
        /// <param name="columnIndex">The column index.</param>
        /// <returns>The Type of the data at the given column.</returns>
        public Type ColumnType(int columnIndex)
        {
            ExcelColumn col = Column(columnIndex);

            string numfmt = col.Style.Numberformat?.Format;
            if (numfmt == null)
                return typeof(string);
            if (numfmt == null) {
                if (!col.Style.Numberformat.BuildIn)
                    return typeof(string);
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
                char ch = numfmt[numfmt.Length - 1];
                bool isNum = false;
                bool isTime = false;
                bool isDecimal = false;
                bool isDate = false;
                for (int i = 0; i < numfmt.Length; i++) {
                    ch = numfmt[i];
                    if (ch == '.')
                        isDecimal = true;
                    else if (ch == '0' || ch == '#' || ch == '?')
                        isNum = true;
                    else if (ch == 'd' || ch == 'M')
                        isDate = true;
                    else if (ch == 'y') {
                        if (i < numfmt.Length - 1 && numfmt[i + 1] == 'y')
                            isDate = true;
                    }
                    else if (ch == 'h' || ch == 'H' || ch == 's' || ch == 'm')
                        isTime = true;
                    else if (ch == '*' || ch == '\\')
                        i++; //skip next
                    else if (ch == '"') {
                        i++;
                        while (i < numfmt.Length && numfmt[i] != '"')
                            i++;
                    }
                    //else if (ch == '@')
                    //    return typeof(string);
                }
                if (isDate)
                    return typeof(DateTime);
                else if (isTime)
                    return typeof(TimeSpan);
                else if (currencyRegex.IsMatch(numfmt))
                    return typeof(decimal);
                else if (isDecimal)
                    return typeof(double);
                else if (isNum)
                    return typeof(int);
            }
            return typeof(string);
        }

        /// <summary>
        /// Converts the Worksheet to a DataTable.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="hasHeaders">Determines if the Worksheet has headers.</param>
        /// <returns>The modified DataTable.</returns>
        public DataTable ToDataTable(DataTable table, bool hasHeaders = true)
        {
            int maxRow = Rows + 1;
            int maxCol = Columns + 1;
            if (table.Columns.Count == 0) {
                string[] headers = new string[maxCol];
                for (int col = 1; col < maxCol; col++) {
                    Type ty = ColumnType(col);
                    string header = hasHeaders ? worksheet.Cells[1, col].Value?.ToString() : "Column" + col;
                    table.Columns.Add(header ?? new string(' ', col), ty);
                }
            }
            else if (table.Columns.Count != Columns)
                return null;

            for (int row = hasHeaders ? 2 : 1; row < maxRow; row++) {
                DataRow newRow = table.NewRow();
                for (int col = 1; col < maxCol; col++) {
                    Type colType = table.Columns[col - 1].DataType;
                    if (worksheet.Cells[row, col].Value == null)
                        newRow[col - 1] = DBNull.Value;
                    else if (colType == typeof(string)) {
                        newRow[col - 1] = worksheet.Cells[row, col].Value.ToString();
                    }
                    else {
                        try {
                            if (colType.IsIntegral()) {
                                newRow[col - 1] = long.Parse(worksheet.Cells[row, col].Value.ToString());
                            }
                            else if (colType.IsFloatingPoint()) {
                                newRow[col - 1] = decimal.Parse(worksheet.Cells[row, col].Value.ToString());
                            }
                            else if (colType == typeof(DateTime) || colType == typeof(DateTimeOffset)) {
                                newRow[col - 1] = worksheet.Cells[row, col].GetValue<DateTime>();
                            }
                            else if (colType == typeof(TimeSpan)) {
                                newRow[col - 1] = worksheet.Cells[row, col].GetValue<TimeSpan>();
                            }
                            else
                                newRow[col - 1] = worksheet.Cells[row, col].Value;
                        }
                        catch { }
                    }
                }
                table.Rows.Add(newRow);
            }
            return table.Trim();
        }

        /// <summary>
        /// Converts the Worksheet to a DataTable.
        /// </summary>
        /// <param name="hasHeaders">Determines if the Worksheet has headers.</param>
        /// <returns>A DataTable with data from the Worksheet.</returns>
        public DataTable ToDataTable(bool hasHeaders = true)
        {
            return ToDataTable(new DataTable(), hasHeaders);
        }

        /// <summary>
        /// Enumerates the Worksheet rows.
        /// </summary>
        /// <returns>A list of strings representing the rows in the Worksheet.</returns>
        public IEnumerable<string[]> AsEnumerable()
        {
            int columns = Columns;
            int rows = Rows;
            for (int row = 1; row <= rows; row++) {
                string[] vals = new string[columns];
                for (int col = 0; col < columns; col++) {
                    vals[col] = worksheet.Cells[row, col + 1].Value?.ToString();
                }
                yield return vals;
            }
        }

        /// <summary>
        /// Enumerates the Worksheet rows.
        /// </summary>
        /// <typeparam name="T">The Type to convert the rows to.</typeparam>
        /// <param name="hasHeaders">Determines if the first row should be skipped.</param>
        /// <returns>An enumerable list of objects representing the rows in the Worksheet.</returns>
        public IEnumerable<T> AsEnumerable<T>(bool hasHeaders = true) where T : new()
        {
            Func<string[], T> converter;
            string[] vals = new string[Columns];
            if (hasHeaders) {
                vals = new string[Columns];
                for (int col = 0; col < vals.Length; col++) {
                    vals[col] = Cell<string>(1, col + 1);
                }
                converter = Util.StringsConverter<T>(vals);
            }
            else
                converter = Util.StringsConverter<T>();
            bool[] isTimespan = new bool[Columns];
            for (int i = 0; i < isTimespan.Length; i++) {
                isTimespan[i] = ColumnType(i + 1) == typeof(TimeSpan);
            }
            for (int row = hasHeaders ? 2 : 1; row <= Rows; row++) {
                for (int col = 1; col <= Columns; col++) {
                    //This is the real wrinkle to using reflection - Excel stores all numbers as double including int
                    ExcelRange cell = worksheet.Cells[row, col];
                    //If it is numeric it is a double since that is how excel stores all numbers
                    vals[col - 1] = isTimespan[col - 1] ? DateTime.FromOADate(cell.GetValue<double>()).ToString("h:mm:ss") : cell.GetValue<string>();
                }
                yield return converter(vals);
            }
        }

        /// <summary>
        /// Converts the Worksheet to a List.
        /// </summary>
        /// <typeparam name="T">The Type to convert the rows to.</typeparam>
        /// <returns>A List with data from the Worksheet.</returns>
        public List<T> ToList<T>() where T : new()
        {
            return AsEnumerable<T>().ToList();
        }

        /// <summary>
        /// Adds the Worksheet data to a List.
        /// </summary>
        /// <typeparam name="T">The Type to convert the rows to.</typeparam>
        /// <param name="list">The list to add data to.</param>
        /// <returns>A List with data from the Worksheet.</returns>
        public List<T> ToList<T>(List<T> list) where T : new()
        {
            list.AddRange(AsEnumerable<T>());
            return list;
        }

        /// <summary>
        /// Automatically formats string values to numbers, dates, timespans, currency, percentages, etc.
        /// </summary>
        public void AutoFormat()
        {
            int rows = worksheet.Dimension.Rows;
            int cols = worksheet.Dimension.Columns;
            for (int row = 1; row <= rows; row++) {
                for (int col = 1; col <= cols; col++) {
                    object obj = worksheet.Cells[row, col].Value;
                    if (obj is string) {
                        worksheet.Cells[row, col].Value = Parse(worksheet.Cells[row, col]);
                    }
                }
            }
        }

        private const string currencySymbols = "$¥₤€£฿₿₵¢₡₫₲₱₽₮₩₸₳ℳ₹؋₼﷼₪₭₴";

        /// <summary>
        /// Parses a string into a basic Type.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <returns>The result of the string being parsed.</returns>
        private static object Parse(ExcelRange cell)
        {
            string str = cell.Value as string;
            if (str == null)
                return null;
            string str2 = str.Trim();
            if (str2.Length == 0)
                return str;

            char c = str2[0];
            if (Char.IsDigit(c) || c == '-') {
                char last = str.Last();
                if (last == '%') {
                    if (Decimal.TryParse(str.Substring(0, str.Length - 1), out decimal d)) {
                        cell.Style.Numberformat.Format = "0.00%";
                        return d / 100m;
                    }
                    return str;
                }
                if (currencySymbols.Contains(last)) {
                    if (Decimal.TryParse(str.Substring(0, str.Length - 1), out decimal d)) {
                        cell.Style.Numberformat.Format = "0.00" + last;
                        return d;
                    }
                    return str;
                }
                for (int i = 1; i < str.Length; i++) {
                    c = str[i];
                    if (Char.IsDigit(c))
                        continue;
                    else if (c == '.') {
                        if (TimeSpan.TryParse(str, out TimeSpan ts)) {
                            cell.Style.Numberformat.Format = "h:mm:ss";
                            return ts;
                        }
                        if (Double.TryParse(str, out double d)) {
                            cell.Style.Numberformat.Format = "0.0#";
                            return d;
                        }
                    }
                    else if (c == '/' || c == '-' || c == ',') {
                        if (DateTime.TryParse(str, out DateTime dt)) {
                            cell.Style.Numberformat.Format = "M/d/yyyy H:mm:ss AM/PM";
                            return dt;
                        }
                    }
                    else if (c == ':') {
                        if (TimeSpan.TryParse(str, out TimeSpan ts)) {
                            cell.Style.Numberformat.Format = "h:mm:ss";
                            return ts;
                        }
                    }
                    return str;
                }
                if (Int32.TryParse(str, out int ival)) {
                    cell.Style.Numberformat.Format = "0";
                    return ival;
                }
            }
            else if (currencySymbols.Contains(c)) {
                if (Decimal.TryParse(str.Substring(1), out decimal d)) {
                    cell.Style.Numberformat.Format = c + "0.00";
                    return d;
                }
            }
            else if (DateTime.TryParse(str, out DateTime dt)) {
                cell.Style.Numberformat.Format = "M-d-yyyy H:mm:ss AM/PM";
                return dt;
            }
            else {
                string upper = str2.ToUpper();
                if (str == "FALSE")
                    return false;
                else if (str == "TRUE")
                    return true;
                else if (str == "NULL")
                    return null;
            }
            return str;
        }

        /// <summary>
        /// Finalizes and destructs the object.
        /// </summary>
        ~Worksheet()
        {
            worksheet = null;
        }
    }
}
