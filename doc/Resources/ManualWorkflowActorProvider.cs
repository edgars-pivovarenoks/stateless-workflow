using Stateless.Intrum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity
{
    public class ManualWorkflowActorProvider : Stateless.Intrum.IActorProvider<Actor>
    {
        public IList<Actor> _impersonationList;
        public ManualWorkflowActorProvider()
        {
            _impersonationList = new List<Actor>();
        }
        public Actor[] GetCurrentImpersonations()
        {
            if(!_impersonationList.Any())
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
