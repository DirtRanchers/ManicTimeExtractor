using System;
using System.Xml.Serialization;


namespace ManicTimeExtractor.Model
{
	/// <summary>
	/// Defines a textual match criterion to apply against a string.
	/// </summary>
	public class Criterion
	{
		private Criterion() { }  //required for serialization

		public Criterion(string fragment, MatchType matchType)
		{
			this.Fragment = fragment;
			this.MatchType = matchType;
		}

		[XmlAttribute("matchType")]
		public MatchType MatchType { get; set; }

		[XmlAttribute("fragment")]
		public string Fragment { get; set; }

		/// <summary>
		/// Determines if the specified input string meets this instance's requirements
		/// </summary>
		/// <param name="input"></param>
		internal bool IsMatch(string input)
		{
			switch (MatchType)
			{
				case MatchType.Equals:
					return input.Equals(Fragment, StringComparison.OrdinalIgnoreCase);
				case MatchType.StartsWith:
					return input.StartsWith(Fragment, StringComparison.OrdinalIgnoreCase);
				case MatchType.EndsWith:
					return input.EndsWith(Fragment, StringComparison.OrdinalIgnoreCase);
				case MatchType.Contains:
					return (input.IndexOf(Fragment, StringComparison.OrdinalIgnoreCase) >= 0);
				default:
					return false;
			}
		}
	}
}
