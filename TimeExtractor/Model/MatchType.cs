using System.Xml.Serialization;

namespace ManicTimeExtractor.Model
{
	public enum MatchType
	{
		[XmlEnum("equals")]
		Equals,
		[XmlEnum("startsWith")]
		StartsWith,
		[XmlEnum("endsWith")]
		EndsWith,
		[XmlEnum("contains")]
		Contains
	}
}
