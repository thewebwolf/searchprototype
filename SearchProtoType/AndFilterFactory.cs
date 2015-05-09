using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
namespace SearchProtoType
{
    public class AndFilterFactory<TResult>
    {
        private ConcurrentDictionary<int, int> itemCount = new ConcurrentDictionary<int, int>();
        private int sources = 0;

        public AndFilterFactory(int sources)
        {
            this.sources = sources;
        }
        public TransformManyBlock<TResult, TResult> GetAndFilter()
        {
            return new TransformManyBlock<TResult, TResult>(item =>
            {
                return PassIfNeeded(item);
            });
        }

        private IEnumerable<TResult> PassIfNeeded(TResult item)
        {
            int count = 1;
            if (itemCount.TryRemove(item.GetHashCode(), out count))
            {
                count++; 
            }
            if (itemCount.TryAdd(item.GetHashCode(), ++count))
            {
                if (count >= sources) yield return item;
            };            
        }
    }
}
