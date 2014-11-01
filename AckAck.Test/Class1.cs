using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EventStore.ClientAPI;
using NUnit.Framework;

namespace AckAck.Test
{
    [Serializable]
    public class AddItemCommand : Command<List<string>, int>
    {
        public readonly string Item;

        public AddItemCommand(string item)
        {
            Item = item;
        }

        public override int Execute(List<string> model)
        {
            model.Add(Item);
            return model.Count;
        }
    }

    [TestFixture]
    public class Class1
    {
        [Test]
        public void TplActionBlockSpike()
        {


            BufferBlock<int> buffer = new BufferBlock<int>();
            var reporter = new ActionBlock<int>(i => Console.WriteLine(i));
            ActionBlock<int> actionBlock = new ActionBlock<int>(

                i =>
                {
                    Thread.Sleep(i);
                    reporter.Post(i * 2);
                });
            buffer.LinkTo(actionBlock);
            for (int i = 0; i < 100; i++)
            {
                buffer.Post(i);
            }
            buffer.Complete();
            buffer.Completion.Wait();
        }

        [Test]
        public void Smoke()
        {
            if (_batchSize == 0) _batchSize = 100;
            Console.WriteLine("Batch size: " + _batchSize);
            var sw = new Stopwatch();
            var prevayler = new Prevayler<List<string>>(new List<string>(), _batchSize);
            sw.Start();
            var tasks = Enumerable
                .Range(0, 10000)
                .Select(i => prevayler.ExecuteAsync(new AddItemCommand(i.ToString()))).ToArray();
            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine("async elapsed: " + sw.Elapsed);
            prevayler.Dispose();

        }

        [Test]
        public void SingleEventPerEventStoreCall()
        {
            var eventStore = EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113));
            eventStore.ConnectAsync().Wait();
            var journalWriter = new EventStoreJournal(eventStore);
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

        private int _batchSize;
        [Test]
        public void ProgressiveBatchSizes()
        {
            foreach (var batchSize in Enumerable.Range(0, 12).Select(i => 10 * Math.Pow(2, i)))
            {
                _batchSize = (int)batchSize;
                Smoke();
            }
        }

        [Test]
        public void CorrectnessTest()
        {
            var prevayler = new Prevayler<IntModel>(new IntModel(), 100);
            var sum = 0;
            var tasks = new List<Task>();
            for (int i = 1; i <= 200; i++)
            {
                sum += i;
                var command = new SumCommand(i);
                tasks.Add(
                prevayler.ExecuteAsync(command)
                    .ContinueWith(t => Assert.AreEqual(t.Result, command.Operand)));
            }
            Task.WaitAll(tasks.ToArray());
            int result = prevayler.ExecuteAsync(new GetSumQuery()).Result;
            Assert.AreEqual(result, sum);

        }

        public class IntModel
        {

            public int Value;
        }

        public class GetSumQuery : Query<IntModel, int>
        {

            public override int Execute(IntModel model)
            {
                return model.Value;
            }
        }

        [Serializable]
        public class SumCommand : Command<IntModel,int>
        {
            public readonly int Operand;
            public SumCommand(int i)
            {
                Operand = i;
            }

            public override int Execute(IntModel model)
            {
                model.Value += Operand;
                return Operand;
            }
        }

        [Test]
        public void RingBufferRollover()
        {
            var buffer = new RingBuffer<int>(3, () => -1);

            Assert.AreEqual(0, buffer.Count);
            int zero = buffer.Next();

            Assert.AreEqual(1, buffer.Count);
            int one = buffer.Next();

            Assert.AreEqual(2, buffer.Count);
            int two = buffer.Next();

            Assert.AreEqual(0, zero);
            Assert.AreEqual(1, one);
            Assert.AreEqual(2, two);

            Assert.IsFalse(buffer.HasAvailable);
            Assert.AreEqual(buffer.Capacity, buffer.Count);

            buffer.Release(zero);
            Assert.AreEqual(2, buffer.Count);
            Assert.IsTrue(buffer.HasAvailable);




        }
    }
}
