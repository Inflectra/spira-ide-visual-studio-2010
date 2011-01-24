using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>Holds the saving functions for frmDetailsIncident</summary>
	public partial class frmDetailsIncident : UserControl
	{
		//Are we currently saving our data?
		private bool _isSavingInformation = false;
		private int _clientNumSaving;

		/// <summary>Hit when the user wants to save the incident.</summary>
		/// <param name="sender">The save button.</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			try
			{
				this.barSavingIncident.Value = -5;
				this.barSavingIncident.Maximum = 0;
				this.barSavingIncident.Minimum = -5;

				if (this._isFieldChanged)
				{
					//Set working flag.
					this.IsSaving = true;

					//Get the new values from the form..
					RemoteIncident newIncident = this.save_GetFromFields();

					if (newIncident != null && this.workflow_CheckRequiredFields())
					{
						//Create a client, and save incident and resolution..
						ImportExportClient clientSave = StaticFuncs.CreateClient(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ServerURL.ToString());
						clientSave.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(clientSave_Connection_Authenticate2Completed);
						clientSave.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(clientSave_Connection_ConnectToProjectCompleted);
						clientSave.Incident_UpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientSave_Incident_UpdateCompleted);
						clientSave.Incident_AddResolutionsCompleted += new EventHandler<Incident_AddResolutionsCompletedEventArgs>(clientSave_Incident_AddResolutionsCompleted);
						clientSave.Connection_DisconnectCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientSave_Connection_DisconnectCompleted);

						//Fire off the connection.
						this._clientNumSaving = 1;
						clientSave.Connection_Authenticate2Async(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).UserName, ((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).UserPass, StaticFuncs.getCultureResource.GetString("app_ReportName"), this._clientNum++);
					}
					else
					{
						//Display message saying that some required fields aren't filled out.
						MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_RequiredFieldsMessage"), StaticFuncs.getCultureResource.GetString("app_General_RequiredFields"), MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::_cntrlSave_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}

			if (this._clientNumSaving == 0)
			{
				this.IsSaving = false;
			}
		}

		#region Client Events
		/// <summary>Hit when we're finished connecting.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientSave_Connection_DisconnectCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			const string METHOD = "clientSave_Connection_DisconnectCompleted()";
			Logger.LogTrace(CLASS + METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

			this._clientNumSaving--;
			this.barSavingIncident.Value++;

			//See if it's okay to reload.
			this.save_CheckIfOkayToLoad();
		}

		/// <summary>Hit when we're finished adding a resolution.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Incident_AddResolutionsCompletedEventArgs</param>
		private void clientSave_Incident_AddResolutionsCompleted(object sender, Incident_AddResolutionsCompletedEventArgs e)
		{
			const string METHOD = "clientSave_Incident_AddResolutionsCompleted()";
			Logger.LogTrace(CLASS + METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

			ImportExportClient client = (sender as ImportExportClient);
			this._clientNumSaving--;
			this.barSavingIncident.Value++;

			if (!e.Cancelled)
			{
				if (e.Error != null)
				{
					//Display error.
				}

				//Regardless of what happens, we're disconnecting here.
				this._clientNumSaving++;
				client.Connection_DisconnectAsync(this._clientNum++);
			}

			//See if it's okay to reload.
			this.save_CheckIfOkayToLoad();

			Logger.LogTrace(CLASS + METHOD + " Exit: " + this._clientNumSaving.ToString() + " left.");
		}

		/// <summary>Hit when we're finished updating the main information.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientSave_Incident_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			const string METHOD = "clientSave_Incident_UpdateCompleted()";
			Logger.LogTrace(CLASS + METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

			ImportExportClient client = (sender as ImportExportClient);
			this._clientNumSaving--;
			this.barSavingIncident.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					//See if we need to add a resolution.
					if (this._isResChanged)
					{
						//We need to save a resolution.
						RemoteIncidentResolution newRes = new RemoteIncidentResolution();
						newRes.CreationDate=DateTime.Now;
						newRes.CreatorId = ((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).UserID;
						newRes.IncidentId = this._ArtifactDetails.ArtifactId;
						newRes.Resolution = this.cntrlResolution.HTMLText;

						this._clientNumSaving++;
						client.Incident_AddResolutionsAsync(new List<RemoteIncidentResolution>() { newRes }, this._clientNum++);
					}
					else
					{
						//We're finished.
						this.barSavingIncident.Value++;
						this._clientNumSaving++;
						client.Connection_DisconnectAsync(this._clientNum++);
					}
				}
				else
				{
					//TODO: Show Error.
					//Cancel calls.
					this._clientNumSaving++;
					client.Connection_DisconnectAsync(this._clientNum++);
				}
			}

			//See if it's okay to reload.
			this.save_CheckIfOkayToLoad();

			Logger.LogTrace(CLASS + METHOD + " Exit: " + this._clientNumSaving.ToString() + " left.");
		}

		/// <summary>Hit when we're finished connecting to the project.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Connection_ConnectToProjectCompletedEventArgs</param>
		private void clientSave_Connection_ConnectToProjectCompleted(object sender, Connection_ConnectToProjectCompletedEventArgs e)
		{
			const string METHOD = "clientSave_Connection_ConnectToProjectCompleted()";
			Logger.LogTrace(CLASS + METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

			ImportExportClient client = (sender as ImportExportClient);
			this._clientNumSaving--;
			this.barSavingIncident.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					if (e.Result)
					{
						//Get the new RemoteIncident
						RemoteIncident newIncident = this.save_GetFromFields();

						if (newIncident != null)
						{
							//Fire off our update calls.
							this._clientNumSaving++;
							client.Incident_UpdateAsync(newIncident, this._clientNum++);
						}
						else
						{
							//TODO: Show Error.
							//Cancel calls.
							this._clientNumSaving++;
							client.Connection_DisconnectAsync(this._clientNum++);
						}
					}
					else
					{
						//TODO: Show Error.
						//Cancel calls.
						this._clientNumSaving++;
						client.Connection_DisconnectAsync(this._clientNum++);
					}
				}
				else
				{
					//TODO: Show Error.
					//Cancel calls.
					this._clientNumSaving++;
					client.Connection_DisconnectAsync(this._clientNum++);
				}
			}

			//See if it's okay to reload.
			this.save_CheckIfOkayToLoad();

			Logger.LogTrace(CLASS + METHOD + " Exit: " + this._clientNumSaving.ToString() + " left.");
		}

		/// <summary>Hit when we're authenticated to the server.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Connection_Authenticate2CompletedEventArgs</param>
		private void clientSave_Connection_Authenticate2Completed(object sender, Connection_Authenticate2CompletedEventArgs e)
		{
			const string METHOD = "clientSave_Connection_Authenticate2Completed()";
			Logger.LogTrace(CLASS + METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

			ImportExportClient client = (sender as ImportExportClient);
			this._clientNumSaving--;
			this.barSavingIncident.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					if (e.Result)
					{
						//Connect to the progect ID.
						this._clientNumSaving++;
						client.Connection_ConnectToProjectAsync(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ProjectID, this._clientNum++);
					}
					else
					{
						//TODO: Show Error.
						//Cancel calls.
						this._clientNumSaving++;
						client.Connection_DisconnectAsync(this._clientNum++);
					}
				}
				else
				{
					//TODO: Show Error.
					//Cancel calls.
					this._clientNumSaving++;
					client.Connection_DisconnectAsync(this._clientNum++);
				}
			}

			//See if it's okay to reload.
			this.save_CheckIfOkayToLoad();

			Logger.LogTrace(CLASS + METHOD + " Exit: " + this._clientNumSaving.ToString() + " left.");
		}
		#endregion

		/// <summary>Checks if it's okay to refresh the data details.</summary>
		private void save_CheckIfOkayToLoad()
		{
			const string METHOD = "save_CheckIfOkayToLoad()";
			Logger.LogTrace(CLASS + METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

			//If we're down to 0, we have to reload our information.
			if (this._clientNumSaving == 0)
			{
				this.IsSaving = false;
				this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Loading");
				this.load_LoadItem();
			}

			Logger.LogTrace(CLASS + METHOD + " Exit");
		}

		/// <summary>Copies over our values from the form into an Incident object.</summary>
		/// <returns>A new RemoteIncident, or Null if error.</returns>
		private RemoteIncident save_GetFromFields()
		{
			const string METHOD = "save_GetFromFields()";

			RemoteIncident retIncident = null;
			try
			{
				retIncident = new RemoteIncident();

				//*Fixed fields..
				retIncident.IncidentId = this._Incident.IncidentId;
				retIncident.ProjectId = this._Incident.ProjectId;
				retIncident.CreationDate = this._Incident.CreationDate;
				retIncident.LastUpdateDate = this._Incident.LastUpdateDate;

				//*Standard fields..
				retIncident.Name = this.cntrlIncidentName.Text.Trim();
				retIncident.IncidentTypeId = ((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId;
				retIncident.IncidentStatusId = this._IncCurrentStatus.Value;
				retIncident.OpenerId = ((RemoteUser)this.cntrlDetectedBy.SelectedItem).UserId;
				retIncident.OwnerId = ((RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId;
				retIncident.PriorityId = ((RemoteIncidentPriority)this.cntrlPriority.SelectedItem).PriorityId;
				retIncident.SeverityId = ((RemoteIncidentSeverity)this.cntrlSeverity.SelectedItem).SeverityId;
				retIncident.DetectedReleaseId = ((RemoteRelease)this.cntrlDetectedIn.SelectedItem).ReleaseId;
				retIncident.ResolvedReleaseId = ((RemoteRelease)this.cntrlResolvedIn.SelectedItem).ReleaseId;
				retIncident.VerifiedReleaseId = ((RemoteRelease)this.cntrlVerifiedIn.SelectedItem).ReleaseId;
				if (this._isDescChanged)
					retIncident.Description = this.cntrlDescription.HTMLText;
				else
					retIncident.Description = this._Incident.Description;

				//*Schedule fields..
				retIncident.StartDate = this.cntrlStartDate.SelectedDate;
				retIncident.ClosedDate = this.cntrlEndDate.SelectedDate;
				retIncident.EstimatedEffort = ((!string.IsNullOrWhiteSpace(this.cntrlEstEffortH.Text.Trim())) ? int.Parse(this.cntrlEstEffortH.Text.Trim()) * 60 : 0) +
					((!string.IsNullOrWhiteSpace(this.cntrlEstEffortM.Text.Trim())) ? int.Parse(this.cntrlEstEffortM.Text.Trim()) : 0);
				retIncident.ActualEffort = ((!string.IsNullOrWhiteSpace(this.cntrlActEffortH.Text.Trim())) ? int.Parse(this.cntrlActEffortH.Text.Trim()) * 60 : 0) +
					((!string.IsNullOrWhiteSpace(this.cntrlActEffortM.Text.Trim())) ? int.Parse(this.cntrlActEffortM.Text.Trim()) : 0);

				//Custom fields..
				foreach (UIElement eleItem in this.gridCustomProperties.Children)
				{
					//Check to see if the item is a control..
					if (eleItem is Control)
					{
						dynamic custControl = eleItem as dynamic;
						RemoteCustomProperty prop = ((eleItem as Control).Tag) as RemoteCustomProperty;

						if (prop != null)
						{
							int? intSelectedList = null;
							string strSelectedText = null;
							if (prop.CustomPropertyTypeId == 1)
								strSelectedText = custControl.Text;
							else if (prop.CustomPropertyTypeId == 2)
								intSelectedList = custControl.SelectedValue;

							switch (prop.CustomPropertyName)
							{
								case "TEXT_01":
									retIncident.Text01 = strSelectedText;
									break;
								case "TEXT_02":
									retIncident.Text02 = strSelectedText;
									break;
								case "TEXT_03":
									retIncident.Text03 = strSelectedText;
									break;
								case "TEXT_04":
									retIncident.Text04 = strSelectedText;
									break;
								case "TEXT_05":
									retIncident.Text05 = strSelectedText;
									break;
								case "TEXT_06":
									retIncident.Text06 = strSelectedText;
									break;
								case "TEXT_07":
									retIncident.Text07 = strSelectedText;
									break;
								case "TEXT_08":
									retIncident.Text08 = strSelectedText;
									break;
								case "TEXT_09":
									retIncident.Text09 = strSelectedText;
									break;
								case "TEXT_10":
									retIncident.Text10 = strSelectedText;
									break;
								case "LIST_01":
									retIncident.List01 = intSelectedList;
									break;
								case "LIST_02":
									retIncident.List02 = intSelectedList;
									break;
								case "LIST_03":
									retIncident.List03 = intSelectedList;
									break;
								case "LIST_04":
									retIncident.List04 = intSelectedList;
									break;
								case "LIST_05":
									retIncident.List05 = intSelectedList;
									break;
								case "LIST_06":
									retIncident.List06 = intSelectedList;
									break;
								case "LIST_07":
									retIncident.List07 = intSelectedList;
									break;
								case "LIST_08":
									retIncident.List08 = intSelectedList;
									break;
								case "LIST_09":
									retIncident.List09 = intSelectedList;
									break;
								case "LIST_10":
									retIncident.List10 = intSelectedList;
									break;
							}
						}
					}
				}

			}
			catch (Exception ex)
			{
				//TODO: Log error here.

				retIncident = null;
			}

			//Return
			return retIncident;
		}
	}
}
