using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.WindowsService
{
    [RunInstaller(true)]
    public class TeamCodingSyncServerInstaller : Installer
    {
        public TeamCodingSyncServerInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();
            
            processInstaller.Account = ServiceAccount.NetworkService;

            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.Description = "TeamCoding Sync is the optional companion service to the TeamCoding Visual Studio extension.";
            
            serviceInstaller.DisplayName = TeamCodingSyncServer.SyncServerServiceName;
            serviceInstaller.ServiceName = TeamCodingSyncServer.SyncServerServiceName;
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
