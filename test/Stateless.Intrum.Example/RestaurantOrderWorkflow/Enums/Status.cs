namespace Stateless.Workflow.Example
{
    public enum Status
    {
        CustomerArrived,
        WaitingForOrder,
        OrderPlacedWithWaiter,
        OrderHandedToKithcen,
        DiashBeingPrepared,
        DishReadyToServe,
        DishPickedUp,
        DishServedToCustomer,
        BillRequested,
        BillProvided,
        CustomerPayed,
        ReminderBrought,
        CustomerLeft
    }
}
