namespace Stateless.Workflow
{
    /// <summary>
    /// Delcares interface for workflow actor provider
    /// </summary>
    /// <typeparam name="TActor">The type of the actor.</typeparam>
    public interface IActorProvider<TActor>
    {
        /// <summary>
        /// Gets the current impersonations.
        /// </summary>
        /// <returns>List of actor types provided by caller</returns>
        TActor[] GetCurrentImpersonations();
    }
}
