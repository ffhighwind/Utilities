using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
	internal class TableEqualityComparer<T> : IEqualityComparer<T> where T : class
	{
		bool IEqualityComparer<T>.Equals(T x, T y)
		{
			for (int i = 0; i < TableData<T>.CompareProperties.Length; i++) {
				object a = TableData<T>.CompareProperties[i].GetValue(x);
				object b = TableData<T>.CompareProperties[i].GetValue(y);
				if (a != b)
					return false;
			}
			return true;
		}

		int IEqualityComparer<T>.GetHashCode(T obj)
		{
			int hashCode = TableData<T>.TableName.GetHashCode();
			for (int i = 0; i < TableData<T>.CompareProperties.Length; i++) {
				hashCode += TableData<T>.CompareProperties[i].GetValue(obj).GetHashCode();
			}
			return hashCode;
		}
	}
}
