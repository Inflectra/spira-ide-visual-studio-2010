using System.Windows;
using System.Windows.Controls;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010
{
	/// <summary>
	/// Interaction logic for MyControl.xaml
	/// </summary>
	public partial class MyControl : UserControl
	{
		public MyControl()
		{
			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		private void button1_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", this.ToString()),
							"SpiraTeam Project Explorer");

			Spira_ImportExport client = new Spira_ImportExport("http://localhost/Spira", "fredbloggs", "fred1bloggs");
		}
	}
}