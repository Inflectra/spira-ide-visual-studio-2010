using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business
{
	public partial class Spira_ImportExport
	{
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