using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;


namespace ManicTimeExtractor
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		/// <summary>
		/// Catch all unhandled exceptions and display the information in a
		/// dialog box.  If there are inner exceptions, they all will be
		/// displayed also.
		/// </summary>
		/// <param name="sender">
		/// The sender of the exception, not used here.
		/// </param>
		/// <param name="e">
		/// EventArgs that contain the exception to be displayed.
		/// </param>
		private void Application_DispatcherUnhandledException(
			object sender,
			DispatcherUnhandledExceptionEventArgs e)
		{

				var appName = "ManicTime Extractor";
				e.Handled = true;
				Exception currentException = e.Exception;
				StringBuilder exceptionMessage = new StringBuilder();
				while (currentException != null)
				{
					exceptionMessage.Append(
						$"{currentException.Message}\n{currentException.StackTrace}\n");
					currentException = currentException.InnerException;
				}

				MessageBox.Show($"An error occurred that {appName} is unable to handle. " +
					"Before exiting, it will now attempt to create an email about the error " +
					"for its developer.\n\n" +
					"Ensure Outlook is already started, and then click 'OK'",
					$"{appName} - Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

			try
			{
				var a = new Microsoft.Office.Interop.Outlook.Application();
				var item = a.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
				var time = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
				item.Subject = $@"Error Report: {appName} - {time}";
                item.To = "richard.ernst@dh.com";
				item.Body = "Add any information you think might be helpful in diagnoses " +
							"between the arrows, then 'Send'.\n\n=======>\n\n\n\n=======>\n\n" +
							exceptionMessage.ToString();
				item.Display(true);
			}
			catch
			{
				/* ignore, app will just fail silently at this point */
			}

			Environment.Exit(1);
		}
	}
}
