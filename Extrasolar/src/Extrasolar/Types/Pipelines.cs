using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extrasolar.Types
{
    /// <summary>
    /// Represents a series of handlers that run in a specified order.
    /// Handlers will be called in order until one signals that the parameter has been handled.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class Pipelines<TInput, TResult>
    {
        protected List<Func<TInput, Task<TResult>>> StartHandlers { get; } = new List<Func<TInput, Task<TResult>>>();
        protected List<Func<TInput, Task<TResult>>> EndHandlers { get; } = new List<Func<TInput, Task<TResult>>>();

        /// <summary>
        /// Appends the handler to the end of the start handler list
        /// </summary>
        /// <param name="handler"></param>
        public void AddItemToStart(Func<TInput, Task<TResult>> handler)
        {
            lock (StartHandlers)
            {
                StartHandlers.Add(handler);
            }
        }

        /// <summary>
        /// Appends the handler to the end of the end handler list
        /// </summary>
        /// <param name="handler"></param>
        public void AddItemToEnd(Func<TInput, Task<TResult>> handler)
        {
            lock (EndHandlers)
            {
                EndHandlers.Add(handler);
            }
        }

        public IEnumerable<Func<TInput, Task<TResult>>> GetHandlers()
        {
            foreach (var startHandler in StartHandlers)
            {
                yield return startHandler;
            }
            foreach (var endHandler in EndHandlers)
            {
                yield return endHandler;
            }
        }
    }
}