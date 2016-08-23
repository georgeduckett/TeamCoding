using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio
{
    public class SolutionEventsHandler : IVsSolutionEvents
    {
        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            if (fNewSolution == 0)
            {
                TeamCodingPackage.Current.Settings.LoadFromJsonFile();
            }

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
        public int OnAfterBackgroundSolutionLoadComplete() => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch) => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnBeforeBackgroundSolutionLoadBegins() => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch) => Microsoft.VisualStudio.VSConstants.S_OK;
        public void OnBeforeOpenProject(ref Guid guidProjectID, ref Guid guidProjectType, string pszFileName) { }
        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnBeforeCloseSolution(object pUnkReserved) => Microsoft.VisualStudio.VSConstants.S_OK;
        public int OnAfterCloseSolution(object pUnkReserved) => Microsoft.VisualStudio.VSConstants.S_OK;
    }
}
