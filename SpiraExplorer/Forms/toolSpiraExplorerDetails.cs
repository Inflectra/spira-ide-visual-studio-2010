using Microsoft.VisualStudio.Shell;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>
	/// This class implements the tool window exposed by this package and hosts a user control.
	///
	/// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
	/// usually implemented by the package implementer.
	///
	/// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
	/// implementation of the IVsUIElementPane interface.
	/// </summary>
	//[Guid("cfe746fb-e114-49da-8cb5-d2f8a1b0274e")]
	public class toolSpiraExplorerDetails : ToolWindowPane
	{
		/// <summary>
		/// Standard constructor for the tool window.
		/// </summary>
		public toolSpiraExplorerDetails() :
			base(null)
		{
			// Set the window title reading it from the resources.
			this.Caption = Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Resources.ToolWindowTitle;
			// Set the image that will appear on the tab of the window frame
			// when docked with an other window
			// The resource ID correspond to the one defined in the resx file
			// while the Index is the offset in the bitmap strip. Each image in
			// the strip being 16x16.
			this.BitmapResourceID = 301;
			this.BitmapIndex = 1;
		}

		/// <summary>The contents of the tool window.</summary>
		public object FormControl
		{
			get
			{
				return base.Content;
			}
			set
			{
				base.Content = value;
			}
		}
	}
}
