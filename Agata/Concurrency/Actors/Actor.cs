using System;
using System.Collections.Concurrent;
using System.Threading;
using Agata.Logging;

namespace Agata.Concurrency.Actors
{
    public sealed class Actor
    {
        private static readonly ILog Log = Logging.Log.For<Actor>();

        private const int ActorWaitState = 0;
        private const int ActorDrainState = 1;

        private class ActorAction<T> : IAction
        {
            private readonly T _subject;
            private readonly Action<T> _action;

            public ActorAction(T subject, Action<T> action)
            {
                _subject = subject;
                _action = action;
            }

            public void Invoke()
            {
                _action(_subject);
            }
        }

        private class DrainAction : IAction
        {
            private readonly Actor _actor;

            public DrainAction(Actor actor)
            {
                _actor = actor;
            }

            public void Invoke()
            {
                _actor.Drain();
            }
        }

        private readonly string _name;
        private readonly ConcurrentQueue<IAction> _actionsQueue = new ConcurrentQueue<IAction>();
        private readonly IThreadPool _threadPool;
        private readonly object _subject;
        private readonly DrainAction _drain;
        private volatile int _state;

        public Actor(string name, IThreadPool threadPool, object subject)
        {
            _name = name;
            _threadPool = threadPool;
            _subject = subject;
            _state = ActorWaitState;
            _drain = new DrainAction(this);
        }

        public bool IsWait => _state == ActorWaitState;
        
        public void Schedule<T>(Action<T> action)
        {
            var actorAction = new ActorAction<T>((T) _subject, action);
            _actionsQueue.Enqueue(actorAction);
            ScheduleDrain();
        }

        private void Drain()
        {
            if (!_actionsQueue.TryDequeue(out var action))
            {
                Log.Error($"Drain called on empty queue (actor_name={_name})");
                return;
            }

            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Log.Error(
                    $"An error occured on actor action execution (actor_name={_name}): {e.Message}" +
                    $"{Environment.NewLine}{e.StackTrace}");
            }

            if (!_actionsQueue.IsEmpty)
            {
                _threadPool.Schedule(_drain);
                return;
            }

            Interlocked.Exchange(ref _state, ActorWaitState);
            ScheduleDrain();
        }

        private void ScheduleDrain()
        {
            if (_actionsQueue.IsEmpty)
            {
                return;
            }

            var prev = Interlocked.CompareExchange(ref _state, ActorDrainState, ActorWaitState);
            if (prev != ActorWaitState)
            {
                return;
            }
            
            _threadPool.Schedule(_drain);
        }
    }
}