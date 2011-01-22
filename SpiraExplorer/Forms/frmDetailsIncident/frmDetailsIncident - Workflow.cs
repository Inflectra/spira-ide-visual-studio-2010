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
		private Dictionary<int, int> _IncWkfFields_Current;
		private Dictionary<int, int> _IncWkfCustom_Current;
		private Dictionary<int, int> _IncWkfFields_Updated;
		private Dictionary<int, int> _IncWkfCustom_Updated;
		private int? _IncCurrentType;
		private int? _IncCurrentStatus;
		private int? _IncSelectedType;
		private int? _IncSelectedStatus;

		/// <summary>Checks if all required fields are entered correctly. Will highlight those that aren't.</summary>
		/// <returns>Boolean - true if all is okay, false if there is an error.</returns>
		private bool workflow_CheckRequiredFields()
		{
			bool retValue = true;

			Dictionary<int, int> SelectedWorkflowFields = this._IncWkfFields_Current;
			Dictionary<int, int> SelectedWorkflowCustom = this._IncWkfCustom_Current;

			//See if we need to use the updated workflow.
			if (this._IncSelectedStatus.HasValue || this._IncSelectedType.HasValue)
			{
				SelectedWorkflowFields = this._IncWkfFields_Updated;
				SelectedWorkflowCustom = this._IncWkfCustom_Updated;
			}

			//We're using the Updated Workflow. First scan normal fields.
			foreach (KeyValuePair<int, int> wkfDefinition in SelectedWorkflowFields)
			{
				if (wkfDefinition.Value == 2)
				{
					//It is required. Get the field.

				}
			}
			//Now scan custom fields.


			if (string.IsNullOrEmpty(this.cntrlIncidentName.Text.Trim()))
			{
				retValue = false;
				this.cntrlIncidentName.Tag = 2.ToString();
			}

			if (this.lblType.FontWeight == FontWeights.Bold && !((RemoteIncidentType)this.cntrlType.SelectedItem).IncidentTypeId.HasValue)
			{
				retValue = false;
				this.cntrlType.Tag = 2.ToString();
			}

			if (this.lblDetectedBy.FontWeight == FontWeights.Bold && !((RemoteUser)this.cntrlDetectedBy.SelectedItem).UserId.HasValue)
			{
				retValue = false;
				this.cntrlDetectedBy.Tag = 2.ToString();
			}

			if (this.lblOwnedBy.FontWeight == FontWeights.Bold && !((RemoteUser)this.cntrlOwnedBy.SelectedItem).UserId.HasValue)
			{
				retValue = false;
				this.cntrlOwnedBy.Tag = 2.ToString();
			}

			if (this.lblPriority.FontWeight == FontWeights.Bold && !((RemoteIncidentPriority)this.cntrlPriority.SelectedItem).PriorityId.HasValue)
			{
				retValue = false;
				this.cntrlPriority.Tag = 2.ToString();
			}

			if (this.lblSeverity.FontWeight == FontWeights.Bold && !((RemoteIncidentSeverity)this.cntrlSeverity.SelectedItem).SeverityId.HasValue)
			{
				retValue = false;
				this.cntrlSeverity.Tag = 2.ToString();
			}

			if (this.lblDetectedIn.FontWeight == FontWeights.Bold && !((RemoteRelease)this.cntrlDetectedIn.SelectedItem).ReleaseId.HasValue)
			{
				retValue = false;
				this.cntrlDetectedIn.Tag = 2.ToString();
			}

			if (this.lblResolvedIn.FontWeight == FontWeights.Bold && !((RemoteRelease)this.cntrlResolvedIn.SelectedItem).ReleaseId.HasValue)
			{
				retValue = false;
				this.cntrlResolvedIn.Tag = 2.ToString();
			}

			if (this.lblVerifiedIn.FontWeight == FontWeights.Bold && !((RemoteRelease)this.cntrlVerifiedIn.SelectedItem).ReleaseId.HasValue)
			{
				retValue = false;
				this.cntrlVerifiedIn.Tag = 2.ToString();
			}

			if (this.lblStartDate.FontWeight == FontWeights.Bold && !this.cntrlStartDate.SelectedDate.HasValue)
			{
				retValue = false;
				this.cntrlStartDate.Tag = 2.ToString();
			}

			if (this.lblEndDate.FontWeight == FontWeights.Bold && !this.cntrlEndDate.SelectedDate.HasValue)
			{
				retValue = false;
				this.cntrlEndDate.Tag = 2.ToString();
			}

			if (this.lblEstEffort.FontWeight == FontWeights.Bold && string.IsNullOrEmpty(this.cntrlEstEffortH.Text.Trim()) && string.IsNullOrEmpty(this.cntrlEstEffortH.Text.Trim()))
			{
				retValue = false;
				this.cntrlEstEffortH.Tag = 2.ToString();
			}

			if (this.lblActEffort.FontWeight == FontWeights.Bold && string.IsNullOrEmpty(this.cntrlActEffortH.Text.Trim()) && string.IsNullOrEmpty(this.cntrlActEffortH.Text.Trim()))
			{
				retValue = false;
				this.cntrlActEffortH.Tag = 2.ToString();
			}

			return retValue;
		}

		/// <summary>Gets the field control for the specified integer id.</summary>
		/// <param name="WorkflowFieldId">The ID of the field.</param>
		/// <returns>A control representing the Workflow Field ID.</returns>
		private Control workflow_GetFieldControl(int WorkflowFieldId)
		{
			Control retCont = null;

			switch (WorkflowFieldId)
			{
				case 1: //Priority
					retCont = this.cntrlSeverity;
					break;
				case 2: //Severity
					retCont = this.cntrlPriority;
					break;
				case 4: //Type
					retCont = this.cntrlType;
					break;
				case 5: //Opener/Detector
					retCont = this.cntrlDetectedBy;
					break;
				case 6: //Owner
					retCont = this.cntrlOwnedBy;
					break;
				case 7: //Detected Release
					retCont = this.cntrlDetectedIn;
					break;
				case 8: //Resolved Release
					retCont = this.cntrlResolvedIn;
					break;
				case 9: //Verified Release
					retCont = this.cntrlVerifiedIn;
					break;
				case 10: //Name
					retCont = this.cntrlIncidentName;
					break;
				case 11: //Description
					retCont = this.cntrlDescription;
					break;
				case 12: //Resolution
					retCont = this.cntrlResolution;
					break;
				case 14: //Closed Date
					retCont = this.cntrlEndDate;
					break;
				case 45: //Start Date
					retCont = this.cntrlStartDate;
					break;
				case 47: //Estimated Effory
					retCont = this.cntrlEstEffortH;
					break;
				case 48: //Actual Effort
					retCont = this.cntrlActEffortH;
					break;
				case 126: //Projected Effort
					//retCont = this.cntrlProEffortH;
					break;
				case 127: //Remaining Effort
					//retCont = this.cntrlRemEffortH;
					break;

				case 3: //Status
				case 13: //Creation Date
				case 15: //LastUpdate Date
				case 46: //Completion %
				case 94: //Incident ID
					break;
			}

			return retCont;
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

		/// <summary>Generates a dictionary of IDs and WorkflowFields available for Incidents. Optionally will assign settings given.</summary>
		/// <param name="forWorkflow">Optionally assign statuses from the workflow dictionary given.</param>
		/// <returns>The dictionary of IDs and WorkflowFields</returns>
		private Dictionary<int, WorkflowField> workflow_GenerateFields(Dictionary<int, int> forWorkflow = null)
		{
			//TODO: Get the Projected and Remaining effort fields.

			Dictionary<int, WorkflowField> retDict = new Dictionary<int, WorkflowField>();
			if (forWorkflow == null) forWorkflow = new Dictionary<int, int>();

			//Load up each control..
			retDict.Add(1, new WorkflowField(1, "Priority", this.cntrlPriority, false, ((forWorkflow.ContainsKey(1)) ? (WorkflowField.FieldStatusEnum)forWorkflow[1] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(2, new WorkflowField(2, "Severity", this.cntrlSeverity, false, ((forWorkflow.ContainsKey(2)) ? (WorkflowField.FieldStatusEnum)forWorkflow[2] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(3, new WorkflowField(3, "Status", null, true, WorkflowField.FieldStatusEnum.Required));
			retDict.Add(4, new WorkflowField(4, "Type", this.cntrlType, false, ((forWorkflow.ContainsKey(4)) ? (WorkflowField.FieldStatusEnum)forWorkflow[4] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(5, new WorkflowField(5, "Opener", this.cntrlDetectedBy, false, ((forWorkflow.ContainsKey(5)) ? (WorkflowField.FieldStatusEnum)forWorkflow[5] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(6, new WorkflowField(6, "Owner", this.cntrlOwnedBy, false, ((forWorkflow.ContainsKey(6)) ? (WorkflowField.FieldStatusEnum)forWorkflow[6] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(7, new WorkflowField(7, "Detected Release", this.cntrlDetectedIn, false, ((forWorkflow.ContainsKey(7)) ? (WorkflowField.FieldStatusEnum)forWorkflow[7] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(8, new WorkflowField(8, "Resolved Release", this.cntrlResolvedIn, false, ((forWorkflow.ContainsKey(8)) ? (WorkflowField.FieldStatusEnum)forWorkflow[8] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(9, new WorkflowField(9, "Verified Release", this.cntrlVerifiedIn, false, ((forWorkflow.ContainsKey(9)) ? (WorkflowField.FieldStatusEnum)forWorkflow[9] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(10, new WorkflowField(10, "Name", this.cntrlIncidentName, true, WorkflowField.FieldStatusEnum.Required));
			retDict.Add(11, new WorkflowField(11, "Description", this.cntrlDescription, false, ((forWorkflow.ContainsKey(11)) ? (WorkflowField.FieldStatusEnum)forWorkflow[11] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(12, new WorkflowField(12, "Resolution", this.cntrlResolution, false, ((forWorkflow.ContainsKey(12)) ? (WorkflowField.FieldStatusEnum)forWorkflow[12] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(13, new WorkflowField(13, "Creation Date", null, true));
			retDict.Add(14, new WorkflowField(14, "End Date", this.cntrlEndDate, false, ((forWorkflow.ContainsKey(14)) ? (WorkflowField.FieldStatusEnum)forWorkflow[14] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(15, new WorkflowField(15, "Last Modified Date", null, true));
			retDict.Add(45, new WorkflowField(45, "Start Date", this.cntrlStartDate, false, ((forWorkflow.ContainsKey(45)) ? (WorkflowField.FieldStatusEnum)forWorkflow[45] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(46, new WorkflowField(46, "Completion %", null, true));
			retDict.Add(47, new WorkflowField(47, "Estimated Effort", this.cntrlEstEffortH, false, ((forWorkflow.ContainsKey(47)) ? (WorkflowField.FieldStatusEnum)forWorkflow[47] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(48, new WorkflowField(48, "Actual Effort", this.cntrlActEffortH, false, ((forWorkflow.ContainsKey(48)) ? (WorkflowField.FieldStatusEnum)forWorkflow[48] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(94, new WorkflowField(94, "Incident ID", null, true));
			retDict.Add(126, new WorkflowField(126, "Projected Effort", null, false, ((forWorkflow.ContainsKey(126)) ? (WorkflowField.FieldStatusEnum)forWorkflow[126] : WorkflowField.FieldStatusEnum.Disabled)));
			retDict.Add(127, new WorkflowField(127, "Remaining Effort", null, false, ((forWorkflow.ContainsKey(127)) ? (WorkflowField.FieldStatusEnum)forWorkflow[127] : WorkflowField.FieldStatusEnum.Disabled)));

			//Return it.
			return retDict;
		}

		/// <summary>Loads the specified workflow field list into the given field dictionary.</summary>
		/// <param name="newWorkflowFields">The new workflow status to load.</param>
		/// <param name="existWorkflowFields">The existing list of WorkflowFields</param>
		private void workflow_LoadWorkflow(Dictionary<int, int> newWorkflowFields, ref Dictionary<int, WorkflowField> existWorkflowFields)
		{
			//Loop through all WorkflowFields.
			foreach (KeyValuePair<int, WorkflowField> workField in existWorkflowFields)
			{
				//Check if it's in the new workflow, if the field is changeable.
				if (!workField.Value.IsFixed)
				{
					if (newWorkflowFields.ContainsKey(workField.Key))
						workField.Value.FieldStatus = (WorkflowField.FieldStatusEnum)newWorkflowFields[workField.Key];
					else
						workField.Value.FieldStatus = WorkflowField.FieldStatusEnum.Disabled;
				}
			}
		}

		/// <summary>Class that holds workflow fields and their UI Controls and statuses.</summary>
		private class WorkflowField
		{
			public WorkflowField(int Number, string Name, Control Control, bool isFixed, FieldStatusEnum Status = FieldStatusEnum.Disabled)
			{
				this.FieldID = Number;
				this.FieldName = Name;
				this.FieldControl = Control;
				this.IsFixed = isFixed;
				this.FieldStatus = Status;
			}

			/// <summary>The workflow field id.</summary>
			public int FieldID
			{
				get;
				set;
			}

			/// <summary>The plain-text name of the field.</summary>
			public string FieldName
			{
				get;
				set;
			}

			/// <summary>Whether or not the field's status is fixed and cannot be changed.</summary>
			public bool IsFixed
			{
				get;
				set;
			}

			/// <summary>The current status of the field.</summary>
			public FieldStatusEnum FieldStatus
			{
				get;
				set;
			}

			/// <summary>The UI Control for the field.</summary>
			public Control FieldControl
			{
				get;
				set;
			}

			/// <summary>Enumeration of available statuses for Workflow fields.</summary>
			public enum FieldStatusEnum : int
			{
				Hidden = -1,
				Disabled = 0,
				Enabled = 1,
				Required = 2
			}

		}
	}
}
