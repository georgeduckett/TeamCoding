using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TeamCoding.Extensions
{
    public static class WaitHandleExtensions
    { // http://stackoverflow.com/a/18766131
        public static Task<bool> AsTask(this WaitHandle handle) => AsTask(handle, Timeout.InfiniteTimeSpan);
        public static Task<bool> AsTask(this WaitHandle handle, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<bool>();
            var registration = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) =>
            {
                var localTcs = (TaskCompletionSource<bool>)state;
                localTcs.TrySetResult(!timedOut);
            }, tcs, timeout, executeOnlyOnce: true);
            tcs.Task.ContinueWith((_, state) => ((RegisteredWaitHandle)state).Unregister(null), registration, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}
