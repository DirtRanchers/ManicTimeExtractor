using System.Reflection;

namespace ManicTimeExtractor
{
	internal class Constants
	{
		internal const string TotalLoggableText = "Total Loggable";
		internal const int DisplayOrderLoggableLines = 5;
		internal const int DisplayOrderTotalLoggableLine = 20;
		internal const int DisplayOrderBlankLine = 25;
		internal const int DisplayOrderNonLoggableLines = 30;

		internal static readonly string AppNameClean =
			Assembly.GetExecutingAssembly().GetName().Name;
	}
}
