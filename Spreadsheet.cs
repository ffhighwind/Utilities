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
        private ExcelPackage doc = null;
        private bool disposed = false;

        /// <summary>
        /// Creates an empty Excel Spreadsheet. An exception will be thrown when trying 
        /// to access data without Opening an Excel Spreadsheet.
        /// </summary>
        public Spreadsheet() { }

        /// <summary>
        /// Opens or creates an Excel Spreadsheet.
        /// </summary>
        /// <param name="path">The path of the Excel Spreadsheet.</param>
        public Spreadsheet(string path)
        {
            Open(path, null);
        }

        /// <summary>
        /// Opens or creates an Excel Spreadsheet.
        /// </summary>
        /// <param name="path">The path of the Excel Spreadsheet.</param>
        /// <param name="password">The password to the Excel Spreadsheet.</param>
        public Spreadsheet(string path, string password)
        {
            Open(path, password);
        }

        /// <summary>
        /// Reads a stream and constructs an Excel Spreadsheet from it.
        /// </summary>
        /// <param name="stream">The stream containing the Excel data.</param>
        public Spreadsheet(Stream stream)
        {
            Open(stream, null);
        }

        /// <summary>
        /// Reads a stream and constructs an Excel Spreadsheet from it.
        /// </summary>
        /// <param name="stream">The stream containing the Excel data.</param>
        /// <param name="password">The password to the Excel Spreadsheet.</param>
        public Spreadsheet(Stream stream, string password)
        {
            Open(stream, password);
        }

        /// <summary>
        /// Opens or creates an Excel Spreadsheet.
        /// </summary>
        /// <param name="path">The path of the Excel Spreadsheet.</param>
        public bool Open(string path)
        {
            return Open(path, null);
        }

        /// <summary>
        /// Opens or creates an Excel Spreadsheet.
        /// </summary>
        /// <param name="path">The path of the Excel Spreadsheet.</param>
        /// <param name="password">The password to the Excel Spreadsheet.</param>
        public bool Open(string path, string password)
        {
            try {
                FileInfo fi = new FileInfo(path);
                if (doc != null && !doc.File.Equals(fi)) {
                    doc.Dispose();
                    doc = null;
                }
                //if (fi.Exists && fi.IsReadOnly)
                //    doc = new ExcelPackage(new FileStream(fi.FullName, FileMode.OpenOrCreate, FileAccess.Read), password);
                //else
                if (doc == null)
                    doc = new ExcelPackage(fi, password);
                if (doc != null && doc.Workbook.Worksheets.Count == 0)
                    Add();
                return doc != null;
            }
            catch (Exception ex) {
                string msg = (path != null && !path.EndsWith(".xlsx")) ? "wrong file extension." : ex.Message;
                Console.Error.WriteLine("Error Excel.Spreadsheet.Open({0}): {1}", path, msg);
            }
            return false;
        }

        /// <summary>
        /// Returns whether the Spreadsheet is currently open and usable.
        /// </summary>
        public bool IsOpen {
            get { return doc != null; }
        }

        /// <summary>
        /// Reads a stream and constructs an Excel Spreadsheet from it.
        /// </summary>
        /// <param name="stream">The stream containing the Excel data.</param>
        /// <returns>True on success. False on failure.</returns>
        public bool Open(Stream stream)
        {
            return Open(stream, null);
        }

        /// <summary>
        /// Reads a stream and constructs an Excel Spreadsheet from it.
        /// </summary>
        /// <param name="stream">The stream containing the Excel data.</param>
        /// <param name="password">The password to the Excel Spreadsheet.</param>
        /// <returns>True on success. False on failure.</returns>
        public bool Open(Stream stream, string password)
        {
            try {
                if (doc != null) {
                    doc.Dispose();
                    doc = null;
                }
                if (doc == null)
                    doc = new ExcelPackage(stream, password);
                if (doc != null && doc.Workbook.Worksheets.Count == 0)
                    Add();
                return doc != null;
            }
            catch (Exception ex) {
                Console.Error.WriteLine("Error Spreadsheet.Open(Stream): " + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Clears all data in the Excel Spreadsheet.
        /// </summary>
        public void Clear()
        {
            for (int i = doc.Workbook.Worksheets.Count - 1; i >= 0; i--) {
                doc.Workbook.Worksheets.Delete(i + (doc.Compatibility.IsWorksheets1Based ? 1 : 0));
            }
            Add();
        }

        /// <summary>
        /// Loads a DataSet into the Excel Spreadsheet.
        /// </summary>
        /// <param name="dataset">The DataSet to load.</param>
        /// <param name="printHeaders">Determines if the first row of each DataTable includes headers.</param>
        /// <param name="useTableNames">Determines if the DataTable names will be used for the sheet names.</param>
        public void Load(DataSet dataset, bool printHeaders = true, bool useTableNames = true)
        {
            for (int i = doc.Workbook.Worksheets.Count - 1; i >= 0; i--) {
                doc.Workbook.Worksheets.Delete(i + (doc.Compatibility.IsWorksheets1Based ? 1 : 0));
            }
            for (int i = 0; i < dataset.Tables.Count; i++) {
                DataTable table = dataset.Tables[i];
                Worksheet ws = useTableNames ? Add(table.TableName) : Add();
                if (table.Rows.Count > 0 || printHeaders)
                    ws.Load(table, printHeaders);
            }
        }

        /// <summary>
        /// Loads a DataTable into the Excel Spreadsheet.
        /// </summary>
        /// <param name="table">The DataTable to load.</param>
        /// <param name="printHeaders">Determines if the first row of the DataTable includes headers.</param>
        /// <param name="useTableNames">Determines if the DataTable name will be used for the sheet name.</param>
        public void Load(DataTable table, bool printHeaders = true, bool useTableName = true)
        {
            Clear();
            ExcelWorksheet ws = doc.Workbook.Worksheets.First();
            ws.Name = useTableName ? table.TableName : "Sheet1";
            new Worksheet(ws).Load(table, printHeaders);
        }

        /// <summary>
        /// Loads a DataTable into the Excel Spreadsheet.
        /// </summary>
        /// <param name="reader">The IDataReader to load.</param>
        /// <param name="printHeaders">Determines if the first row of the IDataReader includes headers.</param>
        /// <param name="sheetname">The name of the new Worksheet.</param>
        public void Load(IDataReader reader, bool printHeaders = true, string sheetname = "Sheet1")
        {
            Clear();
            ExcelWorksheet ws = doc.Workbook.Worksheets.First();
            ws.Name = sheetname;
            new Worksheet(ws).Load(reader, printHeaders);
        }

        /// <summary>
        /// Loads an enumerable list of objects into the Excel Spreadsheet.
        /// </summary>
        /// <param name="list">The enumerable list to load.</param>
        /// <param name="sheetname">The name of the new Worksheet.</param>
        public void Load(IEnumerable<object[]> list, string sheetname = "Sheet1")
        {
            Clear();
            ExcelWorksheet ws = doc.Workbook.Worksheets.First();
            ws.Name = sheetname;
            new Worksheet(ws).Load(list);
        }

        /// <summary>
        /// Loads an enumerable list into the Excel Spreadsheet.
        /// </summary>
        /// <typeparam name="T">The type of objects in the enumerable list.</typeparam>
        /// <param name="list">The enumerable list to load.</param>
        /// <param name="sheetname">The name of the new Worksheet.</param>
        public void Load<T>(IEnumerable<T> list, string sheetname = "Sheet1")
        {
            Clear();
            ExcelWorksheet ws = doc.Workbook.Worksheets.First();
            ws.Name = sheetname;
            new Worksheet(ws).Load(list);
        }

        /// <summary>
        /// Loads comma-separated text into the Excel Spreadsheet.
        /// </summary>
        /// <param name="csvtext">The comma-separated text to load from.</param>
        /// <param name="sheetname">The name of the new Worksheet.</param>
        public void Load(string csvtext, string sheetname = "Sheet1")
        {
            Clear();
            ExcelWorksheet ws = doc.Workbook.Worksheets.First();
            ws.Name = sheetname;
            new Worksheet(ws).Load(csvtext);
        }

        /// <summary>
        /// Reads the data from the Excel Spreadsheet into a DataSet.
        /// </summary>
        /// <param name="dataset">The DataSet to modify.</param>
        /// <param name="hasHeaders">Determines if the first rows of each sheet include headers.</param>
        /// <returns>The modified DataSet.</returns>
        public DataSet ToDataSet(DataSet dataset, bool hasHeaders = true)
        {
            for (int i = 0; i < doc.Workbook.Worksheets.Count; i++) {
                DataTable table = this[i].ToDataTable(hasHeaders);
                table.TableName = this[i].Name;
                dataset.Tables.Add(table);
            }
            return dataset;
        }

        /// <summary>
        /// Creates a new DataSet from the Excel Spreadsheet.
        /// </summary>
        /// <param name="hasHeaders">Determines if the first rows of each sheet include headers.</param>
        /// <returns>The modified DataSet.</returns>
        public DataSet ToDataSet(bool hasHeaders = true)
        {
            DataSet dataset = new DataSet();
            return ToDataSet(dataset, hasHeaders);
        }

        /// <summary>
        /// The number of Worksheets in the Excel Spreadsheet.
        /// </summary>
        public int Sheets {
            get {
                return doc.Workbook.Worksheets.Count;
            }
        }

        /// <summary>
        /// The number of Worksheets in the Excel Spreadsheet.
        /// </summary>
        public IEnumerable<Worksheet> Worksheets {
            get {
                for (int i = 0; i < doc.Workbook.Worksheets.Count; i++)
                    yield return this[i];
            }
        }

        public bool AutoFilter {
            set {
                for (int i = 0; i < doc.Workbook.Worksheets.Count; i++) {
                    ExcelWorksheet worksheet = doc.Workbook.Worksheets[i + (doc.Compatibility.IsWorksheets1Based ? 1 : 0)];
                    worksheet.Cells[worksheet.Dimension.Address].AutoFilter = value;
                }
            }
        }

        public void AutoFormat()
        {
            for (int i = 0; i < doc.Workbook.Worksheets.Count; i++)
                this[i].AutoFormat();
        }

        public void AutoFit()
        {
            for (int i = 0; i < doc.Workbook.Worksheets.Count; i++) {
                ExcelWorksheet worksheet = doc.Workbook.Worksheets[i + (doc.Compatibility.IsWorksheets1Based ? 1 : 0)];
                worksheet.Cells.AutoFitColumns(0);
                /*
                for (int col = 1; col <= worksheet.Dimension.Columns; col++) {
                    worksheet.Column(col).AutoFit();
                } */
            }
        }

        public bool BestFit {
            set {
                for (int i = 0; i < doc.Workbook.Worksheets.Count; i++) {
                    ExcelWorksheet worksheet = doc.Workbook.Worksheets[i + (doc.Compatibility.IsWorksheets1Based ? 1 : 0)];
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++) {
                        worksheet.Column(col).BestFit = value;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new Worksheet to the Excel Spreadsheet.
        /// </summary>
        public Worksheet Add()
        {
            for (int i = doc.Workbook.Worksheets.Count + 1; ;) {
                if (doc.Workbook.Worksheets["Sheet" + i.ToString()] == null) {
                    return Add("Sheet" + (doc.Workbook.Worksheets.Count + 1));
                }
            }
        }

        /// <summary>
        /// Adds a new Worksheet to the Excel Spreadsheet.
        /// </summary>
        /// <param name="sheetname">The name of the Worksheet to add.</param>
        public Worksheet Add(string sheetname)
        {
            var ws = doc.Workbook.Worksheets.Add(sheetname);
            ws.Cells["A1"].Value = "";
            return new Worksheet(ws);
        }

        /// <summary>
        /// Removes a Worksheet from the Excel Spreadsheet.
        /// </summary>
        /// <param name="sheetname">The name of the Worksheet to remove.</param>
        public void Remove(string sheetname)
        {
            doc.Workbook.Worksheets.Delete(sheetname);
        }

        /// <summary>
        /// Removes a Worksheet from the Excel Spreadsheet.
        /// </summary>
        /// <param name="index">The index of the Worksheet to remove.</param>
        public void Remove(int index)
        {
            if (doc.Compatibility.IsWorksheets1Based)
                index += 1;
            doc.Workbook.Worksheets.Delete(index);
        }

        /// <summary>
        /// Gets a Worksheet from the Excel Spreadsheet.
        /// </summary>
        /// <param name="sheetname">The name of the Worksheet.</param>
        /// <returns></returns>
        public Worksheet this[string sheetname] {
            get {
                ExcelWorksheet worksheet = doc.Workbook.Worksheets[sheetname];
                return worksheet == null ? null : new Worksheet(worksheet);
            }
        }

        /// <summary>
        /// Gets a Worksheet from the Excel Spreadsheet.
        /// </summary>
        /// <param name="sheetIndex">The index of the Worksheet (base 0).</param>
        /// <returns></returns>
        public Worksheet this[int sheetIndex] {
            get {
                if (doc.Compatibility.IsWorksheets1Based)
                    sheetIndex += 1;
                ExcelWorksheet worksheet = doc.Workbook.Worksheets[sheetIndex];
                return worksheet == null ? null : new Worksheet(doc.Workbook.Worksheets[sheetIndex]);
            }
        }

        /// <summary>
        /// The FileInfo for the Excel Spreadsheet.
        /// </summary>
        public FileInfo File {
            get { return doc.File; }
            set { doc.File = value; }
        }

        /// <summary>
        /// Information about the Excel Spreadsheet.
        /// </summary>
        public OfficeProperties Properties {
            get { return doc.Workbook.Properties; }
        }

        /// <summary>
        /// The title of the Excel Spreadsheet.
        /// </summary>
        public string Title {
            get {
                return doc.Workbook.Properties.Title;
            }
            set {
                doc.Workbook.Properties.Title = value;
            }
        }

        /// <summary>
        /// The author of the Excel Spreadsheet.
        /// </summary>
        public string Author {
            get {
                return doc.Workbook.Properties.Author;
            }
            set {
                doc.Workbook.Properties.Author = value;
            }
        }

        /// <summary>
        /// The company name of the Excel Spreadsheet.
        /// </summary>
        public string Company {
            get {
                return doc.Workbook.Properties.Company;
            }
            set {
                doc.Workbook.Properties.Company = value;
            }
        }

        /// <summary>
        /// Saves the Excel Spreadsheet.
        /// </summary>
        public void Save()
        {
            doc.Save();
        }

        /// <summary>
        /// Saves and encrypts the Excel Spreadsheet with a password.
        /// </summary>
        /// <param name="password">The password to the Excel Spreadsheet.</param>
        public void Save(string password)
        {
            doc.Save(password);
        }

        /// <summary>
        /// Saves the Excel Spreadsheet.
        /// </summary>
        /// <param name="path">The file to create.</param>
        public void SaveAs(string path)
        {
            doc.SaveAs(new FileInfo(path));
        }

        /// <summary>
        /// Saves and encrypts the Excel Spreadsheet with a password.
        /// </summary>
        /// <param name="path">The file to create.</param>
        /// <param name="password">The password to the Excel Spreadsheet.</param>
        public void SaveAs(string path, string password)
        {
            doc.SaveAs(new FileInfo(path), password);
        }

        /// <summary>
        /// The EPPlus implementation of the Spreadsheet (Package).
        /// </summary>
        public ExcelPackage Data {
            get {
                return doc;
            }
        }

        /// <summary>
        /// Disposes of the object and releases all data.
        /// </summary>
        /// <param name="disposing">Determines if the object is currently being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    if (doc != null) {
                        doc.Dispose();
                        doc = null;
                    }
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Finalizes and destructs the object.
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
