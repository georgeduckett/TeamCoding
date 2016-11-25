using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters
{
    /// <summary>
    /// Handles sending local IDE model changes to other clients.
    /// </summary>
    public interface ILocalModelPerisister : IDisposable
    {
        Task SendUpdateAsync();
    }
}
