using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;


namespace AckAck
{
    
    public class TplJournalWriter : IDisposable
    {
        
        private readonly IJournalWriter _journalWriter;
        private Timer _timer;

        //commands start here
        private readonly BatchBlock<CommandContext> _requestQueue;

        //then go here at given intervals or when the batch size is reached
        private ActionBlock<CommandContext[]> _writerBlock;

        //after journaling, commands are passed to the dispatcher for scheduling
        private readonly Dispatcher _dispatcher;

        //profiling stuff
        private List<int> _batchSizes = new List<int>();
        private int _timerInvocations = 0;
        

        public TimeSpan Interval { get; set; }

        public TplJournalWriter(IJournalWriter journalWriter, Dispatcher dispatcher, int batchSize)
        {
            Interval = TimeSpan.FromMilliseconds(16);
            _journalWriter = journalWriter;
            _dispatcher = dispatcher;

            _writerBlock = new ActionBlock<CommandContext[]>(batch => Go(batch));

            _requestQueue = new BatchBlock<CommandContext>(batchSize);
            _requestQueue.LinkTo(_writerBlock);
            
        }


        private void OnTimerTick(object state)
        {
            _requestQueue.TriggerBatch();
            _timerInvocations++;
            SetTimer();
        }

        private void SetTimer()
        {
            _timer.Change(Interval, TimeSpan.FromMilliseconds(-1));            
        }

        public void Post(CommandContext request)
        {
            if (_timer == null)
            {
                _timer = new Timer(OnTimerTick);
                SetTimer();
            }
            _requestQueue.Post(request);
        }

        private void Go(CommandContext[] batch)
        {
            _batchSizes.Add(batch.Length);
            _journalWriter.AppendAsync(batch.Select(ctx => ctx.Command))
                .ContinueWith(t => _dispatcher.Post(batch));
            SetTimer();
        }

        public void Dispose()
        {
            Console.WriteLine("Timer invocations:" + _timerInvocations);
            var histogram = new SortedDictionary<int, int>();
            foreach (var count in _batchSizes)
            {
                if (!histogram.ContainsKey(count)) histogram[count] = 1;
                else histogram[count]++;
            }
            foreach (var key in histogram.Keys)
            {
                Console.WriteLine(key + ": " + histogram[key]);
            }
            _journalWriter.Dispose();
        }
    }
}
