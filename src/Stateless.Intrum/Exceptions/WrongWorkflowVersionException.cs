using System;

namespace Stateless.Workflow.Exceptions
{
    /// <summary>
    /// Wrong workflow version exception, StateObject has been originally created with different Worfkflow.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class WrongWorkflowVersionException : Exception
    {
        private const string ErrorMessageTemplate =
            "Supplied state object workflow version key '{0}' doesn't match workflow instance key '{1}'. This exception prevents processing of wrong objects with wrong workflow versions.";
        /// <summary>
        /// Gets the user friendly message.
        /// </summary>
        /// <value>
        /// The user friendly message.
        /// </value>
        public string UserFriendlyMessage { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongWorkflowVersionException"/> class.
        /// </summary>
        /// <param name="objectVersionKey">The object version key.</param>
        /// <param name="workflowVersionKey">The workflow version key.</param>
        public WrongWorkflowVersionException(string objectVersionKey, string workflowVersionKey)
            : base(ErrorMessageTemplate.With(objectVersionKey, workflowVersionKey))
        {
            UserFriendlyMessage = Message;
        }
    }
}
