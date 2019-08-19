using System.Linq;

namespace Agata.Concurrency
{
    public sealed class AwaitableComposition : IAwaitable
    {
        private readonly IAwaitable[] _awaitables;

        public AwaitableComposition(IAwaitable[] awaitables)
        {
            _awaitables = awaitables;
        }

        public bool Ready => _awaitables.All(_ => _.Ready);
    }
}