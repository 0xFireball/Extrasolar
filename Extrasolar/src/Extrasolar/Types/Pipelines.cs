﻿using System;
using System.Collections.Generic;

namespace Extrasolar.Types
{
    public class Pipelines<TInput, TResult>
    {
        protected List<Func<TInput, TResult>> StartHandlers { get; } = new List<Func<TInput, TResult>>();
        protected List<Func<TInput, TResult>> EndHandlers { get; } = new List<Func<TInput, TResult>>();

        /// <summary>
        /// Appends the handler to the end of the start handler list
        /// </summary>
        /// <param name="handler"></param>
        public void AddItemToStart(Func<TInput, TResult> handler)
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
        public void AddItemToEnd(Func<TInput, TResult> handler)
        {
            lock (EndHandlers)
            {
                EndHandlers.Add(handler);
            }
        }

        public IEnumerable<Func<TInput, TResult>> Handlers
        {
            get
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
}