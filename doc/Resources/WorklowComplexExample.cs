using System;
using AgentPortal.Model;
using AgentPortal.Model.Enums;
using Stateless;
using Stateless.Intrum;
using System.Collections.Generic;
using System.Linq;
using AgentPortal.ApplicationProcessorService.Workflow.V1;
using AgentPortal.DataAccess;
using Curiosity.Entities;

namespace Curiosity
{
    using Status = ApplicationStatus;
    public class LoanApplicationWorklowV1 : WorkflowBase<Status, Activity, TaskType, Actor>, ILoanApplicationWorkflow
    {
        #region Init
        private const string WorkflowVersion = "V1";
        private readonly CDP.IApplicationProcessor _cdpService;
        private readonly CICOne.IApplicationProcessor _cicOneService;
        private readonly Application _application;
        private readonly IApplicationService _applicationService;
        private readonly IRepository<ApplicationTransition> _applicationTransitionRepository;
        private IApplicationWorkflowRules _rules;

        public LoanApplicationWorklowV1(
            Application application,
            IActorProvider<Actor> actorProvider,
            CDP.IApplicationProcessor cdpService,
            CICOne.IApplicationProcessor cicOneService,
            IApplicationService applicationService,
            IRepository<ApplicationTransition> applicationTransitionRepository)
            : base(WorkflowVersion, application.ToStateObject<Status>(), actorProvider)
        {
            _application = application;
            _cdpService = cdpService;
            _cicOneService = cicOneService;
            _applicationService = applicationService;
            _applicationTransitionRepository = applicationTransitionRepository;
        }
        protected override void LogTransition(StateMachine<Status, Activity>.Transition t)
        {
            _applicationTransitionRepository
              .Insert(new ApplicationTransition
              {
                  ApplicationId = _application.Id,
                  FromStatus = t.Source,
                  ToStatus = t.Destination,
                  ByActivity = t.Trigger,
                  CreationTime = DateTime.Now
              });
        }
        #endregion
        #region Setup workflow
        protected override void Configure()
        {
            // Set rule class
            // TODO : Revisit imlementation
            _rules = new LoanApplicationWorkflowRulesV1(() => _application);

            // --------------------------------
            // -- Process
            // --------------------------------
            // Pass-through status to coordinate automatic credit check
            Configure(Status.Preparation)
                .PermitForActors(Activity.Withdraw, Status.Withdrawn, PermitFor(Actor.Agent))
                .PermitForActors(Activity.CheckCredit, Status.CreditChecked, PermitFor(Actor.Agent, Actor.Underwriter));

            // Application submission with credit check
            Configure(Status.CreditChecked)
                .OnEntryFrom(Activity.CheckCredit, DoCreditCheck, TextResources.DoCreditCheck)
                .PermitOnlyIf(Activity.Submit, Status.Preparation, PermitFor(Actor.Agent, Actor.Underwriter),
                    _rules.CreditCheckPending)
                .PermitOnlyIf(Activity.Submit, Status.PendingAmendment, PermitFor(Actor.Agent, Actor.Underwriter),
                    _rules.HasOutstandingFileRequests,
                    _rules.CreditCheckDone)
                .PermitOnlyIf(Activity.Submit, Status.Submitted, PermitFor(Actor.Agent, Actor.Underwriter),
                    _rules.HasSufficientCreditRating,
                    _rules.HasAllRequestedFiles)
                .PermitOnlyIf(Activity.Submit, Status.Refused, PermitFor(Actor.Agent, Actor.Underwriter),
                    _rules.HasBadCreditRating,
                    _rules.HasAllRequestedFiles);

            Configure(Status.Submitted)
                .PermitForActors(Activity.RequestAmendments, Status.PendingAmendment, PermitFor(Actor.Underwriter))
                .PermitForActors(Activity.Withdraw, Status.Withdrawn, PermitFor(Actor.Agent))
                .PermitForActors(Activity.Refuse, Status.Refused, PermitFor(Actor.Underwriter))
                .PermitForActors(Activity.Replace, Status.Replaced, PermitFor(Actor.Agent, Actor.Underwriter))
                .PermitOnlyIf(Activity.CheckBudget, Status.BudgetChecked, PermitFor(Actor.Underwriter),
                    _rules.HasEffectiveInterestRate,
                    _rules.HasCustomerIKO,
                    _rules.HasSpouseIKO);

            Configure(Status.PendingAmendment)
                .PermitOnlyIf(Activity.SubmitAmendments, Status.CreditChecked, PermitFor(Actor.Agent), _rules.HasAllRequestedFiles)
                .PermitForActors(Activity.Withdraw, Status.Withdrawn, PermitFor(Actor.Agent))
                .PermitForActors(Activity.Refuse, Status.Refused, PermitFor(Actor.Underwriter));

            // Pass-through status to coordinate automatic budget check
            Configure(Status.BudgetChecked)
                .OnEntryFrom(Activity.CheckBudget, DoBudgetCheck, TextResources.DoBudgetCheck)
                .PermitIf(Activity.StartApproval, Status.Submitted, _rules.BudgetCheckPending, TextResources.BudgetCheckPending)
                .PermitIf(Activity.StartApproval, Status.ApprovalInProgress, _rules.BudgetCheckDone, TextResources.BudgetCheckDone);

            Configure(Status.ApprovalInProgress)
                .PermitReentry(Activity.ReopenForApproval)
                .PermitReentry(Activity.PrepareContracts)
                .OnEntryFrom(Activity.PrepareContracts, DoPrepareContracts, TextResources.DoPrepareContracts)
                .PermitForActors(Activity.RequestAmendments, Status.PendingAmendment, PermitFor(Actor.Underwriter))
                .PermitForActors(Activity.Withdraw, Status.Withdrawn, PermitFor(Actor.Agent))
                .PermitForActors(Activity.Refuse, Status.Refused, PermitFor(Actor.Underwriter))
                .PermitForActors(Activity.Replace, Status.Replaced, PermitFor(Actor.Agent, Actor.Underwriter))
                .PermitOnlyIf(Activity.Approve, Status.Approved, PermitFor(Actor.Underwriter),
                    _rules.HasPreparedContracts);

            Configure(Status.Approved)
                .PermitForActors(Activity.Replace, Status.Replaced, PermitFor(Actor.Agent, Actor.Underwriter))
                .PermitForActors(Activity.Withdraw, Status.Withdrawn, PermitFor(Actor.Agent))
                .PermitForActors(Activity.Expire, Status.Expired, PermitFor(Actor.Underwriter))
                .PermitForActors(Activity.ReopenForApproval, Status.ApprovalInProgress, PermitFor(Actor.Underwriter))
                .PermitOnlyIf(Activity.Sign, Status.Signed, PermitFor(Actor.Underwriter, Actor.Agent),
                    _rules.HasSignedContracts);

            Configure(Status.Signed)
                .InternalTransition(WithParam<DateTime>(Activity.Sign), SetSignedDate)
                .PermitForActors(Activity.Withdraw, Status.Withdrawn, PermitFor(Actor.Agent))
                .PermitForActors(Activity.Replace, Status.Replaced, PermitFor(Actor.Agent, Actor.Underwriter))
                .PermitForActors(Activity.ReopenForApproval, Status.ApprovalInProgress, PermitFor(Actor.Underwriter))
                .PermitOnlyIf(Activity.StartDisbursement, Status.SentForDisbursement, PermitFor(Actor.Underwriter), _rules.HasSignedDate);

            Configure(Status.SentForDisbursement)
                .OnEntryFrom(Activity.StartDisbursement, ActivateContract, TextResources.ActivateContract)
                .PermitForActors(Activity.ConfirmDisbursement, Status.Disbursed, PermitFor(Actor.Underwriter))
                .PermitForActors(Activity.ReopenForApproval, Status.ApprovalInProgress, PermitFor(Actor.Underwriter));

            Configure(Status.Refused)
                .PermitForActors(Activity.Replace, Status.Replaced, PermitFor(Actor.Agent, Actor.Underwriter))
                .PermitForActors(Activity.ReopenForApproval, Status.ApprovalInProgress, PermitFor(Actor.Underwriter));

            Configure(Status.Expired)
                .PermitForActors(Activity.Replace, Status.Replaced, PermitFor(Actor.Agent, Actor.Underwriter))
                .PermitForActors(Activity.ReopenForApproval, Status.ApprovalInProgress, PermitFor(Actor.Underwriter));

            Configure(Status.Withdrawn)
                .PermitForActors(Activity.Replace, Status.Replaced, PermitFor(Actor.Agent, Actor.Underwriter));

            // Final states
            Configure(Status.Disbursed);
            Configure(Status.Replaced)
                .OnEntryFrom(WithParam<Application>(Activity.Replace), CreateNewApplication, TextResources.CreateNewApplication);

            // --------------------------------
            // -- Restrictions
            // --------------------------------
            // TODO : Add mandatory feature and restrict transitions if not fullfiled somehow?
            ConfigureTasks(Actor.Agent)
                .Allow(
                    TaskType.UpdateApplicationMainInfo)
                    .When(Status.Preparation,
                        Status.Approved)
                .Allow(
                    TaskType.UpdateCredit,
                    TaskType.UpdateCustomer,
                    TaskType.UpdateSpouse)
                    .When(Status.Preparation)
                .Allow(
                    TaskType.UpdateFileRequest,
                    TaskType.UpdateFiles)
                    .When(Status.Preparation,
                        Status.PendingAmendment)
                .Allow(
                    TaskType.UpdateComments)
                    .When(Status.Preparation,
                        Status.PendingAmendment,
                        Status.Submitted,
                        Status.ApprovalInProgress,
                        Status.Approved)
                .Allow(
                    TaskType.UpdateContracts)
                    .When(Status.Approved);

            ConfigureTasks(Actor.Underwriter)
                .Allow(
                    TaskType.UpdateApplicationMainInfo)
                    .When(Status.Preparation,
                        Status.Approved)
                .Allow(
                    TaskType.UpdateCredit,
                    TaskType.UpdateApplicationDetails,
                    TaskType.UpdateCustomer,
                    TaskType.UpdateSpouse,
                    TaskType.UpdateFileRequest,
                    TaskType.UpdateFiles)
                    .When(Status.Preparation,
                        Status.PendingAmendment,
                        Status.Submitted,
                        Status.ApprovalInProgress)
                .Allow(TaskType.UpdateComments)
                    .When(Status.Preparation,
                        Status.PendingAmendment,
                        Status.Submitted,
                        Status.ApprovalInProgress,
                        Status.Approved,
                        Status.Signed)
                .Allow(TaskType.UpdateContracts)
                    .When(Status.Approved,
                        Status.ApprovalInProgress);
        }
        #endregion
        #region Processes and work
        private void DoCreditCheck()
        {
            if (_rules.CreditCheckPending())
                _cdpService
                    .ProcessCDPApplication(_application, 1);
        }
        private void DoBudgetCheck()
        {
            if (_rules.BudgetCheckPending())
                _cdpService
                    .ProcessCDPApplication(_application, 2);
        }
        private void SetSignedDate(DateTime signedDate, StateMachine<Status, Activity>.Transition transition)
        {
            _application
                .SignatureDate = signedDate;
        }
        private void CreateNewApplication(Application applicationCopy)
        {
            _applicationService
                .Add(applicationCopy);
        }
        private void DoPrepareContracts()
        {
            if (!_rules.HasPreparedContracts())
                _cicOneService
                    .ProcessCICOneApplication(_application, 1);
        }
        private void ActivateContract()
        {
            _cicOneService
                .ProcessCICOneApplication(_application, 2);
        }
        #endregion
        #region Execution
        public void CreditCheck()
        {
            // Backward compatibility : 
            // Redirect because it is not implemented in upper layers 
            // CreditCheck() is used for submitting amendments
            if (CurrentStatus == ApplicationStatus.PendingAmendment)
                SubmitAmendments();

            FireIf(Status.Preparation, Activity.CheckCredit)
                .ThenFire(Activity.Submit);
        }
        public void RequestAmendments()
        {
            Fire(Activity.RequestAmendments);
        }
        public void SubmitAmendments()
        {
            FireIf(Status.PendingAmendment, Activity.SubmitAmendments)
                .ThenFire(Activity.Submit);
        }
        public void Approve()
        {
            Fire(Activity.Approve);
        }
        public void BudgetCheck()
        {
            FireIf(Status.Submitted, Activity.CheckBudget)
                .ThenFire(Activity.StartApproval);
        }
        public void Withdraw()
        {
            Fire(Activity.Withdraw);
        }
        public void Refuse()
        {
            Fire(Activity.Refuse);
        }
        public void Expire()
        {
            Fire(Activity.Expire);
        }
        public void StartDisbursement()
        {
            Fire(Activity.StartDisbursement);
        }
        public void ConfirmDisbursement()
        {
            Fire(Activity.ConfirmDisbursement);
        }
        public void ReopenForApproval()
        {
            Fire(Activity.ReopenForApproval);
        }
        public void Sign(DateTime signedDate)
        {
            Fire(Activity.Sign, signedDate);
        }
        public Application Replace()
        {
            var copyComment = $"This is automatic copy of replaced application with order number \"{_application.OrderNo}.\"";

            var applicationCopy = _applicationService
                .Copy(_application, copyComment);

            Fire(Activity.Replace, applicationCopy);

            return
                applicationCopy;
        }
        public bool PrepareContracts()
        {
            Fire(Activity.PrepareContracts);

            return _rules
                .HasPreparedContracts();
        }
        #endregion
    }
}