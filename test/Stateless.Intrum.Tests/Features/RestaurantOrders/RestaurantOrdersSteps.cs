using System;
using TechTalk.SpecFlow;
using FluentAssertions;
using Stateless.Workflow.Example;

namespace Stateless.Workflow.Features.RestaurantOrders
{
    [Binding]
    public class RestaurantOrdersSteps
    {
        public ActorProviderManual ActorProvider { get; set; }
        public IWorkflowProvider WorkflowProvider { get; set; }
        private MealOrder _scenarioOrder;
        private IRules _scenarioRules;
        [Given(@"customer has arrived to restaurant")]
        public void GivenCustomerHasArrivedToRestaurant()
        {
            _scenarioOrder = new MealOrder
            {
                Status = Status.CustomerArrived,
                Version = RestaurantOrderWorkflowDefault.VersionKeyString
            };
        }
        [Given(@"there are free tables available")]
        public void GivenThereAreFreeTablesAvailable()
        {
            _scenarioRules =
                new RestaurantOrderRules(tablesAvailable: true);
        }
        [When(@"waiter provides a table to the customer")]
        public void WhenWaiterProvidesATableToTheCustomer()
        {
            ActorProvider
                .Set(Actor.Waiter);

            WorkflowProvider
                .GetWorkflow(_scenarioOrder, _scenarioRules)
                .ProvideTable();

            ActorProvider
                .Reset();
        }
        [Then(@"waiter starts to wait for order")]
        public void ThenWaiterStartsToWaitForOrder()
        {
            _scenarioOrder.Status
                .ShouldBeEquivalentTo(Status.WaitingForOrder);
        }

        [Given(@"there are no tables available")]
        public void GivenThereAreNoTablesAvailable()
        {
            _scenarioRules =
                new RestaurantOrderRules(tablesAvailable: false);
        }

        [When(@"waiter rejects customer")]
        public void WhenWaiterRejectsCustomer()
        {
            ActorProvider
                .Set(Actor.Waiter);

            WorkflowProvider
                .GetWorkflow(_scenarioOrder, _scenarioRules)
                .RejectCustomer();

            ActorProvider
                .Reset();
        }

        [Then(@"customer leaves restaurant")]
        public void ThenCustomerLeavesRestaurant()
        {
            _scenarioOrder.Status
                .ShouldBeEquivalentTo(Status.CustomerLeft);
        }

    }
}
