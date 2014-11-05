using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace AsyncOrigoSpike.Test
{
    [TestFixture]
    public class TheTests
    {
        public const int CommandsPerRun = 20000;

        [Test,TestCaseSource("ProgressiveBatchSizes")]
        public void TimedBatchCommandRun(int batchSize)
        {
           

            Console.WriteLine("# commands: " + CommandsPerRun);
            Console.WriteLine("# commands per batch: " + batchSize);
            var engine = CreateEngine<List<string>>(batchSize);
            var sw = new Stopwatch();
            sw.Start();
            var tasks = Enumerable
                .Range(0, CommandsPerRun)
                .Select(i => engine.ExecuteAsync(new AddItemCommand(i.ToString()))).ToArray();
            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine("time elapsed: " + sw.Elapsed);
            Console.WriteLine("tps: " +  CommandsPerRun / sw.Elapsed.TotalSeconds);
            Console.WriteLine();
            engine.Dispose();
        }

        [Test]
        public void SingleEventPerEventStoreCall()
        {
            var eventStore = EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113));
            eventStore.ConnectAsync().Wait();
            var journalWriter = new EventStoreWriter(eventStore);
            var sw = new Stopwatch();
            Command[] commands = new Command[1];
            sw.Start();
            var tasks = Enumerable
                .Range(0, 10000)
                .Select(i => journalWriter.AppendAsync(commands)).ToArray();
            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine("elapsed: " + sw.Elapsed);

        }

        public IEnumerable<int> ProgressiveBatchSizes()
        {
            return Enumerable.Range(0, 10).Select(i => 10 * (int)Math.Pow(2, i));
        }

        private IEngine<M> CreateEngine<M>(int batchSize) where M : new()
        {
            var writer = EventStoreWriter.Create();
            //var engine = new DisruptorEngine<M>(new M(), writer, batchSize);
            var engine = new TplEngine<M>(new M(), batchSize, writer);
            Console.WriteLine("Engine: " + engine.GetType());
            return engine;
        }
            
        [Test]
        public void CorrectnessTest()
        {
            var engine = CreateEngine<IntModel>(100);
            var sum = 0;
            var tasks = new List<Task>();
            for (int i = 1; i <= 200; i++)
            {
                sum += i;
                var command = new SumCommand(i);
                tasks.Add(
                engine.ExecuteAsync(command)
                    .ContinueWith(t => Assert.AreEqual(t.Result, command.Operand)));
            }
            Task.WaitAll(tasks.ToArray());
            int result = engine.ExecuteAsync(new GetSumQuery()).Result;
            Assert.AreEqual(result, sum);

        }
    }
}
