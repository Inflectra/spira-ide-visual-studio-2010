﻿using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls;
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
	//[Guid("76C22C24-36B6-4C0C-BF60-FFCB65D1B05B")]
	public class toolSpiraExplorerDetails : ToolWindowPane
	{
		/// <summary>
		/// Standard constructor for the tool window.
		/// </summary>
		public toolSpiraExplorerDetails() :
			base(null)
		{
			base.Caption = "";
			// Set the image that will appear on the tab of the window frame
			// when docked with an other window
			// The resource ID correspond to the one defined in the resx file
			// while the Index is the offset in the bitmap strip. Each image in
			// the strip being 16x16.
			this.BitmapResourceID = 301;
			this.BitmapIndex = 1;

			base.Content = new cntrlDetailsForm();
		}

		public toolSpiraExplorerDetails(object ContentControl)
			: this()
		{
			this.Content = ContentControl;
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

		/// <summary>Hit when the window is attempted to be closed.</summary>
		protected override void OnClose()
		{
			//TODO: Inform window we're closing.
		}
	}
}
