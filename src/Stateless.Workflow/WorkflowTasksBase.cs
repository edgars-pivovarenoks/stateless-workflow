using System;
using System.Collections.Generic;
using System.Linq;
using Stateless.Workflow.Exceptions;
namespace Stateless.Workflow
{
    /// <summary>
    /// Workflow base class supporting tasks control functionality.
    /// </summary>
    /// <typeparam name="TStatus">The type of the status.</typeparam>
    /// <typeparam name="TActivity">The type of the activity.</typeparam>
    /// <typeparam name="TTask">The type of the task.</typeparam>
    /// <typeparam name="TActor">The type of the actor.</typeparam>
    /// <seealso cref="Workflow.IWorkflowTasks{TStatus, TActivity, TTask, TActor}" />
    public abstract partial class WorkflowTasksBase<TStatus, TActivity, TTask, TActor> : WorkflowBase<TStatus, TActivity, TActor>, IWorkflowTasks<TStatus, TActivity, TTask, TActor>
        where TTask : struct
        where TStatus : struct
        where TActivity : struct
        where TActor : struct
    {
        private readonly IActorProvider<TActor> _actorProvider;
        private readonly ITasksProvider<TTask> _tasksProvider;
        /// <summary>
        /// Gets or sets the task restrictions by actor.
        /// </summary>
        /// <value>
        /// The task restrictions by actor.
        /// </value>
        internal Dictionary<TActor, RestrictionList<TStatus, TTask>> TaskRestrictionsByActor { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowTasksBase{TStatus, TActivity, TTask, TActor}"/> class.
        /// </summary>
        /// <param name="versionKey">Workflow instance version key</param>
        /// <param name="statusObject">The loan application.</param>
        /// <param name="actorProvider">The actor provider.</param>
        /// <param name="tasksProvider">The tasks provider.</param>
        protected WorkflowTasksBase(string versionKey,
            IStateObject<TStatus> statusObject,
            IActorProvider<TActor> actorProvider,
            ITasksProvider<TTask> tasksProvider) : base(versionKey, statusObject, actorProvider)
        {
            _tasksProvider = tasksProvider;
            _actorProvider = actorProvider;

            TaskRestrictionsByActor =
                new Dictionary<TActor, RestrictionList<TStatus, TTask>>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requredTasks"></param>
        /// <returns></returns>
        protected TransitionGuard RequireCompleted(params TTask[] requredTasks)
        {
            var tasksGuard = new TransitionGuard(

                guardMethod: () => !requredTasks
                    .Except(_tasksProvider
                    .GetCompletedTasks())
                    .Any(),

                description: requredTasks
                    .Select(r => r.ToString())
                    .ToSeparatedString(", ")

                );

            return
                tasksGuard;
        }
        /// <summary>
        /// Configures the actor restrictions.
        /// </summary>
        /// <param name="actor">The actor.</param>
        /// <returns></returns>
        protected RestrictionList<TStatus, TTask> ConfigureTasksFor(TActor actor)
        {
            var restrictionList = new RestrictionList<TStatus, TTask>()
                .SetStateAccessor(() => StatusObject.Status);

            TaskRestrictionsByActor
                .Add(actor, restrictionList);

            return
                restrictionList;
        }
        /// <summary>
        /// Throws if no tasks are allowed.
        /// </summary>
        /// <exception cref="NoTasksWorkflowException"></exception>
        public void ThrowIfNoTasksAreAllowed()
        {
            if (!AllowsAny())
                throw new NoTasksWorkflowException(_actorProvider.GetCurrentImpersonations().ToSeparatedString(),
                    StatusObject.Status.ToString());
        }
        /// <summary>
        /// Throws if DTO not allowed for defined actors.
        /// </summary>
        /// <param name="actorTypes">The actor actors.</param>
        /// <param name="objects">The objects.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void ThrowIfNotAllowed(TActor[] actorTypes, params object[] objects)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Allows any task.
        /// </summary>
        /// <returns>True if any tasks are allowed</returns>
        public bool AllowsAny()
        {
            return _actorProvider
                .GetCurrentImpersonations()
                .SelectMany(actor => TaskRestrictionsByActor[actor]
                    .GetAllowedCauses())
                .Any();

        }
        /// <summary>
        /// Gets the task permitted statuses.
        /// </summary>
        /// <param name="cause">The cause.</param>
        /// <returns></returns>
        public ICollection<Tuple<string, string>> GetTaskPermittedStatuses(TTask? cause)
        {
            if (!cause.HasValue)
                return new List<Tuple<string, string>>();

            return
                TaskRestrictionsByActor
                    .Select(byactor =>
                        new Tuple<string, string>(
                            byactor.Key.ToString(),
                            byactor.Value.GetCriteriasForCause(cause.Value).ToSeparatedString(", ")
                            )
                    ).ToList();
        }
        /// <summary>
        /// Gets the permitted tasks for actors.
        /// </summary>
        /// <param name="actors">The actor.</param>
        /// <returns></returns>
        public ICollection<TTask> GetPermittedTasksForActors(params TActor[] actors)
        {
            const string actorsParam = "actors";

            actors
                .ThrowIfNull(actorsParam)
                .ThrowIfNone(actorsParam);

            return actors
                .SelectMany(r => TaskRestrictionsByActor[r]
                    .GetAllowedCauses())
                .Distinct()
                .ToList();
        }
        /// <summary>
        /// Throws if not allowed.
        /// </summary>
        /// <param name="objects">The objects.</param>
        public void ThrowIfNotAllowed(params object[] objects)
        {
            var verifyList = objects
                .Where(o => o != null)
                .Select(o => o.GetType())
                .ToArray();

            if (!verifyList.Any())
                return;

            ThrowIfNotAllowedForTypes(verifyList);
        }
        /// <summary>
        /// Is DTOs associated Task denied in workflow, equivalent for !<see cref="Allows(object)"/> method.
        /// </summary>
        /// <param name="obj">The DTO object.</param>
        /// <returns>Not allowed</returns>
        public bool Denies(object obj)
        {
            return
                !Allows(obj);
        }
        /// <summary>
        /// Is DTOs associated Task allowed in workflow.
        /// </summary>
        /// <param name="obj">The DTO object.</param>
        /// <returns>Is allowd</returns>
        public bool Allows(object obj)
        {
            return _actorProvider.GetCurrentImpersonations()
                .Select(actor => TaskRestrictionsByActor[actor])
                .Any(restricitons => restricitons.IsAllowed(obj.GetType()));
        }
        /// <summary>
        /// Is Task in workflow
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        public bool Allows(TTask task)
        {
            return _actorProvider.GetCurrentImpersonations()
                .Select(actor => TaskRestrictionsByActor[actor])
                .Any(restricitons => restricitons.IsAllowed(task));
        }
        /// <summary>
        /// Throws if task is not allowed for current actor and status.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <exception cref="TaskNotAllowedWorkflow"></exception>
        public void ThrowIfNotAllowed(TTask task)
        {
            if (Allows(task))
                return;

            var currentActorTypes = _actorProvider
                 .GetCurrentImpersonations().ToSeparatedString();

            throw
                new TaskNotAllowedWorkflow(task.ToString(), currentActorTypes, CurrentStatusName);
        }
        private void ThrowIfNotAllowedForTypes(params Type[] types)
        {
            // TODO : Create aggregate exception
            foreach (var dtoType in types)
            {
                var isAllowed = _actorProvider.GetCurrentImpersonations()
                    .Select(actor => TaskRestrictionsByActor[actor])
                    .Any(restricitons => restricitons.IsAllowed(dtoType));

                // TODO : Check recursive :
                /*
                var properties = type.GetProperties()
                    .Where(prop => prop.IsDefined(typeof(LocalizedDisplayNameAttribute), false));
                */

                if (!isAllowed)
                    throw
                        new TaskDtoNotAllowedWorkflowException(dtoType.Name, CurrentStatusName, dtoType.GetDescription());
            }

        }
    }
}
