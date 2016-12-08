using System;
using System.Collections.Generic;
using System.Linq;
using Stateless.Workflow.Exceptions;
namespace Stateless.Workflow
{
    /// <summary>
    /// Workflow base class.
    /// </summary>
    /// <typeparam name="TStatus">The type of the status.</typeparam>
    /// <typeparam name="TActivity">The type of the activity.</typeparam>
    /// <typeparam name="TTask">The type of the task.</typeparam>
    /// <typeparam name="TActor">The type of the actor.</typeparam>
    /// <seealso cref="Stateless.Workflow.IWorkflow{TStatus, TActivity, TTask, TActor}" />
    public abstract partial class WorkflowBase<TStatus, TActivity, TTask, TActor> : IWorkflow<TStatus, TActivity, TTask, TActor> where TTask : struct
        where TStatus : struct
        where TActivity : struct
        where TActor : struct
    {
        private readonly Dictionary<string, StateMachine<TStatus, TActivity>.TriggerWithParameters> _triggersWithParameters;
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowBase{TStatus, TActivity, TTask, TActor}"/> class.
        /// </summary>
        /// <param name="versionKey">Workflow instance version key</param>
        /// <param name="statusObject">The loan application.</param>
        /// <param name="actorProvider">The actor provider.</param>
        protected WorkflowBase(string versionKey, IStateObject<TStatus> statusObject, IActorProvider<TActor> actorProvider)
        {
            versionKey
                .ThrowIfNull("versionKey");

            // Init object version if it is empty
            if (String.IsNullOrWhiteSpace(statusObject.WorkflowVersionKey))
                statusObject.WorkflowVersionKey = versionKey;

            if (!versionKey.Equals(statusObject.WorkflowVersionKey))
                throw new WrongWorkflowVersionException(statusObject.WorkflowVersionKey, versionKey);

            VersionKey = versionKey;
            StatusObject = statusObject;
            _actorProvider = actorProvider;

            Machine = new StateMachine<TStatus, TActivity>(
                () => StatusObject.Status,
                s => StatusObject.Status = s);

            TaskRestrictionsByActor =
                new Dictionary<TActor, RestrictionList<TStatus, TTask>>();

            _triggersWithParameters =
                new Dictionary<string, StateMachine<TStatus, TActivity>.TriggerWithParameters>();

            Configure();

            Machine
                .OnTransitioned(LogTransition);
        }
        /// <summary>
        /// Gets the workflow version key.
        /// </summary>
        /// <value>
        /// The workflow version key.
        /// </value>
        public string VersionKey { get; private set; }
        private readonly IActorProvider<TActor> _actorProvider;
        /// <summary>
        /// Gets the machine.
        /// </summary>
        /// <value>
        /// The machine.
        /// </value>
        private StateMachine<TStatus, TActivity> Machine { get; }
        /// <summary>
        /// Permits for.
        /// </summary>
        /// <param name="allowedActors">The allowed actors.</param>
        /// <returns></returns>
        protected TransitionGuard For(params TActor[] allowedActors)
        {
            var actorGuard = new TransitionGuard(

                guardMethod: () => _actorProvider
                    .GetCurrentImpersonations()
                    .Any(allowedActors.Contains),

                description: allowedActors
                    .Select(r => r.ToString())
                    .ToSeparatedString(", ")

                );

            return
                actorGuard;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requredTasks"></param>
        /// <returns></returns>
        protected TransitionGuard RequireDone(params TTask[] requredTasks)
        {
            var tasksGuard = new TransitionGuard(

                guardMethod: () => true,
                /*
                () => _tasksProvider
                    .GetTasksDone()
                    .All(requredTasks),
                    */
                description: requredTasks
                    .Select(r => r.ToString())
                    .ToSeparatedString(", ")

                );

            return
                tasksGuard;
        }
        /// <summary>
        /// Gets or sets the task restrictions by actor.
        /// </summary>
        /// <value>
        /// The task restrictions by actor.
        /// </value>
        internal Dictionary<TActor, RestrictionList<TStatus, TTask>> TaskRestrictionsByActor { get; }
        /// <summary>
        /// Gets or sets the status object.
        /// </summary>
        /// <value>
        /// The status object.
        /// </value>
        public IStateObject<TStatus> StatusObject { get; }
        /// <summary>
        /// To the dot graph.
        /// </summary>
        /// <returns></returns>
        public string ToDotGraph()
        {
            return Machine
                .ToDotGraph();
        }
        /// <summary>
        /// Logs the transition.
        /// </summary>
        /// <param name="transition">The transition.</param>
        protected virtual void LogTransition(StateMachine<TStatus, TActivity>.Transition transition)
        {
        }
        /// <summary>
        /// Configures the specified machine.
        /// </summary>
        protected virtual void Configure()
        {
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
        /// <summary>
        /// Gets the permitted activities in current status.
        /// </summary>
        /// <returns>List of activities</returns>
        public ICollection<TActivity> GetPermittedActivities()
        {
            return Machine
                .PermittedTriggers
                .ToList();
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
        /// Gets the name of the current status.
        /// </summary>
        /// <value>
        /// The name of the current status.
        /// </value>
        public string CurrentStatusName => StatusObject.Status.ToString();
        /// <summary>
        /// Gets the current status.
        /// </summary>
        /// <value>
        /// The current status.
        /// </value>
        public TStatus CurrentStatus => StatusObject.Status;
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
        /// Creates and registers new action with parameters.
        /// </summary>
        /// <typeparam name="TArg0">The type of the arg0.</typeparam>
        /// <param name="activity">The activity.</param>
        public StateMachine<TStatus, TActivity>.TriggerWithParameters<TArg0> WithParam<TArg0>(TActivity activity)
        {
            var activityName = activity.ToString();
            var argTypeName = typeof(TArg0).Name;
            var triggerKey = $"{activityName}{argTypeName}";

            StateMachine<TStatus, TActivity>.TriggerWithParameters trigger;

            if (!_triggersWithParameters.TryGetValue(triggerKey, out trigger))
            {
                trigger = new StateMachine<TStatus, TActivity>.TriggerWithParameters<TArg0>(activity);

                _triggersWithParameters.Add(triggerKey, trigger);
            }

            return
                (StateMachine<TStatus, TActivity>.TriggerWithParameters<TArg0>)trigger;

        }
        /// <summary>
        /// Configures the specified status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>State configurator</returns>
        protected StateMachineConfiguration Configure(TStatus status)
        {
            return new StateMachineConfiguration(this, Machine
                .Configure(status));
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
        /// Fires the specified activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        protected void Fire(TActivity activity)
        {
            Machine
                .Fire(activity);
        }
        /// <summary>
        /// Fires the specified trigger.
        /// </summary>
        /// <typeparam name="TArg0">The type of the arg0.</typeparam>
        /// <param name="trigger">The trigger.</param>
        /// <param name="arg0">The arg0.</param>
        protected void Fire<TArg0>(TActivity trigger, TArg0 arg0)
        {
            Machine
                .Fire(WithParam<TArg0>(trigger), arg0);
        }
        /// <summary>
        /// Safely Fires trigger if condition is correct and always returns same machine instance.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="nextTrigger">The next trigger.</param>
        /// <returns><see cref="FireIfResult{TState,TActivity}"/> instance for Fire command chaining</returns>
        public FireIfResult<TStatus, TActivity> FireIf(TStatus condition, TActivity nextTrigger)
        {
            var proceed = Machine.State.Equals(condition);

            if (proceed)
                Machine.Fire(nextTrigger);

            return
                new FireIfResult<TStatus, TActivity>(proceed, Machine); ;
        }
        /// <summary>
        /// Safely Fires trigger if condition is correct and always returns same machine instance.
        /// </summary>
        /// <typeparam name="TArg0">The type of the arg0.</typeparam>
        /// <param name="condition">The condition.</param>
        /// <param name="nextTrigger">The next trigger.</param>
        /// <param name="arg0">The arg0.</param>
        /// <returns><see cref="FireIfResult{TState,TActivity}"/> instance for Fire command chaining</returns>
        public FireIfResult<TStatus, TActivity> FireIf<TArg0>(TStatus condition, TActivity nextTrigger, TArg0 arg0)
        {
            var proceed = Machine.State.Equals(condition);

            if (proceed)
                Machine.Fire(WithParam<TArg0>(nextTrigger), arg0);

            return
                new FireIfResult<TStatus, TActivity>(proceed, Machine); ;
        }
        /// <summary>
        /// Fires state machine trigger and throws exception if it is not allowed.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <returns>State machine instance</returns>
        /// <exception cref="InvalidWorkflowActivityException"></exception>
        public void FireWithException(TActivity trigger)
        {
            if (!Machine.PermittedTriggers.Contains(trigger))
                throw new InvalidWorkflowActivityException(Machine.State.ToString(), trigger.ToString());

            Machine
                .Fire(trigger);
        }
        /// <summary>
        /// Same as <see cref="FireWithException"/>, but returns machine instance if success.
        /// </summary>
        /// <param name="trigger">The trigger.</param>
        /// <returns>State machine instance</returns>
        public StateMachine<TStatus, TActivity> FireAndContinue(TActivity trigger)
        {
            FireWithException(trigger);

            return
                Machine;
        }
    }
}
