using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;


namespace AsyncOrigoSpike
{
    
    public class TplBatchingJournaler : IDisposable
    {
        
        private readonly IJournalWriter _journalWriter;
        private Timer _timer;

        //commands start here
        private readonly BatchBlock<CommandRequest> _requestQueue;

        //then go here at given intervals or when the batch size is reached
        private ActionBlock<CommandRequest[]> _writerBlock;

        //after journaling, commands are passed to the dispatcher for scheduling
        private readonly ExecutionPipeline _dispatcher;

        //profiling stuff
        private List<int> _batchSizes = new List<int>();
        private int _timerInvocations = 0;
        

        public TimeSpan Interval { get; set; }

        public TplBatchingJournaler(IJournalWriter journalWriter, ExecutionPipeline dispatcher, int batchSize)
        {
            Interval = TimeSpan.FromMilliseconds(16);
            _journalWriter = journalWriter;
            _dispatcher = dispatcher;

            _writerBlock = new ActionBlock<CommandRequest[]>(batch => Go(batch));

            _requestQueue = new BatchBlock<CommandRequest>(batchSize);
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

        public void Post(CommandRequest request)
        {
            if (_timer == null)
            {
                _timer = new Timer(OnTimerTick);
                SetTimer();
            }
            _requestQueue.Post(request);
        }

        private void Go(CommandRequest[] batch)
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
