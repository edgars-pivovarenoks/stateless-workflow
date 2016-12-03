using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stateless.Workflow.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an action is allowed, but pre-conditions for transition are not met.
    /// </summary>
    /// <seealso cref="Stateless.Workflow.Exceptions.WorkflowExceptionBase" />
    public sealed class UnmetTransitionGuardsException : WorkflowExceptionBase
    {
        /// <summary>
        /// The error message template
        /// </summary>
        private const string ErrorMessageTemplate =
            "Also '{1}' is allowed in state '{0}' with target state '{2}', it can not be completed because '{3}' condition(-s) are not met.";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmetTransitionGuardsException"/> class.
        /// </summary>
        /// <param name="currentState">State of the current.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="targetState">State of the target.</param>
        /// <param name="guardDescriptions">The guard descriptions.</param>
        public UnmetTransitionGuardsException(string currentState, string trigger, string targetState, string[] guardDescriptions) :
            base(ErrorMessageTemplate.With(currentState, trigger, targetState, guardDescriptions.ToSeparatedString()))
        {
            CurrentState = currentState;
            TargetState = targetState;
            TriedActivity = trigger;
            UserFriendlyMessage = Message;
            FailedGuards = guardDescriptions;
        }
        /// <summary>
        /// Gets the state of the target.
        /// </summary>
        /// <value>
        /// The state of the target.
        /// </value>
        public string TargetState { get; private set; }
        /// <summary>
        /// Gets the state of the current.
        /// </summary>
        /// <value>
        /// The state of the current.
        /// </value>
        public string CurrentState { get; private set; }
        /// <summary>
        /// Gets the tried activity.
        /// </summary>
        /// <value>
        /// The tried activity.
        /// </value>
        public string TriedActivity { get; private set; }
        /// <summary>
        /// Gets the failed guards.
        /// </summary>
        /// <value>
        /// The failed guards.
        /// </value>
        public string[] FailedGuards { get; private set; }

    }
}
