using System.Xml.Serialization;

namespace ManicTimeExtractor.Model
{
	/// <summary>
	/// Defines a mapping between a tag or partial tag, and the desired 
	/// output category
	/// </summary>
	public class Mapping
	{
		private Mapping() { }  //required for serialization

		public Mapping(string categoryFragment, string category, MatchType matchType)
		{
			this.Criterion = new Criterion(categoryFragment, matchType);
			this.NewCategory = category;
		}

		public Criterion Criterion { get; set; }

		[XmlAttribute("newCategory")]
		public string NewCategory { get; set; }

		public bool TryRemap(string currentCategory, out string newCategory)
		{
			newCategory = Criterion.IsMatch(currentCategory) ? NewCategory : null;
			return newCategory != null;
		}
	}


}
