using System;
using System.Collections.Generic;

namespace Stateless.Workflow
{
    /// <summary>
    /// Helper class to facilitate creation of guard conditions based on roles, used by extension method <see cref="WorkflowExtensions.PermitForActors{TState,TActivity}"/> and created by <see cref="WorkflowTasksBase{TStatus,TActivity,TTask,TActor}.For"/> method.
    /// </summary>
    public class TransitionGuard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionGuard"/> class.
        /// </summary>
        /// <param name="guardMethod">The guard method.</param>
        /// <param name="description">The description.</param>
        public TransitionGuard(Func<bool> guardMethod, string description)
        {
            guardMethod
                .ThrowIfNull("guardMethod");

            description
                .ThrowIfNull("description");

            GuardMethod = guardMethod;
            Description = description;
        }

        /// <summary>
        /// Gets or sets the dynamic role guard method. Assigned by <see cref="WorkflowTasksBase{TStatus,TActivity,TTask,TActor}.For"/> method.
        /// </summary>
        /// <value>
        /// The guard method.
        /// </value>
        public Func<bool> GuardMethod { get; private set; }
        /// <summary>
        /// List of allowed the roles.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        public string Description { get; set; }

    }
}
