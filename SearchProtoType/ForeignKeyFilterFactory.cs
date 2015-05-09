using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SearchProtoType
{
    public class ForeignKeyFilterFactory<TResult, TKey>
    {
        private ConcurrentDictionary<int, TResult> itemStore = new ConcurrentDictionary<int, TResult>();
        private ConcurrentDictionary<int, bool> included = new ConcurrentDictionary<int, bool>();

        public TransformManyBlock<Tuple<TResult, TKey>, TResult> GetFilter()
        {
            return new TransformManyBlock<Tuple<TResult, TKey>, TResult>(item =>
            {
                return PassIfNeeded(item.Item1, item.Item2);
            });
        }

        private IEnumerable<TResult> PassIfNeeded(TResult item, TKey key)
        {
            if (key.GetHashCode() == item.GetHashCode())
            {
                if (!itemStore.ContainsKey(item.GetHashCode())) itemStore.TryAdd(item.GetHashCode(), item);
                if (!included.ContainsKey(key.GetHashCode())) included.TryAdd(key.GetHashCode(), true);

                yield return item;
            }
            else
            {
                if (!itemStore.ContainsKey(item.GetHashCode()))
                {
                    itemStore.TryAdd(item.GetHashCode(), item);
                    if (included.ContainsKey(item.GetHashCode()))
                    {
                        yield return item;
                    }
                }

                if (!included.ContainsKey(key.GetHashCode()))
                {
                    included.TryAdd(key.GetHashCode(), true);
                    TResult tempValue;
                    if (itemStore.TryGetValue(key.GetHashCode(), out tempValue))
                    {
                        yield return tempValue;
                    }
                }
            }

        }
    }
}
