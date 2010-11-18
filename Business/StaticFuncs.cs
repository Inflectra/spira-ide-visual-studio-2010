using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell;

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
	}
}
