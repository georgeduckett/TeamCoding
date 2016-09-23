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
            serviceInstaller.AfterInstall += ServiceInstaller_AfterInstall;
            
            processInstaller.Account = ServiceAccount.LocalService;

            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.Description = "";
            
            serviceInstaller.DisplayName = TeamCodingSyncServer.SyncServerServiceName;
            serviceInstaller.ServiceName = TeamCodingSyncServer.SyncServerServiceName;
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }

        private void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController(TeamCodingSyncServer.SyncServerServiceName))
            {
                sc.Start(); // TODO: Test install process (does it think it's running as a service when ran like this or not?)
            }
        }
    }
}
