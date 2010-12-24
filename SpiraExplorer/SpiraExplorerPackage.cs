using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using EnvDTE;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	[PackageRegistration(UseManagedResourcesOnly = true)] // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // This attribute is used to register the information needed to show the this package in the Help/About dialog of Visual Studio.
	[ProvideMenuResource("Menus.ctmenu", 1)] // This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideToolWindow(typeof(toolSpiraExplorer), MultiInstances = false, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")] // This attribute registers a tool window exposed by this package.
	[ProvideToolWindow(typeof(toolSpiraExplorerDetails), MultiInstances = true, Window = "76C22C24-36B6-4C0C-BF60-FFCB65D1B05B")] // This attribute registers a tool window exposed by this package.
	[Guid(GuidList.guidSpiraExplorerPkgString)]
	public sealed class SpiraExplorerPackage : Package
	{
		private EnvDTE.Events _EnvironEvents;
		SolutionEvents _SolEvents;
		static Dictionary<TreeViewArtifact, int> _windowDetails;
		static int _numWindowIds = -1;

		/// <summary>Default constructor of the package. Inside this method you can place any initialization code that does not require any Visual Studio service because at this point the package object is created but not sited yet inside Visual Studio environment. The place to do all the other initialization is the Initialize method.</summary>
		public SpiraExplorerPackage()
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));

			//Get settings ready..
			if (Settings.Default.AssignedProjects == null)
			{
				Settings.Default.AssignedProjects = new Business.SerializableDictionary<string, string>();
				Settings.Default.Save();
			}
			if (Settings.Default.AllProjects == null)
			{
				Settings.Default.AllProjects = new Business.SerializableList<string>();
				Settings.Default.Save();
			}
		}

		/// <summary>This function is called when the user clicks the menu item that shows the tool window. See the Initialize method to see how the menu item is associated to this function using the OleMenuCommandService service and the MenuCommand class.</summary>
		private void ShowToolWindow(object sender, EventArgs e)
		{
			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			ToolWindowPane window = this.FindToolWindow(typeof(toolSpiraExplorer), 0, true);

			if ((null == window) || (null == window.Frame))
			{
				throw new NotSupportedException(Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Resources.CanNotCreateWindow);
			}
			IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
		}

		#region Package Members
		/// <summary>Initialization of the package; this method is called right after the package is sited, so this is the place where you can put all the initialization code that rely on services provided by VisualStudio.</summary>
		protected override void Initialize()
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
			base.Initialize();

			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (mcs != null)
			{
				// Create the command for the tool window
				CommandID toolwndCommandID = new CommandID(GuidList.guidSpiraExplorerCmdSet, (int)PkgCmdIDList.cmdViewExplorerWindow);
				MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
				mcs.AddCommand(menuToolWin);
			}

			//Attach to the environment to get events..
			this._EnvironEvents = Business.StaticFuncs.GetEnvironment.Events;
			this._SolEvents = Business.StaticFuncs.GetEnvironment.Events.SolutionEvents;
			if (this._EnvironEvents != null && this._SolEvents != null)
			{
				this._SolEvents.Opened += new EnvDTE._dispSolutionEvents_OpenedEventHandler(SolutionEvents_Opened);
				this._SolEvents.AfterClosing += new EnvDTE._dispSolutionEvents_AfterClosingEventHandler(SolutionEvents_AfterClosing);
				this._SolEvents.Renamed += new EnvDTE._dispSolutionEvents_RenamedEventHandler(SolutionEvents_Renamed);
			}
		}
		#endregion

		#region Environment Events
		/// <summary>Hit when an open solution is renamed.</summary>
		/// <param name="OldName">The old name of the solution.</param>
		private void SolutionEvents_Renamed(string OldName)
		{
			//Get the new name of the solution..
			if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
			{
				string NewName = (string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value;
				if (!string.IsNullOrWhiteSpace(NewName))
				{
					//Modify the settings to transfer over projects.
					if (Settings.Default.AssignedProjects.ContainsKey(OldName))
					{
						string strAssignedProjects = Settings.Default.AssignedProjects[OldName];
						Settings.Default.AssignedProjects.Remove(OldName);
						if (Settings.Default.AssignedProjects.ContainsKey(NewName))
							Settings.Default.AssignedProjects[NewName] = strAssignedProjects;
						else
							Settings.Default.AssignedProjects.Add(NewName, strAssignedProjects);
						Settings.Default.Save();
					}

					//Reload projects..
					ToolWindowPane window = this.FindToolWindow(typeof(toolSpiraExplorer), 0, false);
					if (window != null)
					{
						cntlSpiraExplorer toolWindow = (cntlSpiraExplorer)window.Content;
						toolWindow.loadSolution(NewName);
					}
				}
			}
		}

		/// <summary>Hit when the open solution is closed.</summary>
		private void SolutionEvents_AfterClosing()
		{
			//Get the window.
			ToolWindowPane window = this.FindToolWindow(typeof(toolSpiraExplorer), 0, false);
			if (window != null)
			{
				cntlSpiraExplorer toolWindow = (cntlSpiraExplorer)window.Content;
				toolWindow.loadSolution(null);
			}
		}

		/// <summary>Hit when a solution is opened.</summary>
		private void SolutionEvents_Opened()
		{
			if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
			{
				//Get the window.
				ToolWindowPane window = this.FindToolWindow(typeof(toolSpiraExplorer), 0, false);
				if (window != null)
				{
					cntlSpiraExplorer toolWindow = (cntlSpiraExplorer)window.Content;
					toolWindow.loadSolution((string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value);
				}
			}
		}
		#endregion

		public void OpenDetailsToolWindow(TreeViewArtifact Artifact)
		{
			if (SpiraExplorerPackage._windowDetails == null)
				SpiraExplorerPackage._windowDetails = new Dictionary<TreeViewArtifact, int>();

			//Get the window ID if it already exists.
			int NextId = -1;
			if (SpiraExplorerPackage._windowDetails.ContainsKey(Artifact)) //Get the ID if it exists.
				NextId = SpiraExplorerPackage._windowDetails[Artifact];
			else //Figure out the next ID.
			{
				SpiraExplorerPackage._numWindowIds++;
				NextId = SpiraExplorerPackage._numWindowIds;
				SpiraExplorerPackage._windowDetails.Add(Artifact, SpiraExplorerPackage._numWindowIds);
			}

			//Now generate the window..
			//IVsUIShell vsUIShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
			//IVsWindowFrame windowFrame = null;
			//Guid guidToolWindow2 = typeof(toolSpiraExplorerDetails).GUID;
			//vsUIShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref guidToolWindow2, out windowFrame);

			toolSpiraExplorerDetails window = this.FindToolWindow(typeof(toolSpiraExplorerDetails), NextId, true) as toolSpiraExplorerDetails;

			//Generate the details screen.
			object detailContent = null;
			switch (Artifact.ArtifactType)
			{
				case TreeViewArtifact.ArtifactTypeEnum.Incident:
					frmDetailsIncident detIncident = new frmDetailsIncident();
					detIncident.ArtifactDetail = Artifact;
					detIncident.ParentWindowPane = window;
					detailContent = detIncident;
					break;

				case TreeViewArtifact.ArtifactTypeEnum.Requirement:
					//TODO: Create requirement detail screen.
					break;

				case TreeViewArtifact.ArtifactTypeEnum.Task:
					//TODO: Create task detail screen.
					break;
			}

			//Set toolwindow's content.
			if (detailContent != null)
			{
				((cntrlDetailsForm)window.FormControl).Content = detailContent;

				//Get the frame. Needed because we're on a different thread.
				IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
				Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
			}

			//MessageBox.Show("Tried to open " + Artifact.ArtifactType.ToString() + " #" + Artifact.ArtifactId.ToString());
		}
	}
}
