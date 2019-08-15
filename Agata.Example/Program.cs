using System;
using System.Threading;
using System.Threading.Tasks;
using Agata.Concurrency;

namespace Agata.Example
{
    class Program
    {
        private static volatile int _countFromTasks;
        private static volatile int _countFromFutureMap;
        private static volatile int _countFromFutureComplete;

        static void Main(string[] args)
        {
            var awaiter = new SingleThreadPollTaskAwaiter("Tasks awaiter", ThreadPriority.Normal);

            for (var i = 0; i < 10; i++)
            {
                var task = TaskChain();
                Future
                    .From(awaiter, task)
                    .MapR(Execute.On(SystemThreadPool.Instance), _ =>
                    {
                        Interlocked.Increment(ref _countFromFutureMap);
                        return (object) null;
                    })
                    .OnComplete(Execute.On(SystemThreadPool.Instance), _ =>
                    {
                        Interlocked.Increment(ref _countFromFutureComplete);
                    });
            }

            while (true)
            {
                Console.WriteLine($"{_countFromTasks} {_countFromFutureMap} {_countFromFutureComplete}");
                Thread.Sleep(100);
            }
        }

        static async Task<object> TaskChain()
        {
            await Task.Delay(1000);
            await Task.Factory.StartNew(() => Interlocked.Increment(ref _countFromTasks));
            return null;
        }
    }
}