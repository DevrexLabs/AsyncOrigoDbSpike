using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AckAck;
using AckAck.Test;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var batchSize = 500;
            Console.WriteLine("Batch size: " + batchSize);
            var sw = new Stopwatch();
            var prevayler = new Prevayler<List<string>>(new List<string>(), batchSize);
            sw.Start();
            var tasks = Enumerable
                .Range(0, 100000)
                .Select(i => prevayler.ExecuteAsync(new AddItemCommand(i.ToString()))).ToArray();
            Task.WaitAll(tasks);
            sw.Stop();
            Console.WriteLine("async elapsed: " + sw.Elapsed);
            prevayler.Dispose();

        }
    }
}
