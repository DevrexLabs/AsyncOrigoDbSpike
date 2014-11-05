using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            foreach (var engine in CreateEngines<List<string>>(batchSize))
            {
                Console.WriteLine( engine.GetType().Name);
                Console.WriteLine("# commands: " + CommandsPerRun);
                Console.WriteLine("# commands per batch: " + batchSize);

                var sw = new Stopwatch();
                sw.Start();
                var tasks = Enumerable
                    .Range(0, CommandsPerRun)
                    .Select(i => engine.ExecuteAsync(new AddItemCommand(i.ToString()))).ToArray();
                Task.WaitAll(tasks);
                sw.Stop();
                Console.WriteLine("time elapsed: " + sw.Elapsed);
                Console.WriteLine("tps: " + CommandsPerRun/sw.Elapsed.TotalSeconds);
                Console.WriteLine();
                engine.Dispose();
            }
        }

        [Test]
        public void SingleEventPerEventStoreCall()
        {
            var journalWriter = EventStoreWriter.Create();
            var sw = new Stopwatch();
            Command[] commands = new Command[1];
            sw.Start();
            var tasks = Enumerable
                .Range(0, 10000)
                .Select(i => journalWriter.AppendAsync(commands)).ToArray();
            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine("elapsed: " + sw.Elapsed);
            journalWriter.Dispose();
        }

        public IEnumerable<int> ProgressiveBatchSizes()
        {
            return Enumerable.Range(0, 10).Select(i => 10 * (int)Math.Pow(2, i));
        }

        private IEnumerable<IEngine<M>> CreateEngines<M>(int batchSize) where M : new()
        {
            yield return new AkkaEngine<M>(new M(), batchSize, EventStoreWriter.Create());
            yield return new TplEngine<M>(new M(), batchSize, EventStoreWriter.Create());
            yield return new DisruptorEngine<M>(new M(), batchSize, EventStoreWriter.Create());
        }

        private IEnumerable<IEngine<IntModel>> CreateCorrectnessEngines()
        {
            return CreateEngines<IntModel>(100);
        }

        [Test]
        public void CorrectnessTest()
        {
            foreach (var engine in CreateCorrectnessEngines())
            using(engine)
            {
                var sum = 0;
                var tasks = new List<Task>();
                for (int i = 1; i <= CommandsPerRun; i++)
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
                engine.Dispose();
            }
        }
    }
}
