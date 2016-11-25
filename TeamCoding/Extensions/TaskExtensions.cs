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
        public static async Task<TResult> HandleExceptionAsync<TResult>(this Task<TResult> task, Action<Exception> handleException)
        {
            TResult result = default(TResult);
            try
            {
                result = await task;
            }
            catch(Exception ex)
            {
                handleException(ex);
            }
            return result;
        }
        public static Task<TResult> HandleException<TResult>(this Task<TResult> task)
        {
            task.ContinueWith(WriteTaskException, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
        /// <summary>
        /// Handles any exception caused by the task
        /// </summary>
        /// <param name="task">The task to handle exceptions for</param>
        /// <param name="handleException">What to do if there is an exception</param>
        /// <returns>A task that upon completion will have waited for the task, and handled any exception</returns>
        public static async Task HandleExceptionAsync(this Task task, Action<Exception> handleException)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                handleException(ex);
            }
        }
        public static Task HandleException(this Task task)
        {
            task.ContinueWith(WriteTaskException, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
    }
}
