using ManicTimeExtractor.Model;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ManicTimeExtractor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DateTime today = DateTime.Now.Date;
			int dowToday = (int)today.DayOfWeek;
			int adjust = 3 - ((dowToday + 5) % 7);
			datePickerEnd.SelectedDate = today.AddDays(adjust);
			datePickerStart.SelectedDate = today.AddDays(adjust - 6);
			textBoxDbFile.Text = preferences.DatabaseFilepath;
			menuItemStripWorkItem.IsChecked = preferences.StripWorkItemFromCategory;
			menuItemBillable.IsChecked = preferences.BillableDeterminesLoggable;

			CopySamplePreferences();

			SetUiState(true);
			setupComplete = true;
		}

		private void CopySamplePreferences()
		{
			var filename = "sample_preferences.xml";
			var source = Path.GetFullPath($"doc\\{filename}");
			File.Copy(source, $"{Preferences.GetPreferencesFolder()}\\{filename}", true);
		}

		Preferences preferences = Preferences.Instance;

		List<CategoryData> categoryDataCollection = new List<CategoryData>();

		private bool setupComplete = false;

		DateTime startDate => (DateTime)datePickerStart.SelectedDate;
		DateTime endDate => (DateTime)datePickerEnd.SelectedDate;

		private void buttonExtract_Click(object sender, RoutedEventArgs e)
		{
			PopulateGrid(false);

			//try
			//{
			//	try
			//	{
			//		throw new Exception("inner 2");

			//	}
			//	catch (Exception e4)
			//	{
			//		throw new ApplicationException("inner 1", e4);
			//	}
			//}
			//catch (Exception e5)
			//{
			//	throw new InvalidOperationException("main", e5);
			//}
		}

		private void PopulateGrid(bool refreshOnly)
		{

			if (refreshOnly && !dataGrid.Columns.Any())
				return;

			var daysCount = Extractor.GetDaysCount(startDate, endDate);

			if (daysCount > 31)
			{
				MessageBox.Show(this, "Choose a date range of 31 days or less.",
					"Date Selection Error",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (daysCount < 0)
			{
				MessageBox.Show(this,
					"Start date is after End date",
					"Date Selection Error",
					MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			try
			{
				//extract the data
				try
				{
					SetUiState(false);
					ClearGrid();
					categoryDataCollection =
						Extractor.GetCategoryDataCollection(
							startDate, endDate,
							preferences.DatabaseFilepath)
						.OrderBy(x => x.DisplayOrder)
						.ThenBy(x => x.Category)
						.ToList();
				}
				catch (Exception ex)
				{
					MessageBox.Show(this,
						$"Error occurred extracting data from ManicTime DB:\n\n{ex.Message}",
						"Error Extracting DB Data",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
					return;
				}

				//populate the grid with the data
				try
				{
					dataGrid.ItemsSource = categoryDataCollection;
					dataGrid.Width = Double.NaN;
					dataGrid.Columns.Clear();
					dataGrid.Columns.Add(new DataGridTextColumn()
					{
						Header = "Category",
						Binding = new Binding(nameof(CategoryData.Category))
					});

					for (int i = 0; i < daysCount; ++i)
					{
						dataGrid.Columns.Add(new DataGridTextColumn()
						{
							Header = startDate.AddDays(i).ToString("ddd MM/dd"),
							Binding = new Binding($"{nameof(CategoryData.HoursStrings)}[{i}]")
						});
					}

					textBoxGrandTotalLoggable.Text =
						categoryDataCollection
							.SingleOrDefault(x => x.Category == Constants.TotalLoggableText)
							?.Hours.Sum().ToString();
				}
				catch (Exception ex)
				{
					ClearGrid();
					//clear the grid of any data since we don't know what
					//stage it got through before erroring
					MessageBox.Show(this,
						$"Error occurred populating grid:\n\n{ex.Message}",
						"Error Populating Grid",
						MessageBoxButton.OK,
						MessageBoxImage.Error);
					return;
				}
			}
			finally
			{
				SetUiState(true);
			}
		}

		private void ClearGrid()
		{
			dataGrid.ItemsSource = null;
			dataGrid.Columns.Clear();
		}

		private void buttonBrowse_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new CommonOpenFileDialog();
			dialog.Title = "Choose ManicTime database file";
			dialog.IsFolderPicker = false;
			dialog.EnsureFileExists = true;
			dialog.DefaultFileName = System.IO.Path.GetFileName(preferences.DatabaseFilepath);
			dialog.InitialDirectory = System.IO.Path.GetDirectoryName(preferences.DatabaseFilepath);
			dialog.Filters.Add(new CommonFileDialogFilter("SQL Compact 4.0 DB file", ".sdf"));


			var result = dialog.ShowDialog();
			if (result == CommonFileDialogResult.Ok)
			{
				textBoxDbFile.Text = dialog.FileName;
				preferences.DatabaseFilepath = dialog.FileName;
				SetUiState(true);
				preferences.Save();
			}

		}

		/// <summary>
		/// Set state of UI controls that depend on other controls.
		/// </summary>
		/// <param name="enabled">If true, specifies that the entire UI 
		/// should be disabled from receiving user input, (e.g., during a 
		/// long-running process.)  
		/// If false, only those controls are disabled that are not
		/// valid for use, based on content of the rest of the UI.
		/// </param>
		private void SetUiState(bool enabled)
		{

			if (!this.IsInitialized)
			{
				//the UI isn't fully initialized yet, some controls may still
				//be null, so don't try to read/write them yet.
				return;
			}

			if (enabled)
			{
				this.IsEnabled = true;
				this.buttonExtract.IsEnabled =
					File.Exists(textBoxDbFile.Text);
				SetRoundingMenu();
			}
			else
			{
				this.IsEnabled = false;
			}
		}

		private void menuItemUseBillable_Changed(object sender, RoutedEventArgs e)
		{
			if (!setupComplete)
				return;

			preferences.BillableDeterminesLoggable =
				menuItemBillable.IsChecked;
			preferences.Save();
			PopulateGrid(true);
		}

		private void menuItemStripWorkItem_Changed(object sender, RoutedEventArgs e)
		{
			if (!setupComplete)
				return;

			preferences.StripWorkItemFromCategory =
				menuItemStripWorkItem.IsChecked;
			preferences.Save();
			PopulateGrid(true);
		}



		private void menuItemRound_Click(object sender, RoutedEventArgs e)
		{
			var menuItem = (MenuItem)sender;

			var currentIncrement = preferences.RoundingIncrement;

			if (menuItem == menuItemRoundNone)
				preferences.RoundingIncrement = .01M;
			else if (menuItem == menuItemRound10)
				preferences.RoundingIncrement = .10M;
			else if (menuItem == menuItemRound25)
				preferences.RoundingIncrement = .25M;
			else //default
				preferences.RoundingIncrement = .05M;
			preferences.Save();

			SetRoundingMenu();

			if (currentIncrement != preferences.RoundingIncrement)
				PopulateGrid(true);
		}

		private void SetRoundingMenu()
		{
			this.menuItemRoundNone.IsChecked =
				(preferences.RoundingIncrement == .01M);
			this.menuItemRound05.IsChecked =
				(preferences.RoundingIncrement == .05M);
			this.menuItemRound10.IsChecked =
				(preferences.RoundingIncrement == .10M);
			this.menuItemRound25.IsChecked =
				(preferences.RoundingIncrement == .25M);
		}

		private void menuItemAbout_Click(object sender, RoutedEventArgs e)
		{
			var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			MessageBox.Show(
				this, 
				"Application\n" +
				"------------\n" +
				$"{Title} v{currentVersion}\n\n" +
				"Purpose\n" +
				"---------\n" +
				"Extract timesheet data from ManicTime database and " +
				"format it for ease of entry into PWA timesheets\n\n" +
				"Support\n" +
				"---------\n" +
				"Richard Ernst (x3535)", 
				$"About {Title}", 
				MessageBoxButton.OK, MessageBoxImage.None);
        }

		private void menuItemExit_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void menuItemLocalData_Click(object sender, RoutedEventArgs e)
		{
			Process.Start(Preferences.GetPreferencesFolder());
		}
	}

	public class RowStyleSelector : StyleSelector
	{
		public override Style SelectStyle(object item, DependencyObject container)
		{
			FrameworkElement fe = container as FrameworkElement;
			if (fe == null)
				throw new ArgumentException("not a FrameworkElement", "container");

			var order = (item as CategoryData)?.DisplayOrder;

			Style style = null;
			if (order == null)
				style = null;
			else if (order == Constants.DisplayOrderTotalLoggableLine)
				style = fe.TryFindResource("totalsRowStyle") as Style;
			else if (order == Constants.DisplayOrderNonLoggableLines)
				style = fe.TryFindResource("personalRowStyle") as Style;
			return style;
		}
	}
}
