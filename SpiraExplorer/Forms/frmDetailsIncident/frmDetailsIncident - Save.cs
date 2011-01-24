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
		private bool isInSaveMode = false;

		/// <summary>Hit when the user wants to save the incident.</summary>
		/// <param name="sender">The save button.</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			RemoteIncident test = this.save_GetFromFields();

			try
			{
				if (this._isFieldChanged)
				{
					//Set working flag.
					this.isInSaveMode = true;

					//Get the new values from the form..
					RemoteIncident newIncident = this.save_GetFromFields();

					if (newIncident != null && this.save_CheckFieldValues())
					{
					}

					//if (this.workflow_CheckFieldValues(out badFields))
					//{

					//    //Update the form to show we're saving.

					//    RemoteIncident newIncident = new RemoteIncident();

					//    //Add a resolution.
					//    RemoteIncidentResolution newRes = new RemoteIncidentResolution();
					//    if (this._isResChanged)
					//    {
					//        newRes.CreationDate = DateTime.Now;
					//        newRes.CreatorId = this._Project.UserID;
					//        newRes.IncidentId = newIncident.IncidentId.Value;
					//        newRes.Resolution = this.cntrlResolution.HTMLText;
					//    }
					//    //this._NumRunning++;
					//    //this._Client.Incident_UpdateAsync(newIncident, this._NumAsync++);
					//    //if (this._isResChanged)
					//    //{
					//    //     this._NumRunning++;
					//    //     this._Client.Incident_AddResolutionsAsync(new RemoteIncidentResolution[] { newRes }, this._NumAsync++);
					//    //}
					//}
					//else
					//{
					//    string errMsg = "";
					//    if (badFields.Split(';').Length > 3)
					//        errMsg = "You must fill out all required fields before saving.";
					//    else
					//    {
					//        errMsg = "The ";
					//        foreach (string fieldName in badFields.Split(';'))
					//        {
					//            errMsg += fieldName + ", ";
					//        }
					//        errMsg = errMsg.Trim(' ').Trim(',');
					//        errMsg += " field" + ((badFields.Split(';').Length > 1) ? "s" : "");
					//        errMsg += " " + ((badFields.Split(';').Length > 1) ? "are" : "is");
					//        errMsg += " required before saving.";
					//    }
					//    //this.msgErrMessage.Text = errMsg;
					//    //this.panelError.Visibility = Visibility.Visible;
					//    //this.panelWarning.Visibility = Visibility.Collapsed;
					//    //this.panelInfo.Visibility = Visibility.Collapsed;
					//    //this.panelNone.Visibility = Visibility.Collapsed;
					//}

				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::_cntrlSave_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
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
