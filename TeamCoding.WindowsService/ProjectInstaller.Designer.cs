namespace TeamCoding.WindowsService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TeamCodingServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.TeamCodingServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // TeamCodingServiceProcessInstaller
            // 
            this.TeamCodingServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalService;
            this.TeamCodingServiceProcessInstaller.Password = null;
            this.TeamCodingServiceProcessInstaller.Username = null;
            // 
            // TeamCodingServiceInstaller
            // 
            this.TeamCodingServiceInstaller.Description = "Server for syncing Team Coding Visual Studio Extensions users.";
            this.TeamCodingServiceInstaller.DisplayName = "TeamCoding Sync Server";
            this.TeamCodingServiceInstaller.ServiceName = "TeamCoding Sync";
            this.TeamCodingServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.TeamCodingServiceProcessInstaller,
            this.TeamCodingServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller TeamCodingServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller TeamCodingServiceInstaller;
    }
}