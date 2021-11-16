using System.IO;
using log4net.Config;
using UnityEngine;

public static class LoggerConfig
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ConfigureLogging()
	{
		XmlConfigurator.Configure(new FileInfo($"{Application.dataPath}/log4net.xml"));
	}
}