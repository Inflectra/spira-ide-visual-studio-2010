using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.SpiraTeam_Client;
using Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business
{
	public static class StaticFuncs
	{
		/// <summary>Readonly. Returns the resource manager for the loaded library.</summary>
		public static ResourceManager getCultureResource
		{
			get
			{
				Assembly addinAssembly = Assembly.GetExecutingAssembly();
				Assembly satelliteAssembly = addinAssembly.GetSatelliteAssembly(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
				string resAssName = "";
				foreach (string resName in satelliteAssembly.GetManifestResourceNames())
				{
					if (resName == "SpiraExplorer.resources.Properties.Resources.resources")
						resAssName = resName.Substring(0, resName.LastIndexOf("."));
				}

				try
				{
					return new ResourceManager(resAssName, satelliteAssembly);
				}
				catch
				{
					return null;
				}
			}
		}

		/// <summary>Returns the environment window.</summary>
		/// <returns>DTE2 object</returns>
		public static EnvDTE80.DTE2 GetEnvironment
		{
			get
			{
				return (EnvDTE80.DTE2)Package.GetGlobalService(typeof(SDTE));
			}
		}

		/// <summary>Creates an Image control for a specified resource.</summary>
		/// <param name="Key">The key name of the resource to use. Will search and use Product-dependent resources first.</param>
		/// <param name="Size">Size of the desired image, or null.</param>
		/// <param name="Stretch">Desired stretch setting of image, or null.</param>
		/// <returns>Resulting image, or null if key is not found.</returns>
		public static System.Windows.Controls.Image getImage(string key, System.Windows.Size size)
		{
			System.Windows.Controls.Image retImage = new System.Windows.Controls.Image();

			try
			{
				if (!size.IsEmpty && (size.Height != 0 && size.Width != 0))
				{
					retImage.Height = size.Height;
					retImage.Width = size.Width;
				}

				BitmapSource image = null;

				Bitmap imgBmp = (System.Drawing.Bitmap)StaticFuncs.getCultureResource.GetObject(key);

				if (imgBmp != null)
				{
					IntPtr bmStream = imgBmp.GetHbitmap();
					System.Windows.Int32Rect rect = new Int32Rect(0, 0, imgBmp.Width, imgBmp.Height);

					image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmStream, IntPtr.Zero, rect, BitmapSizeOptions.FromEmptyOptions());
				}

				retImage.Source = image;
			}
			catch (Exception ex)
			{
				//TODO: Log Error.
			}

			return retImage;
		}

		/// <summary>Creates a web client for use.</summary>
		/// <param name="serverAddress">The base address of the server.</param>
		/// <returns>ImportExportClient</returns>
		public static ImportExportClient CreateClient(string serverAddress)
		{
			ImportExportClient retClient = null;

			try
			{
				//The endpoint address.
				EndpointAddress EndPtAddr = new EndpointAddress(new Uri(serverAddress + Settings.Default.app_ServiceURI));
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
					Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business.Spira_ImportExport.PermissiveCertificatePolicy.Enact("");
				}

				retClient = new SpiraTeam_Client.ImportExportClient(wsDualHttp, EndPtAddr);
			}
			catch (Exception ex)
			{
				//TODO: Log error.
				retClient = null;
			}

			return retClient;

		}
	}
}
