using System;
using System.Collections;
using System.Windows.Forms;

namespace Utilities.Comparers
{
    public class ListViewItemComparer : IComparer
    {
        private Type ColType;

        private SortOrder Sorting { get; set; } = SortOrder.Ascending;

        public SortOrder SortOrder {
            get => Sorting;
            set {
                Sorting = value;
                if (Sorting == SortOrder.Ascending) {
                    Multiplier = 1;
                }
                else if (Sorting == SortOrder.Descending)
                    Multiplier = -1;
                else
                    Multiplier = 0;
            }
        }

        private int Multiplier;

        public int ColumnIndex { get; set; }

        public Type ColumnType {
            get => ColType;
            set {
                ColType = value;
                CompareFunc = ParseComparer.GetComparer(ColType).Compare;
            }
        }

        private Func<object, object, int> CompareFunc { get; set; } = ParseComparer.String.Compare;

        public int Compare(object x, object y)
        {
            if (Multiplier == 0)
                return 0;
            string lx = (x as ListViewItem).SubItems[ColumnIndex].Text;
            string ly = (y as ListViewItem).SubItems[ColumnIndex].Text;
            if (lx == ly)
                return 0;
            else if (lx == null) {
                if (ly == null)
                    return 0;
                return -1;
            }
            else if (ly == null)
                return 1;
            try {
                return Multiplier * CompareFunc(lx, ly);
            }
            catch { // do nothing
            }
            return lx.CompareTo(ly);
        }
    }
}
