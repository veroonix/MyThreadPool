using System;
using System.Diagnostics;
namespace TaskQueue
{
    public class TaskQueue
    {
        private readonly Thread[] _Threads;
        private readonly ThreadPriority _Prioroty;
        private readonly AutoResetEvent _WorkingEvent = new(false);
        private readonly AutoResetEvent _ExecuteEvent = new(true);
        private readonly Queue<Action> _Works = new();


        public TaskQueue(int MaxThreadsCount)
        {
            if (MaxThreadsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxThreadsCount),MaxThreadsCount, "Число потоков в пуле потоков должно быть больше или равно 1" );
            _Threads = new Thread[MaxThreadsCount];
            _Prioroty = ThreadPriority.Normal;
            Initialize();
        }

        private void Initialize()
        {
            
            for (int i = 0; i < _Threads.Length; i++)
            {
                var name = $"Thread[{i}]";
                Thread thread = new Thread(WorkingThread)
                {
                    Name = name,
                    IsBackground = true,
                    Priority = _Prioroty

                };
                _Threads[i] = thread;
                thread.Start();
            }
        }

        public void Execute(Action Work)
        {
            _ExecuteEvent.WaitOne(); 
            _Works.Enqueue(Work);
            _ExecuteEvent.Set(); 

            _WorkingEvent.Set();
        }

        private void WorkingThread()
        {
            var threadName = Thread.CurrentThread.Name;
            Trace.TraceInformation("Поток {0} запущен с id:{1}", threadName, Environment.CurrentManagedThreadId);
            while (true)
            {
                _WorkingEvent.WaitOne();

                _ExecuteEvent.WaitOne(); 

                while (_Works.Count == 0)           //пока в очереди нет заданий
                {
                    _ExecuteEvent.Set(); 
                    _WorkingEvent.WaitOne();
                    _ExecuteEvent.WaitOne();
                }

                var work = _Works.Dequeue();
                if (_Works.Count > 0)
                {
                    _WorkingEvent.Set();
                }
                _ExecuteEvent.Set(); 

                Trace.TraceInformation(
                    "Поток {0}[id:{1}] выполняет задание", 
                    threadName, Environment.CurrentManagedThreadId);
                try
                {
                    var timer = Stopwatch.StartNew();
                    work();
                    timer.Stop();

                    Trace.TraceInformation(
                        "Поток {0}[id:{1}] выполнил задание за {2}мс",
                        threadName, Environment.CurrentManagedThreadId, timer.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Ошибка выполнения задания в потоке {0}:{1}", threadName, e);
                }
            
            }
            Trace.TraceInformation("Поток {0} завершил свою работу", threadName);
        }
    }

}
