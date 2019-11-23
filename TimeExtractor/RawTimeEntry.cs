using System;

namespace ManicTimeExtractor
{
	//data from the DB, one "row" per time slice per hierarchical tag
	//e.g.,  a single 5 minute time slice with the tag  "Cat1"+"Subcat1"
	//       will have two rows here, one for each part of the tag, one
	//       with Tag="Cat1" and TagOrder=1,  and another with Tag="Subcat1"
	//       and TagOrder=2
	internal class RawTimeEntry
	{
		public int ActivityId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public string Tag { get; set; }

		public int TagOrder { get; set; }
		public string NotesXml { get; set; }
	}
}
