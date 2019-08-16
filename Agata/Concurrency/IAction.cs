namespace Agata.Concurrency
{
    /// <summary>
    /// Represents an interface of action.
    /// It's very close to .Net Action but can be implemented with pools for reduce GC impact. 
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Invokes this action.
        /// </summary>
        void Invoke();
    }
}