using log4net.Appender;
using log4net.Core;
using UnityEngine;

public class ConsoleLogger : AppenderSkeleton
{
	protected override void Append(LoggingEvent loggingEvent)
	{
		var message = RenderLoggingEvent(loggingEvent);
		Debug.Log(message);
	}
}