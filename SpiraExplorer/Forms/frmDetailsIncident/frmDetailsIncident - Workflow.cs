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

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>Holds the saving functions for frmDetailsIncident</summary>
	public partial class frmDetailsIncident : UserControl
	{
		#region Private Workflow Fields
		//Holds the definitions for the fields.
		private Dictionary<int, WorkflowField> _WorkflowFields;
		private Dictionary<int, WorkflowField> _WorkflowCustom;
		//Holds current status information for fields.
		private Dictionary<int, WorkflowField.WorkflowStatusEnum> _WorkflowFields_Current;
		private Dictionary<int, WorkflowField.WorkflowStatusEnum> _WorkflowCustom_Current;
		//Holds updated status information for fields.
		private Dictionary<int, WorkflowField.WorkflowStatusEnum> _WorkflowFields_Updated;
		private Dictionary<int, WorkflowField.WorkflowStatusEnum> _WorkflowCustom_Updated;

		//Holds current Status & Type information
		private int? _IncCurrentType;
		private int? _IncCurrentStatus;
		//Holds updated Status & Type information
		private int? _IncSelectedType;
		private int? _IncSelectedStatus;

		//Holds available transitions for the loaded Incident.
		private List<RemoteWorkflowIncidentTransition> _WorkflowTransitions;
		#endregion

		/// <summary>Generates a dictionary of IDs and WorkflowFields available for Incidents. Optionally will assign settings given.</summary>
		/// <param name="forWorkflow">Optionally assign statuses from the workflow dictionary given.</param>
		/// <returns>The dictionary of IDs and WorkflowFields</returns>
		private Dictionary<int, WorkflowField> workflow_GenerateStandardFields()
		{
			Dictionary<int, WorkflowField> retDict = new Dictionary<int, WorkflowField>();

			//TODO: Get the Projected and Remaining effort fields.
			//Load up each control..
			retDict.Add(1, new WorkflowField(1, "Severity", this.cntrlSeverity, false, false, this.lblSeverity));
			retDict.Add(2, new WorkflowField(2, "Priority", this.cntrlPriority, false, false, this.lblPriority));
			retDict.Add(3, new WorkflowField(3, "Status", null, true, false, this.lblStatus));
			retDict.Add(4, new WorkflowField(4, "Type", this.cntrlType, false, false, this.lblType));
			retDict.Add(5, new WorkflowField(5, "Opener", this.cntrlDetectedBy, false, false, this.lblDetectedBy));
			retDict.Add(6, new WorkflowField(6, "Owner", this.cntrlOwnedBy, false, false, this.lblOwnedBy));
			retDict.Add(7, new WorkflowField(7, "Detected Release", this.cntrlDetectedIn, false, false, this.lblDetectedIn));
			retDict.Add(8, new WorkflowField(8, "Resolved Release", this.cntrlResolvedIn, false, false, this.lblResolvedIn));
			retDict.Add(9, new WorkflowField(9, "Verified Release", this.cntrlVerifiedIn, false, false, this.lblVerifiedIn));
			retDict.Add(10, new WorkflowField(10, "Name", this.cntrlIncidentName, true, false));
			retDict.Add(11, new WorkflowField(11, "Description", this.cntrlDescription, false, false, this.lblDescription));
			retDict.Add(12, new WorkflowField(12, "Resolution", this.cntrlResolution, false, false));
			retDict.Add(13, new WorkflowField(13, "Creation Date", null, true, true));
			retDict.Add(14, new WorkflowField(14, "End Date", this.cntrlEndDate, false, false, this.lblEndDate));
			retDict.Add(15, new WorkflowField(15, "Last Modified Date", null, true, true));
			retDict.Add(45, new WorkflowField(45, "Start Date", this.cntrlStartDate, false, false, this.lblStartDate));
			retDict.Add(46, new WorkflowField(46, "Completion %", null, true, false));
			retDict.Add(47, new WorkflowField(47, "Estimated Effort", this.cntrlEstEffortH, false, false, this.lblEstEffort));
			retDict.Add(48, new WorkflowField(48, "Actual Effort", this.cntrlActEffortH, false, false, this.lblActEffort));
			retDict.Add(94, new WorkflowField(94, "Incident ID", null, true, false));
			retDict.Add(126, new WorkflowField(126, "Projected Effort", null, false, false, null));
			retDict.Add(127, new WorkflowField(127, "Remaining Effort", null, false, false, null));

			//Return it.
			return retDict;
		}

		/// <summary>Creates a useable dictionary of Field ID and Status from the given list of Workflow Fields.</summary>
		/// <param name="workflowFields">A list of Workflow Fields</param>
		/// <returns>A dictionary of FieldId and current Status.</returns>
		private Dictionary<int, WorkflowField.WorkflowStatusEnum> workflow_LoadFieldStatus(List<RemoteWorkflowIncidentFields> workflowFields)
		{
			Dictionary<int, WorkflowField.WorkflowStatusEnum> retList = new Dictionary<int, WorkflowField.WorkflowStatusEnum>();

			//Copy over all fields, first.
			foreach (KeyValuePair<int, WorkflowField> kvpField in this._WorkflowFields)
			{
				retList.Add(kvpField.Key, WorkflowField.WorkflowStatusEnum.Disabled);
			}

			//Now update the ones that need it.
			foreach (RemoteWorkflowIncidentFields wkfField in workflowFields)
			{
				if ((int)retList[wkfField.FieldId] < wkfField.FieldStateId)
				{
					retList[wkfField.FieldId] = (WorkflowField.WorkflowStatusEnum)wkfField.FieldStateId;
				}
			}

			return retList;
		}

		/// <summary>Creates a useable dictionary of Field ID and Status from the given list of Workflow Custom Fields.</summary>
		/// <param name="workflowFields">A list of Workflow Fields</param>
		/// <returns>A dictionary of FieldId and current Status.</returns>
		private Dictionary<int, WorkflowField.WorkflowStatusEnum> workflow_LoadFieldStatus(List<RemoteWorkflowIncidentCustomProperties> workflowFields)
		{
			Dictionary<int, WorkflowField.WorkflowStatusEnum> retList = new Dictionary<int, WorkflowField.WorkflowStatusEnum>();

			//Copy over all fields, first.
			foreach (KeyValuePair<int, WorkflowField> kvpField in this._WorkflowCustom)
			{
				retList.Add(kvpField.Key, WorkflowField.WorkflowStatusEnum.Disabled);
			}

			//Now update the ones that need it.
			foreach (RemoteWorkflowIncidentCustomProperties wkfField in workflowFields)
			{
				if ((int)retList[wkfField.CustomPropertyId] < wkfField.FieldStateId)
				{
					retList[wkfField.CustomPropertyId] = (WorkflowField.WorkflowStatusEnum)wkfField.FieldStateId;
				}
			}

			return retList;
		}

		/// <summary>Set the enabled and required fields for the current stage in the workflow.</summary>
		/// <param name="WorkFlowFields">The Dictionary of Workflow Fields</param>
		private void workflow_SetEnabledFields(Dictionary<int, WorkflowField> FieldList, Dictionary<int, WorkflowField.WorkflowStatusEnum> WorkflowFields)
		{
			try
			{
				//We need to loop through each field, and set it appropriately.
				foreach (KeyValuePair<int, WorkflowField> incField in FieldList)
				{
					if (!incField.Value.IsHidden && !incField.Value.IsFixed)
					{
						//Try to set Enabled/Disabled
						if (incField.Value.FieldControl != null)
						{
							try
							{
								incField.Value.FieldControl.IsEnabled = ((int)WorkflowFields[incField.Key] > 0);
							}
							catch (Exception ex)
							{
								//TODO: Log error
							}
						}

						//Try to set Bold/Normal for label.
						if (incField.Value.FieldLabel != null)
						{
							try
							{
								((dynamic)incField.Value.FieldLabel).FontWeight = ((WorkflowFields[incField.Key] == WorkflowField.WorkflowStatusEnum.Requird) ? FontWeights.Bold : FontWeights.Normal);
							}
							catch (Exception ex)
							{
								//TODO: Log error
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				//Connect.logEventMessage("wpfDetailsIncident::workflow_SetEnabledFields", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Simple function to clear all required fields of their Required Hishlight.</summary>
		private void workflow_ClearAllRequiredHighlights()
		{
			if (this._WorkflowCustom == null) this._WorkflowCustom = new Dictionary<int, WorkflowField>();
			if (this._WorkflowFields == null) this._WorkflowFields = new Dictionary<int, WorkflowField>();

			//Custom fields.
			foreach (KeyValuePair<int, WorkflowField> kvpField in this._WorkflowCustom)
				if (kvpField.Value.FieldControl != null)
					kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControl");

			//Standard fields.
			foreach (KeyValuePair<int, WorkflowField> kvpField in this._WorkflowFields)
				if (kvpField.Value.FieldControl != null)
					kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControl");

		}

		/// <summary>Checks all required fields for a value.</summary>
		/// <returns>True if all is good. False if fields marked as required are left unset.</returns>
		private bool workflow_CheckRequiredFields()
		{
			bool retValue = true;

			//First, get the set of required fields we want to check against.
			bool useUpdate = (this._IncSelectedStatus.HasValue || this._IncSelectedType.HasValue);
			Dictionary<int, WorkflowField.WorkflowStatusEnum> workflowField = ((useUpdate) ? this._WorkflowFields_Updated : this._WorkflowFields_Current);
			Dictionary<int, WorkflowField.WorkflowStatusEnum> workflowCustom = ((useUpdate) ? this._WorkflowCustom_Updated : this._WorkflowCustom_Current);

			#region Loop through Standard Fields
			//Loop through each field..
			foreach (KeyValuePair<int, WorkflowField> kvpField in this._WorkflowFields)
			{
				//We only care about required fields.
				if (workflowField[kvpField.Key] == WorkflowField.WorkflowStatusEnum.Requird && !kvpField.Value.IsHidden && !kvpField.Value.IsFixed)
				{
					//Find the field and act upon it. Based on type on control and datatype inside it.
					switch (kvpField.Key)
					{
						case 1: // Severity
							if (!((RemoteIncidentSeverity)((ComboBox)kvpField.Value.FieldControl).SelectedItem).SeverityId.HasValue)
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 2: // Priority
							if (!((RemoteIncidentPriority)((ComboBox)kvpField.Value.FieldControl).SelectedItem).PriorityId.HasValue)
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 4: // Type
							if (!((RemoteIncidentType)((ComboBox)kvpField.Value.FieldControl).SelectedItem).IncidentTypeId.HasValue)
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 5: // Detector
						case 6: // Owner
							if (!((RemoteUser)((ComboBox)kvpField.Value.FieldControl).SelectedItem).UserId.HasValue)
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 7: // Detected Release
						case 8: // Resolved Release
						case 9: // Verified Release
							if (!((RemoteRelease)((ComboBox)kvpField.Value.FieldControl).SelectedItem).ReleaseId.HasValue)
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 10: // Name
							if (string.IsNullOrWhiteSpace(((TextBox)kvpField.Value.FieldControl).Text))
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 11: // Description 
						case 12: // Resolution
							if (string.IsNullOrWhiteSpace(((cntrlRichTextEditor)kvpField.Value.FieldControl).HTMLText))
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 14: // Closed Date
						case 45: // Start Date
							if (!((DatePicker)kvpField.Value.FieldControl).SelectedDate.HasValue)
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 47: // Estimated Effort
							if (string.IsNullOrWhiteSpace(this.cntrlEstEffortH.Text.Trim()) && string.IsNullOrWhiteSpace(this.cntrlEstEffortM.Text.Trim()))
							{
								this.cntrlEstEffortH.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								this.cntrlEstEffortM.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 48: // Actual Effort
							if (string.IsNullOrWhiteSpace(this.cntrlActEffortH.Text.Trim()) && string.IsNullOrWhiteSpace(this.cntrlActEffortM.Text.Trim()))
							{
								this.cntrlActEffortH.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								this.cntrlActEffortM.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case 126: // Projected Effort
							//TODO: Get Projected Date controls.
							break;

						case 127: // Projected Effort
							//TODO: Get Projected Date controls.
							break;

						case 3: // Status
						case 13: // Creation Date
						case 15: // Last Modified Date
						case 46: // Completion %
						case 94: // Incident ID
							break;
					}
				}
			}
			#endregion

			#region Loop through Custom Fields
			foreach (KeyValuePair<int, WorkflowField> kvpField in this._WorkflowCustom)
			{
				//We only care about required fields.
				if (workflowCustom[kvpField.Key] == WorkflowField.WorkflowStatusEnum.Requird && !kvpField.Value.IsHidden && !kvpField.Value.IsFixed)
				{
					//Depends on which control type it is..
					string cntrlType = kvpField.Value.FieldControl.GetType().ToString().ToLowerInvariant();
					cntrlType = cntrlType.Substring(cntrlType.LastIndexOf(".") + 1);

					switch (cntrlType)
					{
						case "textbox":
							if (string.IsNullOrWhiteSpace(((TextBox)kvpField.Value.FieldControl).Text))
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;

						case "combobox":
							if (((ComboBox)kvpField.Value.FieldControl).SelectedItem == null)
							{
								kvpField.Value.FieldControl.Style = (Style)this.FindResource("PaddedControlRequiredHighlight");
								retValue = false;
							}
							break;
					}
				}
			}
			#endregion

			return retValue;
		}

		/// <summary>Class that holds workflow fields and their UI Controls and statuses.</summary>
		private class WorkflowField
		{
			public WorkflowField(int Number, string Name, Control Control, bool Fixed, bool Hidden, UIElement Label = null)
			{
				this.FieldID = Number;
				this.FieldName = Name;
				this.FieldControl = Control;
				this.IsFixed = Fixed;
				this.IsHidden = Hidden;
				this.FieldLabel = Label;
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

			/// <summary>Whether or not the field is hidden (not on the form).</summary>
			public bool IsHidden
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

			public UIElement FieldLabel
			{
				get;
				set;
			}

			/// <summary>Status to refer fields to.</summary>
			public enum WorkflowStatusEnum : int
			{
				/// <summary>Field is disabled, cannot be changed.</summary>
				Disabled = 0,
				/// <summary>Field is enabled, it can be changed.</summary>
				Enabled = 1,
				/// <summary>Field is required. It has to be entered in, cannot be null or unset.</summary>
				Requird = 2
			}
		}
	}
}
