using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Properties;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>
	/// Interaction logic for cntlSpiraExplorer.xaml
	/// </summary>
	public partial class cntlSpiraExplorer : UserControl
	{
		private List<TreeViewArtifact> _Projects = new List<TreeViewArtifact>();
		private int _numActiveClients = 0;

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
						newProj.ArtifactTag = Business.SpiraProject.GenerateFromString(strProj);
						newProj.ArtifactId = ((Business.SpiraProject)newProj.ArtifactTag).ProjectID;
						newProj.ArtifactName = ((Business.SpiraProject)newProj.ArtifactTag).ProjectName;
						newProj.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Project;
						newProj.ArtifactIsFolder = true;
						newProj.Parent = null;

						this._Projects.Add(newProj);
					}
				}

				//Refresh the treeview.
				this.refreshProjects();

				//Enable the control buttons.
				this.btnShowClosed.IsEnabled = true;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex);
			}
		}

		private void refreshTreeNodeServerData(TreeViewArtifact itemToRefresh)
		{
			//Depending what is highlighted will specify what needs to be updated.
			if (itemToRefresh.ArtifactIsFolder)
			{
				//Update children..
				foreach (TreeViewArtifact subChild in itemToRefresh.Items)
				{
					//If it's a folder, call recursively.
					if (subChild.ArtifactIsFolder)
						this.refreshTreeNodeServerData(subChild);
				}

				if (itemToRefresh.ArtifactType.GetType() == typeof(Spira_ImportExport))
				{
					Spira_ImportExport clientExist = (Spira_ImportExport)itemToRefresh.ArtifactTag;

					//Kill it.
					try
					{
						clientExist.Client.Abort();
					}
					catch { }
					finally
					{
						try
						{
							clientExist.Client.Connection_Disconnect();
						}
						catch { }
					}
					clientExist = null;
					itemToRefresh.ArtifactTag = null;
				}

				//Now refresh this one if necessary.
				if (itemToRefresh.ArtifactType != TreeViewArtifact.ArtifactTypeEnum.None && itemToRefresh.ArtifactType != TreeViewArtifact.ArtifactTypeEnum.Project)
				{
					//We're spawning one off, make the bar visible.
					this.barLoading.Visibility = System.Windows.Visibility.Visible;
					this.trvProject.Cursor = System.Windows.Input.Cursors.AppStarting;

					//Generate a new client to go get data for.
					Spira_ImportExport clientRefresh = new Spira_ImportExport(((SpiraProject)itemToRefresh.ArtifactParentProject.ArtifactTag).ServerURL.ToString(), ((SpiraProject)itemToRefresh.ArtifactParentProject.ArtifactTag).UserName, ((SpiraProject)itemToRefresh.ArtifactParentProject.ArtifactTag).UserPass);
					clientRefresh.ConnectionReady += new EventHandler(_client_ConnectionReady);
					clientRefresh.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
					clientRefresh.Client.Incident_RetrieveCompleted += new EventHandler<Business.SpiraTeam_Client.Incident_RetrieveCompletedEventArgs>(_client_Incident_RetrieveCompleted);
					clientRefresh.Client.Requirement_RetrieveCompleted += new EventHandler<Requirement_RetrieveCompletedEventArgs>(_client_Requirement_RetrieveCompleted);
					clientRefresh.Client.Task_RetrieveCompleted += new EventHandler<Task_RetrieveCompletedEventArgs>(_client_Task_RetrieveCompleted);
					clientRefresh.Client.Connection_DisconnectCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_client_Connection_DisconnectCompleted);
					clientRefresh.ClientNode = itemToRefresh;
					itemToRefresh.ArtifactTag = clientRefresh;

					clientRefresh.Connect();
					this._numActiveClients++;
				}
			}
			else if (itemToRefresh.ArtifactType == TreeViewArtifact.ArtifactTypeEnum.Project)
			{
				//Loop through each child.
				foreach (TreeViewArtifact childItem in itemToRefresh.Items)
					this.refreshTreeNodeServerData(childItem);
			}
		}

		/// <summary>Refreshes the display for all loaded projects.</summary>
		private void refreshProjects()
		{
			try
			{
				//All this does is create the tree structure for each project, then calls
				//  refreshTreeNodeServerData on each Project TreeNode.

				//Clear the tree and refresh data.
				this.trvProject.ItemsSource = null;
				this.trvProject.Items.Clear();
				this.trvProject.ItemsSource = this._Projects;
				this.trvProject.Items.Refresh();
				this.barLoading.Visibility = Visibility.Visible;
				this.trvProject.Cursor = System.Windows.Input.Cursors.AppStarting;

				foreach (TreeViewArtifact trvProj in this._Projects)
				{
					//Create the 'My' nodes.
					TreeViewArtifact folderIncMy = new TreeViewArtifact();
					folderIncMy.ArtifactIsFolder = true;
					folderIncMy.ArtifactName = string.Format(StaticFuncs.getCultureResource.GetString("app_Tree_My"), StaticFuncs.getCultureResource.GetString("app_Tree_Incidents"));
					folderIncMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
					folderIncMy.ArtifactIsFolderMine = true;
					TreeViewArtifact folderReqMy = new TreeViewArtifact();
					folderReqMy.ArtifactIsFolder = true;
					folderReqMy.ArtifactName = string.Format(StaticFuncs.getCultureResource.GetString("app_Tree_My"), StaticFuncs.getCultureResource.GetString("app_Tree_Requirements"));
					folderReqMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Requirement;
					folderReqMy.ArtifactIsFolderMine = true;
					TreeViewArtifact folderTskMy = new TreeViewArtifact();
					folderTskMy.ArtifactIsFolder = true;
					folderTskMy.ArtifactName = string.Format(StaticFuncs.getCultureResource.GetString("app_Tree_My"), StaticFuncs.getCultureResource.GetString("app_Tree_Tasks"));
					folderTskMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Task;
					folderTskMy.ArtifactIsFolderMine = true;

					if (Settings.Default.ShowUnassigned)
					{
						//In this case, we need to create different root folders, as well as Unassigned folders.
						//Create real roots.
						TreeViewArtifact folderInc = new TreeViewArtifact() { ArtifactIsFolder = true, Parent = trvProj, ArtifactName = StaticFuncs.getCultureResource.GetString("app_Tree_Incidents") };
						TreeViewArtifact folderReq = new TreeViewArtifact() { ArtifactIsFolder = true, Parent = trvProj, ArtifactName = StaticFuncs.getCultureResource.GetString("app_Tree_Requirements") };
						TreeViewArtifact folderTsk = new TreeViewArtifact() { ArtifactIsFolder = true, Parent = trvProj, ArtifactName = StaticFuncs.getCultureResource.GetString("app_Tree_Tasks") };

						//Create unassigned nodes.
						TreeViewArtifact folderIncUn = new TreeViewArtifact();
						folderIncUn.ArtifactIsFolder = true;
						folderIncUn.ArtifactName = string.Format(StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), StaticFuncs.getCultureResource.GetString("app_Tree_Incidents"));
						folderIncUn.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
						folderIncUn.Parent = folderInc;
						TreeViewArtifact folderReqUn = new TreeViewArtifact();
						folderReqUn.ArtifactIsFolder = true;
						folderReqUn.ArtifactName = string.Format(StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), StaticFuncs.getCultureResource.GetString("app_Tree_Requirements"));
						folderReqUn.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Requirement;
						folderReqUn.Parent = folderReq;
						TreeViewArtifact folderTskUn = new TreeViewArtifact();
						folderTskUn.ArtifactIsFolder = true;
						folderTskUn.ArtifactName = string.Format(StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), StaticFuncs.getCultureResource.GetString("app_Tree_Tasks"));
						folderTskUn.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Task;
						folderTskUn.Parent = folderTsk;

						//Add them to their parents and to the project.
						folderIncMy.Parent = folderInc;
						folderReqMy.Parent = folderReq;
						folderTskMy.Parent = folderTsk;

						folderInc.Items.Add(folderIncMy);
						folderInc.Items.Add(folderIncUn);
						folderReq.Items.Add(folderReqMy);
						folderReq.Items.Add(folderReqUn);
						folderTsk.Items.Add(folderTskMy);
						folderTsk.Items.Add(folderTskUn);

						trvProj.Items.Add(folderInc);
						trvProj.Items.Add(folderReq);
						trvProj.Items.Add(folderTsk);
					}
					else
					{
						folderIncMy.Parent = trvProj;
						folderReqMy.Parent = trvProj;
						folderTskMy.Parent = trvProj;

						trvProj.Items.Add(folderIncMy);
						trvProj.Items.Add(folderReqMy);
						trvProj.Items.Add(folderTskMy);
					}

					//Now, refresh the project.
					this.refreshTreeNodeServerData(trvProj);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex);
			}
		}

		#region Client Events
		/// <summary>Hit when the Incident clients are finished retrieving data.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveCompletedEventArgs</param>
		private void _client_Incident_RetrieveCompleted(object sender, Business.SpiraTeam_Client.Incident_RetrieveCompletedEventArgs e)
		{
			//Grab parent node.
			TreeViewArtifact parentNode = e.UserState as TreeViewArtifact;

			if (parentNode != null)
			{
				parentNode.Items.Clear();

				//We got results back. Let's do something with them!
				if (e.Error == null)
				{
					foreach (RemoteIncident incident in e.Result)
					{
						//Make new node.
						TreeViewArtifact newNode = new TreeViewArtifact();
						newNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
						newNode.ArtifactTag = incident;
						newNode.ArtifactName = incident.Name;
						newNode.ArtifactIsFolder = false;
						newNode.ArtifactId = incident.IncidentId.Value;
						newNode.Parent = parentNode;

						parentNode.Items.Add(newNode);
					}
				}
				else
				{
					this.addErrorNode(ref parentNode, e.Error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
				}
			}
			else
			{
				//No parent node. Log error, exit.
				Logger.LogMessage("Did not get a parent folder!", System.Diagnostics.EventLogEntryType.Error);
			}

			//Disconnect the client, subtract from the count.
			try
			{
				((ImportExportClient)sender).Connection_DisconnectAsync();
			}
			catch { }
			parentNode.ArtifactTag = null;
			this._numActiveClients--;
			this.refreshTree();
		}

		/// <summary>Hit when a client ran into an error while trying to connect to the server.</summary>
		/// <param name="sender">Spira_ImportExport</param>
		/// <param name="e">Spira_ImportExport.ConnectionException</param>
		private void _client_ConnectionError(object sender, Business.Spira_ImportExport.ConnectionException e)
		{
			//There was an error trying to connect. Mark the node as error.
			Business.Spira_ImportExport _client = sender as Business.Spira_ImportExport;
			if (_client != null)
			{
				if (_client.ClientNode != null)
				{
					TreeViewArtifact treeNode = _client.ClientNode;
					this.addErrorNode(ref treeNode, e.error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
					_client.ClientNode = treeNode;

					//Refresh treeview.
					this._numActiveClients--;
					this.refreshTree();
				}
			}
		}

		/// <summary>Hit when a client is connected and ready to get data for the tree node.</summary>
		/// <param name="sender">Spira_ImportExport</param>
		/// <param name="e">EventArgs</param>
		private void _client_ConnectionReady(object sender, EventArgs e)
		{
			//Connection ready, connect to the project.
			Business.Spira_ImportExport client = sender as Business.Spira_ImportExport;
			if (client != null)
			{
				//Get the parent project..
				TreeViewArtifact nodeProject = client.ClientNode.ArtifactParentProject;

				client.Client.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(_client_Connection_ConnectToProjectCompleted);
				client.Client.Connection_ConnectToProjectAsync(nodeProject.ArtifactId, client);
			}
		}

		/// <summary>Hit when the client is connected and logged into a project.</summary>
		/// <param name="sender">Spira_ImportExport</param>
		/// <param name="e">Connection_ConnectToProjectCompletedEventArgs</param>
		private void _client_Connection_ConnectToProjectCompleted(object sender, Connection_ConnectToProjectCompletedEventArgs e)
		{
			//Connection ready. Let's fire off our query.
			Business.Spira_ImportExport client = e.UserState as Business.Spira_ImportExport;

			if (e.Error == null && client != null)
			{
				//Get the parent project..
				TreeViewArtifact nodeProject = client.ClientNode.ArtifactParentProject;

				switch (client.ClientNode.ArtifactType)
				{
					case TreeViewArtifact.ArtifactTypeEnum.Incident:
						string strIncidnet = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Incidents");
						string strMyIncident = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), strIncidnet);
						string strUnIncident = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), strIncidnet);
						if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strMyIncident.ToLowerInvariant().Trim())
						{
							//Send this client off to get all incidents assigned to User.
							client.Client.Incident_RetrieveAsync(Spira_ImportExport.GenerateFilter(((SpiraProject)nodeProject.ArtifactTag).UserID, Settings.Default.ShowCompleted, "IN"), Spira_ImportExport.GenerateSort(), 1, 99999, client.ClientNode);
						}
						else if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strUnIncident.ToLowerInvariant().Trim())
						{
							//This will only be hit if they have Unassigned incidents displayed. Otherwise,
							//  this whole section isn't run. No need to check the setting here.
							client.Client.Incident_RetrieveAsync(Spira_ImportExport.GenerateFilter(-999, Settings.Default.ShowCompleted, "IN"), Spira_ImportExport.GenerateSort(), 1, 99999, client.ClientNode);
						}
						else
						{
							// Do nothing. Something wrong.
							Logger.LogMessage("Folder has invalid name.", System.Diagnostics.EventLogEntryType.Error);
							this._numActiveClients--;
							this.refreshTree();
						}
						break;

					case TreeViewArtifact.ArtifactTypeEnum.Requirement:
						string strRequirement = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Requirements");
						string strMyRequirement = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), strRequirement);
						string strUnRequirement = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), strRequirement);
						if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strMyRequirement.ToLowerInvariant().Trim())
						{
							//Send this client off to get all incidents assigned to User.
							client.Client.Requirement_RetrieveAsync(Spira_ImportExport.GenerateFilter(((SpiraProject)nodeProject.ArtifactTag).UserID, Settings.Default.ShowCompleted, "RQ"), 1, 999999, client.ClientNode);
						}
						else if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strUnRequirement.ToLowerInvariant().Trim())
						{
							//This will only be hit if they have Unassigned incidents displayed. Otherwise,
							//  this whole section isn't run. No need to check the setting here.
							client.Client.Requirement_RetrieveAsync(Spira_ImportExport.GenerateFilter(-999, Settings.Default.ShowCompleted, "RQ"), 1, 99999, client.ClientNode);
						}
						else
						{
							// Do nothing. Something wrong.
							Logger.LogMessage("Folder has invalid name.", System.Diagnostics.EventLogEntryType.Error);
							this._numActiveClients--;
							this.refreshTree();
						}
						break;

					case TreeViewArtifact.ArtifactTypeEnum.Task:
						string strTask = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Tasks");
						string strMyTask = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), strTask);
						string strUnTask = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), strTask);
						if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strMyTask.ToLowerInvariant().Trim())
						{
							//Send this client off to get all incidents assigned to User.
							client.Client.Task_RetrieveAsync(Spira_ImportExport.GenerateFilter(((SpiraProject)nodeProject.ArtifactTag).UserID, Settings.Default.ShowCompleted, "TK"), Spira_ImportExport.GenerateSort(), 1, 99999, client.ClientNode);
						}
						else if (client.ClientNode.ArtifactName.ToLowerInvariant().Trim() == strUnTask.ToLowerInvariant().Trim())
						{
							//This will only be hit if they have Unassigned incidents displayed. Otherwise,
							//  this whole section isn't run. No need to check the setting here.
							client.Client.Task_RetrieveAsync(Spira_ImportExport.GenerateFilter(-999, Settings.Default.ShowCompleted, "TK"), Spira_ImportExport.GenerateSort(), 1, 99999, client.ClientNode);
						}
						else
						{
							// Do nothing. Something wrong.
							Logger.LogMessage("Folder has invalid name.", System.Diagnostics.EventLogEntryType.Error);
							this._numActiveClients--;
							this.refreshTree();
						}
						break;
				}
			}
			else
			{
				//Add an error node.
				if (client != null)
				{
					TreeViewArtifact treeNode = client.ClientNode;
					this.addErrorNode(ref treeNode, e.Error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
					client.ClientNode = treeNode;

					//Refresh treeview.
					this._numActiveClients--;
					this.refreshTree();
				}
			}
		}

		/// <summary>Hit when a client sent to retrieve Requirements is finished with results.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Requirement_RetrieveCompletedEventArgs</param>
		private void _client_Requirement_RetrieveCompleted(object sender, Requirement_RetrieveCompletedEventArgs e)
		{
			//Grab parent node.
			TreeViewArtifact parentNode = e.UserState as TreeViewArtifact;

			if (parentNode != null)
			{
				parentNode.Items.Clear();

				//We got results back. Let's do something with them!
				if (e.Error == null)
				{
					foreach (RemoteRequirement requirement in e.Result)
					{
						if (!requirement.Summary)
						{
							//Make new node.
							TreeViewArtifact newNode = new TreeViewArtifact();
							newNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Requirement;
							newNode.ArtifactTag = requirement;
							newNode.ArtifactName = requirement.Name;
							newNode.ArtifactIsFolder = false;
							newNode.ArtifactId = requirement.RequirementId.Value;
							newNode.Parent = parentNode;

							parentNode.Items.Add(newNode);
						}
					}
				}
				else
				{
					this.addErrorNode(ref parentNode, e.Error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
				}
			}
			else
			{
				//No parent node. Log error, exit.
				Logger.LogMessage("No parent folder.", System.Diagnostics.EventLogEntryType.Error);
			}

			//Disconnect the client, subtract from the count.
			try
			{
				((ImportExportClient)sender).Connection_DisconnectAsync();
			}
			catch { }
			parentNode.ArtifactTag = null;
			this._numActiveClients--;
			this.refreshTree();
		}

		/// <summary>Hit when a client sent to retrieve Tasks is finished with results.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Task_RetrieveCompletedEventArgs</param>
		private void _client_Task_RetrieveCompleted(object sender, Task_RetrieveCompletedEventArgs e)
		{
			//Grab parent node.
			TreeViewArtifact parentNode = e.UserState as TreeViewArtifact;

			if (parentNode != null)
			{
				parentNode.Items.Clear();

				//We got results back. Let's do something with them!
				if (e.Error == null)
				{
					foreach (RemoteTask task in e.Result)
					{
						//Make new node.
						TreeViewArtifact newNode = new TreeViewArtifact();
						newNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Task;
						newNode.ArtifactTag = task;
						newNode.ArtifactName = task.Name;
						newNode.ArtifactIsFolder = false;
						newNode.ArtifactId = task.TaskId.Value;
						newNode.Parent = parentNode;

						parentNode.Items.Add(newNode);
					}
				}
				else
				{
					this.addErrorNode(ref parentNode, e.Error, StaticFuncs.getCultureResource.GetString("app_Error_RetrieveShort"));
				}
			}
			else
			{
				//No parent node. Log error, exit.
				Logger.LogMessage("No parent node!", System.Diagnostics.EventLogEntryType.Error);
			}

			//Disconnect the client, subtract from the count.
			try
			{
				((ImportExportClient)sender).Connection_DisconnectAsync();
			}
			catch { }
			parentNode.ArtifactTag = null;
			this._numActiveClients--;
			this.refreshTree();
		}

		/// <summary>Hit when a client is finished connecting.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _client_Connection_DisconnectCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			//We're finished disconnecting. Let's null it out.
			sender = null;
		}

		#endregion

		/// <summary>Will erase all nodes in the treeview and display the 'No Solution Loaded' message.</summary>
		private void noSolutionLoaded()
		{
			try
			{
				this._solutionName = null;
				this.trvProject.ItemsSource = null;
				this.trvProject.Items.Clear();
				this.trvProject.Items.Add(this._nodeNoSolution);
				this.barLoading.Visibility = Visibility.Collapsed;
				this.trvProject.Cursor = System.Windows.Input.Cursors.Arrow;
				this.btnRefresh.IsEnabled = false;
				this.btnShowClosed.IsEnabled = false;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex);
			}
		}

		/// <summary>Will erase all nodes in the treeview and display the 'No Projects Loaded' message.</summary>
		private void noProjectsLoaded()
		{
			try
			{
				this.trvProject.ItemsSource = null;
				this.trvProject.Items.Clear();
				this.trvProject.Items.Add(this._nodeNoProjects);
				this.barLoading.Visibility = Visibility.Collapsed;
				this.trvProject.Cursor = System.Windows.Input.Cursors.Arrow;
				this.btnShowClosed.IsEnabled = false;
				this.btnRefresh.IsEnabled = false;
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex);
			}
		}

		/// <summary>Refreshes tree and other items when needed.</summary>
		private void refreshTree()
		{
			//Calls the code to refresh the tree, and hide the progress br if necessary.
			this.trvProject.Items.Refresh();

			if (this._numActiveClients == 0)
			{
				this.barLoading.Visibility = System.Windows.Visibility.Collapsed;
				this.trvProject.Cursor = System.Windows.Input.Cursors.Arrow;
			}
		}

		/// <summary>Creates an error node in the given node.</summary>
		/// <param name="nodeToAddTo">TreeViewArtifact node to add the error to.</param>
		/// <param name="exception">Exception for the error.</param>
		/// <param name="Title">The title of the error node.</param>
		private void addErrorNode(ref TreeViewArtifact nodeToAddTo, Exception exception, String Title)
		{
			TreeViewArtifact errorNode = new TreeViewArtifact();
			errorNode.ArtifactIsError = true;
			errorNode.ArtifactName = Title;
			errorNode.ArtifactTag = exception;
			errorNode.Parent = nodeToAddTo;

			//Clear existing, add error.
			nodeToAddTo.Items.Clear();
			nodeToAddTo.Items.Add(errorNode);
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