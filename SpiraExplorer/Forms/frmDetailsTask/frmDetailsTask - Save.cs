﻿using System;
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
using System.Linq;
using System.ServiceModel;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>Holds the saving functions for frmDetailsIncident</summary>
	public partial class frmDetailsTask : UserControl
	{
		//Are we currently saving our data?
		private bool _isSavingInformation = false;
		private int _clientNumSaving;
		private RemoteTask _TaskConcurrent;

		/// <summary>Hit when the user wants to save the incident.</summary>
		/// <param name="sender">The save button.</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			try
			{
				this.barSavingTask.Value = -5;
				this.barSavingTask.Maximum = 0;
				this.barSavingTask.Minimum = -5;

				if (this._isFieldChanged || this._isResChanged || this._isDescChanged)
				{
					//Set working flag.
					this.IsSaving = true;

					//Get the new values from the form..
					RemoteTask newTask = this.save_GetFromFields();

					if (newTask != null)
					{
						//Create a client, and save incident and resolution..
						ImportExportClient clientSave = StaticFuncs.CreateClient(((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).ServerURL.ToString());
						clientSave.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(clientSave_Connection_Authenticate2Completed);
						clientSave.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(clientSave_Connection_ConnectToProjectCompleted);
						clientSave.Task_UpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientSave_Task_UpdateCompleted);
						clientSave.Task_CreateCommentCompleted += new EventHandler<Task_CreateCommentCompletedEventArgs>(clientSave_Task_CreateCommentCompleted);
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
				Logger.LogMessage(ex, "Clicked to Save the Task");
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
			this.barSavingTask.Value++;

			//See if it's okay to reload.
			this.save_CheckIfOkayToLoad();
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
			this.barSavingTask.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					if (e.Result)
					{
						//Get the new RemoteIncident
						RemoteTask newTask = this.save_GetFromFields();

						if (newTask != null)
						{
							//Fire off our update calls.
							this._clientNumSaving++;
							client.Task_UpdateAsync(newTask, this._clientNum++);
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
			this.barSavingTask.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					if (e.Result)
					{
						//Connect to the project ID.
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

		/// <summary>Hit when the client is finished updating the task.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">AsyncCompletedEventArgs</param>
		private void clientSave_Task_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			const string METHOD = "clientSave_Incident_UpdateCompleted()";
			Logger.LogTrace(CLASS + METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

			ImportExportClient client = (sender as ImportExportClient);
			this._clientNumSaving--;
			this.barSavingTask.Value++;

			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					//See if we need to add a resolution.
					if (this._isResChanged)
					{
						//We need to save a resolution.
						RemoteComment newRes = new RemoteComment();
						newRes.CreationDate = DateTime.Now;
						newRes.UserId = ((SpiraProject)this._ArtifactDetails.ArtifactParentProject.ArtifactTag).UserID;
						newRes.ArtifactId = this._ArtifactDetails.ArtifactId;
						newRes.Text = this.cntrlResolution.HTMLText;

						this._clientNumSaving++;
						client.Task_CreateCommentAsync(newRes, this._clientNum++);
					}
					else
					{
						//We're finished.
						this.barSavingTask.Value++;
						this._clientNumSaving++;
						client.Connection_DisconnectAsync(this._clientNum++);
					}
				}
				else
				{
					//Log error.
					Logger.LogMessage(e.Error, "Saving Incident Changes to Database");

					//If we get a concurrency error, get the current data.
					if (e.Error is FaultException<ServiceFaultMessage> && ((FaultException<ServiceFaultMessage>)e.Error).Detail.Type == "DataAccessConcurrencyException")
					{
						client.Task_RetrieveByIdCompleted += new EventHandler<Task_RetrieveByIdCompletedEventArgs>(clientSave_Task_RetrieveByIdCompleted);

						//Fire it off.
						this._clientNumSaving++;
						client.Incident_RetrieveByIdAsync(this._ArtifactDetails.ArtifactId, this._clientNum++);
					}
					else
					{
						//Display the error screen here.

						//Cancel calls.
						this._clientNumSaving++;
						client.Connection_DisconnectAsync(this._clientNum++);
					}
				}
			}

			//See if it's okay to reload.
			this.save_CheckIfOkayToLoad();

			Logger.LogTrace(CLASS + METHOD + " Exit: " + this._clientNumSaving.ToString() + " left.");
		}

		/// <summary>Hit when we had a concurrency issue, and had to reload the task.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Task_RetrieveByIdCompletedEventArgs</param>
		private void clientSave_Task_RetrieveByIdCompleted(object sender, Task_RetrieveByIdCompletedEventArgs e)
		{
			const string METHOD = "clientSave_Incident_RetrieveByIdCompleted()";
			Logger.LogTrace(CLASS + METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

			ImportExportClient client = (sender as ImportExportClient);
			this._clientNumSaving--;
			this.barSavingTask.Value++;


			if (!e.Cancelled)
			{
				if (e.Error == null)
				{
					//We got new information here. Let's see if it can be merged.
					bool canBeMerged = this.save_CheckIfConcurrencyCanBeMerged(e.Result);
					this._TaskConcurrent = e.Result;

					if (canBeMerged)
					{
						this.gridLoadingError.Visibility = System.Windows.Visibility.Collapsed;
						this.gridSavingConcurrencyMerge.Visibility = System.Windows.Visibility.Visible;
						this.gridSavingConcurrencyNoMerge.Visibility = System.Windows.Visibility.Collapsed;
						this.display_SetOverlayWindow(this.panelSaving, System.Windows.Visibility.Hidden);
						this.display_SetOverlayWindow(this.panelError, System.Windows.Visibility.Visible);

						//Save the client to the 'Merge' button.
						this.btnConcurrencyMergeYes.Tag = sender;
					}
					else
					{
						//TODO: Display error message here, tell users they must refresh their data.
						this.gridLoadingError.Visibility = System.Windows.Visibility.Collapsed;
						this.gridSavingConcurrencyMerge.Visibility = System.Windows.Visibility.Collapsed;
						this.gridSavingConcurrencyNoMerge.Visibility = System.Windows.Visibility.Visible;
						this.display_SetOverlayWindow(this.panelSaving, System.Windows.Visibility.Hidden);
						this.display_SetOverlayWindow(this.panelError, System.Windows.Visibility.Visible);
					}
				}
				else
				{
					//We even errored on retrieving information. Somethin's really wrong here.
					//Display error.
					Logger.LogMessage(e.Error, "Getting updated Concurrency Incident");
				}
			}

			Logger.LogTrace(CLASS + METHOD + " Exit");
		}

		/// <summary>Hit when the client is finished adding a new comment.</summary>
		/// <param name="sender">ImportExportClient</param>
		/// <param name="e">Task_CreateCommentCompletedEventArgs</param>
		private void clientSave_Task_CreateCommentCompleted(object sender, Task_CreateCommentCompletedEventArgs e)
		{
			const string METHOD = "clientSave_Incident_AddResolutionsCompleted()";
			Logger.LogTrace(CLASS + METHOD + " Enter: " + this._clientNumSaving.ToString() + " running.");

			ImportExportClient client = (sender as ImportExportClient);
			this._clientNumSaving--;
			this.barSavingTask.Value++;

			if (!e.Cancelled)
			{
				if (e.Error != null)
				{
					//Log message.
					Logger.LogMessage(e.Error, "Adding Comment to Incident");
					//Display error that the item saved, but adding the new resolution didn't.
					MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_Incident_AddCommentErrorMessage"), StaticFuncs.getCultureResource.GetString("app_Incident_UpdateError"), MessageBoxButton.OK, MessageBoxImage.Error);
				}

				//Regardless of what happens, we're disconnecting here.
				this._clientNumSaving++;
				client.Connection_DisconnectAsync(this._clientNum++);
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
				this.lblLoadingTask.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Refreshing");
				this.load_LoadItem();
			}

			Logger.LogTrace(CLASS + METHOD + " Exit");
		}

		/// <summary>Returns whether the given Concurrent Incident can be safely merged with the user's values.</summary>
		/// <param name="moddedTask">The concurrent incident.</param>
		private bool save_CheckIfConcurrencyCanBeMerged(RemoteTask moddedTask)
		{
			bool retValue = false;

			//Get current values..
			RemoteTask userTask = this.save_GetFromFields();

			if (userTask != null && moddedTask != null)
			{
				//Okay, check all fields. We want to see if a user-changed field (userTask) was also
				//   changed by someone else. If it was, we return false (they cannot be merged). Otherwise,
				//   we return true (they can be merged).
				//So we check the user-entered field against the original field. If they are different,
				//   check the original field against the concurrent field. If they are different, return
				//   false. Otherwise to both if's, return true.
				//We just loop through all available fields. The fielNum here has no reference to workflow
				//   field ID, _WorkflowFields is just used to get the count of fields to check against.
				int fieldNum = 1;
				bool fieldCheck = true;
				while (fieldNum <= 34 && fieldCheck == true)
				{
					switch (fieldNum)
					{
						case 1:
							if (userTask.ActualEffort != this._Task.ActualEffort) fieldCheck = (this._Task.ActualEffort == moddedTask.ActualEffort);
							break;
						case 2:
							if (userTask.CreationDate != this._Task.CreationDate) fieldCheck = (this._Task.CreationDate == moddedTask.CreationDate);
							break;
						case 3:
							if (userTask.CreatorId != this._Task.CreatorId) fieldCheck = (this._Task.CreatorId == moddedTask.CreatorId);
							break;
						case 4:
							if (StaticFuncs.StripTagsCharArray(userTask.Description).ToLowerInvariant().Trim() != StaticFuncs.StripTagsCharArray(this._Task.Description).ToLowerInvariant().Trim()) fieldCheck = (StaticFuncs.StripTagsCharArray(this._Task.Description).ToLowerInvariant().Trim() == StaticFuncs.StripTagsCharArray(moddedTask.Description).ToLowerInvariant().Trim());
							break;
						case 5:
							if (userTask.EndDate != this._Task.EndDate) fieldCheck = (this._Task.EndDate == moddedTask.EndDate);
							break;
						case 6:
							if (userTask.EstimatedEffort != this._Task.EstimatedEffort) fieldCheck = (this._Task.EstimatedEffort == moddedTask.EstimatedEffort);
							break;
						case 7:
							if (userTask.List01 != this._Task.List01) fieldCheck = (this._Task.List01 == moddedTask.List01);
							break;
						case 8:
							if (userTask.List02 != this._Task.List02) fieldCheck = (this._Task.List02 == moddedTask.List02);
							break;
						case 9:
							if (userTask.List03 != this._Task.List03) fieldCheck = (this._Task.List03 == moddedTask.List03);
							break;
						case 10:
							if (userTask.List04 != this._Task.List04) fieldCheck = (this._Task.List04 == moddedTask.List04);
							break;
						case 11:
							if (userTask.List05 != this._Task.List05) fieldCheck = (this._Task.List05 == moddedTask.List05);
							break;
						case 12:
							if (userTask.List06 != this._Task.List06) fieldCheck = (this._Task.List06 == moddedTask.List06);
							break;
						case 13:
							if (userTask.List07 != this._Task.List07) fieldCheck = (this._Task.List07 == moddedTask.List07);
							break;
						case 14:
							if (userTask.List08 != this._Task.List08) fieldCheck = (this._Task.List08 == moddedTask.List08);
							break;
						case 15:
							if (userTask.List09 != this._Task.List09) fieldCheck = (this._Task.List09 == moddedTask.List09);
							break;
						case 16:
							if (userTask.List10 != this._Task.List10) fieldCheck = (this._Task.List10 == moddedTask.List10);
							break;
						case 17:
							if (userTask.Name.TrimEquals(this._Task.Name)) fieldCheck = (this._Task.Name.TrimEquals(moddedTask.Name));
							break;
						case 18:
							if (userTask.OwnerId != this._Task.OwnerId) fieldCheck = (this._Task.OwnerId == moddedTask.OwnerId);
							break;
						case 19:
							if (userTask.ReleaseId != this._Task.ReleaseId) fieldCheck = (this._Task.ReleaseId == moddedTask.ReleaseId);
							break;
						case 20:
							if (userTask.RemainingEffort != this._Task.RemainingEffort) fieldCheck = (this._Task.RemainingEffort == moddedTask.RemainingEffort);
							break;
						case 21:
							if (userTask.RequirementId != this._Task.RequirementId) fieldCheck = (this._Task.RequirementId == moddedTask.RequirementId);
							break;
						case 22:
							if (userTask.StartDate != this._Task.StartDate) fieldCheck = (this._Task.StartDate == moddedTask.StartDate);
							break;
						case 23:
							if (userTask.TaskPriorityId != this._Task.TaskPriorityId) fieldCheck = (this._Task.TaskPriorityId == moddedTask.TaskPriorityId);
							break;
						case 24:
							if (userTask.TaskStatusId != this._Task.TaskStatusId) fieldCheck = (this._Task.TaskStatusId == moddedTask.TaskStatusId);
							break;
						case 25:
							if (userTask.Text01.TrimEquals(this._Task.Text01)) fieldCheck = (this._Task.Text01.TrimEquals(moddedTask.Text01));
							break;
						case 26:
							if (userTask.Text02.TrimEquals(this._Task.Text02)) fieldCheck = (this._Task.Text02.TrimEquals(moddedTask.Text02));
							break;
						case 27:
							if (userTask.Text03.TrimEquals(this._Task.Text03)) fieldCheck = (this._Task.Text03.TrimEquals(moddedTask.Text03));
							break;
						case 28:
							if (userTask.Text04.TrimEquals(this._Task.Text04)) fieldCheck = (this._Task.Text04.TrimEquals(moddedTask.Text04));
							break;
						case 29:
							if (userTask.Text05.TrimEquals(this._Task.Text05)) fieldCheck = (this._Task.Text05.TrimEquals(moddedTask.Text05));
							break;
						case 30:
							if (userTask.Text06.TrimEquals(this._Task.Text06)) fieldCheck = (this._Task.Text06.TrimEquals(moddedTask.Text06));
							break;
						case 31:
							if (userTask.Text07.TrimEquals(this._Task.Text07)) fieldCheck = (this._Task.Text07.TrimEquals(moddedTask.Text07));
							break;
						case 32:
							if (userTask.Text08.TrimEquals(this._Task.Text08)) fieldCheck = (this._Task.Text08.TrimEquals(moddedTask.Text08));
							break;
						case 33:
							if (userTask.Text09.TrimEquals(this._Task.Text09)) fieldCheck = (this._Task.Text09.TrimEquals(moddedTask.Text09));
							break;
						case 34:
							if (userTask.Text10.TrimEquals(this._Task.Text10)) fieldCheck = (this._Task.Text10.TrimEquals(moddedTask.Text10));
							break;
					}
					fieldNum++;
				}
				retValue = fieldCheck;
			}
			return retValue;
		}

		/// <summary>Copies over our values from the form into an Incident object.</summary>
		/// <returns>A new RemoteIncident, or Null if error.</returns>
		private RemoteTask save_GetFromFields()
		{
			const string METHOD = "save_GetFromFields()";

			RemoteTask retTask = null;
			try
			{
				retTask = new RemoteTask();

				//*Fixed fields..
				retTask.TaskId = this._Task.TaskId;
				retTask.ProjectId = this._Task.ProjectId;
				retTask.CreationDate = this._Task.CreationDate;
				retTask.LastUpdateDate = this._Task.LastUpdateDate;
				retTask.RequirementId = this._Task.RequirementId;

				//*Standard fields..
				retTask.Name = this.cntrlTaskName.Text.Trim();
				retTask.TaskPriorityId = (int?)this.cntrlPriority.SelectedValue;
				retTask.CreatorId = ((RemoteUser)this.cntrlDetectedBy.SelectedItem).UserId;
				retTask.OwnerId = ((RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId;
				retTask.ReleaseId = ((RemoteRelease)this.cntrlDetectedIn.SelectedItem).ReleaseId;
				if (this._isDescChanged)
					retTask.Description = this.cntrlDescription.HTMLText;
				else
					retTask.Description = this._Task.Description;

				//*Schedule fields..
				retTask.StartDate = this.cntrlStartDate.SelectedDate;
				retTask.EndDate = this.cntrlEndDate.SelectedDate;
				string strEstEffortH = this.cntrlEstEffortH.Text.Replace('_', ' ').Trim();
				string strEstEffortM = this.cntrlEstEffortM.Text.Replace('_', ' ').Trim();
				if (string.IsNullOrWhiteSpace(strEstEffortH) && string.IsNullOrWhiteSpace(strEstEffortM))
					retTask.EstimatedEffort = null;
				else
					retTask.EstimatedEffort = int.Parse(strEstEffortH) * 60 + int.Parse(strEstEffortM);
				string strActEffortH = this.cntrlActEffortH.Text.Replace('_', ' ').Trim();
				string strActEffortM = this.cntrlActEffortM.Text.Replace('_', ' ').Trim();
				if (string.IsNullOrWhiteSpace(strActEffortH) && string.IsNullOrWhiteSpace(strActEffortM))
					retTask.ActualEffort = null;
				else
					retTask.ActualEffort = int.Parse(strActEffortH) * 60 + int.Parse(strActEffortM);
				string strRemEffortH = this.cntrlRemEffortH.Text.Replace('_', ' ').Trim();
				string strRemEffortM = this.cntrlRemEffortM.Text.Replace('_', ' ').Trim();
				if (string.IsNullOrWhiteSpace(strRemEffortH) && string.IsNullOrWhiteSpace(strRemEffortM))
					retTask.RemainingEffort = null;
				else
					retTask.RemainingEffort = int.Parse(strRemEffortH) * 60 + int.Parse(strRemEffortM);

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
									retTask.Text01 = strSelectedText;
									break;
								case "TEXT_02":
									retTask.Text02 = strSelectedText;
									break;
								case "TEXT_03":
									retTask.Text03 = strSelectedText;
									break;
								case "TEXT_04":
									retTask.Text04 = strSelectedText;
									break;
								case "TEXT_05":
									retTask.Text05 = strSelectedText;
									break;
								case "TEXT_06":
									retTask.Text06 = strSelectedText;
									break;
								case "TEXT_07":
									retTask.Text07 = strSelectedText;
									break;
								case "TEXT_08":
									retTask.Text08 = strSelectedText;
									break;
								case "TEXT_09":
									retTask.Text09 = strSelectedText;
									break;
								case "TEXT_10":
									retTask.Text10 = strSelectedText;
									break;
								case "LIST_01":
									retTask.List01 = intSelectedList;
									break;
								case "LIST_02":
									retTask.List02 = intSelectedList;
									break;
								case "LIST_03":
									retTask.List03 = intSelectedList;
									break;
								case "LIST_04":
									retTask.List04 = intSelectedList;
									break;
								case "LIST_05":
									retTask.List05 = intSelectedList;
									break;
								case "LIST_06":
									retTask.List06 = intSelectedList;
									break;
								case "LIST_07":
									retTask.List07 = intSelectedList;
									break;
								case "LIST_08":
									retTask.List08 = intSelectedList;
									break;
								case "LIST_09":
									retTask.List09 = intSelectedList;
									break;
								case "LIST_10":
									retTask.List10 = intSelectedList;
									break;
							}
						}
					}
				}

			}
			catch (Exception ex)
			{
				//TODO: Log error here.

				retTask = null;
			}

			//Return
			return retTask;
		}

		#region Concurrency Button Events
		/// <summary>Hit when the user does not want to save, and is forced to refresh the loaded data.</summary>
		/// <param name="sender">btnConcurrencyMergeNo, btnConcurrencyRefresh</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnConcurrencyRefresh_Click(object sender, RoutedEventArgs e)
		{
			//Hide the error panel, jump to loading..
			this.display_SetOverlayWindow(this.panelError, System.Windows.Visibility.Collapsed);
			this.display_SetOverlayWindow(this.panelStatus, System.Windows.Visibility.Visible);
			this.lblLoadingTask.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Refreshing");

			this.load_LoadItem();
		}

		/// <summary>Hit when the user wants to merge their changes with the concurrent incident.</summary>
		/// <param name="sender">btnConcurrencyMergeYes</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnConcurrencyMergeYes_Click(object sender, RoutedEventArgs e)
		{
			try
			{

				e.Handled = true;
				//Get the client.
				ImportExportClient client = ((dynamic)sender).Tag as ImportExportClient;
				if (client != null)
				{
					//Switch screens again...
					this.display_SetOverlayWindow(this.panelSaving, System.Windows.Visibility.Visible);
					this.display_SetOverlayWindow(this.panelError, System.Windows.Visibility.Hidden);
					this.barSavingTask.Value--;

					//Re-launch the saving..
					RemoteIncident incMerged = this.save_MergeConcurrency(this.save_GetFromFields(), this._TaskConcurrent, this._Task);

					this._clientNumSaving++;
					client.Incident_UpdateAsync(incMerged, this._clientNum++);
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "Resaving saved Incident");
				//TODO: Clean up here.
			}
		}
		#endregion

		/// <summary>Merges two RemoteIncidents into one for re-saving.</summary>
		/// <param name="incUserSaved">The user-saved incident to merge with the Concurrent incident.</param>
		/// <param name="incConcurrent">The concurrent incident to merge with the User incident.</param>
		/// <param name="incOriginal">The original unchanged incident used for reference.</param>
		/// <returns>A new RemoteIncident suitable for saving.</returns>
		/// <remarks>This should only be called when it is known that there are no conflicting values between the User-Saved incident and the Concurrent incident.</remarks>
		private RemoteIncident save_MergeConcurrency(RemoteIncident incUserSaved, RemoteIncident incConcurrent, RemoteIncident incOriginal)
		{
			//If the field was not changed by the user (incUserSaved == incOriginal), then use the incConcurrent. (Assuming that the
			// incConcurrent has a possible updated value.
			//Otherwise, use the incUserSaved value.
			try
			{
				RemoteIncident retIncident = new RemoteIncident();

				retIncident.ActualEffort = ((incUserSaved.ActualEffort == incOriginal.ActualEffort) ? incConcurrent.ActualEffort : incUserSaved.ActualEffort);
				retIncident.ClosedDate = ((incUserSaved.ClosedDate == incOriginal.ClosedDate) ? incConcurrent.ClosedDate : incUserSaved.ClosedDate);
				retIncident.CreationDate = incOriginal.CreationDate;
				string strDescUser = StaticFuncs.StripTagsCharArray(incUserSaved.Description);
				string strDescOrig = StaticFuncs.StripTagsCharArray(incOriginal.Description);
				retIncident.Description = ((strDescOrig.TrimEquals(strDescOrig)) ? incConcurrent.Description : incUserSaved.Description);
				retIncident.DetectedReleaseId = ((incUserSaved.DetectedReleaseId == incOriginal.DetectedReleaseId) ? incConcurrent.DetectedReleaseId : incUserSaved.DetectedReleaseId);
				retIncident.EstimatedEffort = ((incUserSaved.EstimatedEffort == incOriginal.EstimatedEffort) ? incConcurrent.EstimatedEffort : incUserSaved.EstimatedEffort);
				retIncident.IncidentId = incOriginal.IncidentId;
				retIncident.IncidentStatusId = ((incUserSaved.IncidentStatusId == incOriginal.IncidentStatusId) ? incConcurrent.IncidentStatusId : incUserSaved.IncidentStatusId);
				retIncident.IncidentTypeId = ((incUserSaved.IncidentTypeId == incOriginal.IncidentTypeId) ? incConcurrent.IncidentTypeId : incUserSaved.IncidentTypeId);
				retIncident.LastUpdateDate = incConcurrent.LastUpdateDate;
				retIncident.List01 = ((incUserSaved.List01 == incOriginal.List01) ? incConcurrent.List01 : incUserSaved.List01);
				retIncident.List02 = ((incUserSaved.List02 == incOriginal.List02) ? incConcurrent.List02 : incUserSaved.List02);
				retIncident.List03 = ((incUserSaved.List03 == incOriginal.List03) ? incConcurrent.List03 : incUserSaved.List03);
				retIncident.List04 = ((incUserSaved.List04 == incOriginal.List04) ? incConcurrent.List04 : incUserSaved.List04);
				retIncident.List05 = ((incUserSaved.List05 == incOriginal.List05) ? incConcurrent.List05 : incUserSaved.List05);
				retIncident.List06 = ((incUserSaved.List06 == incOriginal.List06) ? incConcurrent.List06 : incUserSaved.List06);
				retIncident.List07 = ((incUserSaved.List07 == incOriginal.List07) ? incConcurrent.List07 : incUserSaved.List07);
				retIncident.List08 = ((incUserSaved.List08 == incOriginal.List08) ? incConcurrent.List08 : incUserSaved.List08);
				retIncident.List09 = ((incUserSaved.List09 == incOriginal.List09) ? incConcurrent.List09 : incUserSaved.List09);
				retIncident.List10 = ((incUserSaved.List10 == incOriginal.List10) ? incConcurrent.List10 : incUserSaved.List10);
				retIncident.Name = ((incUserSaved.Name.TrimEquals(incOriginal.Name)) ? incConcurrent.Name : incUserSaved.Name);
				retIncident.OpenerId = ((incUserSaved.OpenerId == incOriginal.OpenerId) ? incConcurrent.OpenerId : incUserSaved.OpenerId);
				retIncident.OwnerId = ((incUserSaved.OwnerId == incOriginal.OwnerId) ? incConcurrent.OwnerId : incUserSaved.OwnerId);
				retIncident.PriorityId = ((incUserSaved.PriorityId == incOriginal.PriorityId) ? incConcurrent.PriorityId : incUserSaved.PriorityId);
				retIncident.ProjectId = incOriginal.ProjectId;
				retIncident.RemainingEffort = ((incUserSaved.RemainingEffort == incOriginal.RemainingEffort) ? incConcurrent.RemainingEffort : incUserSaved.RemainingEffort);
				retIncident.ResolvedReleaseId = ((incUserSaved.ResolvedReleaseId == incOriginal.ResolvedReleaseId) ? incConcurrent.ResolvedReleaseId : incUserSaved.ResolvedReleaseId);
				retIncident.SeverityId = ((incUserSaved.SeverityId == incOriginal.SeverityId) ? incConcurrent.SeverityId : incUserSaved.SeverityId);
				retIncident.StartDate = ((incUserSaved.StartDate == incOriginal.StartDate) ? incConcurrent.StartDate : incUserSaved.StartDate);
				retIncident.TestRunStepId = ((incUserSaved.TestRunStepId == incOriginal.TestRunStepId) ? incConcurrent.TestRunStepId : incUserSaved.TestRunStepId);
				retIncident.Text01 = ((retIncident.Text01.TrimEquals(incOriginal.Text01)) ? incConcurrent.Text01 : incUserSaved.Text01);
				retIncident.Text02 = ((retIncident.Text02.TrimEquals(incOriginal.Text02)) ? incConcurrent.Text02 : incUserSaved.Text02);
				retIncident.Text03 = ((retIncident.Text03.TrimEquals(incOriginal.Text03)) ? incConcurrent.Text03 : incUserSaved.Text03);
				retIncident.Text04 = ((retIncident.Text04.TrimEquals(incOriginal.Text04)) ? incConcurrent.Text04 : incUserSaved.Text04);
				retIncident.Text05 = ((retIncident.Text05.TrimEquals(incOriginal.Text05)) ? incConcurrent.Text05 : incUserSaved.Text05);
				retIncident.Text06 = ((retIncident.Text06.TrimEquals(incOriginal.Text06)) ? incConcurrent.Text06 : incUserSaved.Text06);
				retIncident.Text07 = ((retIncident.Text07.TrimEquals(incOriginal.Text07)) ? incConcurrent.Text07 : incUserSaved.Text07);
				retIncident.Text08 = ((retIncident.Text08.TrimEquals(incOriginal.Text01)) ? incConcurrent.Text08 : incUserSaved.Text08);
				retIncident.Text09 = ((retIncident.Text09.TrimEquals(incOriginal.Text09)) ? incConcurrent.Text09 : incUserSaved.Text09);
				retIncident.Text10 = ((retIncident.Text10.TrimEquals(incOriginal.Text10)) ? incConcurrent.Text10 : incUserSaved.Text10);
				retIncident.VerifiedReleaseId = ((incUserSaved.VerifiedReleaseId == incOriginal.VerifiedReleaseId) ? incConcurrent.VerifiedReleaseId : incUserSaved.VerifiedReleaseId);

				//Return our new incident.
				return retIncident;
			}
			catch (Exception ex)
			{
				//Log error, return null.
				Logger.LogMessage(ex, "Combining concurrent Incident.");
				return null;
			}
		}
	}
}