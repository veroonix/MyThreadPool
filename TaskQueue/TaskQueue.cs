using System;
using System.Diagnostics;
namespace TaskQueue
{
    public class TaskQueue
    {
        private readonly Thread[] _Threads;
        private readonly AutoResetEvent _WorkingEvent = new(false);
        private readonly AutoResetEvent _ExecuteEvent = new(true);
        private readonly Queue<Action> _Works = new();


        public TaskQueue(int MaxThreadsCount)
        {
            if (MaxThreadsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxThreadsCount),MaxThreadsCount, "Число потоков в пуле потоков должно быть больше или равно 1" );
            _Threads = new Thread[MaxThreadsCount];
        }

        private void Initialize()
        {
            for (int i = 0; i < _Threads.Length; i++)
            {
                Thread thread = new Thread(WorkingThread);
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
            while (true)
            {
                _WorkingEvent.WaitOne();
                _ExecuteEvent.WaitOne(); 

                var work = _Works.Dequeue();
                _ExecuteEvent.Set(); 

                try
                {
                     work();
                }
                catch (Exception e)
                {
                    Trace.TraceError("Ошибка выполнения задания в потоке", e);
                }
            
            }
            
        }
    }

}
