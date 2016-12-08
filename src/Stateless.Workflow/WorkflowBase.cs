using Stateless.Workflow.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Stateless.Workflow
{
    /// <summary>
    /// Workflow base class
    /// </summary>
    /// <typeparam name="TStatus"></typeparam>
    /// <typeparam name="TActivity"></typeparam>
    /// <typeparam name="TActor"></typeparam>
    public partial class WorkflowBase<TStatus, TActivity, TActor> : IWorkflow<TStatus, TActivity, TActor>
        where TStatus : struct
        where TActivity : struct
        where TActor : struct
    {
        private readonly IActorProvider<TActor> _actorProvider;
        private readonly Dictionary<string, StateMachine<TStatus, TActivity>.TriggerWithParameters> _triggersWithParameters;
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowTasksBase{TStatus, TActivity, TTask, TActor}"/> class.
        /// </summary>
        /// <param name="versionKey">Workflow instance version key</param>
        /// <param name="statusObject">The loan application.</param>
        /// <param name="actorProvider">The actor provider.</param>
        protected WorkflowBase(string versionKey,
            IStateObject<TStatus> statusObject,
            IActorProvider<TActor> actorProvider)
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
