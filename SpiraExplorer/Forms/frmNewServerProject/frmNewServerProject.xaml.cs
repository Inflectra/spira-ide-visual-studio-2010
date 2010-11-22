using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Forms
{
	/// <summary>
	/// Interaction logic for wpfNewSpiraProject.xaml
	/// </summary>
	public partial class wpfNewSpiraProject : Window
	{
		public wpfNewSpiraProject()
		{
			try
			{
				InitializeComponent();

				//Set the title & icon.
				this.Title = Business.StaticFuncs.getCultureResource.GetString("strNewSpiraProject");
				try
				{
					System.Drawing.Icon ico = (System.Drawing.Icon)Business.StaticFuncs.getCultureResource.GetObject("icoLogo");
					MemoryStream icoStr = new MemoryStream();
					ico.Save(icoStr);
					icoStr.Seek(0, SeekOrigin.Begin);
					this.Icon = BitmapFrame.Create(icoStr);
				}
				catch (Exception ex)
				{
					//TODO: Log error.
				}

				//Set initial colors and form status.
				this.barProg.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(Business.StaticFuncs.getCultureResource.GetString("barForeColor"));
				this.barProg.IsIndeterminate = false;
				this.barProg.Value = 0;
				this.grdAvailProjs.IsEnabled = false;
				this.grdEntry.IsEnabled = true;
				this.btnConnect.Tag = false;
				this.btnConnect.Click += new RoutedEventHandler(btnConnect_Click);
				int num = this.cmbProjectList.Items.Add("-- No Projects Available --");
				this.cmbProjectList.SelectedIndex = num;
				this.cmbProjectList.SelectionChanged += new SelectionChangedEventHandler(cmbProjectList_SelectionChanged);
			}
			catch (Exception ex)
			{
				//TODO: Log error.
			}
		}

		/// <summary>Hit when the project list selection is changed.</summary>
		/// <param name="sender">cmbProjectList</param>
		/// <param name="e">SelectionChangedEventArgs</param>
		private void cmbProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				//If they selected a valid project, let them save.
				if (this.cmbProjectList.SelectedItem != null)
				{
					if (this.cmbProjectList.SelectedItem.GetType() == typeof(Business.SpiraProject))
					{
						this.btnSave.IsEnabled = true;
					}
					else
					{
						this.btnSave.IsEnabled = false;
					}
				}
				else
				{
					this.btnSave.IsEnabled = false;
				}
			}
			catch (Exception ex)
			{
				//TODO: Log error.
			}
		}

		/// <summary>Hit when the user user clicks the Connect button.</summary>
		/// <param name="sender">btnConnect</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnConnect_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			try
			{
				bool tag = (bool)this.btnConnect.Tag;
				if (tag)
				{
					//They want to cancel the connect.
					try
					{
						this._client.CancelAsync(null);
					}
					catch
					{ }
					this._client.Dispose();

					//Set form.
					this.barProg.IsIndeterminate = false;
					this.btnConnect.Tag = false;
					this.btnConnect.Content = "_Get Projects";
					this.grdEntry.IsEnabled = true;
				}
				else
				{
					if (this.txbServer.Text.ToLowerInvariant().EndsWith(".asmx") || this.txbServer.Text.ToLowerInvariant().EndsWith(".aspx"))
					{
						MessageBox.Show("Your server url cannot contain a page in its address.", "Invalid Server URL", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					}
					else
					{
						//Start the connections.
						this.barProg.IsIndeterminate = true;
						this.barProg.Foreground = (Brush)new System.Windows.Media.BrushConverter().ConvertFrom(this._resources.GetString("barForeColor"));
						this.grdEntry.IsEnabled = false;
						this.btnConnect.Content = "_Cancel";
						this.btnConnect.Tag = true;
						this.txtStatus.Text = "Connecting to server...";
						this.cmbProjectList.Items.Clear();
						this.grdAvailProjs.IsEnabled = false;

						//Create new client.
						ImportExport client = new ImportExport();
						client.Connection_Authenticate2Completed += new Connection_Authenticate2CompletedEventHandler(client_ActionCompleted);
						client.User_RetrieveByUserNameCompleted += new User_RetrieveByUserNameCompletedEventHandler(client_ActionCompleted);
						client.Project_RetrieveCompleted += new Project_RetrieveCompletedEventHandler(client_ActionCompleted);

						this._client = client;
						this._client.CookieContainer = new System.Net.CookieContainer();
						this._client.Url = new Uri(this.txbServer.Text + Connect.SpiraProject.URL_APIADD).AbsoluteUri;
						this._client.Connection_Authenticate2Async(this.txbUserID.Text, this.txbUserPass.Password, this._resources.GetString("strAddinProgNamePretty"));
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfNewSpiraProject::btnConnect_Click", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the client finished a piece of work.</summary>
		/// <param name="sender">The client.</param>
		/// <param name="e">EventArgs</param>
		private void client_ActionCompleted(object sender, EventArgs e)
		{
			try
			{
				if (e.GetType() == typeof(Connection_Authenticate2CompletedEventArgs))
				{
					Connection_Authenticate2CompletedEventArgs evt = (Connection_Authenticate2CompletedEventArgs)e;
					if (evt.Error == null)
					{
						this.txtStatus.Text = "Getting user information...";
						this._client.User_RetrieveByUserNameAsync(this.txbUserID.Text);
					}
					else
					{
						this.btnConnect_Click(null, null);
						//Just act like they canceled the service, then set error flag.
						this.barProg.Foreground = System.Windows.Media.Brushes.Red;
						this.barProg.Value = 1;
						this.txtStatus.Text = "Could not connect!";
						this.txtStatus.ToolTip = evt.Error.Message;
					}
				}
				else if (e.GetType() == typeof(User_RetrieveByUserNameCompletedEventArgs))
				{
					User_RetrieveByUserNameCompletedEventArgs evt = (User_RetrieveByUserNameCompletedEventArgs)e;
					if (evt.Error == null)
					{
						this.txtStatus.Text = "Getting Projects...";
						this.txbUserNum.Text = evt.Result.UserId.ToString();
						this._client.Project_RetrieveAsync();
					}
					else
					{
						this.btnConnect_Click(null, null);
						//Just act like they canceled the service, then set error flag.
						this.barProg.Foreground = System.Windows.Media.Brushes.Red;
						this.barProg.Value = 1;
						this.txtStatus.Text = "Could not get user info.";
						this.txtStatus.ToolTip = evt.Error.Message;
					}
				}
				else if (e.GetType() == typeof(Spira_ImportExport.Project_RetrieveCompletedEventArgs))
				{
					Project_RetrieveCompletedEventArgs evt = (Project_RetrieveCompletedEventArgs)e;
					if (evt.Error == null)
					{
						this.cmbProjectList.Items.Clear();
						//Load projects here.
						if (evt.Result.Length > 0)
						{
							foreach (RemoteProject RemoteProj in evt.Result)
							{
								Connect.SpiraProject Project = new Connect.SpiraProject();
								Project.ProjectID = RemoteProj.ProjectId.Value;
								Project.ServerURL = new Uri(this.txbServer.Text);
								Project.UserName = this.txbUserID.Text;
								Project.UserPass = this.txbUserPass.Password;
								Project.UserID = int.Parse(this.txbUserNum.Text);

								this.cmbProjectList.Items.Add(Project);
							}
							this.cmbProjectList.SelectedIndex = 0;
							this.grdAvailProjs.IsEnabled = true;
							this.grdEntry.IsEnabled = true;
							this.barProg.IsIndeterminate = false;
							this.barProg.Value = 0;
							this.btnConnect.Content = "_Get Projects";
							this.btnConnect.Tag = false;
							this.txtStatus.Text = "";
							this.txtStatus.ToolTip = null;
						}
						else
						{
							int num = this.cmbProjectList.Items.Add("-- No Projects Available --");
							this.cmbProjectList.SelectedIndex = num;
							//Reset form.
							this.grdEntry.IsEnabled = true;
							this.barProg.IsIndeterminate = false;
							this.btnConnect.Content = "_Get Projects";
							this.btnConnect.Tag = false;
						}
					}
					else
					{
						this.btnConnect_Click(null, null);
						//Just act like they canceled the service, then set error flag.
						this.barProg.Foreground = System.Windows.Media.Brushes.Red;
						this.barProg.Value = 1;
						this.txtStatus.Text = "Could not get projects.";
						this.txtStatus.ToolTip = evt.Error.Message;
					}
				}
			}
			catch (Exception ex)
			{
				Connect.logEventMessage("wpfNewSpiraProject::client_ActionCompleted", ex, System.Diagnostics.EventLogEntryType.Error);
			}
		}

		/// <summary>Hit when the user wants to save their selection.</summary>
		/// <param name="sender">btnSave</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			this.DialogResult = true;
		}

		/// <summary>Hit when the user wants to cancel.</summary>
		/// <param name="sender">btnCancel</param>
		/// <param name="e">RoutedEventArgs</param>
		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			this.DialogResult = false;
		}
	}
}
