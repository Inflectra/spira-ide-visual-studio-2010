using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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
			this.lblLoadingIncident.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Loading");
			this.lblExpanderDetails.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ExpanderDetails");
			this.lblName.Text = StaticFuncs.getCultureResource.GetString("app_General_Name") + ":";
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
		}

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

		/// <summary>The detail item for this display.</summary>
		public TreeViewArtifact ArtifactDetail
		{
			get
			{
				return this._ArtifactDetails;
			}
			set
			{
				this._ArtifactDetails = value;
				this._Project = value.ArtifactParentProject.ArtifactTag as SpiraProject;

				//TODO: Load details information.
				this.load_LoadItem();
			}
		}

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
						if (this.cntrlStatus.SelectedItem is RemoteWorkflowIncidentTransition)
							newIncident.IncidentStatusId = ((RemoteWorkflowIncidentTransition)this.cntrlStatus.SelectedItem).IncidentStatusId_Output;
						else if (this.cntrlStatus.SelectedItem is RemoteIncidentStatus)
							newIncident.IncidentStatusId = ((RemoteIncidentStatus)this.cntrlStatus.SelectedItem).IncidentStatusId.Value;
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
		private void _cntrlStatusType_Changed(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (!this.isInLoadMode)
				{
					//Update window title to indicate there's a change.
					if (!this.ParentWindowPane.Caption.EndsWith("*"))
						((IVsWindowFrame)this.ParentWindowPane.Frame).SetProperty((int)__VSFPROPID2.VSFPROPID_OverrideDirtyState, true);

					//this.ParentWindowPane.Caption = this.ParentWindowPane.Caption + " *";

					//See if they selected the original setting..
					if ((this.cntrlType.SelectedItem != null) && (this.cntrlStatus.SelectedItem != null))
					{
						//See if they selected the original setting..
						if ((this.cntrlStatus.SelectedItem is RemoteIncidentStatus) &&
							(this.cntrlType.SelectedItem is RemoteIncidentType &&
							((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId == this._IncCurrentType))
						{
							//We're in the original setting, so restore previous workflow.
							this.loadItem_SetEnabledFields(this._IncWkfFields_Current);
						}
						else
						{
							//This is a potentially different workflow, so create the client to go out and get fields.
							ImportExportClient wkfClient = StaticFuncs.CreateClient(this._Project.ServerURL.ToString());
							wkfClient.Connection_Authenticate2Completed += new EventHandler<Connection_Authenticate2CompletedEventArgs>(wkfClient_Connection_Authenticate2Completed);
							wkfClient.Connection_ConnectToProjectCompleted += new EventHandler<Connection_ConnectToProjectCompletedEventArgs>(wkfClient_Connection_ConnectToProjectCompleted);
							wkfClient.Incident_RetrieveWorkflowFieldsCompleted += new EventHandler<Incident_RetrieveWorkflowFieldsCompletedEventArgs>(wkfClient_Incident_RetrieveWorkflowFieldsCompleted);
							wkfClient.Incident_RetrieveWorkflowCustomPropertiesCompleted += new EventHandler<Incident_RetrieveWorkflowCustomPropertiesCompletedEventArgs>(wkfClient_Incident_RetrieveWorkflowCustomPropertiesCompleted);

							//Connect.
							wkfClient.Connection_Authenticate2Async(this._Project.UserName, this._Project.UserPass, StaticFuncs.getCultureResource.GetString("app_ReportName"));
						}
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::_cntrlStatusType_Changed", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when a textbox or dropdown list changes.</summary>
		/// <param name="sender">cntrlIncidentName, cntrlDetectedBy, cntrlOwnedBy, cntrlPriority, cntrlSeverity, cntrlDetectedIn, cntrlResolvedIn, cntrlVerifiedIn, cntrlDescription</param>
		/// <param name="e"></param>
		private void _cntrl_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (!this.isInLoadMode)
				{
					if (!this.ParentWindowPane.Caption.EndsWith("*"))
					{
						((IVsWindowFrame)this.ParentWindowPane.Frame).SetProperty((int)__VSFPROPID2.VSFPROPID_OverrideDirtyState, true);
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

		#endregion

		/// <summary>Generates a pretty Error message string.</summary>
		/// <param name="e">Exception.</param>
		/// <returns>String of the error messages.</returns>
		private string getErrorMessage(Exception e)
		{
			Exception ex = e;
			string errMsg = "» " + ex.Message;
			while (ex.InnerException != null)
			{
				errMsg += Environment.NewLine + "» " + ex.InnerException.Message;
				ex = ex.InnerException;
			}

			return errMsg;
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

		/// <summary>The parent windowframe of the control, for accessing window settings.</summary>
		public ToolWindowPane ParentWindowPane
		{
			get;
			set;
		}

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
						//Show the div..
						this.panelStatus.Visibility = System.Windows.Visibility.Visible;
						this.barLoadingIncident.Value = 0;
					}
					else
					{
						this.barLoadingIncident.Value = 1;

						//Fade it out.
						Storyboard storyFadeOut = new Storyboard();
						DoubleAnimation animFadeOut = new DoubleAnimation(1, 0, new TimeSpan(0, 0, 0, 0, 500));
						Storyboard.SetTarget(animFadeOut, this.panelStatus);
						Storyboard.SetTargetProperty(animFadeOut, new PropertyPath(Control.OpacityProperty));
						animFadeOut.Completed += new EventHandler(animFadeOut_Completed);
						storyFadeOut.Children.Add(animFadeOut);

						//Start the animation.
						storyFadeOut.Begin();
					}

					this._isLoadingInformation = value;
				}
			}
		}

		/// <summary>Hit when the fadeout is complete.</summary>
		/// <param name="sender">DoubleAnimation</param>
		/// <param name="e">EventArgs</param>
		private void animFadeOut_Completed(object sender, EventArgs e)
		{
			this.panelStatus.Visibility = System.Windows.Visibility.Collapsed;
		}
	}
}
