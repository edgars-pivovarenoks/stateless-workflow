namespace Stateless.Workflow.Example
{
    public interface IWorkflowProvider
    {
        IRestaurantOrderWorkflow GetWorkflow(MealOrder order, IRules rules);
    }
}