using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Agata.Example
{
    public class TestAsyncAwait
    {
        private readonly int _iterations;
        private readonly HttpClient _client;
        private int _totalLen;

        public TestAsyncAwait(int iterations)
        {
            var handler = new HttpClientHandler {MaxConnectionsPerServer = 1000};
            _client = new HttpClient(handler);
            _iterations = iterations;
        }

        public void Run()
        {
            _totalLen = 0;
            var tasks = new List<Task>();
            for (var i = 0; i < _iterations; i++)
            {
                tasks.Add(
                    SendRequest(_client, $"https://postman-echo.com/get?req_number={i}&date={DateTime.Now.Ticks}"));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine(_totalLen);
        }

        private async Task SendRequest(HttpClient client, string url)
        {
            var str = await client.GetStringAsync(url);
            var len = str.Length;
            Interlocked.Add(ref _totalLen, len);
        }
    }
}