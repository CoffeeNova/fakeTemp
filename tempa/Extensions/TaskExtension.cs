using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeJelly.tempa.Extensions
{
    public static class TaskExtension
    {
        public static async void CriticalTask(this Task task)
        {
            CriticalTasks.Add(task);
            await task;
            CriticalTasks.Remove(task);
        }
    }
}
