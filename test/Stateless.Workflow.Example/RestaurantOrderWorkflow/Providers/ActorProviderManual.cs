using Stateless.Workflow;
using Stateless.Workflow.Example;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Stateless.Workflow.Example
{
    public class ActorProviderManual : IActorProvider<Actor>
    {
        public IList<Actor> _impersonationList;
        public ActorProviderManual()
        {
            _impersonationList = new List<Actor>();
        }
        public Actor[] GetCurrentImpersonations()
        {
            if (!_impersonationList.Any())
                throw new InvalidOperationException("No Roles are set, please call SetCurrentRoleMethod(Role) to set current role.");

            return
                _impersonationList
                    .ToArray();
        }

        public void Set(Actor actor)
        {
            Reset();
            _impersonationList.Add(actor);
        }

        public void Reset()
        {
            _impersonationList.Clear();
        }
    }
}