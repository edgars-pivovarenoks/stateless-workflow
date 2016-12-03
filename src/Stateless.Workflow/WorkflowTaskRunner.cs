using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Workflow
{
    /// <summary>
    /// Workflow task run helper for controlled task execution according to configured persmissions.
    /// </summary>
    /// <typeparam name="TStatus">The type of the status.</typeparam>
    /// <typeparam name="TActivity">The type of the activity.</typeparam>
    /// <typeparam name="TTask">The type of the task.</typeparam>
    /// <typeparam name="TActor">The type of the actor.</typeparam>
    /// <seealso cref="Dictionary{TKey,TValue}" />
    /// <seealso cref="System.Action" />
    public class WorkflowTaskRunner<TStatus, TActivity, TTask, TActor> : Dictionary<TTask, Action>
        where TTask : struct
        where TStatus : struct
        where TActivity : struct
        where TActor : struct
    {
        /// <summary>
        /// Report of executed tasks
        /// </summary>
        /// <typeparam name="T">The type of the status.</typeparam>
        public class TaskRunReport<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TaskRunReport{T}"/> class.
            /// </summary>
            /// <param name="tasks">The tasks.</param>
            internal TaskRunReport(IEnumerable<T> tasks)
            {
                Tasks = new List<T>(tasks);
            }
            /// <summary>
            /// Gets the tasks.
            /// </summary>
            /// <value>
            /// The tasks.
            /// </value>
            public ICollection<T> Tasks { get; private set; }
        }

        private readonly IWorkflow<TStatus, TActivity, TTask, TActor> _workflow;
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowTaskRunner{TStatus, TActivity, TTask, TActor}"/> class.
        /// </summary>
        /// <param name="workflow">The workflow.</param>
        public WorkflowTaskRunner(IWorkflow<TStatus, TActivity, TTask, TActor> workflow)
        {
            _workflow = workflow;
        }
        /// <summary>
        /// Runs configured and permitted tasks in the workflow instance
        /// </summary>
        /// <returns>Task execution instance of <see cref="TaskRunReport{TStatus}"/> containing executes task types</returns>
        public TaskRunReport<TTask> Do()
        {
            _workflow
                .ThrowIfNoTasksAreAllowed();

            var tasksBatch = this.Where(task => _workflow
                .Allows(task.Key))
                .Select(work => work)
                .ToList();

            tasksBatch
                .Select(doTask => doTask.Value)
                .ToList()
                .ForEach(doTask => doTask());

            var tasksDone = tasksBatch
                .Select(taskType => taskType.Key)
                .ToList();

            return 
                new TaskRunReport<TTask>(tasksDone);
        }
    }
}
