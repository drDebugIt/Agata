using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Agata.Concurrency;
using Agata.Concurrency.Actors;
using NUnit.Framework;

namespace Agata.Tests.ActorsTests
{
    [TestFixture]
    public class ActorsTests
    {
        private class Counter
        {
            internal long Value;

            public void Add(int value)
            {
                Value += value;
            }
        }

        [Test]
        public void Actor_ShouldExecuteActionsInSerializableOrder_SystemThreadPool()
        {
            var counter = new Counter();
            var system = new ActorSystem("Test", SystemThreadPool.Instance).RegisterFactoryOf(() => counter);
            var actor = system.ActorOf<Counter>("counter");

            var result = 0L;
            var tasks = new List<Task>();
            for (var i = 0; i < 1000000; i++)
            {
                var valueToAdd = i;
                result += i;
                var task = Task.Factory.StartNew(() => actor.Schedule(_ => _.Add(valueToAdd)));
                tasks.Add(task);
            }

            var sw = Stopwatch.StartNew();
            Task.WaitAll(tasks.ToArray());
            var tasksWaitTime = sw.Elapsed;
            while (actor.IsBusy)
            {
                Thread.Yield();
            }

            var totalWaitTime = sw.Elapsed;
            Console.WriteLine(totalWaitTime - tasksWaitTime);

            Assert.AreEqual(result, counter.Value);
        }

        [Test]
        public void Actor_ShouldExecuteActionsInSerializableOrder_SystemThreadPool_NoConcurrency()
        {
            var counter = new Counter();
            var system = new ActorSystem("Test", SystemThreadPool.Instance).RegisterFactoryOf(() => counter);
            var actor = system.ActorOf<Counter>("counter");

            var result = 0L;
            for (var i = 0; i < 1000000; i++)
            {
                var valueToAdd = i;
                result += i;
                actor.Schedule(_ => _.Add(valueToAdd));
            }

            var sw = Stopwatch.StartNew();
            var tasksWaitTime = sw.Elapsed;
            while (actor.IsBusy)
            {
                Thread.Yield();
            }

            var totalWaitTime = sw.Elapsed;
            Console.WriteLine(totalWaitTime - tasksWaitTime);

            Assert.AreEqual(result, counter.Value);
        }

        [Test]
        public void Actor_ShouldExecuteActionsInSerializableOrder_HeliosThreadPool()
        {
            var counter = new Counter();
            var system = new ActorSystem("Test", new FixedThreadPool(threadsNumber: 4)).RegisterFactoryOf(() => counter);
            var actor = system.ActorOf<Counter>("counter");

            var result = 0L;

            var tasks = new List<Task>();
            for (var i = 0; i < 1000000; i++)
            {
                var valueToAdd = i;
                result += i;
                var task = Task.Factory.StartNew(() => actor.Schedule(_ => _.Add(valueToAdd)));
                tasks.Add(task);
            }

            var sw = Stopwatch.StartNew();
            Task.WaitAll(tasks.ToArray());

            var tasksWaitTime = sw.Elapsed;
            while (actor.IsBusy)
            {
                Thread.Yield();
            }

            var totalWaitTime = sw.Elapsed;
            Console.WriteLine(totalWaitTime - tasksWaitTime);
            Assert.AreEqual(result, counter.Value);
        }

        [Test]
        public void Actor_ShouldExecuteActionsInSerializableOrder_HeliosThreadPool_NoConcurrency()
        {
            var counter = new Counter();
            var system = new ActorSystem("Test", new FixedThreadPool(threadsNumber: 4)).RegisterFactoryOf(() => counter);
            var actor = system.ActorOf<Counter>("counter");
            var result = 0L;

            for (var i = 0; i < 1000000; i++)
            {
                var valueToAdd = i;
                result += i;
                actor.Schedule(_ => _.Add(valueToAdd));
            }

            var sw = Stopwatch.StartNew();
            var tasksWaitTime = sw.Elapsed;
            while (actor.IsBusy)
            {
                Thread.Yield();
            }

            var totalWaitTime = sw.Elapsed;
            Console.WriteLine(totalWaitTime - tasksWaitTime);
            Assert.AreEqual(result, counter.Value);
        }
    }
}