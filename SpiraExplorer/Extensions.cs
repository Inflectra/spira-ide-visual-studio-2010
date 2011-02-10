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

		public static string Truncate(this string str1, int maxLength, TruncateOptionsEnum Options)
		{
			if (string.IsNullOrWhiteSpace(str1))
			{
				return "";
			}

			if (str1.Length <= maxLength)
			{
				return str1;
			}

			bool includeEllipsis = ((Options & TruncateOptionsEnum.IncludeEllipsis) == TruncateOptionsEnum.IncludeEllipsis);
			bool finishWord = ((Options & TruncateOptionsEnum.FinishWord) == TruncateOptionsEnum.FinishWord);
			bool allowLastWordOverflow = ((Options & TruncateOptionsEnum.AllowLastWordToGoOverMaxLength) == TruncateOptionsEnum.AllowLastWordToGoOverMaxLength);

			string retValue = str1;

			if (includeEllipsis)
			{
				maxLength -= 3;
			}

			int lastSpaceIndex = retValue.LastIndexOf(" ",
				maxLength, StringComparison.CurrentCultureIgnoreCase);

			if (!finishWord)
			{
				retValue = retValue.Remove(maxLength);
			}
			else if (allowLastWordOverflow)
			{
				int spaceIndex = retValue.IndexOf(" ",
					maxLength, StringComparison.CurrentCultureIgnoreCase);
				if (spaceIndex != -1)
				{
					retValue = retValue.Remove(spaceIndex);
				}
			}
			else if (lastSpaceIndex > -1)
			{
				retValue = retValue.Remove(lastSpaceIndex);
			}

			if (includeEllipsis && retValue.Length < str1.Length)
			{
				retValue += "...";
			}
			return retValue;
		}

		public enum TruncateOptionsEnum
		{
			None = 0x0,
			FinishWord = 0x1,
			AllowLastWordToGoOverMaxLength = 0x2,
			IncludeEllipsis = 0x4
		}
	}
}
