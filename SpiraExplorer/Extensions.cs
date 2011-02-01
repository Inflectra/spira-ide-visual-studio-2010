using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010
{
	public static class Extensions
	{
		public static bool TrimEquals(this string obj1, string obj2)
		{
			if (obj1 == null && obj2 == null)
				return true;
			else if (obj1 != null && obj2 != null)
			{
				if (obj1.GetType() == typeof(string) && obj2.GetType() == typeof(string))
				{
					return string.Equals(((string)obj1).Trim(), ((string)obj2).Trim());
				}
				else
					return false;
			}
			else
				return false;
		}
	}
}
