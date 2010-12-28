using System;
using System.Diagnostics;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2010.Business
{
	public static class Logger
	{
		private static EventLog _eventLog;
		public static bool TracingEnabled = true;

		static Logger()
		{
			//If the event log is null, try to create it.
			try
			{
				if (!System.Diagnostics.EventLog.SourceExists(StaticFuncs.getCultureResource.GetString("app_ReportName")))
					System.Diagnostics.EventLog.CreateEventSource(StaticFuncs.getCultureResource.GetString("app_ReportName"), "Application");
				if (Logger._eventLog == null)
					Logger._eventLog = new System.Diagnostics.EventLog("Application", ".", StaticFuncs.getCultureResource.GetString("app_ReportName"));
			}
			catch (Exception ex)
			{
				//TODO: Write to local file here.
			}
		}

		/// <summary>Logs a message to the EventLog using the specified type.</summary>
		/// <param name="Message">The message to log.</param>
		/// <param name="messageType">The type of the message.</param>
		public static void LogMessage(string Message, EventLogEntryType messageType = EventLogEntryType.Information)
		{
			Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.FFFFFFF") + ": " + Message);

			if ((messageType != EventLogEntryType.Information && messageType != EventLogEntryType.SuccessAudit) || Logger.TracingEnabled)
				if (Logger._eventLog != null)
					Logger._eventLog.WriteEntry(Message, messageType);
		}

		/// <summary>Logs an exception to the EventLog as an Error type message.</summary>
		/// <param name="ex">The exception to log.</param>
		public static void LogMessage(Exception ex, string message = null)
		{
			//Generate message.
			string strTrace = ex.StackTrace;
			string strMsg = ex.Message;
			while (ex.InnerException != null)
			{
				strMsg += Environment.NewLine + ex.InnerException.Message;
				ex = ex.InnerException;
			}
			strMsg += Environment.NewLine + Environment.NewLine + strTrace;
			if (message != null) strMsg = message + Environment.NewLine + strMsg;

			Logger.LogMessage(strMsg, EventLogEntryType.Error);
		}

		/// <summary>Logs a debug message to the debug console and EventLog if enabled.</summary>
		/// <param name="Message">The string to log.</param>
		public static void LogTrace(string Message)
		{
			//Log it to the debug console.
			Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.FFFFFFF") + ": " + Message);
			//Log it to the event log if enabled.
			if (Logger.TracingEnabled)
				Logger.LogMessage(Message, EventLogEntryType.Information);
		}

	}
}
