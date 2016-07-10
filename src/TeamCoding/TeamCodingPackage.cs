//------------------------------------------------------------------------------
// <copyright file="TeamCodingPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using TeamCoding.Extensions;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using EnvDTE;
using TeamCoding.VisualStudio;
using TeamCoding.SourceControl;
using System.Windows.Interop;
using EnvDTE80;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI.Shell.Controls;
using System.Collections.Generic;

namespace TeamCoding
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(TeamCodingPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists)]
    public sealed class TeamCodingPackage : Package
    {
        /// <summary>
        /// TeamCodingPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "ac66efb2-fad5-442d-87e2-b9b4a206f14d";

        public static TeamCodingPackage Current { get; private set; }

        public readonly LocalIDEModel IdeModel = new LocalIDEModel();
        internal ModelChangeManager IdeChangeManager { get; private set; }
        public readonly MachineIdentityProvider IdentityProvider = new MachineIdentityProvider();
        public readonly ExternalModelManager RemoteModelManager = new ExternalModelManager();

        // TODO: Update and cache this when opening/closing a solution
        public DocumentTabPanel DocTabPanel => (DocumentTabPanel)GetWpfMainWindow((DTE)GetService(typeof(DTE))).FindChild("PART_TabPanel");

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCodingPackage"/> class.
        /// </summary>
        public TeamCodingPackage()
        {
            Current = this;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            
            IdeChangeManager = new ModelChangeManager(IdeModel);

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (!Zombied)
                {
                    System.Threading.Thread.Sleep(10000);
                    RemoteModelManager.SyncChanges();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            RemoteModelManager.SyncChanges();
            base.Dispose(disposing);
        }
        private System.Windows.Media.Visual GetWpfMainWindow(DTE dte)
        {
            if (dte == null)
            {
                throw new ArgumentNullException("dte");
            }

            var hwndMainWindow = (IntPtr)dte.MainWindow.HWnd;
            if (hwndMainWindow == IntPtr.Zero)
            {
                throw new NullReferenceException("DTE.MainWindow.HWnd is null.");
            }

            var hwndSource = HwndSource.FromHwnd(hwndMainWindow);
            if (hwndSource == null)
            {
                throw new NullReferenceException("HwndSource for DTE.MainWindow is null.");
            }

            return hwndSource.RootVisual;
        }
    }
}
