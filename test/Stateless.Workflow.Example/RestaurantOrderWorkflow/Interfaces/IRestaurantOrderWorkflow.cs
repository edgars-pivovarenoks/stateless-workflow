namespace Stateless.Workflow.Example
{
    public interface IRestaurantOrderWorkflow
    {
        void ProvideTable();
        void RejectCustomer();
    }
}