using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Workflow.Example
{
    public class TasksProviderAll : ITasksProvider<TaskType>
    {
        public ICollection<TaskType> GetCompletedTasks()
        {
            return Enum.GetValues(typeof(TaskType))
                .Cast<TaskType>()
                .ToList();
        }
    }
}
