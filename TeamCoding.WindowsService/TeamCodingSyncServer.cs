using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.WindowsService
{
    public partial class TeamCodingSyncServer : ServiceBase
    {
        public const string SyncServerServiceName = "TeamCoding Sync";
        private const int DefaultPort = 23023;
        private Multicaster Multicaster;
        private int Port;
        public TeamCodingSyncServer()
        {
            ServiceName = SyncServerServiceName;
            InitializeComponent();
        }
        
        protected override void OnStart(string[] args)
        {
            CanPauseAndContinue = CanShutdown = CanHandlePowerEvent = CanStop = true;

            if (args.Length == 0 || !int.TryParse(args[0], out Port))
            {
                // TODO: Also read from a config file
                Port = DefaultPort;
            }

            CreateMulticaster();
        }
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.Suspend:
                case PowerBroadcastStatus.QuerySuspend: DisposeMulticaster();break;
                case PowerBroadcastStatus.QuerySuspendFailed:
                case PowerBroadcastStatus.ResumeAutomatic:
                case PowerBroadcastStatus.ResumeCritical:
                case PowerBroadcastStatus.ResumeSuspend: CreateMulticaster();break;
            }

            return true;
        }
        protected override void OnShutdown()
        {
            DisposeMulticaster();
        }
        protected override void OnContinue()
        {
            CreateMulticaster();
        }
        protected override void OnPause()
        {
            DisposeMulticaster();
        }
        protected override void OnStop()
        {
            DisposeMulticaster();
        }
        private void CreateMulticaster()
        {
            if (Multicaster == null)
            {
                Multicaster = new Multicaster(Port);
            }
        }
        private void DisposeMulticaster()
        {
            if (Multicaster != null)
            {
                Multicaster.Dispose();
                Multicaster = null;
            }
        }
    }
}
