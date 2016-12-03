using System;

namespace Stateless.Workflow.Exceptions
{
    /// <summary>
    /// Base for workflow related exceptions
    /// </summary>
    /// <seealso cref="System.Exception" />
    public abstract class WorkflowExceptionBase : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowExceptionBase"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="userFriendlyMessage">The user friendly message.</param>
        protected WorkflowExceptionBase(string message, string userFriendlyMessage) : base(message) { UserFriendlyMessage = userFriendlyMessage; }
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowExceptionBase"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        protected WorkflowExceptionBase(string message) : base(message) { }
        /// <summary>
        /// Gets or sets the user friendly message.
        /// </summary>
        /// <value>
        /// The user friendly message.
        /// </value>
        public string UserFriendlyMessage { get; protected set; }
    }
}
