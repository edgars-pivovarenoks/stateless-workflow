namespace Stateless.Workflow
{
    /// <summary>
    /// Defines minimum interface for object holding the status
    /// </summary>
    /// <typeparam name="TStatus">The type of the status.</typeparam>
    public interface IStateObject<TStatus> 
        where TStatus : struct
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        TStatus Status { get; set; }
        /// <summary>
        /// Gets or sets the workflow version key.
        /// </summary>
        /// <value>
        /// The workflow version key.
        /// </value>
        string WorkflowVersionKey { get; set; }
    }
}
