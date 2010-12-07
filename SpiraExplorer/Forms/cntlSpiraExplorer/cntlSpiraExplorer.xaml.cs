﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Properties;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>Interaction logic for cntlSpiraExplorer.xaml</summary>
	public partial class cntlSpiraExplorer : UserControl
	{
		#region Internal Vars
		string _solutionName = null;
		private TreeViewItem _nodeNoSolution = null;
		private TreeViewItem _nodeNoProjects = null;
		private List<TreeViewArtifact> _treeNodeList;
		#endregion
		#region Public Events
		public event EventHandler<OpenItemEventArgs> OpenDetails;
		#endregion

		/// <summary>Creates a new instance of the control.</summary>
		public cntlSpiraExplorer()
		{
			try
			{
				//Overall initialization.
				InitializeComponent();

				//Set button images and events.
				// - Config button
				Image btnConfigImage = Business.StaticFuncs.getImage("imgProject", new Size(16, 16));
				btnConfigImage.Stretch = Stretch.None;
				this.btnConfig.Content = btnConfigImage;
				// - Show Completed button
				Image btnCompleteImage = Business.StaticFuncs.getImage("imgShowCompleted", new Size(16, 16));
				btnCompleteImage.Stretch = Stretch.None;
				this.btnShowClosed.Content = btnCompleteImage;
				this.btnShowClosed.IsChecked = Settings.Default.ShowCompleted;
				// - Refresh Button
				Image btnRefreshImage = Business.StaticFuncs.getImage("imgRefresh", new Size(16, 16));
				btnRefreshImage.Stretch = Stretch.None;
				this.btnRefresh.Content = btnRefreshImage;
				// - Set bar color.
				this.barLoading.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(StaticFuncs.getCultureResource.GetString("app_Colors_StyledBarColor"));

				//Load nodes.
				this.CreateStandardNodes();

				//If a solution is loaded now, get the loaded solution.
				if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
					this.loadSolution((string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value);
				else
					this.loadSolution(null);
			}
			catch (Exception ex)
			{
				//TODO: Log error.
				throw ex;
			}
		}

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

		/// <summary>Hit when the selected item changes in the treeview.</summary>
		/// <param name="sender">trvProject</param>
		/// <param name="e">RoutedPropertyChangedEventArgs</param>
		private void trvProject_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			e.Handled = true;

			//If it's a TreeViewArtifact item.
			if (this.trvProject.SelectedItem != null && this.trvProject.SelectedItem.GetType() == typeof(TreeViewArtifact))
			{
				//Only if it's NOT not a folder.
				TreeViewArtifact selItem = this.trvProject.SelectedItem as TreeViewArtifact;
				this.btnRefresh.IsEnabled = (selItem != null && selItem.ArtifactIsFolder);
			}
			else
				this.btnRefresh.IsEnabled = false;
		}

		/// <summary>Hit when the user wants to refresh the list.</summary>
		/// <param name="sender">btnRefresh, btnShowClosed</param>
		/// <param name="e">Event Args</param>
		private void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				TreeViewArtifact selItem = this.trvProject.SelectedItem as TreeViewArtifact;
				if (selItem != null) this.refreshTreeNodeServerData(selItem);
			}
			catch (Exception ex)
			{
				//TODO: Error logging here.
			}
		}

		/// <summary>Hit when the user wants to show/not show closed items.</summary>
		/// <param name="sender">TobbleButton</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnShowClosed_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			//We need to save the setting here.
			Settings.Default.ShowCompleted = this.btnShowClosed.IsChecked.Value;
			Settings.Default.Save();

			//Refresh the item list.
			this.refreshProjects();
		}

		/// <summary>Hit when a toolbar button IsEnabled is changed, for greying out icons.</summary>
		/// <param name="sender">toolButton</param>
		/// <param name="e">DependencyPropertyChangedEventArgs</param>
		private void toolButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				UIElement btnChanged = sender as UIElement;
				if (btnChanged != null)
					btnChanged.Opacity = ((btnChanged.IsEnabled) ? 1 : .5);
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
					//If a solution is loaded now, get the loaded solution.
					if (Business.StaticFuncs.GetEnvironment.Solution.IsOpen)
						this.loadSolution((string)Business.StaticFuncs.GetEnvironment.Solution.Properties.Item("Name").Value);
					else
						this.loadSolution(null);
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
		public void loadSolution(string solName)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(solName))
				{
					this.noSolutionLoaded();
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
