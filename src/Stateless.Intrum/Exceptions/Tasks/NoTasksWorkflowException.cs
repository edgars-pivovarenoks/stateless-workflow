using System;

namespace Stateless.Workflow.Exceptions
{
    /// <summary>
    /// The exception that is thrown when there are no allowed tasks configured for current status and actor.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class NoTasksWorkflowException : Exception
    {
        /// <summary>
        /// The error message template
        /// </summary>
        private const string ErrorMessageTemplate =
            "There are no tasks defined for role {0} in status {1}. This exception might indicate that caller code is not handling current status correctly. To avoid this exception you can first check allowed taks in current state by calling Any() or AllowsAny() methods.";
        /// <summary>
        /// Gets the user friendly message.
        /// </summary>
        /// <value>
        /// The user friendly message.
        /// </value>
        public string UserFriendlyMessage { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="NoTasksWorkflowException"/> class.
        /// </summary>
        /// <param name="actorName">Name of the actor.</param>
        /// <param name="currentStatus">The current status.</param>
        public NoTasksWorkflowException(string actorName, string currentStatus)
            : base(ErrorMessageTemplate.With(actorName, currentStatus))
        {
            UserFriendlyMessage = Message;
        }
    }
}
