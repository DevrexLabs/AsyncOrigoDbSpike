using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AckAck.Test
{
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
        public void Smoke()
        {
            var sw = new Stopwatch();
            var prevayler = new Prevayler<List<string>>(new List<string>());
            

            sw.Start();
            foreach (int i in Enumerable.Range(0, 10))
            {
                int result = prevayler.Execute(new AddItemCommand(i.ToString()));
                Console.WriteLine("sync result: " + result);

            }

            var tasks = new List<Task>();
            foreach (int i in Enumerable.Range(0, 100000))
            {
               var task = prevayler.ExecuteAsync(new AddItemCommand(i.ToString()));
                tasks.Add(task.ContinueWith(t => Console.WriteLine("async result:" + t.Result)));

            }

            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            Console.WriteLine("Elapsed: " + sw.Elapsed);

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
