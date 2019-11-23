using System.Linq;

namespace ManicTimeExtractor.Model
{
	public class CategoryData
	{

		public CategoryData(string category, int daysCount, int displayOrder)
		{
			Category = category;
			DisplayOrder = displayOrder;
			Hours = new decimal[daysCount];
		}
		public string Category { get; private set; }

		public int DisplayOrder { get; private set; }

		public decimal[] Hours { get; private set; }

		public string[] HoursStrings => GetHoursStrings();

		private string[] GetHoursStrings()
		{
			var values = Hours.Select(x => x.ToString());
			if (Category != Constants.TotalLoggableText)
			{
				values = values
					.Select(x => x.Contains(".") ? x.TrimEnd('0') : x)
					.Select(x => x == "0" ? string.Empty : x);
			}
			return values.ToArray();
		}
	}
}
