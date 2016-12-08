using Stateless.Workflow;
namespace Stateless.Workflow.Example
{
    public class RestaurantOrderWorkflowDefault : WorkflowBase<Status, Activity, TaskType, Actor>, IRestaurantOrderWorkflow
    {
        public const string VersionKeyString = "MealOrder_V1";
        private IRules _rules;
        public RestaurantOrderWorkflowDefault(IStateObject<Status> order, IRules rules, IActorProvider<Actor> actorProvider) : base(VersionKeyString, order, actorProvider)
        {
            _rules = rules;


            Configure(Status.CustomerArrived)
                .PermitOnlyIf(Activity.RejectCustomer, Status.CustomerLeft, For(Actor.Waiter),
                    _rules.NoTablesAvailable)
                .PermitOnlyIf(Activity.ProvideTable, Status.WaitingForOrder, For(Actor.Waiter),
                    _rules.TablesAvailable);

            Configure(Status.WaitingForOrder)
                .PermitForActors(Activity.MakeAnOrder, Status.OrderPlacedWithWaiter, For(Actor.Customer));

            Configure(Status.OrderPlacedWithWaiter)
                .PermitForActors(Activity.HandOrderToKithcen, Status.OrderHandedToKithcen, For(Actor.Waiter));

            Configure(Status.OrderHandedToKithcen)
                .PermitForActors(Activity.BeginPreparation, Status.DiashBeingPrepared, For(Actor.Cook));

            ConfigureTasksFor(Actor.Waiter)
                .Allow(TaskType.GreetCustomer, TaskType.ProvideMenu)
                .When(Status.CustomerArrived);

            
        }
        public void ProvideTable()
        {
            Fire(Activity.ProvideTable);
        }
        public void RejectCustomer()
        {
            Fire(Activity.RejectCustomer);
        }
    }
}