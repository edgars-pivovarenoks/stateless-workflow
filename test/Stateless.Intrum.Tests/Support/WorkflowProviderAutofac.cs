using Autofac;
using Stateless.Workflow;
using Stateless.Workflow.Example;

public class WorkflowProviderAutofac : IWorkflowProvider
{
    private const string orderParamName = "order";
    private const string rulesParamName = "rules";
    private IComponentContext _scope;
    public WorkflowProviderAutofac(IComponentContext scope)
    {
        _scope = scope;
    }
    public IRestaurantOrderWorkflow GetWorkflow(MealOrder order, IRules rules)
    {
        // Refactor away from "locator" pattern
        // http://docs.autofac.org/en/latest/best-practices/
        return _scope
            .ResolveNamed<IRestaurantOrderWorkflow>(order.Version,
                new[] {
                    new NamedParameter(orderParamName, order.ToStateObject<Status>()),
                    new NamedParameter(rulesParamName, rules)
                });
    }
}
