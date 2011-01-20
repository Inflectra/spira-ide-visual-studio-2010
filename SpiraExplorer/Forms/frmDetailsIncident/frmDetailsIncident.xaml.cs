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

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>
	/// Interaction logic for wpfDetailsIncident.xaml
	/// </summary>
	public partial class frmDetailsIncident : UserControl
	{
		private const string CLASS = "frmDetailsIncident:";

		#region Private Data-Changed Vars
		private bool _isDescChanged;
		private bool _isResChanged;
		private bool _isFieldChanged;
		#endregion
		#region Private Mode Vars
		private bool _isLoadingInformation;
		private bool _isWorkflowChanging;
		#endregion

		private TreeViewArtifact _ArtifactDetails;

		/// <summary>Creates a new instance of our IncidentDetailsForm.</summary>
		public frmDetailsIncident()
		{
			InitializeComponent();

			//Load images needed..
			this.imgLoadingIncident.Source = StaticFuncs.getImage("imgInfoWPF", new Size(48, 48)).Source;
			this.imgLoadingError.Source = StaticFuncs.getImage("imgErrorWPF", new Size(48, 48)).Source;
			//Load strings needed..
			this.toolTxtSave.Text = StaticFuncs.getCultureResource.GetString("app_General_Save");
			this.toolTxtRefresh.Text = StaticFuncs.getCultureResource.GetString("app_General_Refresh");
			this.toolTxtLoadWeb.Text = StaticFuncs.getCultureResource.GetString("app_General_ViewBrowser");
			this.toolTxtTimer.Text = StaticFuncs.getCultureResource.GetString("app_General_StartTimer");
			this.mnuTxtActions.Text = StaticFuncs.getCultureResource.GetString("app_Incident_StatusActions") + ":";
			this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Loading");
			this.lblExpanderDetails.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ExpanderDetails");
			this.lblName.Text = StaticFuncs.getCultureResource.GetString("app_General_Name") + ":";
			this.lblTxtToken.Text = StaticFuncs.getCultureResource.GetString("app_General_CopyToClipboard");
			this.lblType.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Type") + ":";
			this.lblStatus.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Status") + ":";
			this.lblDetectedBy.Text = StaticFuncs.getCultureResource.GetString("app_Incident_DetectedBy") + ":";
			this.lblOwnedBy.Text = StaticFuncs.getCultureResource.GetString("app_Incident_OwnedBy") + ":";
			this.lblPriority.Text = StaticFuncs.getCultureResource.GetString("app_General_Priority") + ":";
			this.lblSeverity.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Severity") + ":";
			this.lblDetectedIn.Text = StaticFuncs.getCultureResource.GetString("app_Incident_DetectedRelease") + ":";
			this.lblResolvedIn.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ResolvedRelease") + ":";
			this.lblVerifiedIn.Text = StaticFuncs.getCultureResource.GetString("app_Incident_VerifiedRelease") + ":";
			this.lblDescription.Text = StaticFuncs.getCultureResource.GetString("app_General_Description") + ":";
			this.lblExpanderResolution.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ExpanderResolution");
			this.lblExpanderSchedule.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ExpanderSchedule");
			this.lblPerComplete.Text = StaticFuncs.getCultureResource.GetString("app_Incident_PerComplete") + ":";
			this.lblStartDate.Text = StaticFuncs.getCultureResource.GetString("app_General_StartDate") + ":";
			this.lblEndDate.Text = StaticFuncs.getCultureResource.GetString("app_General_EndDate") + ":";
			this.lblEstEffort.Text = StaticFuncs.getCultureResource.GetString("app_Incident_EstEffort") + ":";
			this.lblActEffort.Text = StaticFuncs.getCultureResource.GetString("app_General_ActEffort") + ":";
			this.lblExpanderCustom.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ExpanderCustom");
			this.lblExpanderAttachments.Text = StaticFuncs.getCultureResource.GetString("app_General_Attachments");
		}

		#region Class Initializers
		public frmDetailsIncident(ToolWindowPane ParentWindow)
			: this()
		{
			this.ParentWindowPane = ParentWindow;
		}

		public frmDetailsIncident(TreeViewArtifact artifactDetails, ToolWindowPane parentWindow)
			: this(parentWindow)
		{
			this.ArtifactDetail = artifactDetails;
		}

		#endregion

		#region Control Event Handlers

		/// <summary>Hit when the user clicks to save the incident.</summary>
		/// <param name="sender">The save button.</param>
		/// <param name="e">Event Args</param>
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			try
			{
				if (this._isFieldChanged)
				{
					string badFields = "";

					if (this.workflow_CheckFieldValues(out badFields))
					{
						this.isInSaveMode = true;
						//Update the form to show we're saving.

						RemoteIncident newIncident = new RemoteIncident();

						//Copy over our base fields..
						newIncident.Name = this.cntrlIncidentName.Text;
						newIncident.IncidentTypeId = ((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.Value;
						//TODO: Convert to handling menu.
						//if (this.cntrlStatus.SelectedItem is RemoteWorkflowIncidentTransition)
						//     newIncident.IncidentStatusId = ((RemoteWorkflowIncidentTransition)this.cntrlStatus.SelectedItem).IncidentStatusId_Output;
						//else if (this.cntrlStatus.SelectedItem is RemoteIncidentStatus)
						//     newIncident.IncidentStatusId = ((RemoteIncidentStatus)this.cntrlStatus.SelectedItem).IncidentStatusId.Value;
						newIncident.OpenerId = ((this.cntrlDetectedBy.SelectedItem.GetType() == typeof(RemoteUser)) ? ((RemoteUser)this.cntrlDetectedBy.SelectedItem).UserId.Value : -1);
						newIncident.OwnerId = ((this.cntrlOwnedBy.SelectedItem.GetType() == typeof(RemoteUser)) ? ((RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId.Value : new int?());
						newIncident.PriorityId = ((this.cntrlPriority.SelectedItem.GetType() == typeof(RemoteIncidentPriority)) ? ((RemoteIncidentPriority)this.cntrlPriority.SelectedItem).PriorityId : new int?());
						newIncident.SeverityId = ((this.cntrlSeverity.SelectedItem.GetType() == typeof(RemoteIncidentSeverity)) ? ((RemoteIncidentSeverity)this.cntrlSeverity.SelectedItem).SeverityId : new int?());
						newIncident.DetectedReleaseId = ((this.cntrlDetectedIn.SelectedItem.GetType() == typeof(RemoteRelease)) ? ((RemoteRelease)this.cntrlDetectedIn.SelectedItem).ReleaseId : new int?());
						newIncident.ResolvedReleaseId = ((this.cntrlResolvedIn.SelectedItem.GetType() == typeof(RemoteRelease)) ? ((RemoteRelease)this.cntrlResolvedIn.SelectedItem).ReleaseId : new int?());
						newIncident.VerifiedReleaseId = ((this.cntrlVerifiedIn.SelectedItem.GetType() == typeof(RemoteRelease)) ? ((RemoteRelease)this.cntrlVerifiedIn.SelectedItem).ReleaseId : new int?());
						if (this._isDescChanged)
							newIncident.Description = this.cntrlDescription.HTMLText;
						else
							newIncident.Description = this._Incident.Description;
						//Now custom properties.
						//TODO: Custom Property fields. (Needs API update.)
						newIncident.List01 = this._Incident.List01;
						newIncident.List02 = this._Incident.List02;
						newIncident.List03 = this._Incident.List03;
						newIncident.List04 = this._Incident.List04;
						newIncident.List05 = this._Incident.List05;
						newIncident.List06 = this._Incident.List06;
						newIncident.List07 = this._Incident.List07;
						newIncident.List08 = this._Incident.List08;
						newIncident.List09 = this._Incident.List09;
						newIncident.List10 = this._Incident.List10;
						newIncident.Text01 = this._Incident.Text01;
						newIncident.Text02 = this._Incident.Text02;
						newIncident.Text03 = this._Incident.Text03;
						newIncident.Text04 = this._Incident.Text04;
						newIncident.Text05 = this._Incident.Text05;
						newIncident.Text06 = this._Incident.Text06;
						newIncident.Text07 = this._Incident.Text07;
						newIncident.Text08 = this._Incident.Text08;
						newIncident.Text09 = this._Incident.Text09;
						newIncident.Text10 = this._Incident.Text10;
						//Schedule fields.
						newIncident.StartDate = this.cntrlStartDate.SelectedDate;
						newIncident.ClosedDate = this.cntrlEndDate.SelectedDate;
						newIncident.CompletionPercent = ((string.IsNullOrEmpty(this.cntrlPerComplete.Text)) ? 0 : int.Parse(this.cntrlPerComplete.Text));
						int? EstH = ((string.IsNullOrEmpty(this.cntrlEstEffortH.Text)) ? new int?() : int.Parse(this.cntrlEstEffortH.Text));
						int? EstM = ((string.IsNullOrEmpty(this.cntrlEstEffortM.Text)) ? new int?() : int.Parse(this.cntrlEstEffortM.Text));
						newIncident.EstimatedEffort = ((!EstH.HasValue && !EstM.HasValue) ? new int?() : (((!EstH.HasValue) ? 0 : EstH.Value * 60) + ((!EstM.HasValue) ? 0 : EstM.Value)));
						int? ActH = ((string.IsNullOrEmpty(this.cntrlActEffortH.Text)) ? new int?() : int.Parse(this.cntrlActEffortH.Text));
						int? ActM = ((string.IsNullOrEmpty(this.cntrlActEffortM.Text)) ? new int?() : int.Parse(this.cntrlActEffortM.Text));
						newIncident.ActualEffort = ((!ActH.HasValue && !ActM.HasValue) ? new int?() : (((!ActH.HasValue) ? 0 : ActH.Value * 60) + ((!ActM.HasValue) ? 0 : ActM.Value)));
						//Now the set fields.
						newIncident.IncidentId = this._Incident.IncidentId;
						newIncident.LastUpdateDate = this._Incident.LastUpdateDate;
						newIncident.CreationDate = this._Incident.CreationDate;
						newIncident.ProjectId = this._Incident.ProjectId;

						//Add a resolution.
						RemoteIncidentResolution newRes = new RemoteIncidentResolution();
						if (this._isResChanged)
						{
							newRes.CreationDate = DateTime.Now;
							newRes.CreatorId = this._Project.UserID;
							newRes.IncidentId = newIncident.IncidentId.Value;
							newRes.Resolution = this.cntrlResolution.HTMLText;
						}
						//this._NumRunning++;
						//this._Client.Incident_UpdateAsync(newIncident, this._NumAsync++);
						//if (this._isResChanged)
						//{
						//     this._NumRunning++;
						//     this._Client.Incident_AddResolutionsAsync(new RemoteIncidentResolution[] { newRes }, this._NumAsync++);
						//}
					}
					else
					{
						string errMsg = "";
						if (badFields.Split(';').Length > 3)
							errMsg = "You must fill out all required fields before saving.";
						else
						{
							errMsg = "The ";
							foreach (string fieldName in badFields.Split(';'))
							{
								errMsg += fieldName + ", ";
							}
							errMsg = errMsg.Trim(' ').Trim(',');
							errMsg += " field" + ((badFields.Split(';').Length > 1) ? "s" : "");
							errMsg += " " + ((badFields.Split(';').Length > 1) ? "are" : "is");
							errMsg += " required before saving.";
						}
						//this.msgErrMessage.Text = errMsg;
						//this.panelError.Visibility = Visibility.Visible;
						//this.panelWarning.Visibility = Visibility.Collapsed;
						//this.panelInfo.Visibility = Visibility.Collapsed;
						//this.panelNone.Visibility = Visibility.Collapsed;
					}

				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::_cntrlSave_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the Type or Status dropdown is changed. Have to reload workflow and update fields.</summary>
		/// <param name="sender">cntrlType / cntrlStatus</param>
		/// <param name="e">Event Args</param>
		private void _cntrlType_Changed(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (!this.IsLoading && !this._isWorkflowChanging)
				{
					this._isWorkflowChanging = true;
					//Show the overlay.
					this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_LoadingWorkflow");
					this.barLoadingIncident.Value = 0;
					this.barLoadingIncident.Maximum = 4;
					this.display_SetStatusWindow(Visibility.Visible);

					//See if they want to or need to confirm..
					MessageBoxResult areTheySure = MessageBoxResult.Yes;
					if (this._isFieldChanged || this._isDescChanged || this._isResChanged)
						areTheySure = MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_WorkflowResetFieldsMessage"), StaticFuncs.getCultureResource.GetString("app_General_AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

					if (areTheySure == MessageBoxResult.Yes)
					{
						//Update window title to indicate there's a change.
						if (((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.Value != this._Incident.IncidentTypeId.Value)
						{
							((IVsWindowFrame)this.ParentWindowPane.Frame).SetProperty((int)__VSFPROPID2.VSFPROPID_OverrideDirtyState, true);
							if (!this.ParentWindowPane.Caption.EndsWith("*"))
							{
								this.ParentWindowPane.Caption = this.ParentWindowPane.Caption + " *";
							}
						}
						else
						{
							((IVsWindowFrame)this.ParentWindowPane.Frame).SetProperty((int)__VSFPROPID2.VSFPROPID_OverrideDirtyState, false);
							if (this.ParentWindowPane.Caption.EndsWith("*"))
							{
								this.ParentWindowPane.Caption = this.ParentWindowPane.Caption.Trim(new char[] { ' ', '*' });
							}
						}

						//Set the selected item..
						if (this.cntrlType.SelectedItem is RemoteIncidentType)
						{
							this._IncSelectedType = ((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.Value;

							//Reset data and then reset combobox.
							this.loadItem_DisplayInformation(this._Incident);
							this.loadItem_PopulateType(this.cntrlType, this._IncSelectedType);

							//Update workflow fields..
							this.workflow_ChangeWorkflowStep();
						}
					}
					else
					{
						//Reset drop-down..
						this.loadItem_PopulateType(this.cntrlType, this._Incident.IncidentTypeId.Value);
						this.display_SetStatusWindow(Visibility.Collapsed);
					}
					this._isWorkflowChanging = false;
				}
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex);
			}
		}

		/// <summary>Hit when a textbox or dropdown list changes.</summary>
		/// <param name="sender">cntrlIncidentName, cntrlDetectedBy, cntrlOwnedBy, cntrlPriority, cntrlSeverity, cntrlDetectedIn, cntrlResolvedIn, cntrlVerifiedIn, cntrlDescription</param>
		/// <param name="e"></param>
		private void _cntrl_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (!this.IsLoading)
				{
					((IVsWindowFrame)this.ParentWindowPane.Frame).SetProperty((int)__VSFPROPID2.VSFPROPID_OverrideDirtyState, true);
					if (!this.ParentWindowPane.Caption.EndsWith("*"))
					{
						this.ParentWindowPane.Caption = this.ParentWindowPane.Caption + " *";
					}

					this._isFieldChanged = true;
					this.btnSave.IsEnabled = true;

					if (sender is cntrlRichTextEditor)
					{
						if (((cntrlRichTextEditor)sender).Name == "cntrlDescription")
						{
							this._isDescChanged = true;
						}
						else if (((cntrlRichTextEditor)sender).Name == "cntrlResolution")
						{
							this._isResChanged = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::TextBlock_MouseDown", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when a toolbar button's Enabled property is changed, to 'grey' out the button images.</summary>
		/// <param name="sender">UIElement</param>
		/// <param name="e">DependencyPropertyChangedEventArgs</param>
		private void toolButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			UIElement control = sender as UIElement;
			if (control != null)
				control.Opacity = ((control.IsEnabled) ? 1 : .5);
		}

		/// <summary>Hit when a toolbar is loaded. Hides the overflow arrow.</summary>
		/// <param name="sender">ToolBar</param>
		/// <param name="e">RoutedEventArgsparam>
		private void _toolbar_Loaded(object sender, EventArgs e)
		{
			ToolBar toolBar = sender as ToolBar;
			var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
			if (overflowGrid != null)
			{
				overflowGrid.Visibility = Visibility.Collapsed;
			}
		}

		/// <summary>Hit when the fadeout is complete.</summary>
		/// <param name="sender">DoubleAnimation</param>
		/// <param name="e">EventArgs</param>
		private void animFadeOut_Completed(object sender, EventArgs e)
		{
			this.panelStatus.Visibility = System.Windows.Visibility.Collapsed;
		}

		/// <summary>Hit when the user clicks the Actions menu.</summary>
		/// <param name="sender">MenuItem</param>
		/// <param name="e">RoutedEventArgs</param>
		private void mnuActions_Click(object sender, RoutedEventArgs e)
		{
			//Show the overlay.
			this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_LoadingWorkflow");
			this.barLoadingIncident.Value = 0;
			this.barLoadingIncident.Maximum = 4;
			this.display_SetStatusWindow(Visibility.Visible);
			this._isWorkflowChanging = true;

			//See if they want to or need to confirm..
			MessageBoxResult areTheySure = MessageBoxResult.Yes;
			if (this._isFieldChanged || this._isDescChanged || this._isResChanged)
				areTheySure = MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_WorkflowResetFieldsMessage"), StaticFuncs.getCultureResource.GetString("app_General_AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

			if (areTheySure == MessageBoxResult.Yes)
			{

				//Get the item they clicked.
				RemoteWorkflowIncidentTransition wkfTrans = (((MenuItem)e.OriginalSource).Header) as RemoteWorkflowIncidentTransition;
				if (wkfTrans != null)
				{
					if (wkfTrans.Name.Trim().StartsWith("»"))
					{
						//They selected a different status, update the selected index and call the workflow.
						this._IncSelectedStatus = wkfTrans.IncidentStatusId_Output;

						//Reset fields..
						this.loadItem_DisplayInformation(this._Incident);

						//Update the field..
						this.cntrlIncidentStatus.Text = wkfTrans.Name;

						//Clear menu items, add the 'revert' menu.
						this.mnuActions.Items.Clear();
						RemoteWorkflowIncidentTransition tempTrans = new RemoteWorkflowIncidentTransition();
						tempTrans.Name = String.Format(StaticFuncs.getCultureResource.GetString("app_Incident_Revert"), this._Incident.IncidentStatusName);
						tempTrans.IncidentStatusId_Output = this._Incident.IncidentStatusId.Value;
						tempTrans.IncidentStatusName_Output = this._Incident.IncidentStatusName;
						this.mnuActions.Items.Add(tempTrans);

						//Update workflow fields..
						this.workflow_ChangeWorkflowStep();


						//Set changed flag.
						((IVsWindowFrame)this.ParentWindowPane.Frame).SetProperty((int)__VSFPROPID2.VSFPROPID_OverrideDirtyState, true);
						if (!this.ParentWindowPane.Caption.EndsWith("*"))
						{
							this.ParentWindowPane.Caption = this.ParentWindowPane.Caption + " *";
						}
					}
					else
					{
						//They reverted. Need to reset things here.
						this._IncSelectedStatus = null;
						this._IncSelectedType = null;

						//Reset fields..
						this.loadItem_DisplayInformation(this._Incident);

						//Update workflow fields..
						this.workflow_ChangeWorkflowStep();

						//Revert changed flag.
						((IVsWindowFrame)this.ParentWindowPane.Frame).SetProperty((int)__VSFPROPID2.VSFPROPID_OverrideDirtyState, false);
						if (this.ParentWindowPane.Caption.EndsWith("*"))
						{
							this.ParentWindowPane.Caption = this.ParentWindowPane.Caption.Trim(new char[] { ' ', '*' });
						}

					}
				}
			}
			else
			{
				this.display_SetStatusWindow(Visibility.Collapsed);
			}
			this._isWorkflowChanging = false;
		}

		/// <summary>Hit when the Timer button's status is changed.</summary>
		/// <param name="sender">btnStartStopTimer</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnStartStopTimer_CheckedChanged(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			//Make sure the value's not null. If it is, we're defaulting to unchecked.
			if (!this.btnStartStopTimer.IsChecked.HasValue) this.btnStartStopTimer.IsChecked = false;

			if (this.btnStartStopTimer.IsChecked.Value)
			{
				this.toolTxtTimer.Text = StaticFuncs.getCultureResource.GetString("app_General_StopTimer");
				//Set the timer..
				this._ArtifactDetails.IsTimed = true;
			}
			else
			{
				this.toolTxtTimer.Text = StaticFuncs.getCultureResource.GetString("app_General_StartTimer");

				//Set the timer.
				this._ArtifactDetails.IsTimed = false;

				//Get the value and add it to the incident.
				TimeSpan workedSpan = this._ArtifactDetails.WorkTime;

				//Add it to the Incident.
				int intActH = 0;
				int intActM = 0;
				if (!string.IsNullOrWhiteSpace(cntrlActEffortH.Text))
				{
					try
					{
						intActH = int.Parse(cntrlActEffortH.Text);
					}
					catch { }
				}
				if (!string.IsNullOrWhiteSpace(cntrlActEffortM.Text))
				{
					try
					{
						intActM = int.Parse(cntrlActEffortM.Text);
					}
					catch { }
				}
				intActH += (workedSpan.Days * 24) + workedSpan.Hours;
				intActM += workedSpan.Minutes;
				//Add it up again..
				TimeSpan newWorked = new TimeSpan(intActH, intActM, 0);
				//Copy new values to the temporary storage fields and the display fields.
				this._tempHoursWorked = ((newWorked.Days * 24) + newWorked.Hours);
				this._tempMinutedWorked = newWorked.Minutes;
				this.cntrlActEffortH.Text = this._tempHoursWorked.ToString();
				this.cntrlActEffortM.Text = this._tempMinutedWorked.ToString();
			}
		}

		/// <summary>Hit when a masked text box's text is changed.</summary>
		/// <param name="sender">MaskedTextBox</param>
		/// <param name="e">TextChangedEventArgs</param>
		private void cntrlMasked_TextChanged(object sender, TextChangedEventArgs e)
		{
			//Simply call the real one.
			this._cntrl_TextChanged(sender, e);
		}

		/// <summary>Hit when the View in Web button is clicked.</summary>
		/// <param name="sender">btnLoadWeb</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnLoadWeb_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			//Fire off the url.
			try
			{
				System.Diagnostics.Process.Start(this._IncidentUrl);
			}
			catch (Exception ex)
			{
				Logger.LogMessage(ex, "Error launching URL: " + this._IncidentUrl);
				MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_ErrorLaunchingUrlMessage"), StaticFuncs.getCultureResource.GetString("app_General_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>Hit when the user clicks the Refresh button.</summary>
		/// <param name="sender">btRefresh</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnRefresh_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			MessageBoxResult isUserSure = MessageBoxResult.Yes;
			if (this._isDescChanged || this._isFieldChanged || this._isResChanged)
			{
				isUserSure = MessageBox.Show(StaticFuncs.getCultureResource.GetString("app_General_LoseChangesMessage"), StaticFuncs.getCultureResource.GetString("app_General_AreYouSure"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
			}

			if (isUserSure == MessageBoxResult.Yes)
			{
				//User is sure, re-launch the loading.
				this.load_LoadItem();
			}
		}

		/// <summary>Hit when the user want to copy the token ID to the clipboard.</summary>
		/// <param name="sender">TextBlock</param>
		/// <param name="e">MouseButtonEventArgs</param>
		private void lblToken_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			e.Handled = true;

			Clipboard.SetText(this._ArtifactDetails.ArtifactIDDisplay);
		}

		#endregion

		#region Properties
		/// <summary>The parent windowframe of the control, for accessing window settings.</summary>
		public ToolWindowPane ParentWindowPane
		{
			get;
			set;
		}

		/// <summary>This specifies whether or not we are in the process of loading data for display.</summary>
		private bool IsLoading
		{
			get
			{
				return this._isLoadingInformation;
			}
			set
			{
				if (this._isLoadingInformation != value)
				{
					if (value)
					{
						this.display_SetStatusWindow(Visibility.Visible);
					}
					else
					{
						this.barLoadingIncident.Value = 1;
						this.display_SetStatusWindow(Visibility.Hidden);
					}

					this._isLoadingInformation = value;
				}
			}
		}

		/// <summary>Returns the string that it to be displayed in the docked tab.</summary>
		public string TabTitle
		{
			get
			{
				if (this._ArtifactDetails != null)
					return this._ArtifactDetails.ArtifactName + " " + this._ArtifactDetails.ArtifactIDDisplay;
				else
					return "";
			}
		}

		/// <summary>The detail item for this display.</summary>
		public TreeViewArtifact ArtifactDetail
		{
			get
			{
				return this._ArtifactDetails;
			}
			set
			{
				//See if they've made any changes..
				this._ArtifactDetails = value;
				this._ArtifactDetails.WorkTimerChanged += new EventHandler(_ArtifactDetails_WorkTimerChanged);
				this._Project = value.ArtifactParentProject.ArtifactTag as SpiraProject;

				//Set tab title.
				if (this.ParentWindowPane != null)
					this.ParentWindowPane.Caption = this.TabTitle;

				//Set isworking flag..
				this.btnStartStopTimer.IsChecked = value.IsTimed;

				//Load details.
				this.load_LoadItem();
			}
		}

		/// <summary>Hit when the worktimer is changed from another source.</summary>
		/// <param name="sender">TreeViewArtifact</param>
		/// <param name="e">EventArgs</param>
		private void _ArtifactDetails_WorkTimerChanged(object sender, EventArgs e)
		{
			this.btnStartStopTimer.IsChecked = this._ArtifactDetails.IsTimed;
		}
		#endregion

		/// <summary>Use to show or hide the Status Window.</summary>
		/// <param name="visiblity">The visibility of the window.</param>
		private void display_SetStatusWindow(Visibility visiblity)
		{
			//Fade in or out the status window...
			switch (visiblity)
			{
				case System.Windows.Visibility.Visible:
					//Set initial values..
					this.panelStatus.Opacity = 0;
					this.panelStatus.Visibility = System.Windows.Visibility.Visible;

					Storyboard storyFadeIn = new Storyboard();
					DoubleAnimation animFadeIn = new DoubleAnimation(0, 1, new TimeSpan(0, 0, 0, 0, 150));
					Storyboard.SetTarget(animFadeIn, this.panelStatus);
					Storyboard.SetTargetProperty(animFadeIn, new PropertyPath(Control.OpacityProperty));
					storyFadeIn.Children.Add(animFadeIn);

					//Start the animation.
					storyFadeIn.Begin();

					break;

				case System.Windows.Visibility.Collapsed:
				case System.Windows.Visibility.Hidden:
				default:
					//Fade it out.
					Storyboard storyFadeOut = new Storyboard();
					DoubleAnimation animFadeOut = new DoubleAnimation(1, 0, new TimeSpan(0, 0, 0, 0, 250));
					Storyboard.SetTarget(animFadeOut, this.panelStatus);
					Storyboard.SetTargetProperty(animFadeOut, new PropertyPath(Control.OpacityProperty));
					animFadeOut.Completed += new EventHandler(animFadeOut_Completed);  //To handle actually hiding the layer.
					storyFadeOut.Children.Add(animFadeOut);

					//Start the animation.
					storyFadeOut.Begin();

					break;
			}
		}
	}
}
