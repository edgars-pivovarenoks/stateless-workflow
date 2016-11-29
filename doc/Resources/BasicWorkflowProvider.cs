using AgentPortal.Model;
using Curiosity.Entities;

namespace Curiosity
{
    public class BasicWorkflowProvider : IWorkflowProvider // CuriosityAppServiceBase
    {
        // THIS SERVICE MUST BE EVOLVED WITH DI/IOC
        private readonly Stateless.Intrum.IActorProvider<Actor> _actorProvider;
        private readonly CDP.IApplicationProcessor _cdpService;
        private readonly CICOne.IApplicationProcessor _cicOneService;
        private readonly IApplicationService _applicationService;
        private readonly IRepository<ApplicationTransition> _applicationTransitionsRepository;
            
        public BasicWorkflowProvider(
                Stateless.Intrum.IActorProvider<Actor> actorProvider,
                CDP.IApplicationProcessor cdpService,
                CICOne.IApplicationProcessor cicOneService,
                IApplicationService applicationService, 
                IRepository<ApplicationTransition> applicationTransitionsRepository)
        {
            _actorProvider = actorProvider;
            _cdpService = cdpService;
            _cicOneService = cicOneService;
            _applicationService = applicationService;
            _applicationTransitionsRepository = applicationTransitionsRepository;
        }
        public ILoanApplicationWorkflow GetWorkflow(long id)
        {
            // var app = _loanApplicationRepository
            //        .GetById(id);

            // return 
            //      Get(app);

            return
                null;
        }
        public ILoanApplicationWorkflow GetWorkflow(Application app)
        {
            return
                new LoanApplicationWorklowV1(app, _actorProvider, _cdpService, _cicOneService, _applicationService, _applicationTransitionsRepository);
        }
    }
}
