using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Agata.Logging;
using static Agata.Ensure;

namespace Agata.Concurrency.Actors
{
    /// <summary>
    /// Represents a system which manages actors workflow.
    /// </summary>
    public sealed class ActorSystem
    {
        private static readonly ILog Log = Logging.Log.For<ActorSystem>();

        private readonly string _name;
        private readonly IThreadPool _threadPool;
        private readonly ConcurrentDictionary<Type, Func<object>> _actorFactory;
        private readonly Dictionary<string, IActorRef> _actors;

        /// <summary>
        /// Initializes an actor system with specified thread pool.
        /// </summary>
        public ActorSystem(string name, IThreadPool threadPool)
        {
            _name = NotBlank(name, nameof(name));
            _threadPool = NotNull(threadPool, nameof(threadPool));
            _actorFactory = new ConcurrentDictionary<Type, Func<object>>();
            _actors = new Dictionary<string, IActorRef>();
        }

        /// <summary>
        /// Registers factory for actor with specified type.
        /// </summary>
        /// <param name="actorFactory">Factory of actors with type T.</param>
        /// <typeparam name="T">Type of actors produced with specified factory.</typeparam>
        /// <returns>An updated instance of this actors system.</returns>
        public ActorSystem RegisterFactoryOf<T>(Func<T> actorFactory) where T : class
        {
            NotNull(actorFactory, nameof(actorFactory));
            var actorType = typeof(T);
            if (!_actorFactory.TryAdd(actorType, actorFactory))
            {
                var error = $"Factory for actor with type '{actorType}' already registered.";
                throw new InvalidOperationException(error);
            }

            return this;
        }

        /// <summary>
        /// Gets exist or creates new actor of type T which associated with specified name.
        /// </summary>
        /// <typeparam name="T">Type of requested actor.</typeparam>
        /// <returns>Reference to actor associated with specified name.</returns>
        public ActorRef<T> ActorOf<T>(string actorName) where T : class
        {
            NotBlank(actorName, nameof(actorName));

            var actorType = typeof(T);
            if (!_actorFactory.TryGetValue(actorType, out var actorFactory))
            {
                var error =
                    "Can not find actor factory (" +
                    $"actor_system={_name}," +
                    $"actor_name={actorName}," +
                    $"actor_type={actorType})";

                Log.Error(error);
                throw new InvalidOperationException(error);
            }

            lock (_actors)
            {
                if (_actors.TryGetValue(actorName, out var actorRef))
                {
                    if (!(actorRef is ActorRef<T> typedActorRef))
                    {
                        var error =
                            "Actor was found but has unexpected type (" +
                            $"actor_system={_name}," +
                            $"actor_name={actorName}," +
                            $"requested_type={typeof(ActorRef<T>)}," +
                            $"actual_type={actorRef.GetType()})";

                        Log.Error(error);
                        throw new InvalidOperationException(error);
                    }

                    return typedActorRef;
                }

                try
                {
                    var subject = actorFactory();
                    if (subject == null)
                    {
                        var error =
                            "Actor created by actor factory has a null value (" +
                            $"actor_system={_name}," +
                            $"actor_name={actorName}," +
                            $"actor_type={actorType})";

                        Log.Error(error);
                        throw new InvalidOperationException(error);
                    }

                    var sw = Stopwatch.StartNew();
                    var actor = new Actor(actorName, _threadPool, subject);
                    sw.Stop();

                    if (Log.IsDebugEnabled)
                    {
                        var message =
                            "New actor was created (" +
                            $"actor_system={_name}," +
                            $"actor_name={actorName}," +
                            $"actor_type={actorType}," +
                            $"elapsed={sw.Elapsed.TotalMilliseconds:0}ms)";

                        Log.Debug(message);
                    }

                    var typedActorRef = new ActorRef<T>(actor);
                    _actors.Add(actorName, typedActorRef);

                    return typedActorRef;
                }
                catch (Exception e)
                {
                    var error =
                        "Unexpected error on actor creation (" +
                        $"actor_system={_name}," +
                        $"actor_name={actorName}," +
                        $"actor_type={actorType}): " +
                        $"{e.Message}" +
                        $"{Environment.NewLine}" +
                        $"{e.StackTrace}";

                    Log.Error(error);
                    throw;
                }
            }
        }
    }
}