using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Stateless.Workflow
{
    public abstract partial class WorkflowBase<TStatus, TActivity, TActor>
    {
        /// <summary>
        /// 
        /// </summary>
        public class StateMachineConfiguration
        {
            StateMachine<TStatus, TActivity>.StateConfiguration _stateConfig;
            WorkflowBase<TStatus, TActivity, TActor> _workflow;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="workflow"></param>
            /// <param name="stateConfiguration"></param>
            public StateMachineConfiguration(WorkflowBase<TStatus, TActivity, TActor> workflow, StateMachine<TStatus, TActivity>.StateConfiguration stateConfiguration)
            {
                _stateConfig = stateConfiguration;
                _workflow = workflow;
            }
            /// <summary>
            /// Permits the trigger only if all conditions are met.
            /// </summary>
            /// <param name="trigger">The trigger.</param>
            /// <param name="destinationState">State of the destination.</param>
            /// <param name="actorGuard">The role guard.</param>
            /// <param name="tasksGuard">The tasks guard.</param>
            /// <param name="guards">The guards.</param>
            /// <returns>State machine configuration</returns>
            public StateMachineConfiguration PermitOnlyIf(
                TActivity trigger,
                TStatus destinationState,
                TransitionGuard tasksGuard,
                params Func<bool>[] guards)
            {
                return PermitOnlyIf(trigger, destinationState, null, tasksGuard, guards);
            }
            /// <summary>
            /// Permits the trigger only if all conditions are met.
            /// </summary>
            /// <param name="trigger">The trigger.</param>
            /// <param name="destinationState">State of the destination.</param>
            /// <param name="actorGuard">The role guard.</param>
            /// <param name="tasksGuard">The tasks guard.</param>
            /// <param name="guards">The guards.</param>
            /// <returns>State machine configuration</returns>
            public StateMachineConfiguration PermitOnlyIf(
                TActivity trigger,
                TStatus destinationState,
                TransitionGuard actorGuard,
                TransitionGuard tasksGuard,
                params Func<bool>[] guards)
            {
                // TODO : register tasksGuard

                var listOfGuards = new List<TransitionGuard>(new[] { actorGuard });

                var otherGuards = new List<TransitionGuard>();
                if (tasksGuard != null)
                    otherGuards.Add(tasksGuard);

                otherGuards.AddRange(guards
                    .Select(g => new TransitionGuard(g, g.Method.Name)));
                
                listOfGuards
                    .AddRange(otherGuards);

                //var triggerName = trigger.ToString();
                //var destinationStateName = destinationState.ToString();
                //var currentStateName = stateConfig.State.ToString();

                Func<bool> aggregateGuardExpression = () =>
                {
                    var failedGuards = listOfGuards
                        .Where(g => !g.GuardMethod())
                        .Select(g => g.Description)
                        .ToArray();

                    return
                        !failedGuards.Any();

                    // THIS MIGHT NOT WORK WELL WITH MULTIPLE DESTINATIONS 
                    // FOR SAME TRIGGER BECAUSE OTHER VALID GUARD MIGHT NOT 
                    // HAVE CHANCE TO BE EVALUATED BECAUSE OF THIS EXCEPTION
                    // ON THE OTHER HAND THIS GIVES US EXACT REPORT OF FAILED CONDITIONS
                    // Alternative is to "return false" here but get more generic exeption;
                    // throw new UnmetTransitionGuardsException(currentStateName, triggerName, destinationStateName, failedGuards);
                    // return true;

                    // TODO : Check required tasks

                };

                var otherGuardsSummary = otherGuards
                    .Select(g => g.Description.SplitCamelCase())
                    .ToSeparatedString(", ");

                var otherGuardsDescritpion = otherGuards.Any() ? $" and {otherGuardsSummary}" : "";

                _stateConfig
                    .PermitIf(trigger, destinationState, aggregateGuardExpression, $"{actorGuard.Description}{otherGuardsDescritpion}");

                return
                    this;
            }
            /// <summary>
            /// Configures transition guard based on specified list user of roles.
            /// </summary>
            /// <param name="trigger">The trigger.</param>
            /// <param name="destinationState">State of the destination.</param>
            /// <param name="roleGuard">The actor guard, use this.PermitFor() helper method to define list of permitted roles.</param>
            /// <returns></returns>
            public StateMachineConfiguration PermitForActors(
                TActivity trigger,
                TStatus destinationState,
                TransitionGuard roleGuard)
            {
                _stateConfig
                    .PermitIf(trigger, destinationState, roleGuard.GuardMethod, roleGuard.Description);

                return
                    this;
            }
            public StateMachine<TStatus, TActivity> Machine { get { return _stateConfig.Machine; } }
            /// <summary>
            /// 
            /// </summary>
            public TStatus State { get { return _stateConfig.State; } }
            public StateMachineConfiguration Ignore(TActivity trigger)
            {
                _stateConfig
                    .Ignore(trigger);

                return this;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="guard"></param>
            /// <param name="guardDescription"></param>
            /// <returns></returns>
            public StateMachineConfiguration IgnoreIf(TActivity trigger, Func<bool> guard, string guardDescription = null)
            {
                _stateConfig
                    .IgnoreIf(trigger, guard, guardDescription);

                return this;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="internalAction"></param>
            /// <returns></returns>
            public StateMachineConfiguration InternalTransition(TActivity trigger, Action internalAction)
            {
                _stateConfig
                    .InternalTransition(trigger, internalAction);

                return this;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="entryAction"></param>
            /// <returns></returns>
            public StateMachineConfiguration InternalTransition(TActivity trigger, Action<StateMachine<TStatus, TActivity>.Transition> entryAction)
            {
                _stateConfig
                    .InternalTransition(trigger, entryAction);

                return this;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger"></param>
            /// <param name="internalAction"></param>
            /// <returns></returns>
            public StateMachineConfiguration InternalTransition<TArg0>(StateMachine<TStatus, TActivity>.TriggerWithParameters<TArg0> trigger, Action<TArg0, StateMachine<TStatus, TActivity>.Transition> internalAction)
            {
                _stateConfig
                    .InternalTransition(trigger, internalAction);

                return this;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger"></param>
            /// <param name="internalAction"></param>
            /// <returns></returns>
            public StateMachineConfiguration InternalTransition<TArg0>(TActivity trigger, Action<StateMachine<TStatus, TActivity>.Transition> internalAction)
            {
                _stateConfig
                    .InternalTransition(trigger, internalAction);

                return this;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="internalAction"></param>
            /// <returns></returns>
            public StateMachineConfiguration InternalTransitionAsync(TActivity trigger, Func<Task> internalAction)
            {
                _stateConfig
                    .InternalTransitionAsync(trigger, internalAction);

                return this;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="entryAction"></param>
            /// <returns></returns>
            public StateMachineConfiguration InternalTransitionAsync(TActivity trigger, Func<StateMachine<TStatus, TActivity>.Transition, Task> entryAction)
            {
                _stateConfig
                    .InternalTransitionAsync(trigger, entryAction);

                return this;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger"></param>
            /// <param name="internalAction"></param>
            /// <returns></returns>
			public StateMachineConfiguration InternalTransitionAsync<TArg0>(StateMachine<TStatus, TActivity>.TriggerWithParameters<TArg0> trigger, Func<TArg0, StateMachine<TStatus, TActivity>.Transition, Task> internalAction)
            {
                _stateConfig
                    .InternalTransitionAsync(trigger, internalAction);

                return this;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger"></param>
            /// <param name="internalAction"></param>
            /// <returns></returns>
            public StateMachineConfiguration InternalTransitionAsync<TArg0>(TActivity trigger, Func<StateMachine<TStatus, TActivity>.Transition, Task> internalAction)
            {
                _stateConfig
                    .InternalTransitionAsync(trigger, internalAction);

                return this;
            }
        }
    }
}
