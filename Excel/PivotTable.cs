using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Table.PivotTable;
using OfficeOpenXml.Drawing.Chart;
using System.Windows.Forms;

namespace Utilities.Excel
{
    /// <summary>
    /// Wrapper for the EPPlus implementation of <see cref="ExcelPivotTable"/>.
    /// </summary>
    public class PivotTable
    {
        public ExcelPivotTable Data { get; private set; } = null;

        public PivotTable(Worksheet source, Worksheet destination, string name = " ")
        : this(source.Data.Cells[source.Data.Dimension.Address], destination, name) { }

        public PivotTable(ExcelRangeBase source, Worksheet destination, string name = " ")
        {
            Data = destination.Data.PivotTables.Add(destination.Data.Cells["B2"], source, name);
            Data.ShowHeaders = true;
            Data.RowHeaderCaption = name;
            Data.UseAutoFormatting = true;
            Data.ApplyWidthHeightFormats = true;
            Data.ShowDrill = true;
            Data.FirstHeaderRow = 1;
            Data.FirstDataCol = 1;
            Data.FirstDataRow = 2;
            Data.DataOnRows = false;
            Data.DataCaption = " ";

            Data.TableStyle = OfficeOpenXml.Table.TableStyles.Medium9;

            ////Data.MultipleFieldFilters = true;
            ////Data.RowGrandTotals = true;
            ////Data.ColumGrandTotals = true;
            ////Data.Compact = true;
            ////Data.CompactData = true;
            ////Data.GridDropZones = false;
            ////Data.Outline = false;
            ////Data.OutlineData = false;

            ////Data.ShowError = true;
            ////Data.ErrorCaption = "[error]";
            ////Data.RowHeaderCaption = "Claims";
        }

        public string Name {
            get {
                return Data.Name;
            }
            set {
                Data.Name = value;
            }
        }

        public ExcelPivotTableField AddRow(string row, eSortType sorting = eSortType.Ascending)
        {
            ExcelPivotTableField field = Data.Fields[row];
            field.Sort = sorting;
            return Data.RowFields.Add(field);
        }

        public ExcelPivotTableField AddRow(string row, string format, eSortType sorting = eSortType.Ascending)
        {
            ExcelPivotTableField field = Data.Fields[row];
            field.Sort = sorting;
            return Data.RowFields.Add(field);
        }

        public ExcelPivotTableField AddRow(int row, eSortType sorting = eSortType.Ascending)
        {
            ExcelPivotTableField field = Data.Fields[row];
            field.Sort = sorting;
            return Data.RowFields.Add(field);
        }

        public void AddRow(int row, string format, eSortType sorting = eSortType.Ascending)
        {
            ExcelPivotTableField field = Data.Fields[row];
            field.Sort = sorting;
            Data.RowFields.Add(field);
        }

        public ExcelPivotTableDataField AddData(string column, DataFieldFunction function = DataFieldFunction.None, string format = "")
        {
            ExcelPivotTableField colField = Data.Fields[column];
            ExcelPivotTableDataField dataField = Data.DataFields.Add(colField);
            dataField.Format = format;
            dataField.Function = (DataFieldFunctions) function;
            return dataField;
        }

        public ExcelPivotTableDataField AddData(int column, DataFieldFunction function = DataFieldFunction.None, string format = "")
        {
            ExcelPivotTableField colField = Data.Fields[column];
            ExcelPivotTableDataField dataField = Data.DataFields.Add(colField);
            dataField.Format = format;
            dataField.Function = (DataFieldFunctions) function;
            return dataField;
        }

        public ExcelPivotTableField AddPage(string column)
        {
            ExcelPivotTableField field = Data.Fields[column];
            return Data.PageFields.Add(field);
        }

        public ExcelPivotTableField AddPage(int column)
        {
            ExcelPivotTableField field = Data.Fields[column];
            return Data.PageFields.Add(field);
        }
    }
}
