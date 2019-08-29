using System;
using System.Collections.Concurrent;

namespace Agata.Collections
{
    public sealed class FixedArrayPool<T>
    {
        private readonly int _bufferSize;
        private readonly int _maxPoolSize;
        private readonly ConcurrentStack<T[]> _pool;

        public FixedArrayPool(int bufferSize, int maxPoolSize)
        {
            _bufferSize = bufferSize;
            _maxPoolSize = maxPoolSize;
            _pool = new ConcurrentStack<T[]>();
        }

        public T[] Get()
        {
            return _pool.TryPop(out var buffer) ? buffer : new T[_bufferSize];
        }

        public void Return(T[] buffer)
        {
            Ensure.NotNull(buffer, nameof(buffer));

            if (buffer.Length != _bufferSize)
            {
                var error =
                    "Returned buffer size does not correspond with original size (" +
                    $"original_size={_bufferSize}," +
                    $"buffer_size={buffer.Length})";

                throw new InvalidOperationException(error);
            }

            if (_pool.Count >= _maxPoolSize)
            {
                return;
            }

            _pool.Push(buffer);
        }
    }
}