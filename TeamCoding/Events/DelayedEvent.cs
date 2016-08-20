using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TeamCoding.Events
{
    public class DelayedEvent<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly Timer Timer;
        private TEventArgs LatestEventArgs;
        private object LatestSender;
        private readonly object EventArgsLock = new object();
        public event EventHandler<TEventArgs> Event;
        public DelayedEvent(int millisecondsDelay)
        {
            Timer = new Timer(millisecondsDelay) { Enabled = false, AutoReset = false };
            Timer.Elapsed += Timer_Elapsed;
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (EventArgsLock)
            {
                Event?.Invoke(LatestSender, LatestEventArgs);
            };
        }
        public void EventHandler(object sender, TEventArgs e)
        {
            Timer.Stop();
            lock (EventArgsLock)
            {
                LatestSender = sender;
                LatestEventArgs = e;
            }
            Timer.Start();
        }
    }
}
