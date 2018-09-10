using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.Table.PivotTable;

namespace Utilities.Excel
{
    public enum DataFieldFunction
    {
        Average = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.Average,
        Count = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.Count,
        CountNums = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.CountNums,
        Max = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.Max,
        Min = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.Min,
        None = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.None,
        Product = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.Product,
        StdDev = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.StdDev,
        StdDevP = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.StdDevP,
        Sum = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.Sum,
        Var = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.Var,
        VarP = OfficeOpenXml.Table.PivotTable.DataFieldFunctions.VarP,
    }
}
