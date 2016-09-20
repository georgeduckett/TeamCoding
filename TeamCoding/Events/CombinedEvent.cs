using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Events
{
    public class CombinedEvent
    {
        public event EventHandler Event;
        public CombinedEvent(params EventHandler[] events)
        {
            for(int i=0;i<events.Length;i++)
            {
                events[i] += Event.Invoke;
            }
        }
    }
    public class CombinedEvent<TEventArgs>
    {
        public CombinedEvent(EventHandler event1, EventHandler<TEventArgs> event2)
        {

        }
    }
}
