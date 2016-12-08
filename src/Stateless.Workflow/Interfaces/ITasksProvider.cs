using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stateless.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITasksProvider<TTask>
    {
        ICollection<TTask> GetCompletedTasks();
    }
}
