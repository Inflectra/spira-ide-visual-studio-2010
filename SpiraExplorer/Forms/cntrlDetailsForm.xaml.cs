using System.Windows.Controls;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Controls
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class cntrlDetailsForm : UserControl
	{
		public cntrlDetailsForm()
		{
			InitializeComponent();
		}

		public object Content
		{
			get
			{
				return this.cntrlContent.Content;
			}
			set
			{
				this.cntrlContent.Content = value;
			}
		}
	}
}
