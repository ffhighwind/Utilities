using System;
using System.Collections;
using System.Windows.Forms;

namespace Utilities.Comparers
{
	public class ListViewItemComparer : IComparer
	{
		private Type ColType = typeof(string);

		private SortOrder Sorting = SortOrder.Ascending;

		private Func<object, object, int> Comparer = System.Collections.Generic.Comparer<object>.Default.Compare;

		/// <summary>
		/// A multiplier on the result of Compare which depends on the <see cref="SortOrder"/>.
		/// </summary>
		private int Multiplier;

		public SortOrder SortOrder {
			get => Sorting;
			set {
				Sorting = value;
				switch (value) {
					case SortOrder.None:
						Multiplier = 0;
						break;
					case SortOrder.Ascending:
						Multiplier = 1;
						break;
					case SortOrder.Descending:
						Multiplier = -1;
						break;
				}
			}
		}

		public int ColumnIndex { get; set; }

		public Type ColumnType {
			get => ColType;
			set {
				ColType = value;
				Comparer = Utilities.Comparers.Comparers.GetComparer(value).Compare;
			}
		}

		public int Compare(object x, object y)
		{
			if (Sorting == SortOrder.None)
				return 0;
			object lx = (x as ListViewItem).SubItems[ColumnIndex];
			object ly = (y as ListViewItem).SubItems[ColumnIndex];
			return Multiplier * Comparer(x, y);
		}
	}
}