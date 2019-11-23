using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace ManicTimeExtractor.Model
{
	/// <summary>
	/// Persists preferences for users of this utility in an .xml file
	/// </summary>
	public sealed class Preferences
	{
		#region Persisted Properties

		public string DatabaseFilepath { get; set; }
			= GetDefaultDatabaseFilepath();

		public Mapping[] CategoryMappings { get; set; } = new Mapping[] { };

		public bool BillableDeterminesLoggable { get; set; }

		public bool StripWorkItemFromCategory { get; set; }


		public Criterion[] LoggableBlacklist { get; set; } = new Criterion[]
		{
			new Criterion("personal", MatchType.Equals)
		};

		public decimal RoundingIncrement
		{
			get { return roundingIncrement; }
			set { roundingIncrement = validRoundingIncrements.Contains(value) ? value : validRoundingIncrements[1]; }
		}
		private decimal roundingIncrement = .05M;

		#endregion

		private readonly decimal[] validRoundingIncrements = { .01M, .05M, .1M, .25M };


		private static string GetDefaultDatabaseFilepath()
		{
			string sdfFilename = "ManicTime.sdf";
			string localAppDataPath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%");
			string sdfFilepath = $@"{localAppDataPath}\Finkit\ManicTime\{sdfFilename}";
			return sdfFilepath;
		}

		/// <summary>
		/// XML Serializer for persisting to disk
		/// </summary>
		private static XmlSerializer serializer
			= new XmlSerializer(typeof(Preferences));

		/// <summary>
		/// Singleton instance of preferences.
		/// </summary>
		public static Preferences Instance { get; } = Load();

		/// <summary>
		/// Name of preferences disk file
		/// </summary>
		private static string preferencesFilename;

		/// <summary>
		/// Indicates whether any changes will be persisted to disk.   We 
		/// don't write to disk if the load failed, because we don't want to 
		/// wipe out all their changes, even if they were bad.
		/// </summary>
		private static bool memoryOnly;



		/// <summary>
		/// Constructor preventing external instantiation
		/// </summary>
		private Preferences()
		{
			try
			{
				preferencesFilename = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"DH",
					Constants.AppNameClean,
					"preferences.xml");
				Directory.CreateDirectory(Path.GetDirectoryName(preferencesFilename));
			}
			catch
			{
				ShowPreferencesLoadError();
				memoryOnly = true;

				//ignore exceptions, persisting the preferences are just a convenience
				//the utility shouldn't blow up just for failure to load them
			}
		}

		private static void ShowPreferencesLoadError()
		{
			MessageBox.Show("An error occurred loading the previous session's options from disk. " +
				"To prevent loss of those options, no changes made during the current session will be saved to disk.",
				"Error Loading Options", MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		/// <summary>
		/// Saves the preferences to disk.
		/// </summary>
		public void Save()
		{
			if (memoryOnly)
				return;

			try
			{
				using (var stream = new FileStream(preferencesFilename,
					FileMode.Create, FileAccess.Write, FileShare.None))
				{
					serializer.Serialize(stream, this);
				}
			}
			catch
			{
				//ignore exceptions, persisting the preferences are just a convenience
				//the utility shouldn't blow up just for failure to save them
			}
		}

		/// <summary>
		/// Loads the preferences from disk (or initializes them to defaults)
		/// if no disk file exists, or errors occur loading it.)
		/// </summary>
		/// <returns>Current preferences</returns>
		private static Preferences Load()
		{
			try
			{
				Preferences instance = new Preferences();

				if (File.Exists(preferencesFilename))
				{
					using (var stream = new FileStream(preferencesFilename,
						FileMode.Open, FileAccess.Read, FileShare.None))
					{
						instance = (Preferences)serializer.Deserialize(stream);
					}
				}

				var path = instance.DatabaseFilepath;
				instance.DatabaseFilepath =
					Environment.ExpandEnvironmentVariables(path);

				//if load was successful, immediately save the newly generated 
				//instance; this has the effect of creating a new preferences
				//file if there was none (at first execution of the app), and 
				//at first execution of a new release, adds any new
				//properties, and removes obsolete ones
				instance.Save();

				return instance;

			}
			catch
			{
				ShowPreferencesLoadError();
				memoryOnly = true;

				//ignore exceptions, persisting the preferences are just a convenience
				//the utility shouldn't blow up just for failure to load them
				return new Preferences();
			}
		}

		/// <summary>
		/// Returns the directory to the preferences file
		/// </summary>
		internal static string GetPreferencesFolder() =>
			Path.GetDirectoryName(preferencesFilename);
	}
}
