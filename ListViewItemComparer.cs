using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities
{
    public class ListViewItemComparer : IComparer
    {
        private Type ColType;
        private TypeCode ColTypeCode;

        public int ColumnIndex { get; set; }

        public ListView ListView { get; }

        public Type ColumnType {
            get {
                return ColType;
            }
            set {
                ColType = value;
                ColTypeCode = Type.GetTypeCode(value);
                Comparer = Comparers.GetComparer(value);
            }
        }

        private static int NoCompare(object x, object y) { return 0; }

        private Func<object, object, int> Comparer { get; set; } = NoCompare;

        public ListViewItemComparer(ListView listView)
        {
            ListView = listView;
        }

        public int Compare(object x, object y)
        {
            ListViewItem lviX = x as ListViewItem;
            ListViewItem lviY = y as ListViewItem;

            if (ListView.Sorting == SortOrder.None)
                return 0;
            else if (lviX == null) {
                if (lviY == null)
                    return 0;
                else
                    return -1;
            }
            else if (lviY == null)
                return 1;

            int multiplier;
            if (ListView.Sorting == SortOrder.Ascending)
                multiplier = 1;
            else
                multiplier = -1;
            switch (ColTypeCode) {
                case
            }
            Converter(lviX.SubItems[ColumnIndex].Text)
            return Comparer(lviX.SubItems[ColumnIndex].Text, lviX.SubItems[ColumnIndex].Text) * multiplier;
        }
    }
}
