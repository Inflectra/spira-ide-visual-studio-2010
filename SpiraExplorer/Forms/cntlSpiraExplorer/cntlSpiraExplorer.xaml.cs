using System;
using System.Collections.Generic;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>Interaction logic for cntlSpiraExplorer.xaml</summary>
	public partial class cntlSpiraExplorer : UserControl
	{
		#region Internal Vars
		string _solutionName = null;
		private TreeViewItem _nodeNoSolution = null;
		private TreeViewItem _nodeNoProjects = null;
		List<TreeViewArtifact> _treeNodeList;
		EnvDTE.Events _EnvironEvents = null;
		private ResourceManager _resources = null;
		#endregion
		#region Public Events
		public event EventHandler<OpenItemEventArgs> OpenDetails;
		#endregion

		/// <summary>Creates a new instance of the control.</summary>
		public cntlSpiraExplorer()
		{
			try
			{
				//Get the resources first..
				this._resources = Business.StaticFuncs.getCultureResource;

				//Overall initialization.
				InitializeComponent();

				//Set button images and events.
				// - Config button
				Image btnConfigImage = Business.StaticFuncs.getImage("imgSettings", new Size(16, 16));
				btnConfigImage.Stretch = Stretch.None;
				this.btnConfig.Content = btnConfigImage;
				this.btnConfig.Click += new RoutedEventHandler(btnConfig_Click);
				// - Show Completed button
				Image btnCompleteImage = Business.StaticFuncs.getImage("imgShowCompleted", new Size(16, 16));
				btnCompleteImage.Stretch = Stretch.None;
				this.btnShowClosed.Content = btnCompleteImage;
				this.btnShowClosed.IsEnabledChanged += new DependencyPropertyChangedEventHandler(toolButton_IsEnabledChanged);
				this.btnShowClosed.Click += new RoutedEventHandler(btnRefresh_Click);
				// - Refresh Button
				Image btnRefreshImage = Business.StaticFuncs.getImage("imgRefresh", new Size(16, 16));
				btnRefreshImage.Stretch = Stretch.None;
				this.btnRefresh.Content = btnRefreshImage;
				this.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
				this.btnRefresh.IsEnabledChanged += new DependencyPropertyChangedEventHandler(toolButton_IsEnabledChanged);
				// - Set bar color.
				this.barLoading.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(this._resources.GetString("barForeColor"));

				//Load nodes.
				this.CreateStandardNodes();

				//Attach to the Environment events and assign events.
				this._EnvironEvents = Business.StaticFuncs.GetEnvironment.Events;
				this._EnvironEvents.SolutionEvents.Opened += new EnvDTE._dispSolutionEvents_OpenedEventHandler(SolutionEvents_Opened);
				this._EnvironEvents.SolutionEvents.AfterClosing += new EnvDTE._dispSolutionEvents_AfterClosingEventHandler(SolutionEvents_AfterClosing);
				this._EnvironEvents.SolutionEvents.Renamed += new EnvDTE._dispSolutionEvents_RenamedEventHandler(SolutionEvents_Renamed);

				//If a solution is loaded now, get the loaded solution.
				if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
					this.setSolution((string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value);
				else
					this.setSolution(null);
			}
			catch (Exception ex)
			{
				//TODO: Log error.
				throw new Exception("Could not attach to IDE.", ex);
			}
		}

		#region Environment Events
		/// <summary>Hit when a solution is renamed.</summary>
		/// <param name="OldName">The old name of the solution.</param>
		private void SolutionEvents_Renamed(string OldName)
		{
			//Get the new name of the solution..
			string NewName = (string)((EnvDTE80.DTE2)Package.GetGlobalService(typeof(SDTE))).Solution.Properties.Item("Name").Value;
			if (!string.IsNullOrWhiteSpace(NewName))
			{				//Modify the settings to transfer over projects.
				if (Settings.Default.AssignedProjects.ContainsKey(OldName))
				{
					string strAssignedProjects = Settings.Default.AssignedProjects[OldName];
					Settings.Default.AssignedProjects.Remove(OldName);
					Settings.Default.AssignedProjects.Add(NewName, strAssignedProjects);
					Settings.Default.Save();
				}

				//Reload projects..
				this.setSolution(NewName);
			}
		}

		/// <summary>Hit when the loaded solution is closed.</summary>
		private void SolutionEvents_AfterClosing()
		{
			//Set to no solution loaded.
			this.noSolutionLoaded();
		}

		/// <summary>Hit when a new solution is opened.</summary>
		private void SolutionEvents_Opened()
		{
			if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
			{
				string solName = (string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value;
				this.setSolution(solName);
			}
		}
		#endregion

		#region Control Events
		/// <summary>Hit when the user double-clicks on a tree node.</summary>
		/// <param name="sender">treeView</param>
		/// <param name="evt">EventArgs</param>
		private void tree_NodeDoubleClick(object sender, EventArgs evt)
		{
			try
			{
				string itemTag = (string)((TreeViewItem)sender).Tag;

				this.OpenDetails(this, new OpenItemEventArgs(itemTag));
			}
			catch (Exception ex)
			{
				//TODO: Log error here.
			}
		}

		/// <summary>Hit when the user wants to refresh the list.</summary>
		/// <param name="sender">btnRefresh, btnShowClosed</param>
		/// <param name="e">Event Args</param>
		private void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.setSolution(this._solutionName);
			}
			catch (Exception ex)
			{
				//TODO: Error logging here.
			}
		}

		/// <summary>Hit when a toolbar button IsEnabled is changed, for greying out icons.</summary>
		/// <param name="sender">toolButton</param>
		/// <param name="e">DependencyPropertyChangedEventArgs</param>
		private void toolButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				if (sender.GetType() == typeof(Button))
				{
					Button btn = (Button)sender;

					btn.Opacity = ((btn.IsEnabled) ? 1 : .5);
				}
				else if (sender.GetType() == typeof(System.Windows.Controls.Primitives.ToggleButton))
				{
					System.Windows.Controls.Primitives.ToggleButton btn = (System.Windows.Controls.Primitives.ToggleButton)sender;

					btn.Opacity = ((btn.IsEnabled) ? 1 : .5);
				}
			}
			catch (Exception ex)
			{
				//TODO: Error logging.
			}
		}

		/// <summary>Hit when the user clicks on the Configuration button/</summary>
		/// <param name="sender">btnConfig</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnConfig_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				frmAssignProject configProj = new frmAssignProject();

				if (configProj.ShowDialog().Value)
				{
					//Reload projects.
					this.setSolution(this._solutionName);
				}
			}
			catch (Exception ex)
			{
				//TODO: Error logging.
			}
			e.Handled = true;
		}
		#endregion

		/// <summary>Tells the control that a new solution was loaded.</summary>
		/// <param name="solName">The current Solution name.</param>
		private void setSolution(string solName)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(solName))
				{
					this.noSolutionLoaded();
					this.barLoading.Visibility = Visibility.Collapsed;
				}
				else
				{
					//Only get the projects if the solution name changed. (Avoid refreshing when solution name is unchanged.)
					if (this._solutionName != solName)
					{
						//Get projects associated with this solution.
						SerializableDictionary<string, string> availProjects = Settings.Default.AssignedProjects;
						if (availProjects != null && availProjects.ContainsKey(solName) && !string.IsNullOrWhiteSpace(availProjects[solName]))
							this.loadProjects(availProjects[solName]);
						else
						{
							this.noProjectsLoaded();
							this.barLoading.Visibility = Visibility.Collapsed;
						}
						this._solutionName = solName;
					}
				}
			}
			catch (Exception ex)
			{
				//TODO: Log error.
			}
		}

		#region Tree Node Methods
		/// <summary>Creates a node display header.</summary>
		/// <param name="imageKey">The image key to use. null skips image.</param>
		/// <param name="Label">The text to use.</param>
		/// <returns>A StackPanel that can be saved to the node's Header property.</returns>
		private StackPanel createNodeHeader(string imageKey, string Label)
		{
			try
			{
				StackPanel stckPnl = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 2, 2) };
				if (!string.IsNullOrEmpty(imageKey))
				{
					stckPnl.Children.Add(Business.StaticFuncs.getImage(imageKey, new Size(16, 16)));
				}
				stckPnl.Children.Add(new TextBlock() { Text = Label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(3, 0, 0, 0) });

				return stckPnl;
			}
			catch (Exception ex)
			{
				//TODO: Log error.
				return new StackPanel();
			}
		}

		/// <summary>Creates the standard nodes. Run at class creation.</summary>
		private void CreateStandardNodes()
		{
			try
			{
				//Define our standard nodes here.
				// - No Projects
				this._nodeNoProjects = new TreeViewItem();
				this._nodeNoProjects.Header = createNodeHeader("imgNo", "No projects selected for this solution.");

				// - No Solution
				this._nodeNoSolution = new TreeViewItem();
				this._nodeNoSolution.Header = createNodeHeader("imgNo", "No solution loaded.");
			}
			catch (Exception ex)
			{
				//TODO: Log error.
			}
		}

		#endregion

		#region Helper Classes
		/// <summary>Class for Opening the details of a new item.</summary>
		public class OpenItemEventArgs : EventArgs
		{
			public string ItemTag;

			public OpenItemEventArgs(string itemTag)
			{
				this.ItemTag = itemTag;
			}
		}
		#endregion
	}
}
