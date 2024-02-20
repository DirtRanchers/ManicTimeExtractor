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

		public TimeEntry(string tags)
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

		private void PopulateTags(string tags)
		{
			Tags = tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
		}

	}
}
