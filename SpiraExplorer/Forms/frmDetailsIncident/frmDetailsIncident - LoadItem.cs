using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	public partial class frmDetailsIncident : UserControl
	{
		#region Client Values
		private ImportExportClient _client;
		private int _clientNumRunning;
		private int _clientNum;
		private int _clientNumWorkflow;
		#endregion

		#region Private Data Storage Variables

		//The Project and the Incident
		private SpiraProject _Project = null;
		private RemoteIncident _Incident;
		private RemoteIncident _IncidentConcurrency;

		//Other project-specific items.
		private List<RemoteProjectUser> _ProjUsers;
		private List<RemoteRelease> _ProjReleases;
		private List<RemoteIncidentSeverity> _IncSeverity;
		private List<RemoteIncidentPriority> _IncPriority;
		private List<RemoteIncidentType> _IncType;
		private List<RemoteIncidentStatus> _IncStatus;
		private List<RemoteWorkflowIncidentTransition> _IncWkfTransition;

		//Workflow fields..
		private Dictionary<int, int> _IncWkfFields_Current;
		private Dictionary<int, int> _IncWkfCustom_Current;
		private Dictionary<int, int> _IncWkfFields_Updated;
		private Dictionary<int, int> _IncWkfCustom_Updated;
		private int? _IncCurrentType;
		private int? _IncCurrentStatus;
		#endregion

		// Are we in read-only mode? Are we saving?
		private bool isInLoadMode = false;
		private bool isInSaveMode = false;
		private bool isInConcMode = false;

		/// <summary>Loads the item currently assigned to the ArtifactDetail property.</summary>
		/// <returns>Boolean on whether of not load was started successfully.</returns>
		private bool load_LoadItem()
		{
			bool retValue = false;
			if (this.ArtifactDetail != null)
			{
				//Create a client.
				this._client = null;
				this._client = StaticFuncs.CreateClient(((SpiraProject)this.ArtifactDetail.ArtifactParentProject.ArtifactTag).ServerURL.ToString());

				//Set client events.
				this._client.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(_client_Connection_Authenticate2Completed);
				this._client.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(_client_Connection_ConnectToProjectCompleted);
				this._client.Connection_DisconnectCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_client_Connection_DisconnectCompleted);
				this._client.Incident_RetrieveByIdCompleted += new EventHandler<Incident_RetrieveByIdCompletedEventArgs>(_client_Incident_RetrieveByIdCompleted);
				this._client.Incident_RetrieveResolutionsCompleted += new EventHandler<Incident_RetrieveResolutionsCompletedEventArgs>(_client_Incident_RetrieveResolutionsCompleted);
				this._client.Incident_RetrieveSeveritiesCompleted += new EventHandler<Incident_RetrieveSeveritiesCompletedEventArgs>(_client_Incident_RetrieveSeveritiesCompleted);
				this._client.Incident_RetrievePrioritiesCompleted += new EventHandler<Incident_RetrievePrioritiesCompletedEventArgs>(_client_Incident_RetrievePrioritiesCompleted);
				this._client.Incident_RetrieveStatusesCompleted += new EventHandler<Incident_RetrieveStatusesCompletedEventArgs>(_client_Incident_RetrieveStatusesCompleted);
				this._client.Incident_RetrieveTypesCompleted += new EventHandler<Incident_RetrieveTypesCompletedEventArgs>(_client_Incident_RetrieveTypesCompleted);
				this._client.Incident_RetrieveWorkflowCustomPropertiesCompleted += new EventHandler<Incident_RetrieveWorkflowCustomPropertiesCompletedEventArgs>(_client_Incident_RetrieveWorkflowCustomPropertiesCompleted);
				this._client.Incident_RetrieveWorkflowFieldsCompleted += new EventHandler<Incident_RetrieveWorkflowFieldsCompletedEventArgs>(_client_Incident_RetrieveWorkflowFieldsCompleted);
				this._client.Incident_RetrieveWorkflowTransitionsCompleted += new EventHandler<Incident_RetrieveWorkflowTransitionsCompletedEventArgs>(_client_Incident_RetrieveWorkflowTransitionsCompleted);
				this._client.Release_RetrieveCompleted += new EventHandler<Release_RetrieveCompletedEventArgs>(_client_Release_RetrieveCompleted);
				this._client.Project_RetrieveUserMembershipCompleted += new EventHandler<Project_RetrieveUserMembershipCompletedEventArgs>(_client_Project_RetrieveUserMembershipCompleted);
				this._client.CustomProperty_RetrieveForArtifactTypeCompleted += new EventHandler<CustomProperty_RetrieveForArtifactTypeCompletedEventArgs>(_client_CustomProperty_RetrieveForArtifactTypeCompleted);

				//Fire the connection off here.
				this._client.Connection_Authenticate2Async(this._Project.UserName, this._Project.UserPass, StaticFuncs.getCultureResource.GetString("app_ReportName"));

			}

			return retValue;
		}


		#region Client Events

		//**Initial Data Gathering
		/// <summary>Hit once we've disconnected form the server, all work is done.</summary>
		/// <param name="sender">ImportExporClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void _client_Connection_DisconnectCompleted(object sender, AsyncCompletedEventArgs e)
		{
			const string METHOD = "_client_Connection_DisconnectCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning = 0;
			this._clientNum = 0;
			this._client = null;

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we've successfully connected to the server.</summary>
		/// <param name="sender">ImportExporClient</param>
		/// <param name="e">Connection_Authenticate2CompletedEventArgs</param>
		private void _client_Connection_Authenticate2Completed(object sender, Connection_Authenticate2CompletedEventArgs e)
		{
			const string METHOD = "_client_Connection_Authenticate2Completed()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null && e.Result)
				{
					//Connect to our project.
					this._client.Connection_ConnectToProjectAsync(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ProjectID, this._clientNum++);
					this._clientNumRunning++;
				}
				else
				{
					//TODO: Error logging in. Report error.
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we've completed connecting to the project. </summary>
		/// <param name="sender">ImportExporClient</param>
		/// <param name="e">Connection_ConnectToProjectCompletedEventArgs</param>
		private void _client_Connection_ConnectToProjectCompleted(object sender, Connection_ConnectToProjectCompletedEventArgs e)
		{
			const string METHOD = "_client_Connection_ConnectToProjectCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null && e.Result)
				{
					//Here we need to fire off all data retrieval functions:
					// - Project users.
					this._clientNumRunning++;
					this._client.Project_RetrieveUserMembershipAsync(this._clientNum++);
					// - Incident Statuses, Types, Priorities, Severities
					this._clientNumRunning += 5;
					this._client.Incident_RetrievePrioritiesAsync(this._clientNum++);
					this._client.Incident_RetrieveSeveritiesAsync(this._clientNum++);
					this._client.Incident_RetrieveStatusesAsync(this._clientNum++);
					this._client.Incident_RetrieveTypesAsync(this._clientNum++);
					this._client.CustomProperty_RetrieveForArtifactTypeAsync(3, this._clientNum++);
					// - Available Releases
					this._clientNumRunning++;
					this._client.Release_RetrieveAsync(true, this._clientNum++);
					//Resolutions / Comments
					this._clientNumRunning++;
					this._client.Incident_RetrieveResolutionsAsync(this.ArtifactDetail.ArtifactId, this._clientNum++);
				}
				else
				{
					//TODO: Log an error.
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we're finished getting our project users.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Project_RetrieveUserMembershipCompletedEventArgs</param>
		private void _client_Project_RetrieveUserMembershipCompleted(object sender, Project_RetrieveUserMembershipCompletedEventArgs e)
		{
			const string METHOD = "_client_Project_RetrieveUserMembershipCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this._ProjUsers = e.Result;
					//See if we're ready to get the actual data.
					this.load_IsReadyToGetMainData();
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we're finished getting our project releases.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Release_RetrieveCompletedEventArgs</param>
		private void _client_Release_RetrieveCompleted(object sender, Release_RetrieveCompletedEventArgs e)
		{
			const string METHOD = "_client_Release_RetrieveCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this._ProjReleases = e.Result;
					//See if we're ready to get the actual data.
					this.load_IsReadyToGetMainData();
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we're finished getting our incident types.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveTypesCompletedEventArgs</param>
		private void _client_Incident_RetrieveTypesCompleted(object sender, Incident_RetrieveTypesCompletedEventArgs e)
		{
			const string METHOD = "_client_Incident_RetrieveTypesCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this._IncType = e.Result;
					//See if we're ready to get the actual data.
					this.load_IsReadyToGetMainData();
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we're finished getting our incident statuses.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveStatusesCompletedEventArgs</param>
		private void _client_Incident_RetrieveStatusesCompleted(object sender, Incident_RetrieveStatusesCompletedEventArgs e)
		{
			const string METHOD = "_client_Incident_RetrieveStatusesCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this._IncStatus = e.Result;
					//See if we're ready to get the actual data.
					this.load_IsReadyToGetMainData();
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we're finished getting our incident priorities.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrievePrioritiesCompletedEventArgs</param>
		private void _client_Incident_RetrievePrioritiesCompleted(object sender, Incident_RetrievePrioritiesCompletedEventArgs e)
		{
			const string METHOD = "_client_Incident_RetrievePrioritiesCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (e.Error == null)
			{
				this._IncPriority = e.Result;
				//See if we're ready to get the actual data.
				this.load_IsReadyToGetMainData();
			}
			else
			{
				//TODO: Log error
				this._client.Connection_DisconnectAsync();
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we're finished getting our incident severities.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveSeveritiesCompletedEventArgs</param>
		private void _client_Incident_RetrieveSeveritiesCompleted(object sender, Incident_RetrieveSeveritiesCompletedEventArgs e)
		{
			const string METHOD = "_client_Incident_RetrieveSeveritiesCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this._IncSeverity = e.Result;
					//See if we're ready to get the actual data.
					this.load_IsReadyToGetMainData();
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we're finished getting incident resolutions.</summary>
		/// <param name="sender">IMportExportClient</param>
		/// <param name="e">Incident_RetrieveResolutionsCompletedEventArgs</param>
		private void _client_Incident_RetrieveResolutionsCompleted(object sender, Incident_RetrieveResolutionsCompletedEventArgs e)
		{
			const string METHOD = "_client_Incident_RetrieveResolutionsCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this.loadItem_PopulateDiscussion(e.Result);

					//See if we're ready to get the actual data.
					this.load_IsReadyToGetMainData();
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we're finished getting the main incident details.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveByIdCompletedEventArgs</param>
		private void _client_Incident_RetrieveByIdCompleted(object sender, Incident_RetrieveByIdCompletedEventArgs e)
		{
			const string METHOD = "_client_Incident_RetrieveByIdCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					//Load recorded data..
					this._IncCurrentStatus = e.Result.IncidentStatusId;
					this._IncCurrentType = e.Result.IncidentTypeId;

					//Get workflow steps and fields.
					this._clientNumRunning += 3;
					this._client.Incident_RetrieveWorkflowFieldsAsync(this._Incident.IncidentTypeId.Value, this._Incident.IncidentStatusId.Value, this._clientNum++);
					this._client.Incident_RetrieveWorkflowTransitionsAsync(this._Incident.IncidentTypeId.Value, this._Incident.IncidentStatusId.Value, (this._Incident.OpenerId == this._Project.UserID), (this._Incident.OwnerId == this._Project.UserID), this._clientNum++);
					this._client.Incident_RetrieveWorkflowCustomPropertiesAsync(this._Incident.IncidentTypeId.Value, this._Incident.IncidentStatusId.Value, this._clientNum++);
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when the client is finished getting available workflow transitions.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveWorkflowTransitionsCompletedEventArgs</param>
		private void _client_Incident_RetrieveWorkflowTransitionsCompleted(object sender, Incident_RetrieveWorkflowTransitionsCompletedEventArgs e)
		{
			const string METHOD = "_client_Incident_RetrieveWorkflowTransitionsCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this._IncWkfTransition = e.Result;

					//See if we're ready to get the actual data.
					this.load_IsReadyToDisplayData();
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when the client is finished pulling all the workflow fields and their status.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveWorkflowFieldsCompletedEventArgs</param>
		private void _client_Incident_RetrieveWorkflowFieldsCompleted(object sender, Incident_RetrieveWorkflowFieldsCompletedEventArgs e)
		{
			const string METHOD = "_client_Incident_RetrieveWorkflowFieldsCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this._IncWkfFields_Current = this.load_ScanWorkFlowFields(e.Result);

					//See if we're ready to get the actual data.
					this.load_IsReadyToDisplayData();
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when the client is finished getting custom workflow property fields.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveWorkflowCustomPropertiesCompletedEventArgs</param>
		private void _client_Incident_RetrieveWorkflowCustomPropertiesCompleted(object sender, Incident_RetrieveWorkflowCustomPropertiesCompletedEventArgs e)
		{
			const string METHOD = "_client_Incident_RetrieveWorkflowCustomPropertiesCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this._IncWkfCustom_Current = this.load_ScanWorkFlowCustomFields(e.Result);

					//See if we're ready to get the actual data.
					this.load_IsReadyToDisplayData();
				}
				else
				{
					//TODO: Log error
					this._client.Connection_DisconnectAsync();
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when the client finishes getting the custom field values.</summary>
		/// <param name="sender">ImportExaoprtClient</param>
		/// <param name="e">CustomProperty_RetrieveForArtifactTypeCompletedEventArgs</param>
		private void _client_CustomProperty_RetrieveForArtifactTypeCompleted(object sender, CustomProperty_RetrieveForArtifactTypeCompletedEventArgs e)
		{
			const string METHOD = "_client_CustomProperty_RetrieveForArtifactTypeCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					//Here we need to create the labels.
					//TODO: Get labels for custom properties.
					//Now here we need to create text boxes.
					//TODO: Create our needed textboxes.
					//Now here we need to get our lists.
				}
				else
				{
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		//** Workflow updates.
		/// <summary>Hit when we've successfully connected to the server.</summary>
		/// <param name="sender">ImportExporClient</param>
		/// <param name="e">Connection_Authenticate2CompletedEventArgs</param>
		private void wkfClient_Connection_Authenticate2Completed(object sender, Connection_Authenticate2CompletedEventArgs e)
		{
			const string METHOD = "wkfClient_Connection_Authenticate2Completed()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER.");

			if (sender is ImportExportClient)
			{
				ImportExportClient client = sender as ImportExportClient;

				if (!e.Cancelled)
				{
					if (e.Error == null && e.Result)
					{
						//Connect to our project.
						client.Connection_ConnectToProjectAsync(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ProjectID, this._clientNum++);
					}
					else
					{
						//TODO: Log error.
					}
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we've completed connecting to the project. </summary>
		/// <param name="sender">ImportExporClient</param>
		/// <param name="e">Connection_ConnectToProjectCompletedEventArgs</param>
		private void wkfClient_Connection_ConnectToProjectCompleted(object sender, Connection_ConnectToProjectCompletedEventArgs e)
		{
			const string METHOD = "wkfClient_Connection_ConnectToProjectCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER.");

			if (sender is ImportExportClient)
			{
				ImportExportClient client = sender as ImportExportClient;

				if (!e.Cancelled)
				{
					if (e.Error == null && e.Result)
					{
						//Get the current workflow fields here.
						client.Incident_RetrieveWorkflowCustomPropertiesAsync(((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.Value, ((RemoteIncidentStatus)this.cntrlStatus.SelectedItem).IncidentStatusId.Value, this._clientNum++);
						client.Incident_RetrieveWorkflowFieldsAsync(((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.Value, ((RemoteIncidentStatus)this.cntrlStatus.SelectedItem).IncidentStatusId.Value, this._clientNum++);
					}
					else
					{
						//TODO: Log an error.
					}
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when the client is finished pulling all the workflow fields and their status.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveWorkflowFieldsCompletedEventArgs</param>
		private void wkfClient_Incident_RetrieveWorkflowFieldsCompleted(object sender, Incident_RetrieveWorkflowFieldsCompletedEventArgs e)
		{
			const string METHOD = "wkfClient_Incident_RetrieveWorkflowFieldsCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER.");

			if (sender is ImportExportClient)
			{
				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						this._IncWkfFields_Updated = this.load_ScanWorkFlowFields(e.Result);

						//Update main workflow fields.
						this.loadItem_SetEnabledFields(this._IncWkfFields_Updated);
					}
					else
					{
						//TODO: Log error.
					}
				}
			}
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when the client is finished getting custom workflow property fields.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_RetrieveWorkflowCustomPropertiesCompletedEventArgs</param>
		private void wkfClient_Incident_RetrieveWorkflowCustomPropertiesCompleted(object sender, Incident_RetrieveWorkflowCustomPropertiesCompletedEventArgs e)
		{
			const string METHOD = "wkfClient_Incident_RetrieveWorkflowCustomPropertiesCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER.");

			if (sender is ImportExportClient)
			{
				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						this._IncWkfCustom_Updated = this.load_ScanWorkFlowCustomFields(e.Result);

						//Update custom workflow fields.
						//TODO: Create function to enable/mark custom fields.
						//this.loadItem_SetEnabledFields(this._IncWkfFields_Updated);
					}
					else
					{
						//TODO: Log error
					}
				}
			}
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		#endregion

		/// <summary>Checks to make sure that it is okay to go get the main Incident data.
		/// Called after all Async calls except workflow items.
		/// </summary>
		private void load_IsReadyToGetMainData()
		{
			if (this._clientNumRunning == 0)
			{
				//No clients are currently running, we can get the main data now.
				this._client.Incident_RetrieveByIdAsync(this.ArtifactDetail.ArtifactId, this._clientNum++);
				this._clientNumRunning++;
			}
		}

		/// <summary>Checks to see if it's okay to start loading form data.
		/// Called after the two Workflow data retrieval Asyncs.
		/// </summary>
		private void load_IsReadyToDisplayData()
		{
			if (this._clientNumRunning == 0)
			{
				this.loadItem_DisplayInformation(this._Incident);

				//Set Workflow Data. (To disable Fields)
				this.loadItem_SetEnabledFields(this._IncWkfFields_Current);
				//TODO: Create function to enable/mark custom fields.
			}
		}

		/// <summary>Scans the result from a RetrieveWorkflowField call and add the fields into a useable dictionary.</summary>
		/// <param name="workFields">List of RemoteWorkflowIncidentFields</param>
		/// <returns>Dictionary of Field and Status</returns>
		private Dictionary<int, int> load_ScanWorkFlowFields(List<RemoteWorkflowIncidentFields> workFields)
		{
			try
			{
				Dictionary<int, int> retList = new Dictionary<int, int>();
				foreach (RemoteWorkflowIncidentFields Field in workFields)
				{
					if (retList.ContainsKey(Field.FieldId))
					{
						if (Field.FieldStateId > retList[Field.FieldId])
							retList[Field.FieldId] = Field.FieldStateId;
					}
					else
					{
						retList.Add(Field.FieldId, Field.FieldStateId);
					}
				}

				return retList;
			}
			catch (Exception ex)
			{
				//TODO: Log error.
				return new Dictionary<int, int>();
			}
		}

		/// <summary>Scans the result from a RetrieveWorkflowCustomField call and add the fields into a useable dictionary.</summary>
		/// <param name="workFields">List of RemoteWorkflowIncidentFields</param>
		/// <returns>Dictionary of Field and Status</returns>
		private Dictionary<int, int> load_ScanWorkFlowCustomFields(List<RemoteWorkflowIncidentCustomProperties> workCustomFields)
		{
			try
			{
				Dictionary<int, int> retList = new Dictionary<int, int>();
				foreach (RemoteWorkflowIncidentCustomProperties Field in workCustomFields)
				{
					if (retList.ContainsKey(Field.CustomPropertyId))
					{
						if (Field.FieldStateId > retList[Field.CustomPropertyId])
							retList[Field.CustomPropertyId] = Field.FieldStateId;
					}
					else
					{
						retList.Add(Field.CustomPropertyId, Field.FieldStateId);
					}
				}

				return retList;
			}
			catch (Exception ex)
			{
				//TODO: Log error.
				return new Dictionary<int, int>();
			}
		}

		#region Form Load Functions
		/// <summary>Loads the list of available users.</summary>
		/// <param name="box">The ComboBox to load users from.</param>
		/// <param name="SelectedUserID">The user to select.</param>
		private void loadItem_PopulateUser(ComboBox box, int? SelectedUserID)
		{
			try
			{
				//Clear and add our 'none'.
				box.Items.Clear();
				box.SelectedIndex = box.Items.Add(new ComboBoxItem() { Content = StaticFuncs.getCultureResource.GetString("app_General_None") });

				if (!SelectedUserID.HasValue)
					SelectedUserID = -1;

				//Load the project users.
				foreach (Business.SpiraTeam_Client.RemoteProjectUser projUser in this._ProjUsers)
				{
					int numAdded = box.Items.Add(projUser);
					if (projUser.UserId == SelectedUserID)
					{
						box.SelectedIndex = numAdded;
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateUser", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Loads the list of available releases into the specified ComboBox.</summary>
		/// <param name="box">The ComboBox to load users from.</param>
		/// <param name="SelectedUserID">The releaseId to select.</param>
		private void loadItem_PopulateReleaseControl(ComboBox box, int? SelectedRelease)
		{
			try
			{
				//Clear and add our 'none'.
				box.Items.Clear();
				box.SelectedIndex = box.Items.Add(new ComboBoxItem() { Content = StaticFuncs.getCultureResource.GetString("app_General_None") });

				if (!SelectedRelease.HasValue)
					SelectedRelease = -1;

				foreach (Business.SpiraTeam_Client.RemoteRelease Release in this._ProjReleases)
				{
					int numAdded = box.Items.Add(Release);
					if (Release.ReleaseId == SelectedRelease)
					{
						box.SelectedIndex = numAdded;
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateReleaseControl", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Loads the list of available severities into the specified ComboBox.</summary>
		/// <param name="box">The ComboBox to load severities into.</param>
		/// <param name="SelectedUserID">The SeverityId to select.</param>
		private void loadItem_PopulateSeverity(ComboBox box, int? SelectedItem)
		{
			try
			{
				//Clear and add our 'none'.
				box.Items.Clear();
				box.SelectedIndex = box.Items.Add(new Business.SpiraTeam_Client.RemoteIncidentSeverity() { Name = StaticFuncs.getCultureResource.GetString("app_General_None") });

				if (!SelectedItem.HasValue)
					SelectedItem = -1;

				foreach (Business.SpiraTeam_Client.RemoteIncidentSeverity Severity in this._IncSeverity)
				{
					int nunAdded = box.Items.Add(Severity);
					if (Severity.SeverityId == SelectedItem)
					{
						box.SelectedIndex = nunAdded;
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateSeverity", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Loads the list of available priorities into the specified ComboBox.</summary>
		/// <param name="box">The ComboBox to load priorities into.</param>
		/// <param name="SelectedUserID">The PriorityId to select.</param>
		private void loadItem_PopulatePriority(ComboBox box, int? SelectedItem)
		{
			try
			{
				//Clear and add our 'none'.
				box.Items.Clear();
				box.SelectedIndex = box.Items.Add(new Business.SpiraTeam_Client.RemoteIncidentPriority() { Name = "-- None --" });

				if (!SelectedItem.HasValue)
					SelectedItem = -1;

				foreach (Business.SpiraTeam_Client.RemoteIncidentPriority Priority in this._IncPriority)
				{
					int nunAdded = box.Items.Add(Priority);
					if (Priority.PriorityId == SelectedItem)
						box.SelectedIndex = nunAdded;
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulatePriority", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Loads the list of available types into the specified ComboBox.</summary>
		/// <param name="box">The ComboBox to load types into.</param>
		/// <param name="SelectedUserID">The TypeId to select.</param>
		private void loadItem_PopulateType(ComboBox box, int? SelectedItem)
		{
			try
			{
				box.Items.Clear();

				if (!SelectedItem.HasValue)
					SelectedItem = -1;

				foreach (Business.SpiraTeam_Client.RemoteIncidentType Type in this._IncType)
				{
					int numAdded = box.Items.Add(Type);
					if (SelectedItem == Type.IncidentTypeId)
						box.SelectedIndex = numAdded;
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateType", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Loads the list of available statuses and workflow transitions into the specified ComboBox.</summary>
		/// <param name="box">The ComboBox to load statuses into.</param>
		/// <param name="SelectedUserID">The StatusId to select.</param>
		private void loadItem_PopulateStatus(ComboBox box, int? SelectedItem)
		{
			//Loop through all the available ones. We only add the ones that are in the 
			//  workflow transition, or the current status, making sure the current
			//  one is selected.
			try
			{
				//Clear items already there.
				box.Items.Clear();

				//Load ones that are available.
				foreach (Business.SpiraTeam_Client.RemoteIncidentStatus Status in this._IncStatus)
				{
					if (Status.IncidentStatusId == SelectedItem)
					{
						int numAdded = box.Items.Add(Status);
						box.SelectedIndex = numAdded;
					}
					else
					{
						//Loop through available transitions. If this status is available, add it.
						foreach (Business.SpiraTeam_Client.RemoteWorkflowIncidentTransition Transition in this._IncWkfTransition)
						{
							if (Transition.IncidentStatusId_Output == Status.IncidentStatusId)
							{
								Transition.Name = "» " + Transition.Name;
								box.Items.Add(Transition);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateStatus", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Loads the listed discussions into the discussion control.</summary>
		/// <param name="Discussions">The list of COmments to add.</param>
		private void loadItem_PopulateDiscussion(List<RemoteIncidentResolution> Discussions)
		{
			try
			{
				//Erase ones in there.
				this.cntrlDiscussion.Children.Clear();

				if (Discussions.Count < 1)
					this.cntrlDiscussion.Children.Add(new cntlDiscussionFrame("No comments for this item.", ""));
				else
				{
					foreach (Business.SpiraTeam_Client.RemoteIncidentResolution Resolution in Discussions)
					{
						string header = Resolution.CreatorName + " [" + Resolution.CreationDate.ToShortDateString() + " " + Resolution.CreationDate.ToShortTimeString() + "]";
						this.cntrlDiscussion.Children.Add(new cntlDiscussionFrame(header, Resolution.Resolution));
					}
				}

				//Clear the entry box.
				this.cntrlResolution.HTMLText = "";
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateDiscussion", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}
		#endregion


















		#region Field Population

		/// <summary>Set the enabled and required fields for the current stage in the workflow.</summary>
		/// <param name="WorkFlowFields">The Dictionary of Workflow Fields</param>
		private void loadItem_SetEnabledFields(Dictionary<int, int> WorkFlowFields)
		{
			try
			{
				if (WorkFlowFields == null)
					WorkFlowFields = new Dictionary<int, int>();

				// ** Set enabled/disabled fields.
				//Standard fields.
				this.cntrlIncidentName.IsEnabled = (WorkFlowFields.ContainsKey(10));
				this.cntrlType.IsEnabled = (WorkFlowFields.ContainsKey(4));
				this.cntrlDetectedBy.IsEnabled = (WorkFlowFields.ContainsKey(5));
				this.cntrlOwnedBy.IsEnabled = (WorkFlowFields.ContainsKey(6));
				this.cntrlPriority.IsEnabled = (WorkFlowFields.ContainsKey(2));
				this.cntrlSeverity.IsEnabled = (WorkFlowFields.ContainsKey(1));
				this.cntrlDetectedIn.IsEnabled = (WorkFlowFields.ContainsKey(7));
				this.cntrlResolvedIn.IsEnabled = (WorkFlowFields.ContainsKey(8));
				this.cntrlVerifiedIn.IsEnabled = (WorkFlowFields.ContainsKey(9));
				this.cntrlDescription.IsToolbarVisible = (WorkFlowFields.ContainsKey(11));
				this.cntrlDescription.IsEnabled = (WorkFlowFields.ContainsKey(11));
				this.cntrlResolution.Visibility = ((WorkFlowFields.ContainsKey(12)) ? Visibility.Visible : Visibility.Collapsed);

				//Schedule fields.
				this.cntrlStartDate.IsEnabled = (WorkFlowFields.ContainsKey(45));
				this.cntrlEndDate.IsEnabled = (WorkFlowFields.ContainsKey(14));
				this.cntrlPerComplete.IsEnabled = (WorkFlowFields.ContainsKey(46)); //Not workflow configurable.
				this.cntrlEstEffortH.IsEnabled = this.cntrlEstEffortM.IsEnabled = (WorkFlowFields.ContainsKey(47));
				this.cntrlActEffortH.IsEnabled = this.cntrlActEffortM.IsEnabled = (WorkFlowFields.ContainsKey(48));

				// ** Set required fields.
				this.lblType.FontWeight = ((this.workflow_IsFieldRequired(4, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblDetectedBy.FontWeight = ((this.workflow_IsFieldRequired(5, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblOwnedBy.FontWeight = ((this.workflow_IsFieldRequired(6, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblPriority.FontWeight = ((this.workflow_IsFieldRequired(2, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblSeverity.FontWeight = ((this.workflow_IsFieldRequired(1, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblDetectedIn.FontWeight = ((this.workflow_IsFieldRequired(7, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblResolvedIn.FontWeight = ((this.workflow_IsFieldRequired(8, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblVerifiedIn.FontWeight = ((this.workflow_IsFieldRequired(9, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.cntrlDescription.Tag = (this.workflow_IsFieldRequired(11, WorkFlowFields));
				//this.cntrlResolution.Tag = (this.workflow_IsFieldRequired(12, WorkFlowFields));
				// lblDescription
				// lblResolution
				//Schedule fields.
				this.lblStartDate.FontWeight = ((this.workflow_IsFieldRequired(45, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblEndDate.FontWeight = ((this.workflow_IsFieldRequired(14, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblEstEffort.FontWeight = ((this.workflow_IsFieldRequired(47, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);
				this.lblActEffort.FontWeight = ((this.workflow_IsFieldRequired(48, WorkFlowFields)) ? FontWeights.Bold : FontWeights.Normal);

			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_SetEnabledFields", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		#endregion

		/// <summary>Load the specified incident into the data fields.</summary>
		/// <param name="incident">The incident details to load into fields.</param>
		private void loadItem_DisplayInformation(Business.SpiraTeam_Client.RemoteIncident incident)
		{
			try
			{
				//Load information:
				// - Name
				this.cntrlIncidentName.Text = incident.Name;
				// - Users
				this.loadItem_PopulateUser(this.cntrlDetectedBy, incident.OpenerId);
				this.loadItem_PopulateUser(this.cntrlOwnedBy, incident.OwnerId);
				((ComboBoxItem)this.cntrlDetectedBy.Items[0]).IsEnabled = false;
				// - Releases
				this.loadItem_PopulateReleaseControl(this.cntrlDetectedIn, incident.DetectedReleaseId);
				this.loadItem_PopulateReleaseControl(this.cntrlResolvedIn, incident.ResolvedReleaseId);
				this.loadItem_PopulateReleaseControl(this.cntrlVerifiedIn, incident.VerifiedReleaseId);
				// - Priority & Severity
				this.loadItem_PopulatePriority(this.cntrlPriority, incident.PriorityId);
				this.loadItem_PopulateSeverity(this.cntrlSeverity, incident.SeverityId);
				// - Type & Status
				this.loadItem_PopulateType(this.cntrlType, incident.IncidentTypeId);
				this.loadItem_PopulateStatus(this.cntrlStatus, incident.IncidentStatusId);
				// - Description
				this.cntrlDescription.HTMLText = incident.Description;
				// - History
				//TODO: History (need API update)
				// - Schedule
				this.cntrlStartDate.SelectedDate = incident.StartDate;
				this.cntrlEndDate.SelectedDate = incident.ClosedDate;
				this.cntrlPerComplete.Text = incident.CompletionPercent.ToString();
				this.cntrlEstEffortH.Text = ((incident.EstimatedEffort.HasValue) ? Math.Floor(((double)incident.EstimatedEffort / (double)60)).ToString() : "");
				this.cntrlEstEffortM.Text = ((incident.EstimatedEffort.HasValue) ? ((double)incident.EstimatedEffort % (double)60).ToString() : "");
				this.cntrlActEffortH.Text = ((incident.ActualEffort.HasValue) ? Math.Floor(((double)incident.ActualEffort / (double)60)).ToString() : "");
				this.cntrlActEffortM.Text = ((incident.ActualEffort.HasValue) ? ((double)incident.ActualEffort % (double)60).ToString() : "");
				// - Custom Properties
				//TODO: Custom Props (need API update on workflow for field statuses)
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_displayInformation", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private bool workflow_IsFieldRequired(int FieldID, Dictionary<int, int> WorkFlow)
		{
			if (WorkFlow.ContainsKey(FieldID) && WorkFlow[FieldID] == 2)
			{
				return true;
			}
			return false;
		}

		private bool workflow_CheckFieldValues(out string Fields)
		{
			Fields = "";

			if (string.IsNullOrEmpty(this.cntrlIncidentName.Text.Trim()))
				Fields += "Name;";
			if (this.lblType.FontWeight == FontWeights.Bold && this.cntrlType.SelectedItem.GetType() != typeof(Business.SpiraTeam_Client.RemoteIncidentType))
				Fields += "Type;";
			if (this.lblStatus.FontWeight == FontWeights.Bold && this.cntrlStatus.SelectedItem.GetType() != typeof(Business.SpiraTeam_Client.RemoteIncidentStatus))
				Fields += "Status;";
			if (this.lblDetectedBy.FontWeight == FontWeights.Bold && this.cntrlDetectedBy.SelectedItem.GetType() != typeof(Business.SpiraTeam_Client.RemoteUser))
				Fields += "Detected By;";
			if (this.lblOwnedBy.FontWeight == FontWeights.Bold && this.cntrlOwnedBy.SelectedItem.GetType() != typeof(Business.SpiraTeam_Client.RemoteUser))
				Fields += "Owned By;";
			if (this.lblPriority.FontWeight == FontWeights.Bold && !((Business.SpiraTeam_Client.RemoteIncidentPriority)this.cntrlPriority.SelectedItem).PriorityId.HasValue)
				Fields += "Priority;";
			if (this.lblSeverity.FontWeight == FontWeights.Bold && !((Business.SpiraTeam_Client.RemoteIncidentSeverity)this.cntrlSeverity.SelectedItem).SeverityId.HasValue)
				Fields += "Severity;";
			if (this.lblDetectedIn.FontWeight == FontWeights.Bold && this.cntrlDetectedIn.SelectedItem.GetType() != typeof(Business.SpiraTeam_Client.RemoteRelease))
				Fields += "Detected In;";
			if (this.lblResolvedIn.FontWeight == FontWeights.Bold && this.cntrlResolvedIn.SelectedItem.GetType() != typeof(Business.SpiraTeam_Client.RemoteRelease))
				Fields += "Resolved In;";
			if (this.lblVerifiedIn.FontWeight == FontWeights.Bold && this.cntrlVerifiedIn.SelectedItem.GetType() != typeof(Business.SpiraTeam_Client.RemoteRelease))
				Fields += "Verified In;";
			if (this.lblStartDate.FontWeight == FontWeights.Bold && !this.cntrlStartDate.SelectedDate.HasValue)
				Fields += "Start Date;";
			if (this.lblEndDate.FontWeight == FontWeights.Bold && !this.cntrlEndDate.SelectedDate.HasValue)
				Fields += "End Date;";
			if (this.lblEstEffort.FontWeight == FontWeights.Bold && string.IsNullOrEmpty(this.cntrlEstEffortH.Text.Trim()) && string.IsNullOrEmpty(this.cntrlEstEffortH.Text.Trim()))
				Fields += "Estimated Effort;";
			if (this.lblActEffort.FontWeight == FontWeights.Bold && string.IsNullOrEmpty(this.cntrlActEffortH.Text.Trim()) && string.IsNullOrEmpty(this.cntrlActEffortH.Text.Trim()))
				Fields += "Estimated Effort;";
			//if (this.cntrlResolution.Tag.GetType() == typeof(bool) && (bool)this.cntrlResolution.Tag && !this._isResChanged)
			//     Fields += "Resolution;";

			if (string.IsNullOrEmpty(Fields.Trim()))
			{
				return true;
			}
			else
			{
				Fields = Fields.Trim(';').Trim();
				return false;
			}
		}
	}
}
