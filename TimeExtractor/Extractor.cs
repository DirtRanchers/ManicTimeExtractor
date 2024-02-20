using ManicTimeExtractor.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace ManicTimeExtractor
{
	internal class Extractor
	{

		private static Preferences preferences = Preferences.Instance;

		internal static List<CategoryData> GetCategoryDataCollection(DateTime startDate, DateTime endDate, string databaseFilepath)
		{
			InitializeMappings();

			//get raw data from the DB (one entry per time slice per hierarchical tag)
			var rawTimeEntries = GetTimeEntriesFromDB(startDate, endDate, databaseFilepath);

			//aggregate to one object per time slice (each with an internal hierarchical tags collection)
			var timeEntries = rawTimeEntries
				//.GroupBy(x => x.ActivityId)
				.Select(x => new TimeEntry(x.Tag)
				{
					ActivityId = x.ActivityId,
					StartTime = x.StartTime,
					EndTime = x.EndTime,
					NotesXml = x.NotesXml
				})
				.ToList();

			//group by date/category/loggable, each group having a collection of
			//time slices.
			var group1 = timeEntries
				.GroupBy(x => new
				{
					Date = x.StartTime.Date,
					Category = RemapCategory(x.Category),
					IsLoggable = IsLoggable(RemapCategory(x.Category))
				})
				.ToList();

			//group by category/loggable, each group having a collection, by date,
			//of total daily time for that category/loggable key
			var categoryAggregates = group1
				.Select(x => new
				{
					Date = x.Key.Date,
					Category = x.Key.Category,
					IsLoggable = x.Key.IsLoggable,
					TotalElapsedHours = GetRoundedHours(x.Sum(y => y.Elapsed.Ticks))
				})
				.GroupBy(x => new { Category = x.Category, IsLoggable = x.IsLoggable })
				.ToList();

			//transform into a list of timesheet rows 
			var list = new List<CategoryData>();
			int daysCount = GetDaysCount(startDate, endDate);

			//add a grand totals entry
			var cht = new CategoryData(
				Constants.TotalLoggableText,
				daysCount,
				Constants.DisplayOrderTotalLoggableLine);
			list.Add(cht);

			//transform the aggregates into an object we can map to the grid
			foreach (var category in categoryAggregates)
			{
				var ch = new CategoryData(
					category.Key.Category,
					daysCount,
					category.Key.IsLoggable ?
						Constants.DisplayOrderLoggableLines :
						Constants.DisplayOrderNonLoggableLines);
				foreach (var g in category)
				{
					int idx = (g.Date - startDate).Days;
					ch.Hours[idx] = g.TotalElapsedHours;
					//omit "not loggable" category from the grand totals
					if (category.Key.IsLoggable)
						cht.Hours[idx] += g.TotalElapsedHours;
				}
				list.Add(ch);
			}

			//add a blank line to appear after the loggable grand totals, 
			//before the non-loggable stuff
			list.Add(new
				CategoryData(string.Empty, daysCount, Constants.DisplayOrderBlankLine));

			return list;
		}

		/// <summary>
		/// Converts from ticks to hours, rounded 
		/// </summary>
		/// <param name="ticks"></param>
		private static decimal GetRoundedHours(long ticks)
		{
			var rounding = (int)(1 / preferences.RoundingIncrement);
			return (decimal)(Math.Round((new TimeSpan(ticks).TotalHours * rounding), 0) / rounding);
		}

		//actual mappings that have already taken place on this run
		//include "mappings" where output = input.  We save these,
		//so we don't have to refire the pattern twice against
		//an identical input value
		private static Dictionary<string, string> existingMappings;

		private static void InitializeMappings()
		{
			//clear the existing mappings for a re-run, in case the 
			//mappings have been changed
			existingMappings = new Dictionary<string, string>();
		}

		private static string RemapCategory(string currentCategory)
		{
			if (existingMappings.ContainsKey(currentCategory))
				return existingMappings[currentCategory];

			string newCategory = null;
			foreach (var mapping in preferences.CategoryMappings)
			{
				if (mapping.TryRemap(currentCategory, out newCategory))
					break;
			}

			//new = current (i.e., unchanged) if there isn't a mapping defined
			newCategory = newCategory ?? currentCategory;
			existingMappings[currentCategory] = newCategory;
			return newCategory;
		}

		/// <summary>
		/// Indicates whether category is loggable, based on its absence
		/// from the loggable blacklist
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		private static bool IsLoggable(string category)
		{
			foreach (var criterion in preferences.LoggableBlacklist)
			{
				if (criterion.IsMatch(category))
				{
					return false;
				}
			}
			return true;
		}



		/// <summary>
		/// Gets the number of days in the specified range
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		internal static int GetDaysCount(DateTime startDate, DateTime endDate)
				=> (endDate.Date - startDate.Date).Days + 1;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		private static List<RawTimeEntry> GetTimeEntriesFromDB(DateTime startDate, DateTime endDate, string databaseFilepath)
		{
			string query = $@"
				select 
					ActivityId, 
					StartLocalTime as [StartTime], 
					EndLocalTime as [EndTime], 
					Name as [Tag]
				from 
					Ar_Activity
				where 
					ReportId = 1
					and StartLocalTime >= @startDate
					and StartLocalTime < date(@endDate, '+1 day')
				order by 
					StartLocalTime;";

			List<RawTimeEntry> activityEntries;

			using (var dbConnection = new SQLiteConnection($@"Data Source={databaseFilepath}; Version=3; Read Only=True; FailIfMissing=True;"))
			using (var dbCommand = new SQLiteCommand(query, dbConnection))
			using (var adapter = new SQLiteDataAdapter(dbCommand))
			using (var dataset = new DataSet())
			{
				dbCommand.Parameters.Add(
					new SQLiteParameter("@startDate", startDate.Date)
					{
						DbType = DbType.DateTime
					});
				dbCommand.Parameters.Add(
					new SQLiteParameter("@endDate", endDate.Date)
					{
                        DbType = DbType.DateTime
                    });
				adapter.Fill(dataset);
				activityEntries = dataset.Tables[0]
					.AsEnumerable()
					.Select(x => new RawTimeEntry()
					{
						ActivityId = (int)x["ActivityId"],
						StartTime = (DateTime)x["StartTime"],
						EndTime = (DateTime)x["EndTime"],
						Tag = (string)x["Tag"],
						TagOrder = 1,
						NotesXml = ""
					})
					.ToList();
			}

			return activityEntries;
		}
	}
}
