using OfficeOpenXml.Table.PivotTable;

namespace Utilities.Excel
{
	/// <summary>
	/// Sorting methods for PivotTables. This reduces the need to include EPPlus in using statements and references.
	/// </summary>
	public enum Sorting
	{
		None = eSortType.None,
		Ascending = eSortType.Ascending,
		Descending = eSortType.Descending
	}
}
