namespace Stateless.Intrum
{
    /// <summary>
    /// Helper to chain multiple Fire trigger commands
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TActivity">The type of the activity.</typeparam>
    public class FireIfResult<TState, TActivity>
            where TState : struct
            where TActivity : struct
    {
        private bool _processNext;
        private StateMachine<TState, TActivity> _machine;
        /// <summary>
        /// Initializes a new instance of the <see cref="FireIfResult{TState, TActivity}"/> class use by <see cref="WorkflowExtensions"/>.
        /// </summary>
        /// <param name="processNext">if set to <c>true</c> [process next].</param>
        /// <param name="machine">The machine.</param>
        public FireIfResult(bool processNext, StateMachine<TState, TActivity> machine)
        {
            _processNext = processNext;
            _machine = machine;
        }
        /// <summary>
        /// Fire the trigger if previous transition was successful.
        /// </summary>
        /// <param name="trigger">Next trigger.</param>
        public void ThenFire(TActivity trigger)
        {
            if(_processNext)
                _machine.Fire(trigger);
        }
    }
}
