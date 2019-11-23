using System;
using System.Collections.Generic;
using System.Linq;

namespace ManicTimeExtractor.Model
{

	/// <summary>
	/// Represents a single time slice, with all of its hierarchical tags
	/// e.g.,  a 5 minute time slice with tags  "Cat1"+"Subcat1" will 
	/// be represented as a single instance, with a Tags collection having
	/// two entries:  one for "Cat1" and another for "Subcat1"
	/// 
	/// </summary>
	internal class TimeEntry
	{

		public TimeEntry(IEnumerable<string> tags)
		{
			PopulateTags(tags);
		}

		public int ActivityId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public TimeSpan Elapsed => EndTime - StartTime;

		public List<string> Tags { get; set; } = new List<string>();

		public string NotesXml { get; set; }

		public string Category =>
			Tags.FirstOrDefault() ?? "{blank}";

		public bool IsBillable { get; private set; }


		private void PopulateTags(IEnumerable<string> tags)
		{
			Tags = tags.ToList();
			for (int i = 0; i < Tags.Count(); ++i)
			{
				var tag = Tags[i];
				int number;
				if (tag.StartsWith("tfs", StringComparison.OrdinalIgnoreCase)
					&& int.TryParse(tag.Substring(3), out number))
				{
					TfsWorkItem = number;
					if (i == 0 && Preferences.Instance.StripWorkItemFromCategory)
						Tags[0] = "tfs";
					break;
				}
			}

			if (Tags.LastOrDefault() == ":billable")
			{
				IsBillable = true;
				Tags.RemoveAt(Tags.Count() - 1);
			}

		}
		public int? TfsWorkItem { get; set; }
	}
}
