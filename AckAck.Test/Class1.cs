using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
        public void Smoke(int batchSize = 100)
        {
            Console.WriteLine("Batch size: " + batchSize);
            var sw = new Stopwatch();
            var prevayler = new Prevayler<List<string>>(new List<string>(), batchSize);
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

        [Test]
        public void ProgressiveBatchSizes()
        {
            foreach (var batchSize in Enumerable.Range(0, 12).Select(i => 10 * Math.Pow(2, i)))
            {
                Smoke((int)batchSize);
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
