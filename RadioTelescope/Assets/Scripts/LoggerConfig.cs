using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using System;
using System.IO;
using UnityEngine;

public static class LoggerConfig
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ConfigureLogging()
	{
		PatternLayout consoleLayout = new PatternLayout
			{ ConversionPattern = "%p %c{1}:%L - %m%n" };
		consoleLayout.ActivateOptions();
		
		ConsoleLogger console = new ConsoleLogger();
		console.Layout = consoleLayout;
		console.ActivateOptions();
		
		PatternLayout fileLayout = new PatternLayout
			{ ConversionPattern = "%d{ABSOLUTE} %-5p %-22c{1} - %m%n" };
		fileLayout.ActivateOptions();
		
		FileLogger file = new FileLogger();
		file.Layout = fileLayout;
		file.ActivateOptions();
		
		BasicConfigurator.Configure(console, file);
	}
}

public class ConsoleLogger : AppenderSkeleton
{
	private string last = "";
	protected override void Append(LoggingEvent loggingEvent)
	{
		string message = RenderLoggingEvent(loggingEvent);
		if(message == last)
			return;
		
		Debug.Log(message);
		
		last = message;
	}
}

public class FileLogger : AppenderSkeleton
{
	private string last = "";
	private string logName = "";
	
	protected override void Append(LoggingEvent loggingEvent)
	{
		if(logName == "")
		{
			string date = DateTime.Now.ToString().Replace("/", "-").Replace(":", ".");
			logName = "Simulation Log " + date + ".txt"; 
		}
		
		string message = RenderLoggingEvent(loggingEvent);
		
		// Don't write duplicate messages.
		string truncated = message.Substring(message.IndexOf(" ") + 1);
		if(last == truncated)
			return;
		
		using(StreamWriter writer = new StreamWriter(logName, true))
		{
			writer.Write(message);
		}
		last = truncated;
	}
}
