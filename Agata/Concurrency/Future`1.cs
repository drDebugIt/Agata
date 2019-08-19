using System;
using System.Threading;

namespace Agata.Concurrency
{
    /// <summary>
    /// Represents a something that will be accepted in future.
    /// </summary>
    public class Future<T> : IAwaitable
    {
        private enum Status
        {
            Unknown,
            Success,
            Fail
        }

        private class State
        {
            public static readonly State Unknown = new State(
                Status.Unknown,
                default(T),
                null,
                null,
                null);

            public State(
                Status status,
                T result,
                Exception error,
                Execute execute,
                FutureCallback<T> callback)
            {
                Status = status;
                Result = result;
                Execute = execute;
                Callback = callback;
                Error = error;
            }

            internal readonly Status Status;
            internal readonly T Result;
            internal readonly Exception Error;
            internal readonly Execute Execute;
            internal readonly FutureCallback<T> Callback;
        }

        private volatile State _state;

        /// <summary>
        /// Initializes a new instance of Future. 
        /// </summary>
        internal Future()
        {
            _state = State.Unknown;
        }

        /// <summary>
        /// A result of this future execution if future was finished successfully.
        /// If future does not completed yet throws an exception.
        /// </summary>
        public T Result
        {
            get
            {
                var state = _state;
                switch (state.Status)
                {
                    case Status.Unknown: throw new FutureException("Future is not completed yet.");
                    case Status.Fail: throw new FutureException("Can't get result from failed future.");
                    default: return state.Result;
                }
            }
        }

        /// <summary>
        /// An error of this future execution if future was failed.
        /// </summary>
        public Exception Error
        {
            get
            {
                var state = _state;
                switch (state.Status)
                {
                    case Status.Unknown: throw new FutureException("Future is not completed yet.");
                    case Status.Success: throw new FutureException("Can't get error from success future.");
                    default: return state.Error;
                }
            }
        }

        /// <summary>
        /// Determines is this future completed.
        /// It's mean that this future has success or fail result. 
        /// </summary>
        public bool Completed
        {
            get
            {
                var state = _state;
                return state.Status != Status.Unknown;
            }
        }

        bool IAwaitable.Ready => Completed;

        /// <summary>
        /// Transforms this future result to future with another type.
        /// If this future was failed returns failed future.  
        /// </summary>
        /// <param name="execute">A place to service transformation.</param>
        /// <param name="map">A transformation.</param>
        /// <typeparam name="TR">Type of transformation result.</typeparam>
        /// <returns>A future with transformation result.</returns>
        public Future<TR> MapR<TR>(Execute execute, Func<T, TR> map)
        {
            var promise = new Promise<TR>();
            OnComplete(execute, result =>
            {
                if (result.IsFail)
                {
                    promise.Fail(result.Error);
                    return;
                }
                
                try
                {
                    var transformResult = map(result.Value);
                    promise.Success(transformResult);
                }
                catch (Exception transformError)
                {
                    promise.Fail(transformError);
                }
            });

            return promise.Future;
        }

        /// <summary>
        /// Transforms this future result to future with another type.
        /// </summary>
        /// <param name="execute">A place to service transformation.</param>
        /// <param name="map">A transformation.</param>
        /// <typeparam name="TR">Type of transformation result.</typeparam>
        /// <returns>A future with transformation result.</returns>
        public Future<TR> Map<TR>(Execute execute, Func<Try<T>, Try<TR>> map)
        {
            var promise = new Promise<TR>();
            OnComplete(execute, result =>
            {
                try
                {
                    var transformResult = map(result);
                    if (transformResult.IsSuccess)
                    {
                        promise.Success(transformResult.Value);
                    }
                    else
                    {
                        promise.Fail(transformResult.Error);                        
                    }
                }
                catch (Exception transformError)
                {
                    promise.Fail(transformError);
                }
            });

            return promise.Future;
        }

        /// <summary>
        /// Sets callback which executed when this future will completed.
        /// </summary>
        /// <param name="execute">A place which used to service specified callback.</param>
        /// <param name="callback">A callback.</param>
        public void OnComplete(Execute execute, FutureCallback<T> callback)
        {
            Ensure.NotNull(execute, nameof(execute));
            Ensure.NotNull(callback, nameof(callback));

            while (true)
            {
                var oldState = _state;
                if (oldState.Callback != null)
                {
                    throw new FutureException("Can't set Future callback because it already in set.");
                }

                var newState = new State(
                    oldState.Status,
                    oldState.Result,
                    oldState.Error,
                    execute,
                    callback);

                var prevState = Interlocked.CompareExchange(ref _state, newState, oldState);
                if (prevState == oldState)
                {
                    PerformCallback(newState);
                    return;
                }
            }
        }

        /// <summary>
        /// Finishes this future with success result.
        /// </summary>
        /// <param name="result">A result of this future execution.</param>
        internal void Success(T result)
        {
            while (true)
            {
                var oldState = _state;
                if (oldState.Status != Status.Unknown)
                {
                    var message =
                        $"Can't set Future to {Status.Success} state " +
                        $"because it already in {oldState.Status} state.";

                    throw new FutureException(message);
                }

                var newState = new State(
                    Status.Success,
                    result,
                    oldState.Error,
                    oldState.Execute,
                    oldState.Callback);

                var prevState = Interlocked.CompareExchange(ref _state, newState, oldState);
                if (prevState == oldState)
                {
                    PerformCallback(newState);
                    return;
                }
            }
        }

        /// <summary>
        /// Finishes this future with fail result. 
        /// </summary>
        /// <param name="error">An error which occured in this future execution.</param>
        internal void Fail(Exception error)
        {
            while (true)
            {
                var oldState = _state;
                if (oldState.Status != Status.Unknown)
                {
                    var message =
                        $"Can't set Future to {Status.Fail} state " +
                        $"because it already in {oldState.Status} state.";

                    throw new FutureException(message);
                }

                var newState = new State(
                    Status.Fail,
                    oldState.Result,
                    error,
                    oldState.Execute,
                    oldState.Callback);

                var prevState = Interlocked.CompareExchange(ref _state, newState, oldState);
                if (prevState == oldState)
                {
                    PerformCallback(newState);
                    return;
                }
            }
        }

        private static void PerformCallback(State state)
        {
            if (state.Status == Status.Unknown)
            {
                return;
            }
            
            var result = state.Status == Status.Success ? Try<T>.Success(state.Result) : Try<T>.Fail(state.Error);
            state.Execute.Action(() => state.Callback(result));
        }
    }
}