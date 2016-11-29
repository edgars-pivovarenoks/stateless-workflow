using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Intrum
{
    /// <summary>
    /// Helper methods for common state machine scenarios
    /// </summary>
    public static class WorkflowExtensions
    {
        /// <summary>
        /// Configures transition guard based on specified list user of roles.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <typeparam name="TActivity">The type of the activity.</typeparam>
        /// <param name="stateConfig">The state configuration.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="destinationState">State of the destination.</param>
        /// <param name="roleGuard">The role guard, use this.PermitFor() helper method to define list of permitted roles.</param>
        /// <returns></returns>
        public static StateMachine<TState, TActivity>.StateConfiguration PermitForRoles<TState, TActivity>(
            this StateMachine<TState, TActivity>.StateConfiguration stateConfig,
            TActivity trigger,
            TState destinationState,
            TransitionGuard roleGuard)
        {
            return stateConfig
                .PermitIf(trigger, destinationState, roleGuard.GuardMethod, roleGuard.Description);
        }
        /// <summary>
        /// Permits the trigger only if all conditions are met.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <typeparam name="TActivity">The type of the activity.</typeparam>
        /// <param name="stateConfig">The state configuration.</param>
        /// <param name="trigger">The trigger.</param>
        /// <param name="destinationState">State of the destination.</param>
        /// <param name="roleGuard">The role guard.</param>
        /// <param name="guards">The guards.</param>
        /// <returns>State machine configuration</returns>
        public static StateMachine<TState, TActivity>.StateConfiguration PermitOnlyIf<TState, TActivity>(
            this StateMachine<TState, TActivity>.StateConfiguration stateConfig,
            TActivity trigger,
            TState destinationState,
            TransitionGuard roleGuard,
            params Func<bool>[] guards)
        {
            var listOfGuards = new List<TransitionGuard>(new[] { roleGuard });
            
            var otherGuards = guards
                .Select(g => new TransitionGuard(g, g.Method.Name))
                .ToList();

            listOfGuards
                .AddRange(otherGuards);

            var triggerName = trigger.ToString();
            var destinationStateName = destinationState.ToString();
            var currentStateName = stateConfig.State.ToString();

            Func<bool> aggregateGuardExpression = () =>
            {
                var failedGuards = listOfGuards
                    .Where(g => !g.GuardMethod())
                    .Select(g => g.Description)
                    .ToArray();

                if (failedGuards.Any())
                    throw new UnmetTransitionGuardsException(currentStateName, triggerName, destinationStateName, failedGuards);

                return
                    true;
            };

            var otherGuardsSummary = otherGuards
                .Select(g => g.Description.SplitCamelCase())
                .ToSeparatedString(", ");

            var otherGuardsDescritpion = otherGuards.Any() ? $" and {otherGuardsSummary}" : "";

            return stateConfig
                .PermitIf(trigger, destinationState, aggregateGuardExpression, $"{roleGuard.Description}{otherGuardsDescritpion}");
        }
        /// <summary>
        /// Fires state machine trigger and throws exception if it is not allowed.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <typeparam name="TActivity">The type of the activity.</typeparam>
        /// <param name="machine">The machine.</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>State machine instance</returns>
        /// <exception cref="InvalidWorkflowActivityException"></exception>
        public static StateMachine<TState, TActivity> FireWithException<TState, TActivity>(this StateMachine<TState, TActivity> machine, TActivity trigger)
              where TState : struct
              where TActivity : struct
        {
            if (!machine.PermittedTriggers.Contains(trigger))
                throw new InvalidWorkflowActivityException(machine.State.ToString(), trigger.ToString());

            machine
                .Fire(trigger);

            return
                machine;
        }

        /// <summary>
        /// Same as <see cref="FireWithException{TState,TActivity}"/>, but returns machine instance if success.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <typeparam name="TActivity">The type of the activity.</typeparam>
        /// <param name="machine">The machine.</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>State machine instance</returns>
        public static StateMachine<TState, TActivity> FireAndContinue<TState, TActivity>(this StateMachine<TState, TActivity> machine, TActivity trigger)
            where TState : struct
            where TActivity : struct
        {
            machine
                .FireWithException(trigger);

            return
                machine;
        }
        /// <summary>
        /// Safely Fires trigger if condition is correct and always returns same machine instance.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <typeparam name="TActivity">The type of the activity.</typeparam>
        /// <param name="machine">The machine.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="nextTrigger">The next trigger.</param>
        /// <returns><see cref="FireIfResult{TState,TActivity}"/> instance for Fire command chaining</returns>
        public static FireIfResult<TState, TActivity> FireIf<TState, TActivity>(this StateMachine<TState, TActivity> machine, TState condition, TActivity nextTrigger)
            where TState : struct
            where TActivity : struct
        {
            var proceed = machine.State.Equals(condition);
            if (proceed)
                machine.Fire(nextTrigger);

            return
               new FireIfResult<TState, TActivity>(proceed, machine); ;
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
