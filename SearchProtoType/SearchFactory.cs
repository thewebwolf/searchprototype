using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Reflection;
namespace SearchProtoType
{
    public class SearchFactory<TResult, TKey>
    {
        private IEnumerable<TResult> sourceData;

        public SearchFactory(IEnumerable<TResult> sourceData)
        {
            if (sourceData == null) throw new ArgumentNullException("Data can not be null.");
            this.sourceData = sourceData;
        }

        public BufferBlock<TResult> SourceData()
        {
            BufferBlock<TResult> source = new BufferBlock<TResult>();
            foreach (TResult item in sourceData)
            {
                source.SendAsync(item);
            }
            return source;
        }

        public TransformBlock<TResult, FKey> ForeignKeys<FKey>(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.PropertyType.Equals(typeof(FKey))) throw new Exception("ForeignKey types do not match.");

            return new TransformBlock<TResult, FKey>(item =>
            {
                return (FKey)propertyInfo.GetValue(item);
            });
        }

        public TransformManyBlock<TResult, TResult> ExpressionFilter(Expression<Func<TResult, bool>> filter)
        {
            return new TransformManyBlock<TResult, TResult>(item =>
            {
                return Filter(item, filter);
            });
        }

        public TransformManyBlock<TResult, TResult> AndFilter(int sources)
        {
            return new TransformManyBlock<TResult, TResult>(item =>
            {
                return CountCheck(item);
            });
        }

        private IEnumerable<TResult> CountCheck(TResult item)
        {
            yield return item;
        }

        private IEnumerable<TResult> Filter(TResult item, Expression<Func<TResult, bool>> filter)
        {
            var function = filter.Compile();
            if (function(item))
            {
                yield return item;
            }
        }
    }
}
