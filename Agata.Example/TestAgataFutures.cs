using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Agata.Concurrency;

namespace Agata.Example
{
    public class TestAgataFutures
    {
        private readonly int _iterations;
        private readonly HttpClient _client;
        private readonly IAwaiter _awaiter;
        private int _totalLen;

        public TestAgataFutures(int iterations)
        {
            var handler = new HttpClientHandler {MaxConnectionsPerServer = 1000};
            _client = new HttpClient(handler);
            _iterations = iterations;
            _awaiter = new SingleThreadPollTaskAwaiter("Tasks awaiter", ThreadPriority.Normal);
        }

        public void Run()
        {
            _totalLen = 0;
            var futures = new List<Future<Unit>>();
            for (var i = 0; i < _iterations; i++)
            {
                futures.Add(SendRequest(_client,
                    $"https://postman-echo.com/get?req_number={i}&date={DateTime.Now.Ticks}"));
            }

            Future.WaitAll(futures.ToArray());
            Console.WriteLine(_totalLen);
        }

        private Future<Unit> SendRequest(HttpClient client, string url)
        {
            var future = Future.From(_awaiter, client.GetStringAsync(url));
            return future.MapR(Execute.OnLastThread, _ =>
            {
                var len = _.Length;
                Interlocked.Add(ref _totalLen, len);
                return Unit.Instance;
            });
        }
    }
}