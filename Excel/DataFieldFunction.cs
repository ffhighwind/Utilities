using OfficeOpenXml.Table.PivotTable;

namespace Utilities.Excel
{
    /// <summary>
    /// Functions for PivotTables. This reduces the need to include EPPlus in using statements and references.
    /// </summary>
    public enum DataFieldFunction
    {
        Average = DataFieldFunctions.Average,
        Count = DataFieldFunctions.Count,
        CountNums = DataFieldFunctions.CountNums,
        Max = DataFieldFunctions.Max,
        Min = DataFieldFunctions.Min,
        None = DataFieldFunctions.None,
        Product = DataFieldFunctions.Product,
        StdDev = DataFieldFunctions.StdDev,
        StdDevP = DataFieldFunctions.StdDevP,
        Sum = DataFieldFunctions.Sum,
        Var = DataFieldFunctions.Var,
        VarP = DataFieldFunctions.VarP,
    }
}
