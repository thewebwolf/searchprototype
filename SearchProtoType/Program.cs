using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
namespace SearchProtoType
{
    class Program
    {
        static void Main(string[] args)
        {
            SearchFactory<ModelA, int> factoryA = new SearchFactory<ModelA, int>(
                new List<ModelA>() {
                    new ModelA() { Id = 1, Name = "First a", ForeignKey = 3 },
                    new ModelA() { Id = 2, Name = "Second a", ForeignKey = 2 },
                    new ModelA() { Id = 3, Name = "Third a", ForeignKey = 1 }
                });
            SearchFactory<ModelB, int> factoryB = new SearchFactory<ModelB, int>(
                new List<ModelB>() {
                    new ModelB() { Id = 1, Name = "First b", ForeignKey = 3 },
                    new ModelB() { Id = 2, Name = "Second b", ForeignKey = 2 },
                    new ModelB() { Id = 3, Name = "Third b", ForeignKey = 1 }
                });
            SearchFactory<ModelA, int> factoryC = new SearchFactory<ModelA, int>(
                new List<ModelA>() {
                    new ModelA() { Id = 1, Name = "First C", ForeignKey = 3 },
                    new ModelA() { Id = 2, Name = "Second C", ForeignKey = 2 },
                    new ModelA() { Id = 3, Name = "Third C", ForeignKey = 1 }
                });

            AndFilterFactory<ModelA> andFilterFactory = new AndFilterFactory<ModelA>(2);

            var sourceBlockA = factoryA.SourceData();
            var sourceBlockB = factoryB.SourceData();
            var sourceBlockC = factoryC.SourceData();
            var andFilterA = andFilterFactory.GetAndFilter();

            Func<ModelA, bool> nameFilter = input => input.Name.Contains("First");

            var expressionFilterA = factoryA.ExpressionFilter(a => nameFilter(a));
            var actionA = new ActionBlock<ModelA>(item =>
            {
                Console.WriteLine(item.Name);
            });

            var actionB = new ActionBlock<ModelB>(item =>
            {
                Console.WriteLine(item.Name);
            });


            var actionC = new ActionBlock<ModelA>(item =>
            {
                Console.WriteLine(item.Name);
            });

            sourceBlockA.LinkTo(expressionFilterA);
            expressionFilterA.LinkTo(actionA);

            sourceBlockB.LinkTo(actionB);

            sourceBlockC.LinkTo(andFilterA);
            andFilterA.LinkTo(actionC);

            sourceBlockA.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)expressionFilterA).Fault(t.Exception);
                else expressionFilterA.Complete();
            });

            expressionFilterA.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)actionA).Fault(t.Exception);
                else actionA.Complete();
            });

            sourceBlockB.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)actionB).Fault(t.Exception);
                else actionB.Complete();
            });

            sourceBlockC.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)andFilterA).Fault(t.Exception);
                else andFilterA.Complete();
            });

            andFilterA.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)actionC).Fault(t.Exception);
                else actionC.Complete();
            });

            sourceBlockC.Post(new ModelA() { Id = 2, Name = "Second C", ForeignKey = 2 });

            sourceBlockA.Complete();
            sourceBlockB.Complete();
            sourceBlockC.Complete();

            actionA.Completion.Wait();
            actionB.Completion.Wait();
            actionC.Completion.Wait();
            Console.ReadLine();
        }
    }
    public class ModelA
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ForeignKey { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class ModelB
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ForeignKey { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
