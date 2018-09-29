using System;
using System.Collections;
using System.Windows.Forms;

namespace Utilities.Comparers
{
    public class ListViewItemComparer : IComparer
    {
        private Type ColType = typeof(string);

        private SortOrder Sorting { get; set; } = SortOrder.Ascending;

        private Func<string, string, int> Comparer { get; set; } = DefaultCompareFunc;
        private Func<object, object> Converter { get; set; }

        /// <summary>
        /// A multiplier on the result of Compare which depends on the <see cref="SortOrder"/>.
        /// </summary>
        private int Multiplier;

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

        public int ColumnIndex { get; set; }

        public Type ColumnType {
            get => ColType;
            set {
                ColType = value;
                if (value == typeof(string) || !value.IsSubclassOf(typeof(IComparable))) {
                    Comparer = DefaultCompareFunc;
                }
                else {
                    Converter = Converters.Convert.GetConverter(value)(typeof(string));
                    Comparer = ConvertedComparer;
                }
            }
        }

        private static int DefaultCompareFunc(string x, string y)
        {
            return x.CompareTo(y);
        }

        private int ConvertedComparer(string x, string y)
        {
            IComparable xCmp = (IComparable) Converter(x);
            return xCmp.CompareTo(Converter(y));
        }

        public Func<string, string, int> CompareFunc {
            get => Comparer;
            set {
                ColType = typeof(object);
                Comparer = value;
            }
        }

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
                return Multiplier * Comparer(lx, ly);
            }
            catch { // do nothing
            }
            return Multiplier * lx.CompareTo(ly);
        }
    }
}