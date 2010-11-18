using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Properties;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>
	/// Interaction logic for cntlSpiraExplorer.xaml
	/// </summary>
	public partial class cntlSpiraExplorer : UserControl
	{
		private List<TreeViewArtifact> _Projects = new List<TreeViewArtifact>();
		private List<ImportExport> xx_Clients = new List<ImportExport>();
		//Default node numbers.
		private const int NODE_INCIDENTNUM = 0;
		private const int NODE_TASKNUM = 1;
		private const int NODE_REQUIREMENTNUM = 2;

		/// <summary>Loads a new set of projects into the treeview.</summary>
		/// <param name="ProjList">A field-seperated list of project id's.</param>
		private void loadProjects(string ProjList)
		{
			try
			{
				//Get our list of projects.
				this._Projects = new List<TreeViewArtifact>();
				if (!string.IsNullOrEmpty(ProjList))
				{
					foreach (string strProj in ProjList.Split(Business.SpiraProject.CHAR_RECORD))
					{
						TreeViewArtifact newProj = new TreeViewArtifact();
						newProj.ArtifactObject = Business.SpiraProject.GenerateFromString(strProj);
						newProj.ArtifactId = ((Business.SpiraProject)newProj.ArtifactObject).ProjectID;
						newProj.ArtifactName = ((Business.SpiraProject)newProj.ArtifactObject).ProjectName;
						newProj.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Project;
						this._Projects.Add(newProj);
					}
				}

				//Refresh the treeview.
				this.refreshProjects();
			}
			catch (Exception ex)
			{
				//TODO: Log error.
			}
		}

		/// <summary>Refreshes the display for the loaded projects.</summary>
		private void refreshProjects()
		{
			try
			{
				//Turn on the trailing working bar.
				this.barLoading.Visibility = Visibility.Visible;

				//Erase what's in the treeview already.
				this.trvProject.ItemsSource = null;
				this.trvProject.Items.Clear();
				//Clear the client collection.
				//this._Clients = new List<Business.Spira_ImportExport>();

				//Create the clients.
				foreach (TreeViewArtifact Project in this._Projects)
				{
					//-- *INCIDENTS*
					TreeViewArtifact folderIncidents = new TreeViewArtifact();
					folderIncidents.ArtifactName = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Incidents");
					folderIncidents.ArtifactIsFolder = true;

					// -My Incidents
					TreeViewArtifact folderIncidentsMy = new TreeViewArtifact();
					Business.Spira_ImportExport clientIncMy = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
					folderIncidentsMy.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), folderIncidents.ArtifactName);
					folderIncidentsMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
					folderIncidentsMy.ArtifactIsFolder = true;
					clientIncMy.ConnectionReady += new EventHandler(_client_ConnectionReady);
					clientIncMy.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
					clientIncMy.ClientNode = folderIncidentsMy;
					clientIncMy.Connect();

					//Add to project.
					if (Settings.Default.ShowUnassigned)
					{
						Project.Items.Add(folderIncidents);
						folderIncidents.Items.Add(folderIncidentsMy);
					}
					else
						Project.Items.Add(folderIncidentsMy);

					// -Unassigned Incidents
					if (Settings.Default.ShowUnassigned)
					{
						TreeViewArtifact folderIncidentsUn = new TreeViewArtifact();
						Business.Spira_ImportExport clientIncUn = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
						folderIncidentsUn.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), folderIncidents.ArtifactName);
						folderIncidentsUn.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
						folderIncidentsUn.ArtifactIsFolder = true;
						clientIncUn.ConnectionReady += new EventHandler(_client_ConnectionReady);
						clientIncUn.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
						clientIncUn.ClientNode = folderIncidentsUn;
						folderIncidentsUn.ArtifactTag = clientIncUn;
						//Add to project.
						folderIncidents.Items.Add(folderIncidentsUn);
						clientIncUn.Connect();
					}

					//-- *REQUIREMENTS*
					TreeViewArtifact folderRequirements = new TreeViewArtifact();
					folderRequirements.ArtifactName = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Requirements");
					folderRequirements.ArtifactIsFolder = true;

					// -My Requirements
					TreeViewArtifact folderRequirementsMy = new TreeViewArtifact();
					Business.Spira_ImportExport clientReqMy = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
					folderRequirementsMy.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), folderRequirements.ArtifactName);
					folderRequirementsMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
					folderRequirementsMy.ArtifactIsFolder = true;
					clientReqMy.ConnectionReady += new EventHandler(_client_ConnectionReady);
					clientReqMy.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
					clientReqMy.ClientNode = folderRequirementsMy;
					folderRequirementsMy.ArtifactTag = clientReqMy;
					clientReqMy.Connect();

					//Add to project.
					if (Settings.Default.ShowUnassigned)
					{
						Project.Items.Add(folderRequirements);
						folderIncidents.Items.Add(folderRequirementsMy);
					}
					else
						Project.Items.Add(folderRequirementsMy);

					// -Unassigned Incidents
					if (Settings.Default.ShowUnassigned)
					{
						TreeViewArtifact folderRequirementsUn = new TreeViewArtifact();
						Business.Spira_ImportExport clientReqUn = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
						folderRequirementsUn.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), folderIncidents.ArtifactName);
						folderRequirementsUn.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
						folderRequirementsUn.ArtifactIsFolder = true;
						clientReqUn.ConnectionReady += new EventHandler(_client_ConnectionReady);
						clientReqUn.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
						clientReqUn.ClientNode = folderRequirementsUn;
						folderRequirementsUn.ArtifactTag = clientReqUn;
						//Add to project.
						folderIncidents.Items.Add(folderRequirementsUn);
						clientReqUn.Connect();
					}

					//-- *TASKS*
					TreeViewArtifact folderTasks = new TreeViewArtifact();
					folderTasks.ArtifactName = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Tasks");
					folderTasks.ArtifactIsFolder = true;

					// -My Requirements
					TreeViewArtifact folderTasksMy = new TreeViewArtifact();
					Business.Spira_ImportExport clientTskMy = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
					folderTasksMy.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), folderTasks.ArtifactName);
					folderTasksMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
					folderTasksMy.ArtifactIsFolder = true;
					clientTskMy.ConnectionReady += new EventHandler(_client_ConnectionReady);
					clientTskMy.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
					clientTskMy.ClientNode = folderTasksMy;
					folderRequirementsMy.ArtifactTag = clientTskMy;
					clientTskMy.Connect();

					//Add to project.
					if (Settings.Default.ShowUnassigned)
					{
						Project.Items.Add(folderTasks);
						folderIncidents.Items.Add(folderTasksMy);
					}
					else
						Project.Items.Add(folderTasksMy);

					// -Unassigned Incidents
					if (Settings.Default.ShowUnassigned)
					{
						TreeViewArtifact folderTasksUn = new TreeViewArtifact();
						Business.Spira_ImportExport clientTskUn = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
						folderTasksUn.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), folderTasks.ArtifactName);
						folderTasksUn.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
						folderTasksUn.ArtifactIsFolder = true;
						clientTskUn.ConnectionReady += new EventHandler(_client_ConnectionReady);
						clientTskUn.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
						clientTskUn.ClientNode = folderTasksUn;
						folderTasksUn.ArtifactTag = clientTskUn;
						//Add to project.
						folderIncidents.Items.Add(folderTasksUn);
						clientTskUn.Connect();
					}
				}
				//If no projects, hide the bar.
				if (this._Projects.Count == 0)
					this.barLoading.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				//TODO: Log error.
			}
		}

		/// <summary>Hit when a client ran into an error while trying to connect to the server.</summary>
		/// <param name="sender">Spira_ImportExport</param>
		/// <param name="e">Spira_ImportExport.ConnectionException</param>
		void _client_ConnectionError(object sender, Business.Spira_ImportExport.ConnectionException e)
		{
			//There was an error trying to connect. Mark the node as error.
			Business.Spira_ImportExport _client = sender as Business.Spira_ImportExport;
			if (_client != null)
			{
				if (_client.ClientNode != null)
				{
					_client.ClientNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Error;
					_client.ClientNode.Items.Clear();
					//TODO: Create tooltip for error.
					_client.ClientNode.ToolTip = null;
				}
			}
		}

		/// <summary>Hit when a client is connected and ready to get data for the tree node.</summary>
		/// <param name="sender">Spira_ImportExport</param>
		/// <param name="e">EventArgs</param>
		void _client_ConnectionReady(object sender, EventArgs e)
		{
			//Connection ready. Let's fire off our query.

		}

		#region Client Event Handlers
		/// <summary>Hit when the client is finished connecting and logging in.</summary>
		/// <param name="sender">The client that called it.</param>
		/// <param name="e">Event Args.</param>
		private void client_FinishConnecting(object sender, EventArgs e)
		{
			try
			{
				ImportExport client = (ImportExport)sender;
				string eType = e.GetType().ToString();
				eType = eType.Substring(eType.LastIndexOf('.') + 1);

				switch (eType)
				{
					case "Connection_Authenticate2CompletedEventArgs":
						{
							//Connect finished.
							Connection_Authenticate2CompletedEventArgs evt = (Connection_Authenticate2CompletedEventArgs)e;
							ObjectState evtObj = (ObjectState)evt.UserState;
							if (evt.Error == null)
							{
								client.System_GetProductVersionAsync(evtObj);
							}
							else
							{
								//Set error to node.
								TreeViewItem oldNode = (TreeViewItem)this.trvProject.Items[evtObj.NodeNumber];
								this.trvProject.Items.RemoveAt(evtObj.NodeNumber);
								this.trvProject.Items.Insert(evtObj.NodeNumber, this.changeNodeImage(oldNode, "imgError"));
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).ToolTip = new TextBlock() { Text = evt.Error.Message };
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items.Clear();
								//Error, clean up.
								removeClient(client);
							}
						}
						break;

					case "System_GetProductVersionCompletedEventArgs":
						{
							//Connect finished.
							System_GetProductVersionCompletedEventArgs evt = (System_GetProductVersionCompletedEventArgs)e;
							ObjectState evtObj = (ObjectState)evt.UserState;
							if (evt.Error == null)
							{
								evtObj.ClientVersion = evt.Result;
								client.Connection_ConnectToProjectAsync(evtObj.Project.ProjectID, evtObj);
							}
							else
							{
								//Set error to node.
								TreeViewItem oldNode = (TreeViewItem)this.trvProject.Items[evtObj.NodeNumber];
								this.trvProject.Items.RemoveAt(evtObj.NodeNumber);
								this.trvProject.Items.Insert(evtObj.NodeNumber, this.changeNodeImage(oldNode, "imgError"));
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).ToolTip = new TextBlock() { Text = evt.Error.Message };
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items.Clear();
								//Error, clean up.
								removeClient(client);
							}
						}
						break;

					case "Connection_ConnectToProjectCompletedEventArgs":
						{
							//Connect finished.
							Connection_ConnectToProjectCompletedEventArgs evt = (Connection_ConnectToProjectCompletedEventArgs)e;
							ObjectState evtObj = (ObjectState)evt.UserState;
							if (evt.Error == null)
							{
								evtObj.curSearchIsMine = true;
								RemoteFilter[] filters = GenerateFilter(evtObj.Project.UserID, this.btnShowClosed.IsChecked.Value, "IN");
								RemoteSort sort = GenerateSort();
								client.Incident_RetrieveAsync(filters, sort, 1, 9999999, evtObj);
							}
							else
							{
								//Set error to node.
								TreeViewItem oldNode = (TreeViewItem)this.trvProject.Items[evtObj.NodeNumber];
								this.trvProject.Items.RemoveAt(evtObj.NodeNumber);
								this.trvProject.Items.Insert(evtObj.NodeNumber, this.changeNodeImage(oldNode, "imgError"));
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).ToolTip = new TextBlock() { Text = evt.Error.Message };
								((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items.Clear();
								//Error, clean up.
								removeClient(client);
							}
						}
						break;
				}
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
			}
		}

		/// <summary>Hit when the client is finished getting a list of Incidents.</summary>
		/// <param name="sender">The client that caused the event.</param>
		/// <param name="evt">Event Args.</param>
		private void client_FinishIncident(object sender, Incident_RetrieveCompletedEventArgs evt)
		{
			try
			{
				ImportExport client = (ImportExport)sender;
				ObjectState evtObj = (ObjectState)evt.UserState;

				//Make the current node and the existing parent node.
				TreeViewItem incItemNode = new TreeViewItem();
				TreeViewItem incNode = (TreeViewItem)((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items[NODE_INCIDENTNUM];

				//Get the label.
				string nodeLabel = ((evtObj.curSearchIsMine) ? "My" : "Unassigned") + " " + this._resources.GetString("strIncidents");

				if (evt.Error == null)
				{
					//Set the image & change text.
					if (evt.Result.Length > 0)
					{
						incItemNode.Header = createNodeHeader("imgFolderIncident", nodeLabel + " (" + evt.Result.Length + ")");
					}
					else
					{
						incItemNode.Header = createNodeHeader("imgFolderIncident", nodeLabel);
					}

					//Add child nodes.
					foreach (RemoteIncident Inc in evt.Result)
					{
						TreeViewItem incidentNode = new TreeViewItem();
						incidentNode.Header = createNodeHeader("imgIncident", Inc.Name);
						incidentNode.MouseDoubleClick += new MouseButtonEventHandler(tree_NodeDoubleClick);
						incidentNode.Tag = Connect.SpiraProject.GenerateToString(evtObj.Project) + Connect.SpiraProject.CHAR_RECORD + "IN:" + Inc.IncidentId.ToString();

						incItemNode.Items.Add(incidentNode);
					}

					incNode.Items.Add(incItemNode);

					//Fire the next step.
					if (evtObj.curSearchIsMine)
					{
						evtObj.curSearchIsMine = false;
						RemoteFilter[] filters = this.GenerateFilter(-999, this.btnShowClosed.IsChecked.Value, "IN");
						RemoteSort sort = this.GenerateSort();
						client.Incident_RetrieveAsync(filters, sort, 1, 9999999, evtObj);
					}
					else
					{
						evtObj.curSearchIsMine = true;
						RemoteFilter[] filters = this.GenerateFilter(evtObj.Project.UserID, this.btnShowClosed.IsChecked.Value, "TK");
						RemoteSort sort = this.GenerateSort();
						client.Task_RetrieveAsync(filters, sort, 1, 9999999, evtObj);
					}
				}
				else
				{
					//Set error flag on node.
					incItemNode = changeNodeImage(incItemNode, "imgError");
					incItemNode.Items.Clear();
					incItemNode.ToolTip = new TextBlock() { Text = evt.Error.Message };

					//Add it to our projectnode.
					incNode.Items.Add(incItemNode);

					//Error, clean up.
					removeClient(client);
				}
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
			}
		}

		/// <summary>Hit when the client is finishe getting a list of Requirements.</summary>
		/// <param name="sender">The client that caused the event.</param>
		/// <param name="evt">Event Args.</param>
		private void client_FinishRequirement(object sender, Requirement_RetrieveCompletedEventArgs evt)
		{
			try
			{
				ImportExport client = (ImportExport)sender;
				ObjectState evtObj = (ObjectState)evt.UserState;

				//Make the current node and the existing parent node.
				TreeViewItem reqItemNode = new TreeViewItem();
				TreeViewItem reqNode = (TreeViewItem)((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items[NODE_REQUIREMENTNUM];
				reqItemNode = reqNode;

				if (evt.Error == null)
				{
					//Add child nodes.
					int numCounted = 0;
					foreach (RemoteRequirement Req in evt.Result)
					{
						if (!Req.Summary)
						{
							numCounted++;
							TreeViewItem reqDetNode = new TreeViewItem();
							reqDetNode.Header = createNodeHeader("imgRequirement", Req.Name);
							reqDetNode.MouseDoubleClick += new MouseButtonEventHandler(tree_NodeDoubleClick);
							reqDetNode.Tag = Connect.SpiraProject.GenerateToString(evtObj.Project) + Connect.SpiraProject.CHAR_RECORD + "RQ:" + Req.RequirementId.ToString();

							reqItemNode.Items.Add(reqDetNode);
						}
					}

					//Set the image & change text.
					if (numCounted > 0)
					{
						reqItemNode = this.changeNodeText(reqItemNode, this._resources.GetString("strRequirements") + " (" + numCounted.ToString() + ")");
					}

					//Clean up.
					removeClient(client);
				}
				else
				{
					//Set error flag on node.
					reqItemNode = changeNodeImage(reqItemNode, "imgError");
					reqItemNode.Items.Clear();
					reqItemNode.ToolTip = new TextBlock() { Text = evt.Error.Message };

					//Error, clean up.
					removeClient(client);
				}
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
			}
		}

		/// <summary>Hit when the client is finished getting a list of Tasks for the user.</summary>
		/// <param name="sender">The client that caused the event.</param>
		/// <param name="evt">Event Args.</param>
		private void client_FinishTask(object sender, Task_RetrieveCompletedEventArgs evt)
		{
			try
			{
				ImportExport client = (ImportExport)sender;
				ObjectState evtObj = (ObjectState)evt.UserState;

				//Make the current node and the existing parent node.
				TreeViewItem tskItemNode = new TreeViewItem();
				TreeViewItem tskNode = (TreeViewItem)((TreeViewItem)this.trvProject.Items[evtObj.NodeNumber]).Items[NODE_TASKNUM];

				//Get the label.
				string nodeLabel = ((evtObj.curSearchIsMine) ? "My" : "Unassigned") + " " + this._resources.GetString("strTasks");

				if (evt.Error == null)
				{
					//Set the image & change text.
					if (evt.Result.Length > 0)
					{
						tskItemNode.Header = createNodeHeader("imgFolderTask", nodeLabel + " (" + evt.Result.Length + ")");
					}
					else
					{
						tskItemNode.Header = createNodeHeader("imgFolderTask", nodeLabel);
					}

					//Add child nodes.
					foreach (RemoteTask Tsk in evt.Result)
					{
						TreeViewItem taskNode = new TreeViewItem();
						taskNode.Header = createNodeHeader("imgTask", Tsk.Name);
						taskNode.MouseDoubleClick += new MouseButtonEventHandler(tree_NodeDoubleClick);
						taskNode.Tag = Connect.SpiraProject.GenerateToString(evtObj.Project) + Connect.SpiraProject.CHAR_RECORD + "TK:" + Tsk.TaskId.ToString();

						tskItemNode.Items.Add(taskNode);
					}

					tskNode.Items.Add(tskItemNode);

					//Fire the next step.
					if (evtObj.curSearchIsMine)
					{
						evtObj.curSearchIsMine = false;
						RemoteFilter[] filters = this.GenerateFilter(-999, this.btnShowClosed.IsChecked.Value, "TK");
						RemoteSort sort = this.GenerateSort();
						client.Task_RetrieveAsync(filters, sort, 1, 9999999, evtObj);
					}
					else
					{
						evtObj.curSearchIsMine = true;
						RemoteFilter[] filters = this.GenerateFilter(evtObj.Project.UserID, this.btnShowClosed.IsChecked.Value, "RQ");
						RemoteSort sort = this.GenerateSort();
						client.Requirement_RetrieveAsync(filters, 1, 9999999, evtObj);
					}
				}
				else
				{
					//Set error flag on node.
					tskItemNode = changeNodeImage(tskItemNode, "imgError");
					tskItemNode.Items.Clear();
					tskItemNode.ToolTip = new TextBlock() { Text = evt.Error.Message };

					//Add it to our projectnode.
					tskNode.Items.Add(tskItemNode);

					//Error, clean up.
					removeClient(client);
				}
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
			}

		}
		#endregion

		/// <summary>Will erase all nodes in the treeview and display the 'No Solution Loaded' message.</summary>
		private void noSolutionLoaded()
		{
			try
			{
				this.trvProject.Items.Clear();
				this.trvProject.Items.Add(this._nodeNoSolution);
				this.btnShowClosed.IsEnabled = false;
				this.btnRefresh.IsEnabled = false;
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
			}
		}

		/// <summary>Will erase all nodes in the treeview and display the 'No Projects Loaded' message.</summary>
		private void noProjectsLoaded()
		{
			try
			{
				this.trvProject.Items.Clear();
				this.trvProject.Items.Add(this._nodeNoProjects);
				this.btnShowClosed.IsEnabled = false;
				this.btnRefresh.IsEnabled = false;
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
			}
		}

		/// <summary>Removes the client from the list and possible enables form.</summary>
		/// <param name="client">The client to kill and remove.</param>
		private void removeClient(ImportExport client)
		{
			try
			{

				try
				{
					client.CancelAsync(null);
					//client.Connection_Disconnect();
				}
				catch
				{
				}
				client.Dispose();

				//Remove it from the list.
				this._Clients.Remove(client);

				//Null it out.
				client = null;


				if (this._Clients.Count == 0)
				{
					//Enable form.
					this.btnRefresh.IsEnabled = true;
					this.btnShowClosed.IsEnabled = true;
					this.barLoading.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
			}
		}

		private bool checkClientVersion(RemoteVersion version)
		{
			//Return 'true' if version is 2.3.1 p15 or greater. False if not.
			string[] verNums = version.Version.Split('.');

			return false;
		}
	}
}

/*
			catch (FaultException<ServiceFaultMessage> exception)
			{
				//Need to get the underlying message fault
				ServiceFaultMessage detail = exception.Detail;
				if (detail.Type == "SessionNotAuthenticated")
*/