﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls;
using System.Windows.Documents;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.HTMLandXAML;
using Microsoft.VisualStudio.Shell.Interop;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	public partial class frmDetailsRequirement : UserControl
	{
		#region Client Values
		private ImportExportClient _client;
		private int _clientNumRunning; //Holds the number current executing.
		private int _clientNum; //Holds the total amount. Needed to multiple ASYNC() calls.
		#endregion

		#region Private Data Storage Variables

		//The Project and the Incident
		private SpiraProject _Project = null;
		private RemoteRequirement _Requirement;

		//Other project-specific items.
		private List<RemoteProjectUser> _ProjUsers;
		private List<RemoteRelease> _ProjReleases;
		private List<RemoteDocument> _IncDocuments;
		private string _ReqDocumentsUrl;
		private string _RequirementUrl;

		#endregion

		/// <summary>Loads the item currently assigned to the ArtifactDetail property.</summary>
		/// <returns>Boolean on whether of not load was started successfully.</returns>
		private bool load_LoadItem()
		{
			bool retValue = false;
			if (this.ArtifactDetail != null)
			{
				//Clear the loading flag & dirty flags
				this.IsLoading = false;
				this._isDescChanged = false;
				this._isResChanged = false;
				this._isFieldChanged = false;
				this.btnSave.IsEnabled = false;

				//Set flag, reset vars..
				this.IsLoading = true;
				this.barLoadingIncident.Value = 0;
				this._ReqDocumentsUrl = null;
				this._RequirementUrl = null;

				//Create a client.
				this._client = null;
				this._client = StaticFuncs.CreateClient(((SpiraProject)this.ArtifactDetail.ArtifactParentProject.ArtifactTag).ServerURL.ToString());

				//Set client events.
				this._client.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(_client_Connection_Authenticate2Completed);
				this._client.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(_client_Connection_ConnectToProjectCompleted);
				this._client.Connection_DisconnectCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(_client_Connection_DisconnectCompleted);
				this._client.Document_RetrieveForArtifactCompleted += new EventHandler<Document_RetrieveForArtifactCompletedEventArgs>(_client_Document_RetrieveForArtifactCompleted);
				this._client.Release_RetrieveCompleted += new EventHandler<Release_RetrieveCompletedEventArgs>(_client_Release_RetrieveCompleted);
				this._client.Project_RetrieveUserMembershipCompleted += new EventHandler<Project_RetrieveUserMembershipCompletedEventArgs>(_client_Project_RetrieveUserMembershipCompleted);
				this._client.System_GetArtifactUrlCompleted += new EventHandler<System_GetArtifactUrlCompletedEventArgs>(_client_System_GetArtifactUrlCompleted);
				this._client.CustomProperty_RetrieveForArtifactTypeCompleted += new EventHandler<CustomProperty_RetrieveForArtifactTypeCompletedEventArgs>(_client_CustomProperty_RetrieveForArtifactTypeCompleted);

				//Fire the connection off here.
				this._clientNumRunning++;
				this.barLoadingIncident.Maximum = 8;
				this._client.Connection_Authenticate2Async(this._Project.UserName, this._Project.UserPass, StaticFuncs.getCultureResource.GetString("app_ReportName"));

			}

			return retValue;
		}

		#region Client Events
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
					this._clientNumRunning += 9;
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
						custControl.Style = (Style)this.FindResource("PaddedControl");
						custControl.Tag = e.Result[j];
						custControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
						custControl.VerticalAlignment = System.Windows.VerticalAlignment.Top;
						//Add it to the row/column.
						Grid.SetColumn(custControl, ((IsOnFirst) ? 1 : 4));
						Grid.SetRow(custControl, (int)Math.Floor(j / 2D));
						//Add it to the grid.
						this.gridCustomProperties.Children.Add(custControl);

						//Flip the IsOnFirst..
						IsOnFirst = !IsOnFirst;
					}

					//See if we're ready to get the actual data.
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
					if (string.IsNullOrWhiteSpace(this._ReqDocumentsUrl))
					{
						this._ReqDocumentsUrl = e.Result;
						this.load_IsReadyToGetMainData();
					}
					else
					{
						this._RequirementUrl = e.Result.Replace("~", this._Project.ServerURL.ToString());
						this.load_IsReadyToDisplayData();
					}

				}
				else
				{
					Logger.LogMessage(e.Error);
					this._ReqDocumentsUrl = "--none--";
				}

				System.Diagnostics.Debug.WriteLine(CLASS + METHOD + " EXIT. Clients - Running: " + this._clientNumRunning.ToString() + ", Total: " + this._clientNum.ToString());
			}
		}

		#endregion

		/// <summary>Checks to see if it's okay to start loading form data.
		/// Called after the two Workflow data retrieval Asyncs.
		/// </summary>
		private void load_IsReadyToDisplayData()
		{
			Logger.LogTrace("load_IsReadyToDisplayData: Clients Running " + this._clientNumRunning.ToString());

			if (this._clientNumRunning == 0)
			{
				this.loadItem_DisplayInformation(this._Requirement);

				//Turn off loading screen..
				this.IsLoading = false;
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
				box.SelectedIndex = box.Items.Add(new RemoteProjectUser() { FullName = StaticFuncs.getCultureResource.GetString("app_General_None") });

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
				box.SelectedIndex = box.Items.Add(new RemoteRelease() { Name = StaticFuncs.getCultureResource.GetString("app_General_None") });

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

		/// <summary>Selects the given ID in the Priority Control</summary>
		/// <param name="PriorityId">Integer of the selected priority.</param>
		private void loadItem_SelectPriority(int? PriorityId)
		{
			foreach (RequirementPriority availPri in this.cntrlImportance.Items)
			{
				if (availPri.PriorityId == PriorityId)
				{
					this.cntrlImportance.SelectedItem = availPri;
				}
			}
		}

		/// <summary>Selects the given ID in the Status Control</summary>
		/// <param name="PriorityId">Integer of the selected status.</param>
		private void loadItem_SelectStatus(int? StatusId)
		{
			foreach (RequirementStatus availSta in this.cntrlStatus.Items)
			{
				if (availSta.StatusId == StatusId)
				{
					this.cntrlStatus.SelectedItem = availSta;
				}
			}
		}
		#endregion











		/// <summary>Load the specified task into the data fields.</summary>
		/// <param name="task">The task details to load into fields.</param>
		private void loadItem_DisplayInformation(RemoteRequirement requirement)
		{
			const string METHOD = "loadItem_DisplayInformation()";

			try
			{
				#region Base Fields
				// - Name & ID
				this.cntrlIncidentName.Text = requirement.Name;
				this.lblToken.Text = this._ArtifactDetails.ArtifactIDDisplay;
				this.cntrlResolution.HTMLText = "";

				// - Users
				this.loadItem_PopulateUser(this.cntrlCreatedBy, requirement.AuthorId);
				this.loadItem_PopulateUser(this.cntrlOwnedBy, requirement.OwnerId);
				this.cntrlCreatedBy.Items.RemoveAt(0);

				// - Releases
				this.loadItem_PopulateReleaseControl(this.cntrlRelease, requirement.ReleaseId);

				// - Priority & Severity
				this.loadItem_SelectPriority(requirement.ImportanceId);
				this.loadItem_SelectStatus(requirement.StatusId);

				// - Planned Effort
				this.cntrlPlnEffortH.Text = ((requirement.PlannedEffort.HasValue) ? Math.Floor(((double)requirement.PlannedEffort / (double)60)).ToString() : "0");
				this.cntrlPlnEffortM.Text = ((requirement.PlannedEffort.HasValue) ? ((double)requirement.PlannedEffort % (double)60).ToString() : "0");

				// - Description
				this.cntrlDescription.HTMLText = requirement.Description;
				#endregion

				#region History
				//TODO: History (need API update)
				#endregion

				#region Attachments
				//Remove existing rows.
				try
				{
					int intChildTries = 0;
					while (this.gridAttachments.Children.Count > 5 && intChildTries < 10000)
					{
						this.gridAttachments.Children.RemoveAt(5);
						intChildTries++;
					}

					//Remove rows.
					int intRowTries = 0;
					while (this.gridAttachments.RowDefinitions.Count > 1 && intRowTries < 10000)
					{
						this.gridAttachments.RowDefinitions.RemoveAt(1);
						intRowTries++;
					}
				}
				catch (Exception ex)
				{
					Logger.LogMessage(ex, CLASS + METHOD + " Clearing Attachments Grid");
				}
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
							try
							{
								atchUri = new Uri(this._ReqDocumentsUrl.Replace("~", this._Project.ServerURL.ToString()).Replace("{art}", incidentAttachment.AttachmentId.ToString()));
							}
							catch { }
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
				#endregion

				#region Custom Properties
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
									dynControl.Text = requirement.Text01;
									break;
								case "TEXT_02":
									dynControl.Text = requirement.Text02;
									break;
								case "TEXT_03":
									dynControl.Text = requirement.Text03;
									break;
								case "TEXT_04":
									dynControl.Text = requirement.Text04;
									break;
								case "TEXT_05":
									dynControl.Text = requirement.Text05;
									break;
								case "TEXT_06":
									dynControl.Text = requirement.Text06;
									break;
								case "TEXT_07":
									dynControl.Text = requirement.Text07;
									break;
								case "TEXT_08":
									dynControl.Text = requirement.Text08;
									break;
								case "TEXT_09":
									dynControl.Text = requirement.Text09;
									break;
								case "TEXT_10":
									dynControl.Text = requirement.Text10;
									break;
								case "LIST_01":
									dynControl.SelectedValue = requirement.List01;
									break;
								case "LIST_02":
									dynControl.SelectedValue = requirement.List02;
									break;
								case "LIST_03":
									dynControl.SelectedValue = requirement.List03;
									break;
								case "LIST_04":
									dynControl.SelectedValue = requirement.List04;
									break;
								case "LIST_05":
									dynControl.SelectedValue = requirement.List05;
									break;
								case "LIST_06":
									dynControl.SelectedValue = requirement.List06;
									break;
								case "LIST_07":
									dynControl.SelectedValue = requirement.List07;
									break;
								case "LIST_08":
									dynControl.SelectedValue = requirement.List08;
									break;
								case "LIST_09":
									dynControl.SelectedValue = requirement.List09;
									break;
								case "LIST_10":
									dynControl.SelectedValue = requirement.List10;
									break;
							}
						}
					}
				}
				#endregion

				//Set the tab title.
				this.ParentWindowPane.Caption = this.TabTitle;
				this.display_SetWindowChanged(this._isDescChanged || this._isResChanged || this._isFieldChanged);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, CLASS + METHOD);
			}
		}

	}
}
