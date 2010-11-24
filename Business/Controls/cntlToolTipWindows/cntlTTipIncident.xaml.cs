using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.Forms
{
	/// <summary>
	/// Interaction logic for cntlTTipIncident.xaml
	/// </summary>
	public partial class cntlTTipIncident : UserControl
	{
		public cntlTTipIncident()
		{
			InitializeComponent();

			//Set images.
			this.imgProject.Source = StaticFuncs.getImage("imgProject", new System.Windows.Size(16, 16)).Source;
			this.imgIncident.Source = StaticFuncs.getImage("imgIncident", new System.Windows.Size(16, 16)).Source;
			this.imgRelease.Source = StaticFuncs.getImage("imgRelease", new System.Windows.Size(16, 16)).Source;
			//Set strings.
			this.txtItemId.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Id") + ":";
			this.txtProject.Text = StaticFuncs.getCultureResource.GetString("app_Project") + ":";
			this.txtOwner.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Owner") + ":";
			this.txtStatusType.Text = StaticFuncs.getCultureResource.GetString("app_Incident_StatusType") + ":";
			this.txtEstimate.Text = StaticFuncs.getCultureResource.GetString("app_Incident_EstEffort") + ":";
			this.txtProjected.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ProjEffort") + ":";
			this.txtPriority.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Priority") + ":";
			this.txtSeverity.Text = StaticFuncs.getCultureResource.GetString("app_Incident_Severity") + ":";
			this.txtDetected.Text = StaticFuncs.getCultureResource.GetString("app_Incident_DetectedRelease") + ":";
			this.txtResolved.Text = StaticFuncs.getCultureResource.GetString("app_Incident_ResolvedRelease") + ":";
			this.txtVerified.Text = StaticFuncs.getCultureResource.GetString("app_Incident_VerifiedRelease") + ":";
		}

		/// <summary>Holds a reference to the treeviewitem we're displaying.</summary>
		public TreeViewArtifact DataItem
		{
			get;
		}

		/// <summary>Readonly. Gets the string of estimated effort.</summary>
		public string EstEffort
		{
			get
			{
				return (((int)((dynamic)this.DataItem.ArtifactTag).EstimatedEffort) / 60).ToString() + " " + StaticFuncs.getCultureResource.GetString("app_General_HoursAbbr");
			}
		}

		/// <summary>Readonly. Gets the string of projected effort.</summary>
		public string ProjEffort
		{
			get
			{
				return (((int)((dynamic)this.DataItem.ArtifactTag).ProjectedEffort) / 60).ToString() + " " + StaticFuncs.getCultureResource.GetString("app_General_HoursAbbr");
			}
		}
	}
}
