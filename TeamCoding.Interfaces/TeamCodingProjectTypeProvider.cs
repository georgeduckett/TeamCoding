using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding
{
    /// <summary>
    /// Provides singleton instances of concrete types from the TeamCoding project implementing specified interfances
    /// </summary>
    public static class TeamCodingProjectTypeProvider
    {
        private static readonly Dictionary<Type, object> CachedObjectInstances = new Dictionary<Type, object>();
        public static T Get<T>()
        {
            object o;
            if(CachedObjectInstances.TryGetValue(typeof(T), out o))
            {
                return (T)o;
            }
            
            o = Activator.CreateInstance(AppDomain.CurrentDomain.GetAssemblies()
                                                                .Single(a => a.GetName().Name == "TeamCoding")
                                                                .ExportedTypes.Single(t => typeof(T).IsAssignableFrom(t)));

            CachedObjectInstances.Add(typeof(T), o);

            return (T)o;
        }
    }
}
