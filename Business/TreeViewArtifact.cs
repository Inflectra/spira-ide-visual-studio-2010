using System.Windows.Controls;
using System.Windows.Media;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business
{
	public class TreeViewArtifact : TreeViewItem
	{
		//Added properties for the artifact type, ID, ad image.

		public TreeViewArtifact()
		{
			this.ArtifactType = ArtifactTypeEnum.None;
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
		public string ArtifactIdString
		{
			get
			{
				switch (this.ArtifactType)
				{
					case ArtifactTypeEnum.Incident:
						return "[IN:" + this.ArtifactId.ToString() + "]";
					case ArtifactTypeEnum.Requirement:
						return "[RQ:" + this.ArtifactId.ToString() + "]";
					case ArtifactTypeEnum.Task:
						return "[TK:" + this.ArtifactId.ToString() + "]";
					default:
						return "";
				}
			}
		}

		/// <summary>Readonly. Returns the image source for displaying the appropriate image in the TreeView.</summary>
		public ImageSource ArtifactImageSource
		{
			get
			{
				//TODO: Set image resources.
				//TODO: Use the IsFolder to create the image.
				switch (this.ArtifactType)
				{
					case ArtifactTypeEnum.Incident:
						return null;
					case ArtifactTypeEnum.Requirement:
						return null;
					case ArtifactTypeEnum.Task:
						return null;
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

		/// <summary>Readonly. Returns the parent project node (or null if ot found.)</summary>
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
	}
}
