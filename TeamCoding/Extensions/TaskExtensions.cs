using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class TaskExtensions
    {
        private static void WriteTaskException(Task t)
        {
            if (t != null)
            {
                TeamCodingPackage.Current.Logger.WriteError(t.Exception);
            }
        }
        /// <summary>
        /// Handles any exception caused by the task, and returns default(<typeparamref name="TResult"/>)
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the task</typeparam>
        /// <param name="task">The task to handle exceptions for</param>
        /// <param name="handleException">What to do if there is an exception</param>
        /// <returns>The result of the task if there's no exception, otherwise default(<typeparamref name="TResult"/>)</returns>
        public static async Task<TResult> HandleException<TResult>(this Task<TResult> task, Action<Exception> handleException)
        {
            try
            {
                await task;
            }
            catch(Exception ex)
            {
                handleException(ex);
            }
            return task.IsFaulted ? default(TResult) : task.Result;
        }
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
