namespace Stateless.Workflow.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a the performed activity is configured in given state.
    /// </summary>
    /// <seealso cref="Stateless.Workflow.Exceptions.WorkflowExceptionBase" />
    public class InvalidWorkflowActivityException : WorkflowExceptionBase
    {
        /// <summary>
        /// The error message template
        /// </summary>
        private const string ErrorMessageTemplate =
            "No valid leaving transitions are permitted from state '{0}' for activity '{1}'. Consider ignoring the activity.";
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidWorkflowActivityException"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="activity">The activity.</param>
        public InvalidWorkflowActivityException(string state, string activity):
            base(ErrorMessageTemplate.With(state, activity))
        {
            CurrentState = state;
            TriedActivity = activity;
            UserFriendlyMessage = Message;
        }
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
    }
}
