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
            ///////////////////////////////////////////////////////////////////////////////////////
            Console.WriteLine("-----------------------");
            Console.WriteLine("Start A Expression Filter");
            SearchFactory<ModelA, int> factoryA = new SearchFactory<ModelA, int>(
                new List<ModelA>() {
                    new ModelA() { Id = 1, Name = "First a", ForeignKey = 3 },
                    new ModelA() { Id = 2, Name = "Second a", ForeignKey = 2 },
                    new ModelA() { Id = 3, Name = "Third a", ForeignKey = 1 }
                });

            Func<ModelA, bool> nameFilter = input => input.Name.Contains("First");

            var sourceBlockA = factoryA.SourceData();
            var expressionFilterA = factoryA.ExpressionFilter(a => nameFilter(a));
            var actionA = new ActionBlock<ModelA>(item =>
            {
                Console.WriteLine(item.Name);
            });

            sourceBlockA.LinkTo(expressionFilterA);
            expressionFilterA.LinkTo(actionA);

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

            sourceBlockA.Complete();

            actionA.Completion.Wait();

            Console.WriteLine("Completed A");
            Console.WriteLine("-----------------------");

            ///////////////////////////////////////////////////////////////////////////////////////
            Console.WriteLine("Start B Simple All results");
            SearchFactory<ModelB, int> factoryB = new SearchFactory<ModelB, int>(
                new List<ModelB>() {
                    new ModelB() { Id = 1, Name = "First b", ForeignKey = 3 },
                    new ModelB() { Id = 2, Name = "Second b", ForeignKey = 2 },
                    new ModelB() { Id = 3, Name = "Third b", ForeignKey = 1 }
                });

            var sourceBlockB = factoryB.SourceData();

            var actionB = new ActionBlock<ModelB>(item =>
            {
                Console.WriteLine(item.Name);
            });
            sourceBlockB.LinkTo(actionB);


            sourceBlockB.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)actionB).Fault(t.Exception);
                else actionB.Complete();
            });

            sourceBlockB.Complete();
            actionB.Completion.Wait();

            Console.WriteLine("Completed B");
            Console.WriteLine("-----------------------");

            ///////////////////////////////////////////////////////////////////////////////////////
            Console.WriteLine("Start C AND filter");
            SearchFactory<ModelA, int> factoryC = new SearchFactory<ModelA, int>(
                new List<ModelA>() {
                    new ModelA() { Id = 1, Name = "First C", ForeignKey = 3 },
                    new ModelA() { Id = 2, Name = "Second C", ForeignKey = 2 },
                    new ModelA() { Id = 3, Name = "Third C", ForeignKey = 1 }
                });

            AndFilterFactory<ModelA> andFilterFactory = new AndFilterFactory<ModelA>(2);

            var sourceBlockC = factoryC.SourceData();
            var andFilterA = andFilterFactory.GetAndFilter();
            var actionC = new ActionBlock<ModelA>(item =>
            {
                Console.WriteLine(item.Name);
            });


            sourceBlockC.LinkTo(andFilterA);
            andFilterA.LinkTo(actionC);

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
            sourceBlockC.Complete();

            actionC.Completion.Wait();
            Console.WriteLine("Completed C");
            Console.WriteLine("-----------------------");

            ///////////////////////////////////////////////////////////////////////////////////////
            Console.WriteLine("Start D foreign key filter");
            SearchFactory<ModelA, int> factoryD = new SearchFactory<ModelA, int>(
                new List<ModelA>() {
                    new ModelA() { Id = 1, Name = "First D", ForeignKey = 1 },
                    new ModelA() { Id = 2, Name = "Second D", ForeignKey = 2 },
                    new ModelA() { Id = 3, Name = "Third D", ForeignKey = 3 }
                });
            SearchFactory<ModelB, int> factoryE = new SearchFactory<ModelB, int>(
                new List<ModelB>() {
                    new ModelB() { Id = 1, Name = "First E", ForeignKey = 10 },
                    new ModelB() { Id = 2, Name = "Second E", ForeignKey = 2 },
                    new ModelB() { Id = 3, Name = "Third E", ForeignKey = 30 }
                });
            ForeignKeyFilterFactory<ModelA, int> foreignKeyFilterFactory = new ForeignKeyFilterFactory<ModelA, int>();

            var sourceBlockD = factoryD.SourceData();
            var sourceBlockE = factoryE.SourceData();
            var subKeys = factoryE.ForeignKeys<int>(typeof(ModelB).GetProperty("ForeignKey"));
            var joinBlock = new JoinBlock<ModelA, int>(new GroupingDataflowBlockOptions
            {
                Greedy = false
            });
            var foreignKeyFilter = foreignKeyFilterFactory.GetFilter();
            var actionD = new ActionBlock<ModelA>(item =>
            {
                Console.WriteLine(item.Name);
            });
            sourceBlockD.LinkTo(joinBlock.Target1);
            sourceBlockE.LinkTo(subKeys);
            subKeys.LinkTo(joinBlock.Target2);
            joinBlock.LinkTo(foreignKeyFilter);
            foreignKeyFilter.LinkTo(actionD);

            sourceBlockD.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)joinBlock.Target1).Fault(t.Exception);
                else joinBlock.Target1.Complete();
            });

            sourceBlockE.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)subKeys).Fault(t.Exception);
                else subKeys.Complete();
            });
            subKeys.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)joinBlock.Target2).Fault(t.Exception);
                else joinBlock.Target2.Complete();
            });

            joinBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)foreignKeyFilter).Fault(t.Exception);
                else foreignKeyFilter.Complete();
            });
            foreignKeyFilter.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)actionD).Fault(t.Exception);
                else actionD.Complete();
            });
            sourceBlockD.Complete();
            sourceBlockE.Complete();

            actionD.Completion.Wait();
            Console.ReadLine();
        }
    }
}
