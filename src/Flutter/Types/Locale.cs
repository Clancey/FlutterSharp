namespace Flutter
{
	/// <summary>
	/// An identifier used to select a user's language and formatting preferences.
	/// </summary>
	public class Locale
	{
		public string LanguageCode { get; set; }
		public string? CountryCode { get; set; }
		public string? ScriptCode { get; set; }

		public Locale(string languageCode, string? countryCode = null)
		{
			LanguageCode = languageCode;
			CountryCode = countryCode;
		}
	}
}
