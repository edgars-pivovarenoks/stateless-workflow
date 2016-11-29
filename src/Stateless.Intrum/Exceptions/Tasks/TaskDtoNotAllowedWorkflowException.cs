namespace Stateless.Workflow.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a task is not allowed in given status.
    /// </summary>
    /// <seealso cref="Stateless.Workflow.Exceptions.WorkflowExceptionBase" />
    public class TaskDtoNotAllowedWorkflowException : WorkflowExceptionBase
    {
        /// <summary>
        /// The error message template
        /// </summary>
        private const string ErrorMessageTemplate =
            "{0} is not allowed in status {1}";

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDtoNotAllowedWorkflowException"/> class.
        /// </summary>
        /// <param name="dtoName">Name of the dto.</param>
        /// <param name="currentStatusName">Name of the current status.</param>
        /// <param name="userFriendlyMethodName">Name of the user friendly method.</param>
        public TaskDtoNotAllowedWorkflowException(string dtoName, string currentStatusName, string userFriendlyMethodName)
            : base(ErrorMessageTemplate.With(userFriendlyMethodName, currentStatusName))
        {
            UserFriendlyMessage = Message;
        }
    }
}
