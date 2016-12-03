namespace Stateless.Workflow.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the taks is not allowed in given workflow status
    /// </summary>
    /// <seealso cref="Stateless.Workflow.Exceptions.WorkflowExceptionBase" />
    public class TaskNotAllowedWorkflow : WorkflowExceptionBase
    {
        /// <summary>
        /// The error message template
        /// </summary>
        private const string ErrorMessageTemplate =
            "Task '{0}' is not allowed for actor(-s) '{1}' within status '{2}'";
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskNotAllowedWorkflow"/> class.
        /// </summary>
        /// <param name="taskName">Name of the task.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="currentStatus">The current status.</param>
        public TaskNotAllowedWorkflow(string taskName, string roleName, string currentStatus)
            : base(ErrorMessageTemplate.With(taskName, roleName, currentStatus))
        {
            UserFriendlyMessage = Message;
        }
    }

}
