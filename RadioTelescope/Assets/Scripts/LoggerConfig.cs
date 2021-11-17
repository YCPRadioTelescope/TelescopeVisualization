using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using System.IO;
using UnityEngine;

public static class LoggerConfig
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ConfigureLogging()
	{
		PatternLayout consoleLayout = new PatternLayout
			{ ConversionPattern = "%d{ABSOLUTE} %c{1}:%L - %m%n" };
		consoleLayout.ActivateOptions();
		
		ConsoleLogger console = new ConsoleLogger();
		console.Layout = consoleLayout;
		console.ActivateOptions();
		
		PatternLayout fileLayout = new PatternLayout
			{ ConversionPattern = "%d{ABSOLUTE} %c{1} - %m%n" };
		fileLayout.ActivateOptions();
		
		FileLogger file = new FileLogger();
		file.Layout = fileLayout;
		file.ActivateOptions();
		
		BasicConfigurator.Configure(console, file);
		
		// Overwrite any previous log file.
		using(StreamWriter writer = new StreamWriter("Log.txt"));
	}
}

public class ConsoleLogger : AppenderSkeleton
{
	protected override void Append(LoggingEvent loggingEvent)
	{
		var message = RenderLoggingEvent(loggingEvent);
		Debug.Log(message);
	}
}

public class FileLogger : AppenderSkeleton
{
	protected override void Append(LoggingEvent loggingEvent)
	{
		var message = RenderLoggingEvent(loggingEvent);
		using(StreamWriter writer = new StreamWriter("Log.txt", true))
		{
			writer.Write(message);
		}
	}
}
