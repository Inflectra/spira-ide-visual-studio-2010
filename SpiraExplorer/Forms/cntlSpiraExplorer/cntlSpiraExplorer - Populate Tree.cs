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
						newProj.ArtifactTag = Business.SpiraProject.GenerateFromString(strProj);
						newProj.ArtifactId = ((Business.SpiraProject)newProj.ArtifactTag).ProjectID;
						newProj.ArtifactName = ((Business.SpiraProject)newProj.ArtifactTag).ProjectName;
						newProj.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Project;
						newProj.Parent = null;
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
				this.trvProject.Items.Clear();
				this.trvProject.ItemsSource = this._Projects;
				this.trvProject.Items.Refresh();
				//Clear the client collection.
				//this._Clients = new List<Business.Spira_ImportExport>();

				//Create the clients.
				foreach (TreeViewArtifact Project in this._Projects)
				{
					#region Incident Tree
					//-- *INCIDENTS*
					TreeViewArtifact folderIncidents = new TreeViewArtifact();
					folderIncidents.ArtifactName = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Incidents");
					folderIncidents.ArtifactIsFolder = true;
					folderIncidents.Parent = Project;

					// -My Incidents
					TreeViewArtifact folderIncidentsMy = new TreeViewArtifact();
					Business.Spira_ImportExport clientIncMy = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
					folderIncidentsMy.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), folderIncidents.ArtifactName);
					folderIncidentsMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Incident;
					folderIncidentsMy.ArtifactIsFolder = true;
					clientIncMy.ConnectionReady += new EventHandler(_client_ConnectionReady);
					clientIncMy.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
					clientIncMy.Client.Incident_RetrieveCompleted += new EventHandler<Business.SpiraTeam_Client.Incident_RetrieveCompletedEventArgs>(_client_Incident_RetrieveCompleted);
					clientIncMy.ClientNode = folderIncidentsMy;
					clientIncMy.Connect();
					this._numActiveClients++;

					//Add to project.
					if (Settings.Default.ShowUnassigned)
					{
						Project.Items.Add(folderIncidents);
						folderIncidentsMy.Parent = folderIncidents;
						folderIncidents.Items.Add(folderIncidentsMy);
					}
					else
					{
						folderIncidentsMy.Parent = Project;
						Project.Items.Add(folderIncidentsMy);
					}

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
						clientIncUn.Client.Incident_RetrieveCompleted += new EventHandler<Business.SpiraTeam_Client.Incident_RetrieveCompletedEventArgs>(_client_Incident_RetrieveCompleted);
						clientIncUn.ClientNode = folderIncidentsUn;
						folderIncidentsUn.ArtifactTag = clientIncUn;
						//Add to project.
						folderIncidentsUn.Parent = folderIncidents;
						folderIncidents.Items.Add(folderIncidentsUn);
						clientIncUn.Connect();
						this._numActiveClients++;
					}
					#endregion

					#region Requirement Tree
					//-- *REQUIREMENTS*
					TreeViewArtifact folderRequirements = new TreeViewArtifact();
					folderRequirements.ArtifactName = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Requirements");
					folderRequirements.ArtifactIsFolder = true;
					folderRequirements.Parent = Project;


					// -My Requirements
					TreeViewArtifact folderRequirementsMy = new TreeViewArtifact();
					Business.Spira_ImportExport clientReqMy = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
					folderRequirementsMy.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), folderRequirements.ArtifactName);
					folderRequirementsMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Requirement;
					folderRequirementsMy.ArtifactIsFolder = true;
					clientReqMy.ConnectionReady += new EventHandler(_client_ConnectionReady);
					clientReqMy.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
					clientReqMy.Client.Requirement_RetrieveCompleted += new EventHandler<Requirement_RetrieveCompletedEventArgs>(_client_Requirement_RetrieveCompleted);
					clientReqMy.ClientNode = folderRequirementsMy;
					folderRequirementsMy.ArtifactTag = clientReqMy;
					clientReqMy.Connect();
					this._numActiveClients++;

					//Add to project.
					if (Settings.Default.ShowUnassigned)
					{
						Project.Items.Add(folderRequirements);
						folderRequirementsMy.Parent = folderRequirements;
						folderRequirements.Items.Add(folderRequirementsMy);
					}
					else
					{
						folderRequirementsMy.Parent = Project;
						Project.Items.Add(folderRequirementsMy);
					}

					// -Unassigned Incidents
					if (Settings.Default.ShowUnassigned)
					{
						TreeViewArtifact folderRequirementsUn = new TreeViewArtifact();
						Business.Spira_ImportExport clientReqUn = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
						folderRequirementsUn.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), folderRequirements.ArtifactName);
						folderRequirementsUn.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Requirement;
						folderRequirementsUn.ArtifactIsFolder = true;
						clientReqUn.ConnectionReady += new EventHandler(_client_ConnectionReady);
						clientReqUn.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
						clientReqUn.Client.Requirement_RetrieveCompleted += new EventHandler<Requirement_RetrieveCompletedEventArgs>(_client_Requirement_RetrieveCompleted);
						clientReqUn.ClientNode = folderRequirementsUn;
						folderRequirementsUn.ArtifactTag = clientReqUn;
						//Add to project.
						folderRequirementsUn.Parent = folderRequirements;
						folderRequirements.Items.Add(folderRequirementsUn);
						clientReqUn.Connect();
						this._numActiveClients++;
					}
					#endregion

					#region Task Tree
					//-- *TASKS*
					TreeViewArtifact folderTasks = new TreeViewArtifact();
					folderTasks.ArtifactName = Business.StaticFuncs.getCultureResource.GetString("app_Tree_Tasks");
					folderTasks.ArtifactIsFolder = true;
					folderTasks.Parent = Project;

					// -My Requirements
					TreeViewArtifact folderTasksMy = new TreeViewArtifact();
					Business.Spira_ImportExport clientTskMy = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
					folderTasksMy.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_My"), folderTasks.ArtifactName);
					folderTasksMy.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Task;
					folderTasksMy.ArtifactIsFolder = true;
					clientTskMy.ConnectionReady += new EventHandler(_client_ConnectionReady);
					clientTskMy.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
					clientTskMy.Client.Task_RetrieveCompleted += new EventHandler<Task_RetrieveCompletedEventArgs>(_client_Task_RetrieveCompleted);
					clientTskMy.ClientNode = folderTasksMy;
					folderTasksMy.ArtifactTag = clientTskMy;
					clientTskMy.Connect();
					this._numActiveClients++;

					//Add to project.
					if (Settings.Default.ShowUnassigned)
					{
						Project.Items.Add(folderTasks);
						folderTasksMy.Parent = folderTasks;
						folderTasks.Items.Add(folderTasksMy);
					}
					else
					{
						folderTasksMy.Parent = Project;
						Project.Items.Add(folderTasksMy);
					}

					// -Unassigned Incidents
					if (Settings.Default.ShowUnassigned)
					{
						TreeViewArtifact folderTasksUn = new TreeViewArtifact();
						Business.Spira_ImportExport clientTskUn = new Business.Spira_ImportExport(((Business.SpiraProject)Project.ArtifactTag).ServerURL.ToString(), ((Business.SpiraProject)Project.ArtifactTag).UserName, ((Business.SpiraProject)Project.ArtifactTag).UserPass);
						folderTasksUn.ArtifactName = string.Format(Business.StaticFuncs.getCultureResource.GetString("app_Tree_Unassigned"), folderTasks.ArtifactName);
						folderTasksUn.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Task;
						folderTasksUn.ArtifactIsFolder = true;
						clientTskUn.ConnectionReady += new EventHandler(_client_ConnectionReady);
						clientTskUn.ConnectionError += new EventHandler<Business.Spira_ImportExport.ConnectionException>(_client_ConnectionError);
						clientTskUn.Client.Task_RetrieveCompleted += new EventHandler<Task_RetrieveCompletedEventArgs>(_client_Task_RetrieveCompleted);
						clientTskUn.ClientNode = folderTasksUn;
						folderTasksUn.ArtifactTag = clientTskUn;
						//Add to project.
						folderTasksUn.Parent = folderTasks;
						folderTasks.Items.Add(folderTasksUn);
						clientTskUn.Connect();
						this._numActiveClients++;
					}
					#endregion
				}
				//Refresh treeview.
				this.trvProject.Items.Refresh();
				//If no projects, hide the bar.
				if (this._Projects.Count == 0)
					this.barLoading.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				//TODO: Log error.
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
					//Add an error node to the treeview.
					TreeViewArtifact newNode = new TreeViewArtifact();
					newNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Error;
					newNode.ArtifactName = e.Error.Message;
					newNode.ArtifactTag = e.Error;
					newNode.ArtifactIsFolder = false;

					parentNode.Items.Add(newNode);
				}
			}
			else
			{
				//No parent node. Log error, exit.
				//TODO: Log error.
			}

			//Remove the count, refresh.
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
					_client.ClientNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Error;
					_client.ClientNode.Items.Clear();
					//TODO: Create tooltip for error.
					//client.ClientNode.ToolTip = null;
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

				client.Client.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(Client_Connection_ConnectToProjectCompleted);
				client.Client.Connection_ConnectToProjectAsync(nodeProject.ArtifactId, client);
			}
		}

		/// <summary>Hit when the client is connected and logged into a project.</summary>
		/// <param name="sender">Spira_ImportExport</param>
		/// <param name="e">Connection_ConnectToProjectCompletedEventArgs</param>
		private void Client_Connection_ConnectToProjectCompleted(object sender, Connection_ConnectToProjectCompletedEventArgs e)
		{
			//Connection ready. Let's fire off our query.
			Business.Spira_ImportExport client = e.UserState as Business.Spira_ImportExport;
			if (client != null)
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
							//TODO: Log error.
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
							//TODO: Log error.
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
							//TODO: Log error.
							this._numActiveClients--;
							this.refreshTree();
						}
						break;
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
					//Add an error node to the treeview.
					TreeViewArtifact newNode = new TreeViewArtifact();
					newNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Error;
					newNode.ArtifactName = e.Error.Message;
					newNode.ArtifactTag = e.Error;
					newNode.ArtifactIsFolder = false;

					parentNode.Items.Add(newNode);
				}
			}
			else
			{
				//No parent node. Log error, exit.
				//TODO: Log error.
			}

			//Remove the count, refresh.
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
					//Add an error node to the treeview.
					TreeViewArtifact newNode = new TreeViewArtifact();
					newNode.ArtifactType = TreeViewArtifact.ArtifactTypeEnum.Error;
					newNode.ArtifactName = e.Error.Message;
					newNode.ArtifactTag = e.Error;
					newNode.ArtifactIsFolder = false;

					parentNode.Items.Add(newNode);
				}
			}
			else
			{
				//No parent node. Log error, exit.
				//TODO: Log error.
			}

			//Remove the count, refresh.
			this._numActiveClients--;
			this.refreshTree();
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
				this.btnShowClosed.IsEnabled = false;
				this.btnRefresh.IsEnabled = false;
				this.barLoading.Visibility = System.Windows.Visibility.Collapsed;
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
				this.trvProject.ItemsSource = null;
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

		/// <summary>Refreshes tree and other items when needed.</summary>
		private void refreshTree()
		{
			//Calls the code to refresh the tree, and hide the progress br if necessary.
			this.trvProject.Items.Refresh();

			if (this._numActiveClients == 0)
				this.barLoading.Visibility = System.Windows.Visibility.Collapsed;
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