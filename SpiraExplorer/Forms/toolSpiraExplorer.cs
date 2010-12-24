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
	//[Guid("3ae79031-e1bc-11d0-8f78-00a0c9110057")]
	public class toolSpiraExplorer : ToolWindowPane
	{
		private SpiraExplorerPackage _Package;

		/// <summary>
		/// Standard constructor for the tool window.
		/// </summary>
		public toolSpiraExplorer() :
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

			// This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
			// we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
			// the object returned by the Content property.

			cntlSpiraExplorer explorerWindow = new cntlSpiraExplorer();
			explorerWindow.Pane = this;

			base.Content = explorerWindow;
		}
	}
}
