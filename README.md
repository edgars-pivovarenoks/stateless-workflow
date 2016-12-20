# What is Stateless.Workflow?
Stateless.Workflow is a generic and lightweight .NET C# library for creating business workflows using state machines pattern. Project is based on Stateless library that is used to create State Machines in C# code.

### Example 
```csharp
public class LoanApprovalWorklow : WorkflowBase<Status, Activity, TaskType, Actor>
{
    // Parts of code are ommited for illustration purposes.
    protected override void Configure()
    {
        // Application submission
        Configure(Status.Created)
            .OnEntryFrom(Activity.Submit, CheckCreditScore)
            .PermitIf(Activity.Submit, Status.Submitted, HasSufficientCreditScore)
            .PermitIf(Activity.Submit, Status.Refused, HasBadCreditScore);
        // ...
    }
}

public void Submit()
{
    Fire(Activity.Submit);
}

public void Sign(DateTime signedDate)
{
    Fire(Activity.Sign, signedDate);
}
```
# Before You Start
- Be familiar with State Machine paradigm,
- Learn how to use [Stateless](https://github.com/dotnet-state-machine/stateless) state machine
- Be familiar with _Fluent Interface_ pattern

# Features
####  Generic Workflow Base class with built-in concepts of 
- Status - stauses that an target object can be in (ex., _Registered, Submitted, Approved_)
- Activity - business actions that causes object to move to next status (ex., _Submit, Approve, Decline_)
- Task - type of work that is expected to be done in a particular statuses (ex., _UpdateCustomer, VerifyPassportPhoto, SendLetter_)
- Actor - types of actors that are involves in a particular business process (ex., _Operator, Supervisor, Manager_)

#### Workflow Versioning 
- Each workflow implementation must be identified with unique key for example "MORTGAGE_APPROVAL_V1", "LEASE_APPROVAL_LENDER1_V2"
- Proper workflow instantiation is responisibility of application, 
- For workflow instatiation [Factory Pattern](https://www.dotnetperls.com/factory) could be one choice,
- After first run the status object *WorkflowVersionKey* propery will be set to running workflow version,
- The factory can then leverage of *WorkflowVersionKey* an workflow *VersionKey* to continue to run the correct version.

#### Generic Actor Provider 
Please note, Actor in a workflow don't necessary have to be role type in your security layer, to encourage loose coupling workflow will query IActorProvider<TActor> for current user impersonation. Actor provider can be implemented in application and injected into worlflow, this way it is up to the application :
- what workflow actor user is,
- how the workflow actor relate to security in an application. 

#### Actor Guard Clauses
You can limit which Actor can execute particular Activity in given Status. Two methods PermitForActors() & PermitFor() are provided to simplify creation of guards that check actor types of current user.

```csharp
    Configure(Status.PendingAmendment)
        .PermitForActors(Activity.SubmitAmendments, Status.Submitted, For(Actor.Agent));
```

### Multiple Business Rule Clauses

Business rules can be implemented as simple .NET bool methods and configured as Guards for Activities.

```csharp
    Configure(Status.Registered)
        .PermitOnlyIf(Activity.Submit, Status.Submitted, For(Actor.Agent),
            HasProvidedDateOfBirth,
            HasProvidedPassportCopy,
            HasVerifiedEMailAddress);
```

#### Actor Task Permissions
For each Status and Actor we can configure which tasks are permitted, also it is up to application query for permissions.

```csharp
    ConfigureTasks(Actor.Underwriter)
        .Allow(TaskType.UpdateCustomer)
            .When(Status.Preparation, Status.Approved)
        .Allow(TaskType.UpdateCredit, TaskType.UpdateCustomer, TaskType.UpdateFiles)
            .When(Status.Preparation, Status.Submitted)            
```

#### Dynamic Status object
Any .NET object that will implement Status and WorkflowVersion property can be set up as a workflow object.

### Helpful Workflow Methods 
- FireWithException()
- FireAndContinue()
- FireIf()
- ThenFire()
- LogTransition()
- GetTaskPermittedStatuses()
- GetPermittedActivities()
- GetPermittedTasksForActors()
- AllowsAny()
- ThrowIfNoTasksAreAllowed()
- ThrowIfNotAllowed()
- Denies()
- Allows()
- WithParam()

## What's missing?
- Same Actions with conditional Target state are not enough robust;
- Targeting .NET Core :
   - Depends on support in Statless library,
   - There is currently open issue [Target .NET Core #67](https://github.com/dotnet-state-machine/stateless/issues/67);
- Should investigate more  scenarios and benefits of :
- InternalTransition() method,
- SubstateOf() to create super ans sub-states.

## Worth Mentioning
- Stateless library supports export of Machine configuration to [DOT language](https://mdaines.github.io/viz.js/),
- DTO visualization is very helpfule during the development and communication of requirements.

## Links
-----

* [GitHub | dotnet-state-machine/stateless](https://github.com/dotnet-state-machine/stateless)
* [Martin Fowler | Fluent Interface](http://martinfowler.com/bliki/FluentInterface.html)
* [Download Help (CHM file)](doc/Sandcastle/Help/Stateless.Workflow.chm)
* [DOT > Graph Description Language) | Wiki](https://en.wikipedia.org/wiki/DOT_(graph_description_language))
* [Visual Graph generation from DOT | Vis.js & Graphviz](https://mdaines.github.io/viz.js/)
* [Working example implementation & SpecFlow Tests (Work-in-progress)](test/Stateless.Workflow.Example/RestaurantOrderWorkflow)

## Building
-----
Visual Studio 2015 is required to build this project.

## Project Goals

Stateless.Workflow project is an initiative to explore ways to easily create and run lightweight business process workflows.

This project should stay simple but applicable in enterprise development scenarios, enough to delay the resorting to heavy-weight frameworks like Windows Workflow Foundation. 

Please use the issue tracker or the if you'd like to report problems or discuss features.
