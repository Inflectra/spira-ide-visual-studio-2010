﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.SpiraTeam_Client;
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
		private TreeViewItem _nodeError = null;
		#endregion
		#region Public Events
		public event EventHandler<OpenItemEventArgs> OpenDetails;
		#endregion

		/// <summary>Creates a new instance of the control.</summary>
		public cntlSpiraExplorer()
		{
			try
			{
				InitializeComponent();

				//Set button images and events.
				// - Config button
				Image btnConfigImage = getImage("imgSettings", new Size(16, 16));
				btnConfigImage.Stretch = Stretch.None;
				this.btnConfig.Content = btnConfigImage;
				this.btnConfig.Click += new RoutedEventHandler(btnConfig_Click);
				// - Show Completed button
				Image btnCompleteImage = getImage("imgShowCompleted", new Size(16, 16));
				btnCompleteImage.Stretch = Stretch.None;
				this.btnShowClosed.Content = btnCompleteImage;
				this.btnShowClosed.IsEnabledChanged += new DependencyPropertyChangedEventHandler(toolButton_IsEnabledChanged);
				this.btnShowClosed.Click += new RoutedEventHandler(btnRefresh_Click);
				// - Refresh Button
				Image btnRefreshImage = getImage("imgRefresh", new Size(16, 16));
				btnRefreshImage.Stretch = Stretch.None;
				this.btnRefresh.Content = btnRefreshImage;
				this.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
				this.btnRefresh.IsEnabledChanged += new DependencyPropertyChangedEventHandler(toolButton_IsEnabledChanged);
				// - Set bar color.
				this.barLoading.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(this._resources.GetString("barForeColor"));

				//Load nodes.
				this.CreateStandardNodes();
			}
			catch (Exception ex)
			{
				//TODO: Log error here.
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
				wpfAssignProject configWPF = new wpfAssignProject();

				//Set the settings and solution name.
				configWPF.setSettings(this._settingsFile);
				configWPF.setSolution(this._solutionName);

				if (configWPF.ShowDialog().Value)
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

		/// <summary>Converts a resource to a WPF image. Needed for application resources.</summary>
		/// <param name="image">Bitmap of the image to convert.</param>
		/// <returns>BitmapSource suitable for an Image control.</returns>
		private BitmapSource getBMSource(System.Drawing.Bitmap image)
		{
			try
			{
				if (image != null)
				{
					IntPtr bmStream = image.GetHbitmap();
					return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmStream, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
				}
				return null;
			}
			catch (Exception ex)
			{
				//TODO: Log error here.
				return null;
			}
		}

		/// <summary>Creates an Image control for a specified resource.</summary>
		/// <param name="Key">The key name of the resource to use. Will search and use Product-dependent resources first.</param>
		/// <param name="Size">Size of the desired image, or null.</param>
		/// <param name="Stretch">Desired stretch setting of image, or null.</param>
		/// <returns>Resulting image, or null if key is not found.</returns>
		private Image getImage(string key, Size size)
		{
			try
			{
				Image retImage = new Image();
				if (size != null)
				{
					retImage.Height = size.Height;
					retImage.Width = size.Width;
				}

				BitmapSource image = null;
				image = getBMSource((System.Drawing.Bitmap)this._resources.GetObject(key));

				retImage.Source = image;

				return retImage;
			}
			catch (Exception ex)
			{
				//TODO: Log error here.
				return null;
			}
		}

		/// <summary>Generates a node header containing the text and the image type.</summary>
		/// <param name="HeaderText">The text for the header.</param>
		/// <param name="LeadImage">The icon for the header.</param>
		/// <returns>A UI Element possible to set as a header.</returns>
		private UIElement getNodeHeader(string HeaderText, ItemTypeEnumeration LeadImage)
		{
			try
			{
				StackPanel retHead = new StackPanel();
				retHead.Orientation = Orientation.Horizontal;

				//Get the image.
				Image headImage = new Image();
				switch (LeadImage)
				{
					case ItemTypeEnumeration.Incident:
						headImage = this.getImage("icoIncident", new Size(16, 16));
						break;
					case ItemTypeEnumeration.Requirement:
						headImage = this.getImage("icoRequirement", new Size(16, 16));
						break;
					case ItemTypeEnumeration.Task:
						headImage = this.getImage("icoTask", new Size(16, 16));
						break;
					default:
						headImage = this.getImage("icoError", new Size(16, 16));
						break;
				}
				TextBlock headText = new TextBlock();
				headText.Text = HeaderText;

				if (LeadImage != ItemTypeEnumeration.None)
					retHead.Children.Add(headImage);
				retHead.Children.Add(headText);

				return retHead;
			}
			catch (Exception ex)
			{
				//TODO: Error logging.
				return null;
			}
		}

		/// <summary>Tells the control that a new solution was loaded.</summary>
		/// <param name="solName">The current Solution name.</param>
		private void setSolution(string solName)
		{
			try
			{
				//Enable the progress bar as we load data.
				this.barLoading.Visibility = Visibility.Visible;
				this._solutionName = solName;

				if (string.IsNullOrEmpty(solName))
				{
					this.noSolutionLoaded();
					this.barLoading.Visibility = Visibility.Collapsed;
				}
				else
				{
					//Get projects associated with this solution.
					SerializableDictionary<string, string> availProjects = Settings.Default.AssignedProjects;
					if (availProjects != null && availProjects.ContainsKey(solName) && !string.IsNullOrWhiteSpace(availProjects[solName]))
					{
						this.loadProjects(availProjects[solName]);
					}
					else
					{
						this.noProjectsLoaded();
						this.barLoading.Visibility = Visibility.Collapsed;
					}
				}
			}
			catch (Exception ex)
			{
				//TODO: Log error.
			}
		}

		/// <summary>Enumeration of the node's type.</summary>
		//TODO: Change this to be a bitmask?
		private enum ItemTypeEnumeration
		{
			None = 0,
			Task = 1,
			Incident = 2,
			Requirement = 3,
			Folder = 4,
			HasItems = 10,
			HasNoItems = 11,
			Error = -999
		}

		/// <summary>Generates a filter for pulling information from the Spira server.</summary>
		/// <param name="UserNum">The user number of the data to pull for.</param>
		/// <param name="IncludeComplete">Whether or not to include completed/closed items.</param>
		/// <param name="IncTypeCode">The string type code of the artifact. "TK", "IN", "RQ"</param>
		/// <returns>A RemoteFilter set.</returns>
		private RemoteFilter[] GenerateFilter(int UserNum, bool IncludeComplete, string IncTypeCode)
		{
			try
			{
				RemoteFilter userFilter = new RemoteFilter() { PropertyName = "OwnerId", IntValue = UserNum };
				RemoteFilter statusFilter = new RemoteFilter();
				if (!IncludeComplete)
				{
					switch (IncTypeCode.ToUpperInvariant())
					{
						case "IN":
							{
								statusFilter = new RemoteFilter() { PropertyName = "IncidentStatusId", IntValue = -2 };
							}
							break;

						case "TK":
							{
								MultiValueFilter multiValue = new MultiValueFilter();
								multiValue.Values = new List<int> { 1, 2, 4, 5 };
								statusFilter = new RemoteFilter() { PropertyName = "TaskStatusId", MultiValue = multiValue };
							}
							break;

						case "RQ":
							{
								MultiValueFilter multiValue = new MultiValueFilter();
								multiValue.Values = new List<int> { 1, 2, 3, 5, 7 };
								statusFilter = new RemoteFilter() { PropertyName = "ScopeLevelId", MultiValue = multiValue };
							}
							break;
					}
				}

				return new RemoteFilter[] { userFilter, statusFilter };
			}
			catch (Exception ex)
			{
				//TODO: Error logging.
				return null;
			}
		}

		/// <summary>Generates a RemoteSort to be used on client calls.</summary>
		/// <returns>RemoteSort</returns>
		private RemoteSort GenerateSort()
		{
			RemoteSort sort = new RemoteSort();
			sort.PropertyName = "UserId";
			sort.SortAscending = true;

			return sort;
		}

		#region Tree Node Methods
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

				// - Error node.
				this._nodeError = new TreeViewItem();
				this._nodeError.Header = createNodeHeader("imgError", "Error contacting server.");
			}
			catch (Exception ex)
			{
				//TODO: Log error.
			}
		}

		/// <summary>Will change the text label of a TreeViewItem. The TreeViewItem must have a TextBlock in the Header control.</summary>
		/// <param name="inNode">The node to change.</param>
		/// <param name="Label">The string to change it to.</param>
		/// <returns>New modified treenode.</returns>
		private TreeViewItem changeNodeText(TreeViewItem inNode, string Label)
		{
			try
			{
				//Loop through and find the first textblock.
				int ctrlIndex = -1;
				for (int I = 0; I < ((StackPanel)inNode.Header).Children.Count; I++)
				{
					if (((StackPanel)inNode.Header).Children[I].GetType() == typeof(TextBlock))
					{
						ctrlIndex = I;
					}
				}

				if (ctrlIndex > -1)
				{
					((TextBlock)((StackPanel)inNode.Header).Children[ctrlIndex]).Text = Label;
				}
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
			}
			return inNode;
		}

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
					stckPnl.Children.Add(getImage(imageKey, new Size(16, 16)));
				}
				stckPnl.Children.Add(new TextBlock() { Text = Label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(3, 0, 0, 0) });

				return stckPnl;
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
				return new StackPanel();
			}
		}

		/// <summary>Will change the image label of a TreeViewItem. The TreeViewItem must have a TextBlock in the Header control.</summary>
		/// <param name="inNode">The node to change.</param>
		/// <param name="Label">The image key to change to. null will remove the image.</param>
		/// <returns>New modified treenode.</returns>
		private TreeViewItem changeNodeImage(TreeViewItem inNode, string imageKey)
		{
			try
			{
				//Loop through and find the first textblock.
				int ctrlIndex = -1;
				for (int I = 0; I < ((StackPanel)inNode.Header).Children.Count; I++)
				{
					if (((StackPanel)inNode.Header).Children[I].GetType() == typeof(Image))
					{
						ctrlIndex = I;
					}
				}

				if (ctrlIndex > -1)
				{
					if (!string.IsNullOrEmpty(imageKey))
					{
						((StackPanel)inNode.Header).Children.RemoveAt(ctrlIndex);
						((StackPanel)inNode.Header).Children.Insert(ctrlIndex, getImage(imageKey, new Size(16, 16)));
					}
					else
					{
						((StackPanel)inNode.Header).Children.RemoveAt(ctrlIndex);
					}
				}
			}
			catch (Exception ex)
			{
				//TODO: Log error.
			}
			return inNode;
		}

		/// <summary>Creates a project node.</summary>
		/// <param name="projectText">name to display on the node.</param>
		/// <returns>TreeViewitem with folders created.</returns>
		private TreeViewItem createProjectNode(string projectText)
		{
			try
			{
				TreeViewItem retNode = new TreeViewItem();
				//retNode.Resources.Add(SystemColors.HighlightBrushKey, Brushes.LightGreen);


				// - Standard Project Node.
				retNode.Header = createNodeHeader("imgFolder", projectText);

				TreeViewItem nodeIncident = new TreeViewItem();
				nodeIncident.Header = createNodeHeader("imgFolderIncident", "Incidents");


				TreeViewItem nodeTasks = new TreeViewItem();
				nodeTasks.Header = createNodeHeader("imgFolderTask", "Tasks");

				TreeViewItem nodeReqs = new TreeViewItem();
				nodeReqs.Header = createNodeHeader("imgFolderRequirement", "Requirements");

				retNode.Items.Add(nodeIncident);
				retNode.Items.Add(nodeTasks);
				retNode.Items.Add(nodeReqs);

				return retNode;
			}
			catch (Exception ex)
			{
				//TODO: Log error.
				return new TreeViewItem();
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

		/// <summary>Used to hold ObjectState information for Client Async calls.</summary>
		private class ObjectState
		{
			public bool curSearchIsMine = true;
			public int NodeNumber;
			public Connect.SpiraProject Project;
			public Spira_ImportExport.RemoteVersion ClientVersion;
		}
		#endregion
	}
}
