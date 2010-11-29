using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.Forms;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business
{
	public class TreeViewArtifact
	{
		//Added properties for the artifact type, ID, ad image.

		public TreeViewArtifact()
		{
			this.ArtifactType = ArtifactTypeEnum.None;
			this.Items = new List<object>();
		}

		/// <summary>The artifact's name.</summary>
		public string ArtifactName
		{
			get;
			set;
		}

		/// <summary>Used for additional storage. (Like a client, for example.)</summary>
		public object ArtifactTag
		{
			get;
			set;
		}

		/// <summary>The ID of the artifact.</summary>
		public int ArtifactId
		{
			get;
			set;
		}

		/// <summary>The Type of the Artifact.</summary>
		public ArtifactTypeEnum ArtifactType
		{
			get;
			set;
		}

		/// <summary>Readonly. The ID String of the artifact.</summary>
		public string ArtifactDisplay
		{
			get
			{
				string retName = this.ArtifactName;

				if (this.ArtifactIsFolder)
				{
					if (this.ArtifactType != ArtifactTypeEnum.None && this.Items.Count > 0)
						retName += " (" + this.Items.Count.ToString() + ")";
				}
				else
				{
					switch (this.ArtifactType)
					{
						case ArtifactTypeEnum.Incident:
							retName += " [IN:" + this.ArtifactId.ToString() + "]";
							break;

						case ArtifactTypeEnum.Requirement:
							retName += " [RQ:" + this.ArtifactId.ToString() + "]";
							break;

						case ArtifactTypeEnum.Task:
							retName += " [TK:" + this.ArtifactId.ToString() + "]";
							break;
					}
				}

				return retName;
			}
		}

		/// <summary>Readonly. Returns the image source for displaying the appropriate image in the TreeView.</summary>
		public ImageSource ArtifactImageSource
		{
			get
			{
				if (this.ArtifactType == ArtifactTypeEnum.Project)
					return StaticFuncs.getImage("imgProject", new System.Windows.Size(16, 16)).Source;
				else if (this.ArtifactIsFolder)
					switch (this.ArtifactType)
					{
						case ArtifactTypeEnum.Incident:
							return StaticFuncs.getImage("imgFolderIncident", new System.Windows.Size(16, 16)).Source;
						case ArtifactTypeEnum.Requirement:
							return StaticFuncs.getImage("imgFolderRequirement", new System.Windows.Size(16, 16)).Source;
						case ArtifactTypeEnum.Task:
							return StaticFuncs.getImage("imgFolderTask", new System.Windows.Size(16, 16)).Source;
						default:
							return StaticFuncs.getImage("imgFolder", new System.Windows.Size(16, 16)).Source;
					}
				else
					switch (this.ArtifactType)
					{
						case ArtifactTypeEnum.Incident:
							return StaticFuncs.getImage("imgIncident", new System.Windows.Size(16, 16)).Source;
						case ArtifactTypeEnum.Requirement:
							return StaticFuncs.getImage("imgRequirement", new System.Windows.Size(16, 16)).Source;
						case ArtifactTypeEnum.Task:
							return StaticFuncs.getImage("imgTask", new System.Windows.Size(16, 16)).Source;
						default:
							return null;
					}
			}
		}

		/// <summary>Sets whether of not this item is a folder e(containing other items of a type) or an actual artifact.</summary>
		public bool ArtifactIsFolder
		{
			get;
			set;
		}

		public TreeViewArtifact Parent
		{
			get;
			set;
		}

		/// <summary>Readonly. Returns the parent project node (or null if not found.)</summary>
		public TreeViewArtifact ArtifactParentProject
		{
			get
			{
				TreeViewArtifact retNode = null;

				if (this.Parent != null)
				{
					TreeViewArtifact checkNode = this.Parent as TreeViewArtifact;
					while (checkNode != null && checkNode.ArtifactType != ArtifactTypeEnum.Project)
					{
						checkNode = checkNode.Parent as TreeViewArtifact;
					}
					retNode = checkNode;
				}
				return retNode;
			}
		}

		/// <summary>Readonly. Returns a UI element that is used for the tooltip.</summary>
		public UIElement ArtifactTooltip
		{
			get
			{
				UIElement tipReturn = null;
				if (!this.ArtifactIsFolder)
				{
					switch (this.ArtifactType)
					{
						case ArtifactTypeEnum.Incident:
							tipReturn = new cntlTTipIncident();
							((cntlTTipIncident)tipReturn).DataItem = this;
							break;

						case ArtifactTypeEnum.Requirement:
							tipReturn = new cntlTTipRequirement();
							((cntlTTipRequirement)tipReturn).DataItem = this;
							break;

						case ArtifactTypeEnum.Task:
							tipReturn = new cntlTTipTask();
							((cntlTTipTask)tipReturn).DataItem = this;
							break;
					}
				}

				return tipReturn;
			}
		}

		/// <summary>Available types of TreeNodes.</summary>
		public enum ArtifactTypeEnum
		{
			None = 0,
			Task = 1,
			Incident = 2,
			Requirement = 3,
			Project = 4,
			Error = 1024
		}

		/// <summary>Items contained within the current item.</summary>
		public List<object> Items
		{
			get;
			set;
		}

		public string isInError
		{
			get
			{
				string retValue = "False";

				if (this.ArtifactTag != null)
				{
					if (this.ArtifactTag.GetType() == typeof(SpiraTeam_Client.RemoteIncident))
					{
						SpiraTeam_Client.RemoteIncident item = (SpiraTeam_Client.RemoteIncident)this.ArtifactTag;
						if ((item.StartDate.HasValue && item.StartDate < DateTime.Now) && item.CompletionPercent == 0)
							retValue = "True";
					}
					else if (this.ArtifactTag.GetType() == typeof(SpiraTeam_Client.RemoteTask))
					{
						SpiraTeam_Client.RemoteTask item = (SpiraTeam_Client.RemoteTask)this.ArtifactTag;
						if (item.StartDate.HasValue && item.StartDate < DateTime.Now && item.TaskStatusId == 1)
							retValue = "True";
						if (item.EndDate.HasValue && item.EndDate < DateTime.Now)
							retValue = "True";
					}
				}

				return retValue;
			}
		}

		/// <summary>Default indexer.</summary>
		/// <param name="index">The number of the item.</param>
		/// <returns>Child at index.</returns>
		public object this[int index]
		{
			get
			{
				return this.Items[index];
			}
			set
			{
				this.Items[index] = value;
			}
		}
	}
}
