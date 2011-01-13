using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls;
using System.Windows.Documents;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.HTMLandXAML;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	public partial class frmDetailsIncident : UserControl
	{
		#region Client Values
		private ImportExportClient _client;
		private int _clientNumRunning; //Holds the number current executing.
		private int _clientNum; //Holds the total amount. Needed to multiple ASYNC() calls.
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
		private List<RemoteDocument> _IncDocuments;
		private List<RemoteWorkflowIncidentTransition> _IncWkfTransition;
		private string _IncDocumentsUrl;

		//Workflow fields..
		private Dictionary<int, int> _IncWkfFields_Current;
		private Dictionary<int, int> _IncWkfCustom_Current;
		private Dictionary<int, int> _IncWkfFields_Updated;
		private Dictionary<int, int> _IncWkfCustom_Updated;
		private int? _IncCurrentType;
		private int? _IncCurrentStatus;
		private int? _IncSelectedStatus;
		private int? _IncSelectedType;
		#endregion

		// Are we in read-only mode? Are we saving?
		private bool isInSaveMode = false;
		private bool isInConcMode = false;

		/// <summary>Loads the item currently assigned to the ArtifactDetail property.</summary>
		/// <returns>Boolean on whether of not load was started successfully.</returns>
		private bool load_LoadItem()
		{
			bool retValue = false;
			if (this.ArtifactDetail != null)
			{
				//Set flag.
				this.IsLoading = true;
				this.barLoadingIncident.Value = 0;

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
				this._client.Document_RetrieveForArtifactCompleted += new EventHandler<Document_RetrieveForArtifactCompletedEventArgs>(_client_Document_RetrieveForArtifactCompleted);
				this._client.Release_RetrieveCompleted += new EventHandler<Release_RetrieveCompletedEventArgs>(_client_Release_RetrieveCompleted);
				this._client.Project_RetrieveUserMembershipCompleted += new EventHandler<Project_RetrieveUserMembershipCompletedEventArgs>(_client_Project_RetrieveUserMembershipCompleted);
				this._client.System_GetArtifactUrlCompleted += new EventHandler<System_GetArtifactUrlCompletedEventArgs>(_client_System_GetArtifactUrlCompleted);
				this._client.CustomProperty_RetrieveForArtifactTypeCompleted += new EventHandler<CustomProperty_RetrieveForArtifactTypeCompletedEventArgs>(_client_CustomProperty_RetrieveForArtifactTypeCompleted);

				//Fire the connection off here.
				this._clientNumRunning++;
				this.barLoadingIncident.Maximum = 17;
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
			this.barLoadingIncident.Value++;

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
					if (e.Error != null)
					{
						Logger.LogMessage(e.Error);
					}
					else
					{
						Logger.LogMessage("Could not log in!");
					}
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
			this.barLoadingIncident.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null && e.Result)
				{
					this._clientNumRunning += 8;
					//Here we need to fire off all data retrieval functions:
					// - Project users.
					this._client.Project_RetrieveUserMembershipAsync(this._clientNum++);
					// - Incident Statuses, Types, Priorities, Severities
					this._client.Incident_RetrievePrioritiesAsync(this._clientNum++);
					this._client.Incident_RetrieveSeveritiesAsync(this._clientNum++);
					this._client.Incident_RetrieveStatusesAsync(this._clientNum++);
					this._client.Incident_RetrieveTypesAsync(this._clientNum++);
					this._client.CustomProperty_RetrieveForArtifactTypeAsync(3, this._clientNum++);
					// - Available Releases
					this._client.Release_RetrieveAsync(true, this._clientNum++);
					// - Resolutions / Comments
					this._client.Incident_RetrieveResolutionsAsync(this.ArtifactDetail.ArtifactId, this._clientNum++);
					// - System URL
					this._client.System_GetArtifactUrlAsync(-14, this._Project.ProjectID, -2, null, this._clientNum++);
				}
				else
				{
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

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
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

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
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

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
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

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
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

			if (e.Error == null)
			{
				this._IncPriority = e.Result;
				//See if we're ready to get the actual data.
				this.load_IsReadyToGetMainData();
			}
			else
			{
				Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

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
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

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
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					//Load recorded data..
					this._IncCurrentStatus = e.Result.IncidentStatusId;
					this._IncCurrentType = e.Result.IncidentTypeId;
					this._Incident = e.Result;

					//Get workflow steps and fields.
					this._clientNumRunning += 4;
					this._client.Incident_RetrieveWorkflowFieldsAsync(this._Incident.IncidentTypeId.Value, this._Incident.IncidentStatusId.Value, this._clientNum++);
					this._client.Incident_RetrieveWorkflowTransitionsAsync(this._Incident.IncidentTypeId.Value, this._Incident.IncidentStatusId.Value, (this._Incident.OpenerId == this._Project.UserID), (this._Incident.OwnerId == this._Project.UserID), this._clientNum++);
					this._client.Incident_RetrieveWorkflowCustomPropertiesAsync(this._Incident.IncidentTypeId.Value, this._Incident.IncidentStatusId.Value, this._clientNum++);
					this._client.Document_RetrieveForArtifactAsync(3, this._Incident.IncidentId.Value, new List<RemoteFilter>(), new RemoteSort(), this._clientNum++);
				}
				else
				{
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

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
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

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
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

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
					Logger.LogMessage(e.Error);
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
			this.barLoadingIncident.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					//Here create the grid to hold the data.
					this.gridCustomProperties.Children.Clear();
					this.gridCustomProperties.RowDefinitions.Clear();
					for (int i = 0; i < Math.Ceiling(e.Result.Count / 2D); i++)
						this.gridCustomProperties.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

					//Here, create the contols..
					bool IsOnFirst = true;
					for (int j = 0; j < e.Result.Count; j++)
					{
						//** The label first.
						TextBlock lblCustProp = new TextBlock();
						lblCustProp.Text = e.Result[j].Alias + ":";
						lblCustProp.Style = (Style)this.FindResource("PaddedLabel");
						lblCustProp.VerticalAlignment = System.Windows.VerticalAlignment.Top;

						//Add it to the row/column.
						Grid.SetColumn(lblCustProp, ((IsOnFirst) ? 0 : 3));
						Grid.SetRow(lblCustProp, (int)Math.Floor(j / 2D));
						//Add it to the grid.
						this.gridCustomProperties.Children.Add(lblCustProp);

						//** Now the control.
						Control custControl = null;
						if (e.Result[j].CustomPropertyTypeId == 1) //Text field.
						{
							TextBox txtControl = new TextBox();
							txtControl.AcceptsReturn = true;
							txtControl.AcceptsTab = true;
							txtControl.MaxLines = 2;
							txtControl.MinLines = 2;
							txtControl.TextChanged += new TextChangedEventHandler(_cntrl_TextChanged);
							custControl = txtControl;
						}
						else if (e.Result[j].CustomPropertyTypeId == 2) //List field.
						{
							ComboBox lsbControl = new ComboBox();
							lsbControl.SelectedValuePath = "Key";
							lsbControl.DisplayMemberPath = "Value";
							lsbControl.SelectionChanged += new SelectionChangedEventHandler(_cntrl_TextChanged);

							//Load selectable items.
							lsbControl.Items.Add(new KeyValuePair<int, string>(-1, ""));
							foreach (RemoteCustomListValue list in e.Result[j].CustomList.Values)
							{
								KeyValuePair<int, string> item = new KeyValuePair<int, string>(list.CustomPropertyValueId.Value, list.Name);
								lsbControl.Items.Add(item);
							}

							custControl = lsbControl;
						}
						custControl.Style = (Style)this.FindResource("PaddedDropdown");
						custControl.Tag = e.Result[j];
						custControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
						custControl.VerticalAlignment = System.Windows.VerticalAlignment.Top;
						//Add it to the row/column.
						Grid.SetColumn(custControl, ((IsOnFirst) ? 1 : 4));
						Grid.SetRow(custControl, (int)Math.Floor(j / 2D));
						//Add it to the grid.
						this.gridCustomProperties.Children.Add(custControl);

						//Create link between label and control. (For setting enabled/required)
						lblCustProp.Tag = custControl;
						//Flip the IsOnFirst..
						IsOnFirst = !IsOnFirst;
					}

					//HACK: See if we're ready to get the actual data.
					this.load_IsReadyToGetMainData();
				}
				else
				{
					Logger.LogMessage(e.Error);
				}
			}

			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when we're finished getting the attached documents for the artifact.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Document_RetrieveForArtifactCompletedEventArgs</param>
		private void _client_Document_RetrieveForArtifactCompleted(object sender, Document_RetrieveForArtifactCompletedEventArgs e)
		{
			const string METHOD = "_client_Document_RetrieveForArtifactCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;
			this.barLoadingIncident.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					//Get the results into our variable.
					this._IncDocuments = e.Result;
					//We won't load them into display until the other information is displayed.

					this.load_IsReadyToGetMainData();
				}
				else
				{
					Logger.LogMessage(e.Error);
				}
			}
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
		}

		/// <summary>Hit when the client returns with our needed URL</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">System_GetArtifactUrlCompletedEventArgs</param>
		private void _client_System_GetArtifactUrlCompleted(object sender, System_GetArtifactUrlCompletedEventArgs e)
		{
			const string METHOD = "_client_System_GetArtifactUrlCompleted()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());

			this._clientNumRunning--;
			this.barLoadingIncident.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					this._IncDocumentsUrl = e.Result;

					this.load_IsReadyToGetMainData();
				}
				else
				{
					Logger.LogMessage(e.Error);
				}
			}
		}

		//** Workflow updates.
		/// <summary>Hit when we've successfully connected to the server.</summary>
		/// <param name="sender">ImportExporClient</param>
		/// <param name="e">Connection_Authenticate2CompletedEventArgs</param>
		private void wkfClient_Connection_Authenticate2Completed(object sender, Connection_Authenticate2CompletedEventArgs e)
		{
			const string METHOD = "wkfClient_Connection_Authenticate2Completed()";
			System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " ENTER.");

			this._clientNumRunning--;
			this.barLoadingIncident.Value++;

			if (sender is ImportExportClient)
			{
				ImportExportClient client = sender as ImportExportClient;

				if (!e.Cancelled)
				{
					if (e.Error == null && e.Result)
					{
						//Connect to our project.
						this._clientNumRunning++;
						client.Connection_ConnectToProjectAsync(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ProjectID, this._clientNum++);
					}
					else
					{
						if (e.Error != null)
						{
							Logger.LogMessage(e.Error);
						}
						else
						{
							Logger.LogMessage("Could not log in.", System.Diagnostics.EventLogEntryType.Error);
						}
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

			this._clientNumRunning--;
			this.barLoadingIncident.Value++;

			if (sender is ImportExportClient)
			{
				ImportExportClient client = sender as ImportExportClient;

				if (!e.Cancelled)
				{
					if (e.Error == null && e.Result)
					{
						//Get the current status/type..
						int intStatus = ((this._IncSelectedStatus.HasValue) ? this._IncSelectedStatus.Value : this._IncCurrentStatus.Value);
						int intType = ((this._IncSelectedType.HasValue) ? this._IncSelectedType.Value : this._IncCurrentType.Value);
						//Get the current workflow fields here.
						this._clientNumRunning += 2;
						client.Incident_RetrieveWorkflowCustomPropertiesAsync(intType, intStatus, this._clientNum++);
						client.Incident_RetrieveWorkflowFieldsAsync(intType, intStatus, this._clientNum++);
					}
					else
					{
						if (e.Error != null)
						{
							Logger.LogMessage(e.Error);
						}
						else
						{
							Logger.LogMessage("Could not log in.", System.Diagnostics.EventLogEntryType.Error);
						}
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

			this._clientNumRunning--;
			this.barLoadingIncident.Value++;

			if (sender is ImportExportClient)
			{
				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						this._IncWkfFields_Updated = this.load_ScanWorkFlowFields(e.Result);

						//Update main workflow fields.
						this.workflow_SetEnabledFields(this._IncWkfFields_Updated);

						//Hide the status if needed.
						if (this._clientNumRunning == 0)
							this.display_SetStatusWindow(Visibility.Hidden);
					}
					else
					{
						Logger.LogMessage(e.Error);
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

			this._clientNumRunning--;
			this.barLoadingIncident.Value++;

			if (sender is ImportExportClient)
			{
				if (!e.Cancelled)
				{
					if (e.Error == null)
					{
						this._IncWkfCustom_Updated = this.load_ScanWorkFlowCustomFields(e.Result);

						//Update custom workflow fields.
						this.workflow_SetEnabledCustomFields(this._IncWkfCustom_Updated);

						//Hide the status if needed.
						if (this._clientNumRunning == 0)
							this.display_SetStatusWindow(Visibility.Hidden);
					}
					else
					{
						Logger.LogMessage(e.Error);
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
			Logger.LogTrace("load_IsReadyToGetMainData: Clients Running " + this._clientNumRunning.ToString());

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
			Logger.LogTrace("load_IsReadyToDisplayData: Clients Running " + this._clientNumRunning.ToString());

			if (this._clientNumRunning == 0)
			{
				this.loadItem_DisplayInformation(this._Incident);

				//Set Workflow Data. (To disable Fields)
				this.workflow_SetEnabledFields(this._IncWkfFields_Current);
				this.workflow_SetEnabledCustomFields(this._IncWkfCustom_Current);
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
				Logger.LogMessage(ex);
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
				Logger.LogMessage(ex);
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
		private void loadItem_PopulateStatus(MenuItem menu, int? SelectedItem)
		{
			//Loop through all the available ones. We only add the ones that are in the 
			//  workflow transition, or the current status, making sure the current
			//  one is selected.
			try
			{
				//Clear items already there, add the null item.
				menu.Items.Clear();

				//Load ones that are available.
				foreach (Business.SpiraTeam_Client.RemoteIncidentStatus Status in this._IncStatus)
				{
					if (Status.IncidentStatusId == SelectedItem)
					{
						//Display the current status in the label..
						this.cntrlIncidentStatus.Text = Status.Name;
					}
					else
					{
						//Loop through available transitions. If this status is available, add it.
						foreach (Business.SpiraTeam_Client.RemoteWorkflowIncidentTransition Transition in this._IncWkfTransition)
						{
							if (Transition.IncidentStatusId_Output == Status.IncidentStatusId)
							{
								if (!Transition.Name.Trim().StartsWith("»")) Transition.Name = "» " + Transition.Name;
								menu.Items.Add(Transition);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex);
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

		/// <summary>Called when the user changes the workflow step, pulls enabled/required fields.</summary>
		private void workflow_ChangeWorkflowStep()
		{
			//This is a potentially different workflow, so create the client to go out and get fields.
			ImportExportClient wkfClient = StaticFuncs.CreateClient(this._Project.ServerURL.ToString());
			wkfClient.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(wkfClient_Connection_Authenticate2Completed);
			wkfClient.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(wkfClient_Connection_ConnectToProjectCompleted);
			wkfClient.Incident_RetrieveWorkflowFieldsCompleted += new EventHandler<Incident_RetrieveWorkflowFieldsCompletedEventArgs>(wkfClient_Incident_RetrieveWorkflowFieldsCompleted);
			wkfClient.Incident_RetrieveWorkflowCustomPropertiesCompleted += new EventHandler<Incident_RetrieveWorkflowCustomPropertiesCompletedEventArgs>(wkfClient_Incident_RetrieveWorkflowCustomPropertiesCompleted);

			//Connect.
			this._clientNumRunning = 1;
			wkfClient.Connection_Authenticate2Async(this._Project.UserName, this._Project.UserPass, StaticFuncs.getCultureResource.GetString("app_ReportName"));
		}
















		#region Field Workflow Status

		/// <summary>Set the enabled and required fields for the current stage in the workflow.</summary>
		/// <param name="WorkFlowFields">The Dictionary of Workflow Fields</param>
		private void workflow_SetEnabledFields(Dictionary<int, int> WorkFlowFields)
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
				//Connect.logEventMessage("wpfDetailsIncident::workflow_SetEnabledFields", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Set the enabled and required custom fields for the current stage in the workflow.</summary>
		/// <param name="WorkFlowFields">The Dictionary of Workflow Custom Fields</param>
		private void workflow_SetEnabledCustomFields(Dictionary<int, int> CustomFields)
		{
			//We need to scan through the custom fields and find any matches. If no match, set it disabled.
			foreach (UIElement customUI in this.gridCustomProperties.Children)
			{
				if (customUI is TextBlock) //Get each label.
				{
					//Get the linked control & attached RemoteProperty.
					Control custLinked = ((TextBlock)customUI).Tag as Control;

					if (custLinked != null)
					{
						RemoteCustomProperty custProp = ((Control)custLinked).Tag as RemoteCustomProperty;

						if (custProp != null && CustomFields != null)
						{
							if (CustomFields.ContainsKey(custProp.CustomPropertyId))
							{
								//Set IsEnbled
								custLinked.IsEnabled = true;
								//Set label:
								((TextBlock)customUI).FontWeight = (this.workflow_IsFieldRequired(custProp.CustomPropertyId, CustomFields) ? FontWeights.Bold : FontWeights.Normal);
							}
							else
							{
								//Set IsEnabled
								custLinked.IsEnabled = false;
								((TextBlock)customUI).FontWeight = FontWeights.Normal;
							}
						}
					}
				}
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
				this.loadItem_PopulateStatus(this.mnuActions, incident.IncidentStatusId);
				// - Description
				this.cntrlDescription.HTMLText = incident.Description;
				// - History
				//TODO: History (need API update)
				// - Attachments
				//Remove existing rows.
				//Add new rows.
				if (this._IncDocuments != null)
				{
					foreach (RemoteDocument incidentAttachment in this._IncDocuments)
					{
						int numAdding = this.gridAttachments.RowDefinitions.Count;
						//Create textblocks..
						// - Link/Name
						TextBlock txbFilename = new TextBlock();
						Hyperlink linkFile = new Hyperlink();
						linkFile.Inlines.Add(incidentAttachment.FilenameOrUrl);
						//Try to get a URL out of it..
						bool IsUrl = false;
						Uri atchUri = null;
						try
						{
							atchUri = new Uri(incidentAttachment.FilenameOrUrl);
							IsUrl = true;
						}
						catch { }

						if (!IsUrl)
						{
							atchUri = new Uri(this._IncDocumentsUrl.Replace("~", this._Project.ServerURL.ToString()).Replace("{art}", incidentAttachment.AttachmentId.ToString()));
						}
						linkFile.NavigateUri = atchUri;
						linkFile.Click += new RoutedEventHandler(Hyperlink_Click);

						//Add the link to the TextBlock.
						txbFilename.Inlines.Add(linkFile);
						//Create ToolTip.
						txbFilename.ToolTip = new cntrlRichTextEditor() { IsReadOnly = true, IsToolbarVisible = false, HTMLText = incidentAttachment.Description, Width = 200 };
						txbFilename.Style = (Style)this.FindResource("PaddedLabel");

						// - Document Version
						TextBlock txbVersion = new TextBlock();
						txbVersion.Text = incidentAttachment.CurrentVersion;
						txbVersion.Style = (Style)this.FindResource("PaddedLabel");

						// - Author
						TextBlock txbAuthor = new TextBlock();
						txbAuthor.Text = incidentAttachment.AuthorName;
						txbAuthor.Style = (Style)this.FindResource("PaddedLabel");

						// - Date Created
						TextBlock txbDateCreated = new TextBlock();
						txbDateCreated.Text = incidentAttachment.UploadDate.ToShortDateString();
						txbDateCreated.Style = (Style)this.FindResource("PaddedLabel");

						// - Size
						TextBlock txbSize = new TextBlock();
						txbSize.Text = incidentAttachment.Size.ToString() + "kb";
						txbSize.Style = (Style)this.FindResource("PaddedLabel");

						//Create the row, and add the controls to it.
						gridAttachments.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
						Grid.SetColumn(txbFilename, 0);
						Grid.SetRow(txbFilename, numAdding);
						gridAttachments.Children.Add(txbFilename);
						Grid.SetColumn(txbVersion, 1);
						Grid.SetRow(txbVersion, numAdding);
						gridAttachments.Children.Add(txbVersion);
						Grid.SetColumn(txbAuthor, 2);
						Grid.SetRow(txbAuthor, numAdding);
						gridAttachments.Children.Add(txbAuthor);
						Grid.SetColumn(txbDateCreated, 3);
						Grid.SetRow(txbDateCreated, numAdding);
						gridAttachments.Children.Add(txbDateCreated);
						Grid.SetColumn(txbSize, 4);
						Grid.SetRow(txbSize, numAdding);
						gridAttachments.Children.Add(txbSize);
					}
				}
				// - Schedule
				this.cntrlStartDate.SelectedDate = incident.StartDate;
				this.cntrlEndDate.SelectedDate = incident.ClosedDate;
				this.cntrlPerComplete.Text = incident.CompletionPercent.ToString();
				this.cntrlEstEffortH.Text = ((incident.EstimatedEffort.HasValue) ? Math.Floor(((double)incident.EstimatedEffort / (double)60)).ToString() : "");
				this.cntrlEstEffortM.Text = ((incident.EstimatedEffort.HasValue) ? ((double)incident.EstimatedEffort % (double)60).ToString() : "");
				this.cntrlActEffortH.Text = ((incident.ActualEffort.HasValue) ? Math.Floor(((double)incident.ActualEffort / (double)60)).ToString() : "");
				this.cntrlActEffortM.Text = ((incident.ActualEffort.HasValue) ? ((double)incident.ActualEffort % (double)60).ToString() : "");
				// - Custom Properties
				// We search backwards.
				foreach (UIElement cntCustom in this.gridCustomProperties.Children)
				{
					if ((cntCustom as Control) != null)
					{
						if ((cntCustom as Control).Tag.GetType() == typeof(RemoteCustomProperty))
						{
							dynamic dynControl = cntCustom;
							RemoteCustomProperty custProp = (RemoteCustomProperty)((Control)cntCustom).Tag;
							switch (custProp.CustomPropertyName)
							{
								case "TEXT_01":
									dynControl.Text = incident.Text01;
									break;
								case "TEXT_02":
									dynControl.Text = incident.Text02;
									break;
								case "TEXT_03":
									dynControl.Text = incident.Text03;
									break;
								case "TEXT_04":
									dynControl.Text = incident.Text04;
									break;
								case "TEXT_05":
									dynControl.Text = incident.Text05;
									break;
								case "TEXT_06":
									dynControl.Text = incident.Text06;
									break;
								case "TEXT_07":
									dynControl.Text = incident.Text07;
									break;
								case "TEXT_08":
									dynControl.Text = incident.Text08;
									break;
								case "TEXT_09":
									dynControl.Text = incident.Text09;
									break;
								case "TEXT_10":
									dynControl.Text = incident.Text10;
									break;
								case "LIST_01":
									dynControl.SelectedValue = incident.List01;
									break;
								case "LIST_02":
									dynControl.SelectedValue = incident.List02;
									break;
								case "LIST_03":
									dynControl.SelectedValue = incident.List03;
									break;
								case "LIST_04":
									dynControl.SelectedValue = incident.List04;
									break;
								case "LIST_05":
									dynControl.SelectedValue = incident.List05;
									break;
								case "LIST_06":
									dynControl.SelectedValue = incident.List06;
									break;
								case "LIST_07":
									dynControl.SelectedValue = incident.List07;
									break;
								case "LIST_08":
									dynControl.SelectedValue = incident.List08;
									break;
								case "LIST_09":
									dynControl.SelectedValue = incident.List09;
									break;
								case "LIST_10":
									dynControl.SelectedValue = incident.List10;
									break;
							}
						}
					}
				}

				//Clear the loading flag & dirty flags
				this.IsLoading = false;
				this._isDescChanged = false;
				this._isResChanged = false;
				this._isFieldChanged = false;

				//Set the tab title.
				this.ParentWindowPane.Caption = this.TabTitle;
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_displayInformation", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when a HyperLink object is clicked.</summary>
		/// <param name="sender">Hyperlink</param>
		/// <param name="e">RoutedEventArgs</param>
		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			if (sender is Hyperlink)
			{
				System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
			}
		}

		/// <summary>Gets the specified Required status for the given field.</summary>
		/// <param name="FieldID">The Field ID number that is contained in the list.</param>
		/// <param name="WorkFlow">The list of fields to check against.</param>
		/// <returns>True if the field is required, fals if not.</returns>
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
			//if (this.lblStatus.FontWeight == FontWeights.Bold && this.cntrlStatus.SelectedItem.GetType() != typeof(Business.SpiraTeam_Client.RemoteIncidentStatus))
			//     Fields += "Status;";
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
