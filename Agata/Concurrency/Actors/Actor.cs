using System;
using System.Collections.Concurrent;
using System.Threading;
using Agata.Logging;

namespace Agata.Concurrency.Actors
{
    public sealed class Actor
    {
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

        private class KillAction<T> : IAction
        {
            private readonly T _subject;
            private readonly Action<T> _notification;

            public KillAction(T subject, Action<T> notification)
            {
                _subject = subject;
                _notification = notification;
            }

            public void Invoke()
            {
                _notification(_subject);
            }
        }

        private static readonly ILog Log = Logging.Log.For<Actor>();

        private const int ActorWaitState = 0;
        private const int ActorDrainState = 1;

        private readonly string _name;
        private readonly ActorSystem _system;
        private readonly ConcurrentQueue<IAction> _actionsQueue = new ConcurrentQueue<IAction>();
        private readonly IThreadPool _threadPool;
        private readonly object _subject;
        private readonly DrainAction _drain;
        private volatile int _state;
        private volatile IAction _killNotification;

        public Actor(ActorSystem system, IThreadPool threadPool, string name, object subject)
        {
            _system = system;
            _threadPool = threadPool;
            _name = name;
            _subject = subject;
            _state = ActorWaitState;
            _drain = new DrainAction(this);
            _killNotification = null;
        }

        public bool IsWait => _state == ActorWaitState;

        public void Schedule<T>(Action<T> action)
        {
            if (_killNotification != null)
            {
                Log.Warning(
                    "Action scheduled on dead actor (" +
                    $"actor_system={_system.Name}," +
                    $"actor_name={_name}");

                return;
            }

            var actorAction = new ActorAction<T>((T) _subject, action);
            _actionsQueue.Enqueue(actorAction);
            ScheduleDrain();
        }

        public void Kill<T>(Action<T> notification) where T : class
        {
            var killAction = new KillAction<T>((T) _subject, notification);
            var prev = Interlocked.CompareExchange(ref _killNotification, killAction, null);
            if (prev != null)
            {
                Log.Warning(
                    "Kill scheduled on dead actor (" +
                    $"actor_system={_system.Name}," +
                    $"actor_name={_name}");

                return;
            }

            ScheduleDrain();
        }

        private void Drain()
        {
            var killNotification = _killNotification;
            if (killNotification != null)
            {
                KillActorWith(killNotification);
                return;
            }

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
                    $"{Environment.NewLine}" +
                    $"{e.StackTrace}");
            }

            if (!_actionsQueue.IsEmpty)
            {
                _threadPool.Schedule(_drain);
                return;
            }

            Interlocked.Exchange(ref _state, ActorWaitState);
            ScheduleDrain();
        }

        private void KillActorWith(IAction notification)
        {
            _system.RemoveActor(_name);
            
            try
            {
                notification.Invoke();
            }
            catch (Exception e)
            {
                Log.Error(
                    "An error occured on notify killing actor (" +
                    $"actor_system={_system.Name}," +
                    $"actor_name={_name}): " +
                    $"{e.Message}" +
                    $"{Environment.NewLine}" +
                    $"{e.StackTrace}");
            }
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