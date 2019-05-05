using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Utilities.Comparers
{
	/// <summary>
	/// An <seealso cref="IEqualityComparer{T}"/> for <see cref="DataRow"/> objects. 
	/// This is used for removing duplicate rows from a <see cref="DataTable"/>.
	/// </summary>
	public class DataRowEqualityComparer : IEqualityComparer<DataRow>
	{
		/// <summary>
		/// The default <see cref="DataRowEqualityComparer"/>. This compares all elements in a <see cref="DataRow"/>.ItemArray.
		/// </summary>
		public static DataRowEqualityComparer Default { get; } = new DataRowEqualityComparer();

		/// <summary>
		/// Creates a <see cref="DataRowEqualityComparer"/> for comparing <see cref="DataRow"/> objects that follow the same schema.
		/// </summary>
		/// <param name="table">The <see cref="DataTable"/> to create a comparer from.</param>
		/// <param name="columnNames">The <see cref="DataColumn"/> names to compare.</param>
		/// <returns>A <see cref="DataRowEqualityComparer"/> for comparing <see cref="DataRow"/> objects.</returns>
		public static DataRowEqualityComparer Create(DataTable table, params string[] columnNames)
		{
			if (columnNames.Length == 0)
				return Default;
			int[] colIndexes = table.Columns.Cast<DataColumn>().AsEnumerable().Where(col => columnNames.Contains(col.ColumnName)).Select(col => col.Ordinal).ToArray();
			if (colIndexes.Length == table.Columns.Count)
				return Default;
			return new DataRowEqualityComparerAll(colIndexes);
		}

		/// <summary>
		/// Creates a <see cref="DataRowEqualityComparer"/> for comparing <see cref="DataRow"/> objects that follow the same schema.
		/// </summary>
		/// <param name="columnIndexes">The indexes to use when determining if two <see cref="DataRow"/> objects are equal.</param>
		/// <returns>A <see cref="DataRowEqualityComparer"/> for comparing <see cref="DataRow"/> objects.</returns>
		public static DataRowEqualityComparer Create(params int[] columnIndexes)
		{
			return new DataRowEqualityComparerAll(columnIndexes);
		}

		/// <summary>
		/// Returns the hash code of a <see cref="DataRow"/>.
		/// </summary>
		/// <param name="obj">The <see cref="DataRow"/>.</param>
		/// <returns>The hash code.</returns>
		int IEqualityComparer<DataRow>.GetHashCode(DataRow obj)
		{
			return GetHashCode(obj);
		}

		/// <summary>
		/// Returns the hash code of a <see cref="DataRow"/>.
		/// </summary>
		/// <param name="obj">The <see cref="DataRow"/>.</param>
		/// <returns>The hash code.</returns>
		public static int GetHashCode(DataRow obj)
		{
			int hashcode = 749;
			for (int i = 0; i < obj.ItemArray.Length; i++) {
				hashcode ^= obj.ItemArray[i].GetHashCode();
			}
			return hashcode;
		}

		/// <summary>
		/// Determines if two <see cref="DataRow"/> objects are equal.
		/// </summary>
		/// <param name="x">The first <see cref="DataRow"/>.</param>
		/// <param name="y">The second <see cref="DataRow"/>.</param>
		/// <returns>True if two <see cref="DataRow"/> objects are equal. False otherwise.</returns>
		bool IEqualityComparer<DataRow>.Equals(DataRow x, DataRow y)
		{
			bool result = x.ItemArray.Length == y.ItemArray.Length;
			if (result) {
				for (int i = 0; i < x.ItemArray.Length; i++) {
					result = x.ItemArray[i].Equals(y.ItemArray[i]);
					if (!result)
						break;
				}
			}
			return result;
		}

		/// <summary>
		/// An <see cref="IEqualityComparer{T}"/> for <see cref="DataRow"/>. This compares a subset of the elements in <see cref="DataRow"/>.ItemArray.
		/// </summary>
		private class DataRowEqualityComparerAll : DataRowEqualityComparer, IEqualityComparer<DataRow>
		{
			/// <summary>
			/// The column indexes to compare for equality.
			/// </summary>
			private readonly int[] columnIndexes;

			/// <summary>
			/// Initializes a new instance of the <see cref="DataRowEqualityComparerAll"/> class.
			/// </summary>
			/// <param name="columnIndexes">The column indexes to compare for equality.</param>
			public DataRowEqualityComparerAll(params int[] columnIndexes)
			{
				this.columnIndexes = columnIndexes;
			}

			/// <summary>
			/// Returns the hash code of a <see cref="DataRow"/>.
			/// </summary>
			/// <param name="obj">The <see cref="DataRow"/>.</param>
			/// <returns>The hash code.</returns>
			int IEqualityComparer<DataRow>.GetHashCode(DataRow obj)
			{
				int hashcode = 0;
				for (int i = 0; i < columnIndexes.Length; i++) {
					hashcode ^= obj[columnIndexes[i]].GetHashCode();
				}
				return hashcode;
			}

			/// <summary>
			/// Determines if two <see cref="DataRow"/> objects are equal.
			/// </summary>
			/// <param name="x">The first <see cref="DataRow"/>.</param>
			/// <param name="y">The second <see cref="DataRow"/>.</param>
			/// <returns>True if two <see cref="DataRow"/> objects are equal. False otherwise.</returns>
			bool IEqualityComparer<DataRow>.Equals(DataRow x, DataRow y)
			{
				bool result = x.ItemArray.Length == y.ItemArray.Length;
				if (result) {
					for (int i = 0; i < columnIndexes.Length; i++) {
						object xVal = x.ItemArray[columnIndexes[i]];
						object yVal = y.ItemArray[columnIndexes[i]];
						if(xVal == null) {
							result = yVal == null;
						}
						else {
							result = xVal.Equals(yVal);
						}
						if (!result)
							break;
					}
				}
				return result;
			}
		}
	}
}