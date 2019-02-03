using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database.Old
{
	public class TableCache<T> where T : class
	{
		internal ConcurrentDictionary<T, CacheData<T>> Cache = new ConcurrentDictionary<T, CacheData<T>>(new TableEqualityComparer<T>());
		internal ConcurrentDictionary<string, CacheData<List<T>>> Queries = new ConcurrentDictionary<string, CacheData<List<T>>>();

		public IReadOnlyDictionary<T, CacheData<T>> Items { get; private set; }
		public TimeSpan StaleDuration { get; set; } = new TimeSpan(1, 0, 0);

		public TableCache()
		{
			Items = Cache;
		}

		public void Clear()
		{
			Cache.Clear();
			Queries.Clear();
			Queries
		}

		public T Get(T obj)
		{
			return Cache[obj].Value;
		}

		public List<T> Get(IEnumerable<T> objs)
		{
			List<T> list = new List<T>();
			foreach (T obj in objs) {
				list.Add(Cache[obj].Value);
			}
			return list;
		}

		public T TryGet(T obj)
		{
			return Cache.TryGetValue(obj, out CacheData<T> value)
				? value.Value : null;
		}

		public List<T> TryGet(IEnumerable<T> objs, out List<T> notFound)
		{
			notFound = new List<T>();
			List<T> list = new List<T>();
			foreach (T obj in objs) {
				if (Cache.TryGetValue(obj, out CacheData<T> value)) {
					list.Add(value.Value);
				}
				else {
					notFound.Add(obj);
				}
			}
			return list;
		}

		public bool Remove(T obj)
		{
			if(Cache.TryRemove(obj, out removed)) {
				foreach(var map in QueriesCache.Values) {
					if(map.StaleTime)
					map.Map.ContainsKey()
					map.Value
				}
				QueriesCache.Values.TryRemove()
			}
		}


		public int Remove(IEnumerable<T> objs)
		{
			int count = 0;
			foreach (T obj in objs) {
				if (Remove(obj)) {
					count++;
				}
			}
			return count;
		}

		public int Remove(IEnumerable<T> objs, out List<T> notFound)
		{
			int found = 0;
			notFound = new List<T>();
			CacheData<T> removed;
			foreach (T obj in objs) {
				if (!Remove(obj)) {
					notFound.Add(obj);
				}
				else
					found++;
			}
			return found;
		}

		internal T Upsert(T obj, DateTime now)
		{
			DateTime staleTime = now.Add(StaleDuration);
			CacheData<T> newData = new CacheData<T>(obj, staleTime);
			return Cache.AddOrUpdate(obj, newData, 
				(o, curr) => {
					TableData<T>.Copy(newData.Value, curr.Value);
					curr.StaleTime = staleTime;
					return curr;
			}).Value;
		}

		internal List<T> Upsert(IEnumerable<T> objs, DateTime now)
		{
			DateTime staleTime = now.Add(StaleDuration);
			List<T> list = new List<T>();
			foreach (T obj in objs) {
				list.Add(Upsert(obj, staleTime));
			}
			return list;
		}

		/// <summary>
		/// Called before cloning the source object to the destination.
		/// If the return value is true then the destination will be cloned with the source using the default clone function.
		/// This can be replaced with a custom cloning function.
		/// </summary>
		public Func<T, T, bool> ModifiedCallback { get; set; } = (source, dest) => { return true; };
	}
}
