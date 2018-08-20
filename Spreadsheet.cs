using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace Utilities.Excel
{
    /// <summary>
    /// An Excel Spreadsheet wrapper for the EPPlus implementation of ExcelPackage.
    /// </summary>
    public class Spreadsheet : IDisposable
    {
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spreadsheet"/> class.
        /// </summary>
        public Spreadsheet() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spreadsheet"/> class. This opens the file if it exists.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        public Spreadsheet(string path)
        {
            Open(path, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spreadsheet"/> class. This opens the file if it exists.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="password">The password to the file.</param>
        public Spreadsheet(string path, string password)
        {
            Open(path, password);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spreadsheet"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the data.</param>
        public Spreadsheet(Stream stream)
        {
            Open(stream, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spreadsheet"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the data.</param>
        /// <param name="password">The password to the <see cref="Stream"/>.</param>
        public Spreadsheet(Stream stream, string password)
        {
            Open(stream, password);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spreadsheet"/> class. This opens the file if it exists.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        public void Open(string path)
        {
            Open(path, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spreadsheet"/> class. This opens the file if it exists.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="password">The password to the file.</param>
        public void Open(string path, string password)
        {
            try {
                FileInfo fi = new FileInfo(path);
                if (Data != null) {
                    if (Data.File.Equals(fi))
                        return;
                    Data.Dispose();
                    Data = null;
                }
                Data = new ExcelPackage(fi, password);
                if (Data.Workbook.Worksheets.Count == 0)
                    Add();
            }
            catch (Exception ex) {
                if (path != null && !path.EndsWith(".xlsx"))
                    throw new IOException("Invalid file extension: " + path);
                throw ex;
            }
        }

        /// <summary>
        /// Returns whether the <see cref="Spreadsheet"/> is currently open and usable.
        /// </summary>
        public bool IsOpen => Data != null;

        /// <summary>
        /// Reads a <see cref="Stream"/> and constructs a <see cref="Spreadsheet"/> from it.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the Excel data.</param>
        public void Open(Stream stream)
        {
            Open(stream, null);
        }

        /// <summary>
        /// Reads a <see cref="Stream"/> and constructs a <see cref="Spreadsheet"/> from it.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the data.</param>
        /// <param name="password">The password to the <see cref="Spreadsheet"/>.</param>
        public void Open(Stream stream, string password)
        {
            if (Data != null) {
                if (Data.Stream == stream)
                    return;
                Data.Dispose();
                Data = null;
            }
            if (Data == null)
                Data = new ExcelPackage(stream, password);
            if (Data.Workbook.Worksheets.Count == 0)
                Add();
        }

        /// <summary>
        /// Clears the <see cref="Spreadsheet"/>. This removes every <see cref="Worksheet"/> and creates a blank one unless otherwise specified.
        /// </summary>
        /// <param name="removeAllSheets">Determines if every <see cref="Worksheet"/> is removed.
        /// By default an empty <see cref="Worksheet"/> will be added to ensure that the file is valid.</param>
        public void Clear(bool removeAllSheets = false)
        {
            for (int i = Data.Workbook.Worksheets.Count - 1; i >= 0; i--) {
                Data.Workbook.Worksheets.Delete(i + (Data.Compatibility.IsWorksheets1Based ? 1 : 0));
            }
            if (!removeAllSheets)
                Add();
        }

        /// <summary>
        /// Loads a <see cref="DataSet"/> into a <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="dataset">The <see cref="DataSet"/> to load.</param>
        /// <param name="printHeaders">Determines if the first row of each <see cref="DataTable"/> includes headers.</param>
        /// <param name="useTableNames">Determines if the <see cref="DataTable"/> names will be used for the <see cref="Worksheet"/> names.</param>
        public void Load(DataSet dataset, bool printHeaders = true, bool useTableNames = true)
        {
            for (int i = Data.Workbook.Worksheets.Count - 1; i >= 0; i--) {
                Data.Workbook.Worksheets.Delete(i + (Data.Compatibility.IsWorksheets1Based ? 1 : 0));
            }
            for (int i = 0; i < dataset.Tables.Count; i++) {
                DataTable table = dataset.Tables[i];
                Worksheet ws = useTableNames ? Add(table.TableName) : Add();
                if (table.Rows.Count > 0 || printHeaders)
                    ws.Load(table, printHeaders);
            }
        }

        /// <summary>
        /// Loads a <see cref="DataTable"/> into a <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/> to load.</param>
        /// <param name="printHeaders">Determines if the first row of the <see cref="DataTable"/> includes headers.</param>
        /// <param name="useTableName">Determines if the <see cref="DataTable"/> name will be used for the <see cref="Worksheet"/> name.</param>
        public void Load(DataTable table, bool printHeaders = true, bool useTableName = true)
        {
            Clear();
            ExcelWorksheet ws = Data.Workbook.Worksheets.First();
            ws.Name = useTableName ? table.TableName : "Sheet1";
            new Worksheet(ws).Load(table, printHeaders);
        }

        /// <summary>
        /// Loads an <see cref="IDataReader"/> into a <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IDataReader"/> to load.</param>
        /// <param name="printHeaders">Determines if the first row of the <see cref="IDataReader"/> includes headers.</param>
        /// <param name="sheetName">The name of the new <see cref="Worksheet"/>.</param>
        public void Load(IDataReader reader, bool printHeaders = true, string sheetName = "Sheet1")
        {
            Clear();
            ExcelWorksheet ws = Data.Workbook.Worksheets.First();
            ws.Name = sheetName;
            new Worksheet(ws).Load(reader, printHeaders);
        }

        /// <summary>
        /// Loads an <see cref="IEnumerable{T}"/> of objects into a <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="list">The <see cref="IEnumerable{T}"/> to load.</param>
        /// <param name="sheetName">The name of the new <see cref="Worksheet"/>.</param>
        public void Load(IEnumerable<object[]> list, string sheetName = "Sheet1")
        {
            Clear();
            ExcelWorksheet ws = Data.Workbook.Worksheets.First();
            ws.Name = sheetName;
            new Worksheet(ws).Load(list);
        }

        /// <summary>
        /// Loads an <see cref="IEnumerable{T}"/> into a <see cref="Spreadsheet"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of objects in the <see cref="IEnumerable{T}"/>.</typeparam>
        /// <param name="list">The <see cref="IEnumerable{T}"/> to load.</param>
        /// <param name="sheetName">The name of the new <see cref="Worksheet"/>.</param>
        public void Load<T>(IEnumerable<T> list, string sheetName = "Sheet1")
        {
            Clear();
            ExcelWorksheet ws = Data.Workbook.Worksheets.First();
            ws.Name = sheetName;
            new Worksheet(ws).Load(list);
        }

        /// <summary>
        /// Loads comma-separated text into a <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="csvText">The comma-separated text to load.</param>
        /// <param name="sheetName">The name of the new <see cref="Worksheet"/>.</param>
        public void Load(string csvText, string sheetName = "Sheet1")
        {
            Clear();
            ExcelWorksheet ws = Data.Workbook.Worksheets.First();
            ws.Name = sheetName;
            new Worksheet(ws).Load(csvText);
        }

        /// <summary>
        /// Reads the data from the <see cref="Spreadsheet"/> into a <see cref="DataSet"/>.
        /// </summary>
        /// <param name="dataset">The <see cref="DataSet"/> to modify.</param>
        /// <param name="hasHeaders">Determines if the first rows of each <see cref="Worksheet"/> includes headers.</param>
        /// <returns>The <see cref="DataSet"/>.</returns>
        public DataSet ToDataSet(DataSet dataset, bool hasHeaders = true)
        {
            for (int i = 0; i < Data.Workbook.Worksheets.Count; i++) {
                DataTable table = this[i].ToDataTable(hasHeaders);
                table.TableName = this[i].Name;
                dataset.Tables.Add(table);
            }
            return dataset;
        }

        /// <summary>
        /// Creates a <see cref="DataSet"/> from the <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="hasHeaders">Determines if the first rows of each <see cref="Worksheet"/> includes headers.</param>
        /// <returns>The <see cref="DataSet"/>.</returns>
        public DataSet ToDataSet(bool hasHeaders = true)
        {
            DataSet dataset = new DataSet();
            return ToDataSet(dataset, hasHeaders);
        }

        /// <summary>
        /// The <see cref="Worksheet"/> count in the <see cref="Spreadsheet"/>.
        /// </summary>
        public int Sheets => Data.Workbook.Worksheets.Count;

        /// <summary>
        /// The Worksheets in the <see cref="Spreadsheet"/>.
        /// </summary>
        public IEnumerable<Worksheet> Worksheets {
            get {
                for (int i = 0; i < Data.Workbook.Worksheets.Count; i++)
                    yield return this[i];
            }
        }

        /// <summary>
        /// Auto-filtering for all columns in the <see cref="Spreadsheet"/>. This allows sorting and filtering of data.
        /// </summary>
        public bool AutoFilter {
            set {
                for (int i = 0; i < Data.Workbook.Worksheets.Count; i++) {
                    ExcelWorksheet worksheet = Data.Workbook.Worksheets[i + (Data.Compatibility.IsWorksheets1Based ? 1 : 0)];
                    worksheet.Cells[worksheet.Dimension.Address].AutoFilter = value;
                }
            }
        }

        /// <summary>
        /// Converts all string values to numbers, dates, timespans, currencies, percentages, etc.
        /// </summary>
        public void AutoFormat()
        {
            for (int i = 0; i < Data.Workbook.Worksheets.Count; i++)
                this[i].AutoFormat();
        }

        /// <summary>
        /// Resizes the columns to fit the data. Cells with wrapped text and formulas are not counted for this.
        /// </summary>
        /// <param name="minimumWidth">The minimum width of all columns.</param>
        public void AutoFit(double minimumWidth = 0)
        {
            for (int i = 0; i < Data.Workbook.Worksheets.Count; i++) {
                ExcelWorksheet worksheet = Data.Workbook.Worksheets[i + (Data.Compatibility.IsWorksheets1Based ? 1 : 0)];
                worksheet.Cells.AutoFitColumns(minimumWidth);
            }
        }

        /// <summary>
        /// Adds a new <see cref="Worksheet"/> to the <see cref="Spreadsheet"/>.
        /// </summary>
        /// <returns>The <see cref="Worksheet"/>.</returns>
        public Worksheet Add()
        {
            for (int i = Data.Workbook.Worksheets.Count + 1; ; i++) {
                string sheetName = "Sheet" + i.ToString();
                if (Data.Workbook.Worksheets[sheetName] == null) {
                    return Add(sheetName);
                }
            }
        }

        /// <summary>
        /// Adds a new <see cref="Worksheet"/> to the <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="sheetName">The name of the <see cref="Worksheet"/> to add.</param>
        /// <returns>The <see cref="Worksheet"/>.</returns>
        public Worksheet Add(string sheetName)
        {
            ExcelWorksheet ws = Data.Workbook.Worksheets.Add(sheetName);
            ws.Cells["A1"].Value = "";
            return new Worksheet(ws);
        }

        /// <summary>
        /// Removes a <see cref="Worksheet"/> from the <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="sheetName">The name of the <see cref="Worksheet"/> to remove.</param>
        public void Remove(string sheetName)
        {
            Data.Workbook.Worksheets.Delete(sheetName);
        }

        /// <summary>
        /// Removes a <see cref="Worksheet"/> from the <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="index">The index of the <see cref="Worksheet"/> to remove.</param>
        public void Remove(int index)
        {
            if (Data.Compatibility.IsWorksheets1Based)
                index += 1;
            Data.Workbook.Worksheets.Delete(index);
        }

        /// <summary>
        /// Gets a <see cref="Worksheet"/> from the <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="sheetName">The name of the <see cref="Worksheet"/>.</param>
        /// <returns>The <see cref="Worksheet"/>.</returns>
        public Worksheet this[string sheetName] => new Worksheet(Data.Workbook.Worksheets[sheetName]);

        /// <summary>
        /// Gets a Worksheet from the Excel Spreadsheet.
        /// </summary>
        /// <param name="sheetIndex">The index of the Worksheet (base 0).</param>
        /// <returns>The <see cref="Worksheet"/>.</returns>
        public Worksheet this[int sheetIndex] {
            get {
                if (Data.Compatibility.IsWorksheets1Based)
                    sheetIndex += 1;
                return new Worksheet(Data.Workbook.Worksheets[sheetIndex]);
            }
        }

        /// <summary>
        /// The <see cref="FileInfo"/> for the <see cref="Spreadsheet"/>.
        /// </summary>
        public FileInfo File {
            get => Data.File;
            set => Data.File = value;
        }

        /// <summary>
        /// Freezes the top row on every <see cref="Worksheet"/> (locks it for visibility).
        /// </summary>
        public void FreezePanes()
        {
            for (int i = 0; i < Data.Workbook.Worksheets.Count; i++) {
                ExcelWorksheet worksheet = Data.Workbook.Worksheets[i + (Data.Compatibility.IsWorksheets1Based ? 1 : 0)];
                worksheet.View.FreezePanes(2, 1);
            }
        }

        /// <summary>
        /// Properties of the <see cref="Spreadsheet"/>.
        /// </summary>
        public OfficeProperties Properties => Data.Workbook.Properties;

        /// <summary>
        /// The title of the <see cref="Spreadsheet"/>.
        /// </summary>
        public string Title {
            get => Data.Workbook.Properties.Title;
            set => Data.Workbook.Properties.Title = value;
        }

        /// <summary>
        /// The author of the <see cref="Spreadsheet"/>.
        /// </summary>
        public string Author {
            get => Data.Workbook.Properties.Author;
            set => Data.Workbook.Properties.Author = value;
        }

        /// <summary>
        /// The company name of the <see cref="Spreadsheet"/>.
        /// </summary>
        public string Company {
            get => Data.Workbook.Properties.Company;
            set => Data.Workbook.Properties.Company = value;
        }

        /// <summary>
        /// Saves the <see cref="Spreadsheet"/>.
        /// </summary>
        public void Save()
        {
            Data.Save();
        }

        /// <summary>
        /// Saves and encrypts the <see cref="Spreadsheet"/> with a password.
        /// </summary>
        /// <param name="password">The password to the <see cref="Spreadsheet"/>.</param>
        public void Save(string password)
        {
            Data.Save(password);
        }

        /// <summary>
        /// Saves the <see cref="Spreadsheet"/>.
        /// </summary>
        /// <param name="path">The file to create.</param>
        public void SaveAs(string path)
        {
            Data.SaveAs(new FileInfo(path));
        }

        /// <summary>
        /// Saves and encrypts the <see cref="Spreadsheet"/> with a password.
        /// </summary>
        /// <param name="path">The file to create.</param>
        /// <param name="password">The password to the <see cref="Spreadsheet"/>.</param>
        public void SaveAs(string path, string password)
        {
            Data.SaveAs(new FileInfo(path), password);
        }

        /// <summary>
        /// The <see cref="ExcelPackage"/> representation of the <see cref="Spreadsheet"/>.
        /// </summary>
        public ExcelPackage Data { get; private set; } = null;

        /// <summary>
        /// Disposes of the object and releases all data.
        /// </summary>
        /// <param name="disposing">Determines if the object is currently being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    Data.Dispose();
                    Data = null;
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Spreadsheet"/> class.
        /// </summary>
        ~Spreadsheet()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the object and releases all data.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
