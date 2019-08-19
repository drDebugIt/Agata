using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Agata.Concurrency;

namespace Agata.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            const int iterations = 100;

            var asyncAwaitTest = new TestAsyncAwait(iterations);
            var agataFuturesTest = new TestAgataFutures(iterations);

            for (var i = 0; i < 10; i++)
            {
                var sw = Stopwatch.StartNew();
                asyncAwaitTest.Run();
                sw.Stop();
                Console.WriteLine($"Tasks: {sw.Elapsed}");

                sw.Restart();
                agataFuturesTest.Run();
                sw.Stop();
                Console.WriteLine($"Agata: {sw.Elapsed}");
            }
        }
    }
}