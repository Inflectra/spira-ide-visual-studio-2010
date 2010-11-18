
using System;
using System.ServiceModel;
using System.Text;
namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business
{
	public partial class Spira_ImportExport : IDisposable
	{
		#region Static Vars
		static string SERVICE_URL = "/Services/v3_0/ImportExport.svc";
		#endregion

		#region Internal Vars
		private bool _isConnected = false;
		private SpiraTeam_Client.ImportExportClient _client;
		private Exception _lastException;
		private ClientStateEnum _state = ClientStateEnum.Idle;
		private string _server;
		private string _user;
		private string _password;
		#endregion

		#region Events
		public event EventHandler<ConnectionException> ConnectionError;
		public event EventHandler ConnectionReady;
		#endregion

		public Spira_ImportExport(string server, string user, string password)
		{
			//Assign internal vars..
			this._server = server;
			this._user = user;
			this._password = password;

			//The endpoint address.
			EndpointAddress EndPtAddr = new EndpointAddress(new Uri(server + SERVICE_URL));
			//Create the soap client.
			BasicHttpBinding wsDualHttp = new BasicHttpBinding();
			wsDualHttp.CloseTimeout = TimeSpan.FromMinutes(1);
			wsDualHttp.OpenTimeout = TimeSpan.FromMinutes(1);
			wsDualHttp.ReceiveTimeout = TimeSpan.FromMinutes(10);
			wsDualHttp.SendTimeout = TimeSpan.FromMinutes(1);
			wsDualHttp.BypassProxyOnLocal = false;
			wsDualHttp.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
			wsDualHttp.MaxBufferPoolSize = Int32.MaxValue;
			wsDualHttp.MaxReceivedMessageSize = Int32.MaxValue;
			wsDualHttp.MessageEncoding = WSMessageEncoding.Text;
			wsDualHttp.TextEncoding = Encoding.UTF8;
			wsDualHttp.UseDefaultWebProxy = true;
			wsDualHttp.ReaderQuotas.MaxDepth = Int32.MaxValue;
			wsDualHttp.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
			wsDualHttp.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
			wsDualHttp.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
			wsDualHttp.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
			wsDualHttp.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.Certificate;
			wsDualHttp.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default;
			wsDualHttp.Security.Mode = BasicHttpSecurityMode.None;
			//Configure for alternative connection types.
			if (EndPtAddr.Uri.Scheme == "https")
			{
				wsDualHttp.Security.Mode = BasicHttpSecurityMode.Transport;
				wsDualHttp.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

				//Allow self-signed certificates
				PermissiveCertificatePolicy.Enact("");
			}

			try
			{
				this._client = new SpiraTeam_Client.ImportExportClient(wsDualHttp, EndPtAddr);
			}
			catch (Exception ex)
			{
			}

			//Hook up events.
			this._client.Connection_Authenticate2Completed += new EventHandler<SpiraTeam_Client.Connection_Authenticate2CompletedEventArgs>(_client_Connection_Authenticate2Completed);
		}

		/// <summary>Call to initiate connection to server.</summary>
		public void Connect()
		{
			//Connect.
			this._state = ClientStateEnum.Working;
			this._lastException = null;

			this._client.Connection_Authenticate2Async(this._user, this._password, Business.StaticFuncs.getCultureResource.GetString("app_ReportName"), this);
		}

		/// <summary>Hit when the client's finished connecting.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _client_Connection_Authenticate2Completed(object sender, SpiraTeam_Client.Connection_Authenticate2CompletedEventArgs e)
		{
			if (e.Error == null)
			{
				this._isConnected = true;
				if (this.ConnectionReady != null)
					this.ConnectionReady(this, new EventArgs());
			}
			else
			{
				this._lastException = e.Error;
				this._state = ClientStateEnum.Idle;
				if (this.ConnectionError != null)
					this.ConnectionError(this, new ConnectionException(e.Error));
			}
		}

		#region Properties
		/// <summary>The client to communicate with the server with.</summary>
		public SpiraTeam_Client.ImportExportClient Client
		{
			get
			{
				if (this._isConnected)
					return this._client;
				else
					return null;
			}
		}

		/// <summary>The last error that occured.</summary>
		public Exception ClientLastError
		{
			get
			{
				return this._lastException;
			}
		}

		/// <summary>The current status of the client.</summary>
		public ClientStateEnum ClientStatus
		{
			get
			{
				if (this._client.State == CommunicationState.Opened)
					return ClientStateEnum.Working;
				else if (this._client.State == CommunicationState.Faulted)
					return ClientStateEnum.Error;
				else
					return ClientStateEnum.Idle;
			}
		}

		/// <summary>Additional storage for reference on the client.</summary>
		public TreeViewArtifact ClientNode
		{
			get;
			set;
		}
		#endregion

		#region Event Classes
		public class ConnectionException : EventArgs
		{
			private Exception _exception;

			public ConnectionException(Exception ex)
			{
				this._exception = ex;
			}

			public Exception error
			{
				get
				{
					return this._exception;
				}
			}
		}
		#endregion

		#region IDisposable Members

		/// <summary>Called when we're finished with the client. Verifies that the client is disconnected.</summary>
		void IDisposable.Dispose()
		{
			if (this._client != null)
			{
				try
				{
					this._client.Connection_Disconnect();
				}
				catch { }
				finally
				{
					this._client = null;
				}
			}
		}

		#endregion

		public enum ClientStateEnum
		{
			Idle = 0,
			Working = 1,
			Error = 2
		}

		/// <summary>Allows the use of Self-Signed SSL certificates</summary>
		public class PermissiveCertificatePolicy
		{
			string subjectName = "";
			static PermissiveCertificatePolicy currentPolicy;

			public PermissiveCertificatePolicy(string subjectName)
			{
				this.subjectName = subjectName;
				ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertValidate);
			}

			public static void Enact(string subjectName)
			{
				currentPolicy = new PermissiveCertificatePolicy(subjectName);
			}

			public bool RemoteCertValidate(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
			{
				if (cert.Subject == subjectName || subjectName == "")
				{
					return true;
				}

				return false;
			}
		}
	}
}
