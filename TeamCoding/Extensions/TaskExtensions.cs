using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class TaskExtensions
    {
        private static void WriteTaskException(Task t) => TeamCodingPackage.Current.Logger.WriteError(t.Exception);
        public static Task<TResult> HandleException<TResult>(this Task<TResult> task)
        {
            task.ContinueWith(WriteTaskException, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
        public static Task HandleException(this Task task)
        {
            task.ContinueWith(WriteTaskException, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
    }
}
