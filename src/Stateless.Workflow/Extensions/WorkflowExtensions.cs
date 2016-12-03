using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Workflow
{
    /// <summary>
    /// Helper methods for common state machine scenarios
    /// </summary>
    public static class WorkflowExtensions
    {
        /// <summary>
        /// Permits the trigger only if all conditions are met.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <typeparam name="TActivity">The type of the activity.</typeparam>
        /// <param name="stateConfig">The state configuration.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="destinationState">State of the destination.</param>
        /// <param name="actorGuard">The role guard.</param>
        /// <param name="guards">The guards.</param>
        /// <returns>State machine configuration</returns>
        public static StateMachine<TState, TActivity>.StateConfiguration PermitOnlyIf<TState, TActivity>(
            this StateMachine<TState, TActivity>.StateConfiguration stateConfig,
            TActivity trigger,
            TState destinationState,
            TransitionGuard actorGuard,
            params Func<bool>[] guards)
        {
            var listOfGuards = new List<TransitionGuard>(new[] { actorGuard });

            var otherGuards = guards
                .Select(g => new TransitionGuard(g, g.Method.Name))
                .ToList();

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
            };

            var otherGuardsSummary = otherGuards
                .Select(g => g.Description.SplitCamelCase())
                .ToSeparatedString(", ");

            var otherGuardsDescritpion = otherGuards.Any() ? $" and {otherGuardsSummary}" : "";

            return stateConfig
                .PermitIf(trigger, destinationState, aggregateGuardExpression, $"{actorGuard.Description}{otherGuardsDescritpion}");
        }
        /// <summary>
        /// Configures transition guard based on specified list user of roles.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <typeparam name="TActivity">The type of the activity.</typeparam>
        /// <param name="stateConfig">The state configuration.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="destinationState">State of the destination.</param>
        /// <param name="roleGuard">The actor guard, use this.PermitFor() helper method to define list of permitted roles.</param>
        /// <returns></returns>
        public static StateMachine<TState, TActivity>.StateConfiguration PermitForActors<TState, TActivity>(
            this StateMachine<TState, TActivity>.StateConfiguration stateConfig,
            TActivity trigger,
            TState destinationState,
            TransitionGuard roleGuard)
        {
            return stateConfig
                .PermitIf(trigger, destinationState, roleGuard.GuardMethod, roleGuard.Description);
        }
        /// <summary>
        /// Dynamically wraps any state object having public TStatus propery for workflow engine consumption.
        /// </summary>
        /// <typeparam name="TStatus">The type of the status.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>Dynamic state object instance</returns>
        public static IStateObject<TStatus> ToStateObject<TStatus>(this object obj)
            where TStatus : struct
        {
            return
                new DynamicStateObject<TStatus>(obj);
        }
    }

}
