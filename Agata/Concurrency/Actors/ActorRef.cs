using System;

namespace Agata.Concurrency.Actors
{
    /// <summary>
    /// Represents a strong type reference to an actor.
    /// </summary>
    /// <typeparam name="T">A type of underlying actor subject.</typeparam>
    public class ActorRef<T> : IActorRef where T : class
    {
        private readonly Actor _actor;

        /// <summary>
        /// Initializes a new instance of actor reference.
        /// </summary>
        internal ActorRef(Actor actor)
        {
            _actor = Ensure.NotNull(actor, nameof(actor));
        }

        /// <summary>
        /// Determines is actor in Wait state (not busy).
        /// </summary>
        public bool IsWait => _actor.IsWait;

        /// <summary>
        /// Determines is actor has work.
        /// </summary>
        public bool IsBusy => !IsWait;
        
        /// <summary>
        /// Schedules an action which will be performed on underlying subject in serializable order.
        /// </summary>
        /// <param name="action">An action to perform.</param>
        public void Schedule(Action<T> action)
        {
            _actor.Schedule(action);
        }

        /// <summary>
        /// Kills this actor with subject notification.
        /// </summary>
        /// <param name="action">An action to notify actor subject.</param>
        public void Kill(Action<T> action)
        {
            _actor.Kill(action);
        }
    }

    /// <summary>
    /// Describes interface of actor reference with common functions.
    /// </summary>
    public interface IActorRef
    {
    }
}