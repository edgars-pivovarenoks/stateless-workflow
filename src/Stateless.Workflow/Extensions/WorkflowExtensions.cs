using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Workflow
{
    /// <summary>
    /// Helper methods for common state machine scenarios
    /// </summary>
    public static class WorkflowExtensions
    {
        /// <summary>
        /// Dynamically wraps any state object having public TStatus propery for workflow engine consumption.
        /// </summary>
        /// <typeparam name="TStatus">The type of the status.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>Dynamic state object instance</returns>
        public static IStateObject<TStatus> ToStateObject<TStatus>(this object obj)
            where TStatus : struct
        {
            return
                new DynamicStateObject<TStatus>(obj);
        }
    }

}
