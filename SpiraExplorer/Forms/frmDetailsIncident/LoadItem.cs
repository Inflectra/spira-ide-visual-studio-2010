﻿using System;
using System.Collections.Generic;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	public partial class wpfDetailsIncident : UserControl
	{
		#region Private Data Storage Variables
		//The Project and the Incident
		private SpiraProject _Project = null;
		private Business.SpiraTeam_Client.RemoteIncident _Incident;
		private Business.SpiraTeam_Client.RemoteIncident _IncidentConcurrency;
		//Other project-specific items.
		private List<Business.SpiraTeam_Client.RemoteProjectUser> _ProjUsers;
		private List<Business.SpiraTeam_Client.RemoteRelease> _ProjReleases;
		private List<Business.SpiraTeam_Client.RemoteIncidentSeverity> _IncSeverity;
		private List<Business.SpiraTeam_Client.RemoteIncidentPriority> _IncPriority;
		private List<Business.SpiraTeam_Client.RemoteIncidentType> _IncType;
		private List<Business.SpiraTeam_Client.RemoteIncidentStatus> _IncStatus;
		private List<Business.SpiraTeam_Client.RemoteWorkflowIncidentTransition> _IncWkfTransision;
		private List<Business.SpiraTeam_Client.RemoteIncidentResolution> _IncResolutions;
		//Workflow fields.
		private Dictionary<int, int> _IncWkfFields_Current;
		private Dictionary<int, int> _IncWkfFields_Updated;
		#endregion
		#region Internal Client Tracking
		private int _NumRunning = 0;
		private int _NumAsync = 0;
		#endregion
		//Global client.
		private Business.SpiraTeam_Client.ImportExportClient _Client;
		// Are we in read-only mode? Are we saving?
		private bool isInLoadMode = false;
		private bool isInSaveMode = false;
		private bool isInConcMode = false;
		// The item code.
		private string _itemCode = "";

		/// <summary>Loads the specified incident into the form, handling all client creation and UI updates. Entry point for loading new item.</summary>
		/// <param name="inProject">SpiraProject associated with this artifact.</param>
		/// <param name="itemKey">Item key for this artifact. ex: "IN:1234"</param>
		/// <returns>False.</returns>
		//internal bool loadItem(Connect.SpiraProject inProject, string itemKey)
		internal bool loadItem(object inProject, string itemKey)
		{
			try
			{
				this.lblItemTag.Content = itemKey;

				//Hide the form, show the "I'm loading.." bar.
				//this.panelForm.Visibility = Visibility.Collapsed;
				this.panelLoading.Visibility = Visibility.Visible;
				this.panelLoadingError.Visibility = Visibility.Collapsed;
				this.msgLoadingErrorMsg.Text = "";
				//Verify we have an item and project to load.
				//if (inProject != null)
				//     this._Project = inProject;
				this._itemCode = itemKey;

				//Call the real load function.
				this.loadItem_Incident();

			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem", ex, System.Diagnostics.EventLogEntryType.Error);
			}
			return false;
		}

		private void loadItem_Incident()
		{
			try
			{
				this.isInLoadMode = true;
				//Set up the client here.
				//this._Client = new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Business.SpiraTeam_Client.ImportExport();
				//this._Client.Url = this._Project.ServerURL + Connect.SpiraProject.URL_APIADD;
				//this._Client.CookieContainer = new System.Net.CookieContainer();

				//Set all event handlers.
				//this._Client.Connection_Authenticate2Completed += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Business.SpiraTeam_Client.Connection_Authenticate2CompletedEventHandler(loadItem_Incident_2);
				//this._Client.Connection_ConnectToProjectCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Business.SpiraTeam_Client.Connection_ConnectToProjectCompletedEventHandler(loadItem_Incident_3);
				//this._Client.Incident_RetrieveByIdCompleted += new Business.SpiraTeam_Client.Incident_RetrieveByIdCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Project_RetrieveUserMembershipCompleted += new Business.SpiraTeam_Client.Project_RetrieveUserMembershipCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Incident_RetrieveWorkflowFieldsCompleted += new Business.SpiraTeam_Client.Incident_RetrieveWorkflowFieldsCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Incident_RetrieveWorkflowTransitionsCompleted += new Business.SpiraTeam_Client.Incident_RetrieveWorkflowTransitionsCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Release_RetrieveCompleted += new Business.SpiraTeam_Client.Release_RetrieveCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Incident_RetrievePrioritiesCompleted += new Business.SpiraTeam_Client.Incident_RetrievePrioritiesCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Incident_RetrieveSeveritiesCompleted += new Business.SpiraTeam_Client.Incident_RetrieveSeveritiesCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Incident_RetrieveTypesCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Business.SpiraTeam_Client.Incident_RetrieveTypesCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Incident_RetrieveStatusesCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Business.SpiraTeam_Client.Incident_RetrieveStatusesCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Incident_RetrieveResolutionsCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Business.SpiraTeam_Client.Incident_RetrieveResolutionsCompletedEventHandler(loadItem_Incident_4);
				//this._Client.Incident_UpdateCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Business.SpiraTeam_Client.Incident_UpdateCompletedEventHandler(_Client_Incident_UpdateCompleted);
				//this._Client.Incident_AddResolutionsCompleted += new Inflectra.SpiraTest.IDEIntegration.VisualStudio.Business.SpiraTeam_Client.Incident_AddResolutionsCompletedEventHandler(_Client_Incident_UpdateCompleted);

				string[] token = this._itemCode.Split(':');
				if (token.Length == 2)
				{
					int artNum = -1;
					if (int.TryParse(token[1], out artNum))
					{
						//this._Client.Connection_Authenticate2Async(this._Project.UserName, this._Project.UserPass, this._resources.GetString("strAddinProgNamePretty"), this._NumAsync++);
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_Incident", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit after we have successfully logged on. Launches off connecting to the project.</summary>
		/// <param name="sender">ImportExport</param>
		/// <param name="e">Event Args</param>
		private void loadItem_Incident_2(object sender, Business.SpiraTeam_Client.Connection_Authenticate2CompletedEventArgs e)
		{
			try
			{
				if (e.Error == null)
				{
					int incidentID = (int)e.UserState;

					//Get server version.
					//this.loadItem_VerifyVersion(this._Client);

					this._Client.Connection_ConnectToProjectAsync(this._Project.ProjectID, this._NumAsync++);
				}
				else
				{
					this.panelLoading.Visibility = Visibility.Collapsed;
					this.panelLoadingError.Visibility = Visibility.Visible;
					this.msgLoadingErrorMsg.Text = getErrorMessage(e.Error);
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_Incident_2", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit after we successfully connected to the project. Launches all all other data retrievals.</summary>
		/// <param name="sender">ImportExport</param>
		/// <param name="e">Event Args</param>
		private void loadItem_Incident_3(object sender, Business.SpiraTeam_Client.Connection_ConnectToProjectCompletedEventArgs e)
		{
			try
			{
				if ((e.Error == null) && (e.Result != false))
				{
					//Fire off asyncs.
					this._NumRunning = 6;
					this._Client.Incident_RetrieveByIdAsync(int.Parse(this._itemCode.Split(':')[1]), this._NumAsync++);
					this._Client.Release_RetrieveAsync(true, this._NumAsync++);
					this._Client.Incident_RetrievePrioritiesAsync(this._NumAsync++);
					this._Client.Incident_RetrieveSeveritiesAsync(this._NumAsync++);
					this._Client.Incident_RetrieveTypesAsync(this._NumAsync++);
					this._Client.Incident_RetrieveStatusesAsync(this._NumAsync++);
					//if (this.hasWorkFlow_Avail)
					//{
					//     this._NumRunning++;
					//     this._Client.Project_RetrieveUserMembershipAsync(this._NumAsync++);
					//}
				}
				else
				{
					this.panelLoading.Visibility = Visibility.Collapsed;
					this.panelLoadingError.Visibility = Visibility.Visible;
					Exception ex = e.Error;
					string errMsg = "";
					if (e.Error == null)
						errMsg = "Could not connect to project #" + this._Project.ProjectID.ToString();
					else
					{
						errMsg = getErrorMessage(e.Error);
					}
					this.msgLoadingErrorMsg.Text = errMsg;
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_Incident_3", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when any ASync call for data is completed.</summary>
		/// <param name="sender">ImportExport</param>
		/// <param name="e">Event Args</param>
		private void loadItem_Incident_4(object sender, EventArgs e)
		{
			try
			{
				bool isErrorThrown = false;
				string strErrMsg = "";

				string EventType = e.GetType().ToString().Substring(e.GetType().ToString().LastIndexOf('.') + 1);

				switch (EventType)
				{
					#region Incident Complete
					case "Incident_RetrieveByIdCompletedEventArgs":
						{
							//Incident is loaded. Save data, fire off the dependant one.
							Business.SpiraTeam_Client.Incident_RetrieveByIdCompletedEventArgs evt = (Business.SpiraTeam_Client.Incident_RetrieveByIdCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									if (this.isInConcMode)
									{
										// It's a data concurrency load. We're only looking for Incident information.
										this._IncidentConcurrency = evt.Result;
										// Pass it to the flagging function.
										//this.concurrency_HighlightFields(this._IncidentConcurrency);
									}
									else
									{
										// It's not a data concurrency load.
										this._Incident = evt.Result;
										this._NumRunning++;
										this._Client.Incident_RetrieveResolutionsAsync(this._Incident.IncidentId.Value, this._NumAsync++);
										//if (this.hasWorkFlow_Avail)
										//{
										//     this._NumRunning += 2;
										//     this._Client.Incident_RetrieveWorkflowFieldsAsync(this._Incident.IncidentTypeId, this._Incident.IncidentStatusId, this._NumAsync++);
										//     this._Client.Incident_RetrieveWorkflowTransitionsAsync(this._Incident.IncidentTypeId, this._Incident.IncidentStatusId, (this._Incident.OpenerId == this._Project.UserID), (this._Incident.OwnerId == this._Project.UserID), this._NumAsync++);
										//}
									}

									//Subtract one because the one that started this is finished.
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Incident Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}
						}
						break;
					#endregion
					#region Project User Membership Complete
					case "Project_RetrieveUserMembershipCompletedEventArgs":
						{
							Business.SpiraTeam_Client.Project_RetrieveUserMembershipCompletedEventArgs evt = (Business.SpiraTeam_Client.Project_RetrieveUserMembershipCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._ProjUsers = evt.Result;
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Project User Membership Complete. " + this._NumRunning.ToString() + " left.");

								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}
						}
						break;
					#endregion
					#region Workflow Fields Complete
					case "Incident_RetrieveWorkflowFieldsCompletedEventArgs":
						{
							Business.SpiraTeam_Client.Incident_RetrieveWorkflowFieldsCompletedEventArgs evt = (Business.SpiraTeam_Client.Incident_RetrieveWorkflowFieldsCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									//this._IncWkfFields_Current = scanWorkFlowFields(evt.Result);
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Workflow Fields Complete. " + this._NumRunning.ToString() + " left.");

								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}

						}
						break;
					#endregion
					#region Workflow Transisions Complete
					case "Incident_RetrieveWorkflowTransitionsCompletedEventArgs":
						{
							Business.SpiraTeam_Client.Incident_RetrieveWorkflowTransitionsCompletedEventArgs evt = (Business.SpiraTeam_Client.Incident_RetrieveWorkflowTransitionsCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._IncWkfTransision = evt.Result;
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Workflow Transitions Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}
						}
						break;
					#endregion
					#region Releases Complete
					case "Release_RetrieveCompletedEventArgs":
						{
							Business.SpiraTeam_Client.Release_RetrieveCompletedEventArgs evt = (Business.SpiraTeam_Client.Release_RetrieveCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._ProjReleases = evt.Result;
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Releases Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}
						}
						break;
					#endregion
					#region Incident Priorities Complete
					case "Incident_RetrievePrioritiesCompletedEventArgs":
						{
							Business.SpiraTeam_Client.Incident_RetrievePrioritiesCompletedEventArgs evt = (Business.SpiraTeam_Client.Incident_RetrievePrioritiesCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._IncPriority = evt.Result;
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Incident Priorities Complete. " + this._NumRunning.ToString() + " left.");

								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}
						}
						break;
					#endregion
					#region Incident Severites Complete
					case "Incident_RetrieveSeveritiesCompletedEventArgs":
						{
							Business.SpiraTeam_Client.Incident_RetrieveSeveritiesCompletedEventArgs evt = (Business.SpiraTeam_Client.Incident_RetrieveSeveritiesCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._IncSeverity = evt.Result;
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Incident Severities Complete. " + this._NumRunning.ToString() + " left.");

								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}
						}
						break;
					#endregion
					#region Incident Types Complete
					case "Incident_RetrieveTypesCompletedEventArgs":
						{
							Business.SpiraTeam_Client.Incident_RetrieveTypesCompletedEventArgs evt = (Business.SpiraTeam_Client.Incident_RetrieveTypesCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._IncType = evt.Result;
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Incident Types Complete. " + this._NumRunning.ToString() + " left.");

								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}
						}
						break;
					#endregion
					#region Incident Statuses Complete
					case "Incident_RetrieveStatusesCompletedEventArgs":
						{
							Business.SpiraTeam_Client.Incident_RetrieveStatusesCompletedEventArgs evt = (Business.SpiraTeam_Client.Incident_RetrieveStatusesCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._IncStatus = evt.Result;
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Incident Statuses Complete. " + this._NumRunning.ToString() + " left.");
								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}
						}
						break;
					#endregion
					#region Resolutions Complete
					case "Incident_RetrieveResolutionsCompletedEventArgs":
						{
							Business.SpiraTeam_Client.Incident_RetrieveResolutionsCompletedEventArgs evt = (Business.SpiraTeam_Client.Incident_RetrieveResolutionsCompletedEventArgs)e;
							if (!evt.Cancelled)
							{
								if (evt.Error == null)
								{
									this._IncResolutions = evt.Result;
									this._NumRunning -= 1;

									//DEBUG: Logit.
									System.Diagnostics.Debug.WriteLine("» Resolutions Complete. " + this._NumRunning.ToString() + " left.");

								}
								else
								{
									isErrorThrown = true;
									strErrMsg = getErrorMessage(evt.Error);
								}
							}
						}
						break;
					#endregion
				}
				//If all of them have completed, load the form.
				if (isErrorThrown)
				{
					//Kill all Async calls.
					try
					{
						for (int I = 0; I <= this._NumAsync; I++)
						{
							//this._Client.CancelAsync(I);
						}
					}
					finally
					{
						//Display error information.
						this.panelLoading.Visibility = Visibility.Collapsed;
						this.panelLoadingError.Visibility = Visibility.Visible;
						//this.panelForm.Visibility = Visibility.Collapsed;
						this.msgLoadingErrorMsg.Text = strErrMsg;
					}
				}
				else if (this._NumRunning == 0)
				{
					// Load data into fields.
					if (this.isInConcMode)
					{
						this.loadItem_displayInformation(this._IncidentConcurrency);
						this._Incident = this._IncidentConcurrency;
					}
					else
						this.loadItem_displayInformation(this._Incident);
					//this.loadItem_PopulateDiscussion(this._IncResolutions);

					//Set Workflow Data. (To disable Fields)
					this.loadItem_SetEnabledFields(this._IncWkfFields_Current);
					//this.panelContents.IsEnabled = this.hasWorkFlow_Avail;

					//Update screen.
					if (this.isInSaveMode)
					{
						if (this.isInConcMode)
						{
							this.isInConcMode = false;
							this.isInSaveMode = false;
							//this.panelWarning.Visibility = Visibility.Collapsed;
							//this.panelInfo.Visibility = Visibility.Collapsed;
							//this.panelError.Visibility = Visibility.Visible;
							//this.msgErrMessage.Text = "This incident was modified by another user. New data has been loaded." + Environment.NewLine + "Yellow fields were modified by the other user. Red fields were both modified by you and the other user.";
						}
						else
						{
							this.isInSaveMode = false;
							//this.panelWarning.Visibility = Visibility.Collapsed;
							//this.panelError.Visibility = Visibility.Collapsed;
							//this.panelInfo.Visibility = Visibility.Visible;
							//this.msgInfMessage.Text = "Incident saved.";
							//this.concurrency_ResetHighlightFields();
						}
					}
					else
					{
						this.panelLoading.Visibility = Visibility.Collapsed;
						this.panelLoadingError.Visibility = Visibility.Collapsed;
						//this.panelForm.Visibility = Visibility.Visible;
						//this.concurrency_ResetHighlightFields();
					}

					//Finished loading.
					this.isInLoadMode = false;
					this._isFieldChanged = false;
					//this._btnSave.IsEnabled = false;
					this._isDescChanged = false;
					this._isResChanged = false;
					this._DetailsWindow.Caption = this._DetailsWindowTitle;
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_Incident_4", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		#region Field Population
		//private void loadItem_PopulateUser(ComboBox box, int? SelectedUserID, string SelectedUserName, Business.SpiraTeam_Client.ImportExport client)
		private void loadItem_PopulateUser(ComboBox box, int? SelectedUserID, string SelectedUserName, object client)
		{
			try
			{
				//Clear and add our 'none'.
				box.Items.Clear();
				int def = box.Items.Add(new ComboBoxItem() { Content = "-- None --" });
				box.SelectedIndex = def;

				if (!SelectedUserID.HasValue)
					SelectedUserID = -1;

				//Load the project users.
				//if (this.hasWorkFlow_Avail)
				//{
				//     foreach (Business.SpiraTeam_Client.RemoteProjectUser projUser in this._ProjUsers)
				//     {
				//          int numAdded = box.Items.Add(client.User_RetrieveById(projUser.UserId));
				//          if (projUser.UserId == SelectedUserID)
				//          {
				//               box.SelectedIndex = numAdded;
				//          }
				//     }
				//}
				else
				{
					int numAdded = box.Items.Add(SelectedUserName);
					box.SelectedIndex = numAdded;
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateUser", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void loadItem_PopulateReleaseControl(ComboBox Box, int? SelectedRelease)
		{
			try
			{
				//Clear and add our 'none'.
				Box.Items.Clear();
				int def = Box.Items.Add(new ComboBoxItem() { Content = "-- None --" });
				Box.SelectedIndex = def;

				if (!SelectedRelease.HasValue)
					SelectedRelease = -1;

				foreach (Business.SpiraTeam_Client.RemoteRelease Release in this._ProjReleases)
				{
					int numAdded = Box.Items.Add(Release);
					if (Release.ReleaseId == SelectedRelease)
					{
						Box.SelectedIndex = numAdded;
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateReleaseControl", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void loadItem_PopulateSeverity(ComboBox Box, int? SelectedItem)
		{
			try
			{
				//Clear and add our 'none'.
				Box.Items.Clear();
				int def = Box.Items.Add(new Business.SpiraTeam_Client.RemoteIncidentSeverity() { Name = "-- None --" });
				Box.SelectedIndex = def;

				if (!SelectedItem.HasValue)
					SelectedItem = -1;

				foreach (Business.SpiraTeam_Client.RemoteIncidentSeverity Severity in this._IncSeverity)
				{
					int nunAdded = Box.Items.Add(Severity);
					if (Severity.SeverityId == SelectedItem)
					{
						Box.SelectedIndex = nunAdded;
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateSeverity", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void loadItem_PopulatePriority(ComboBox Box, int? SelectedItem)
		{
			try
			{
				//Clear and add our 'none'.
				Box.Items.Clear();
				int def = Box.Items.Add(new Business.SpiraTeam_Client.RemoteIncidentPriority() { Name = "-- None --" });
				Box.SelectedIndex = def;

				if (!SelectedItem.HasValue)
					SelectedItem = -1;

				foreach (Business.SpiraTeam_Client.RemoteIncidentPriority Priority in this._IncPriority)
				{
					int nunAdded = Box.Items.Add(Priority);
					if (Priority.PriorityId == SelectedItem)
					{
						Box.SelectedIndex = nunAdded;
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulatePriority", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void loadItem_PopulateType(ComboBox Box, int? SelectedItem)
		{
			try
			{
				Box.Items.Clear();

				if (!SelectedItem.HasValue)
					SelectedItem = -1;

				foreach (Business.SpiraTeam_Client.RemoteIncidentType Type in this._IncType)
				{
					int numAdded = Box.Items.Add(Type);
					if (SelectedItem == Type.IncidentTypeId)
					{
						Box.SelectedIndex = numAdded;
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateType", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void loadItem_PopulateStatus(ComboBox Box, int? SelectedItem)
		{
			//Loop through all the avaiable ones. We only add the ones that are in the 
			//  workflow transision, or the current status, making sure the current
			//  one is selected.
			try
			{
				Box.Items.Clear();
				foreach (Business.SpiraTeam_Client.RemoteIncidentStatus Status in this._IncStatus)
				{
					if (Status.IncidentStatusId == SelectedItem)
					{
						int numAdded = Box.Items.Add(Status);
						Box.SelectedIndex = numAdded;
					}
					else
					{
						//Loop through. If it's available, add it.
						foreach (Business.SpiraTeam_Client.RemoteWorkflowIncidentTransition Transition in this._IncWkfTransision)
						{
							//if (Transition.IncidentStatusID_Output == Status.IncidentStatusId)
							//{
							//     Transition.Name = "» " + Transition.Name;
							//     Box.Items.Add(Transition);
							//}
						}
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateStatus", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void loadItem_PopulateDiscussion(Business.SpiraTeam_Client.RemoteIncidentResolution[] Discussions)
		{
			try
			{
				//Erase ones in there.
				this.cntrlDiscussion.Children.Clear();
				foreach (Business.SpiraTeam_Client.RemoteIncidentResolution Resolution in Discussions)
				{
					string header = Resolution.CreatorName + " [" + Resolution.CreationDate.ToShortDateString() + " " + Resolution.CreationDate.ToShortTimeString() + "]";
					//this.cntrlDiscussion.Children.Add(new wpfDiscussionFrame(header, Resolution.Resolution));
				}
				if (Discussions.Length < 1)
				{
					//this.cntrlDiscussion.Children.Add(new wpfDiscussionFrame("No comments for this item.", ""));
				}
				//Clear the entry box.
				//this.cntrlResolution.HTMLText = "";
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_PopulateDiscussion", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

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
				//this.cntrlResolution.Visibility = ((WorkFlowFields.ContainsKey(12)) ? Visibility.Visible : Visibility.Collapsed);
				//Schedule fields.
				this.cntrlStartDate.IsEnabled = (WorkFlowFields.ContainsKey(45));
				this.cntrlEndDate.IsEnabled = (WorkFlowFields.ContainsKey(14));
				//this.cntrlPerComplete.IsEnabled = (WorkFlowFields.ContainsKey(46)); //Not workflow configurable.
				this.cntrlEstEffortH.IsEnabled = this.cntrlEstEffortM.IsEnabled = (WorkFlowFields.ContainsKey(47));
				this.cntrlActEffortH.IsEnabled = this.cntrlActEffortM.IsEnabled = (WorkFlowFields.ContainsKey(48));

				// ** Set required fields.
				//lblName
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

		//private void loadItem_VerifyVersion(Business.SpiraTeam_Client.ImportExport client)
		private void loadItem_VerifyVersion(object client)
		{
			try
			{
				//Get the version number and disable any items necessary.
				//Business.SpiraTeam_Client.RemoteVersion version = client.System_GetProductVersion();
				//string[] mainVers = version.Version.Split('.');
				//int verMain = int.Parse(mainVers[0]);
				//int verRev = int.Parse(mainVers[1]);
				//int verBuild = int.Parse(mainVers[2]);

				bool enableCustom = false;
				bool enableHistory = false;
				bool showWorkflowMessage = false;

				//if (verMain > 2)
				//{
				//     enableCustom = true;
				//}
				//else
				//{
				//     if (verMain == 2)
				//     {
				//          if (verRev >= 3)
				//          {
				//               if (verBuild >= 1)
				//               {
				//                    //if (version.Patch.HasValue && version.Patch < 17)
				//                    //{
				//                    //     showWorkflowMessage = true;
				//                    //}
				//               }
				//               else
				//               {
				//                    showWorkflowMessage = true;
				//               }
				//          }
				//          else
				//          {
				//               showWorkflowMessage = true;
				//          }
				//     }
				//     else
				//     {
				//          showWorkflowMessage = true;
				//     }
				//}

				//if (showWorkflowMessage)
				//{
				//     //this.panelWarning.Visibility = Visibility.Visible;
				//     //this.panelNone.Visibility = Visibility.Collapsed;
				//     //this.msgWrnMessage.Text = "Application version is less than 2.3.1(17) and remote workflows are not supported. Incident is read-only.";
				//     //this.hasWorkFlow_Avail = false;
				//}
				//else
				//{
				//     //this.hasWorkFlow_Avail = true;
				//}

				this.cntrlTabCustProps.Visibility = ((enableCustom) ? Visibility.Visible : Visibility.Collapsed);
				this.cntrlTabHistory.Visibility = ((enableHistory) ? Visibility.Visible : Visibility.Collapsed);
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::loadItem_VerifyVersion", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		private void loadItem_displayInformation(Business.SpiraTeam_Client.RemoteIncident incident)
		{
			try
			{
				//Load information:
				// - Name
				this.cntrlIncidentName.Text = incident.Name;
				// - Users
				//this.loadItem_PopulateUser(this.cntrlDetectedBy, incident.OpenerId, incident.OpenerName, this._Client);
				//this.loadItem_PopulateUser(this.cntrlOwnedBy, incident.OwnerId, incident.OwnerName, this._Client);
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
