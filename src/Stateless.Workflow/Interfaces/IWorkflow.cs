using System;
using System.Collections.Generic;

namespace Stateless.Workflow
{
    /// <summary>
    /// Defines interface for workflow base class with tasks control functionality
    /// </summary>
    /// <typeparam name="TStatus"></typeparam>
    /// <typeparam name="TActivity"></typeparam>
    /// <typeparam name="TTask"></typeparam>
    /// <typeparam name="TActor"></typeparam>
    public interface IWorkflowTasks<TStatus, TActivity, TTask, TActor> : IWorkflow<TStatus, TActivity, TActor>
        where TTask : struct
        where TStatus : struct
        where TActivity : struct
        where TActor : struct
    {
        /// <summary>
        /// Gets the task permitted statuses.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        ICollection<Tuple<string, string>> GetTaskPermittedStatuses(TTask? task);
        /// <summary>
        /// Gets the permitted tasks for actors.
        /// </summary>
        /// <param name="actors">The actors.</param>
        /// <returns></returns>
        ICollection<TTask> GetPermittedTasksForActors(params TActor[] actors);
        /// <summary>
        /// Throws if not allowed.
        /// </summary>
        /// <param name="task">The task.</param>
        void ThrowIfNotAllowed(TTask task);
        /// <summary>
        /// Throws if not allowed.
        /// </summary>
        /// <param name="objects">The objects.</param>
        void ThrowIfNotAllowed(params object[] objects);
        /// <summary>
        /// Denieses the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        bool Denies(object obj);
        /// <summary>
        /// Allowses the specified cause.
        /// </summary>
        /// <param name="cause">The cause.</param>
        /// <returns></returns>
        bool Allows(TTask cause);
        /// <summary>
        /// Throws if no tasks are allowed.
        /// </summary>
        void ThrowIfNoTasksAreAllowed();
    }
    /// <summary>
    /// Defines interface for workflow base class 
    /// </summary>
    /// <typeparam name="TStatus">The type of the status.</typeparam>
    /// <typeparam name="TActivity">The type of the activity.</typeparam>
    /// <typeparam name="TTask">The type of the task.</typeparam>
    /// <typeparam name="TActor">The type of the actor.</typeparam>
    public interface IWorkflow<TStatus, TActivity, TActor>
        where TStatus : struct
        where TActivity : struct
        where TActor : struct
    {
        /// <summary>
        /// Gets the version key.
        /// </summary>
        /// <value>
        /// The version key.
        /// </value>
        string VersionKey { get; }
        /// <summary>
        /// Gets the status object.
        /// </summary>
        /// <value>
        /// The status object.
        /// </value>
        IStateObject<TStatus> StatusObject { get; }
        /// <summary>
        /// Gets the current status.
        /// </summary>
        /// <value>
        /// The current status.
        /// </value>
        TStatus CurrentStatus { get; }
        /// <summary>
        /// To the dot graph.
        /// </summary>
        /// <returns></returns>
        string ToDotGraph();
        /// <summary>
        /// Gets the permitted activities.
        /// </summary>
        /// <returns></returns>
        ICollection<TActivity> GetPermittedActivities();
    }
}