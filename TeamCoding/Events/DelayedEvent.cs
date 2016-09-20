using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TeamCoding.Events
{
    public class DelayedEvent
    {
        private readonly Timer Timer;
        private object LatestSender;
        private readonly object EventArgsLock = new object();
        public event EventHandler Event;
        public event EventHandler PassthroughEvent;
        public DelayedEvent(int millisecondsDelay)
        {
            Timer = new Timer(millisecondsDelay) { Enabled = false, AutoReset = false };
            Timer.Elapsed += Timer_Elapsed;
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (EventArgsLock)
            {
                Event?.Invoke(LatestSender, EventArgs.Empty);
            };
        }
        public void Invoke(object sender, EventArgs e)
        {
            PassthroughEvent?.Invoke(sender, e);
            Timer.Stop();
            lock (EventArgsLock)
            {
                LatestSender = sender;
            }
            Timer.Start();
        }
    }
    public class DelayedEvent<TEventArgs> where TEventArgs : EventArgs
    {
        private readonly Timer Timer;
        private TEventArgs LatestEventArgs;
        private object LatestSender;
        private readonly object EventArgsLock = new object();
        public event EventHandler<TEventArgs> Event;
        public event EventHandler<TEventArgs> PassthroughEvent;
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
        public void Invoke(object sender, TEventArgs e)
        {
            PassthroughEvent?.Invoke(sender, e);
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
