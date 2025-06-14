using RadioBrowser.Internals.JsonConverters;
using System;
using System.Text.Json.Serialization;

namespace PtasieRadio.Models
{
	public class StationInfo
	{


		/// <summary>
		///     The name of the station.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		///     The stream URL provided by the user.
		/// </summary>
		[JsonConverter(typeof(UriConverter))]
		public Uri? Url { get; set; }

		/// <summary>
		///     An automatically "resolved" stream URL.
		///     Things resolved are playlists (M3U/PLS/ASX...), HTTP redirects (Code 301/302).
		///     This link is especially useful if you use this API from a platform that
		///     is not able to do a resolve on its own (e.g. JavaScript in browser)
		///     or you just don't want to invest the time in decoding playlists yourself.
		/// </summary>
		[JsonPropertyName("url_resolved")]
		[JsonConverter(typeof(UriConverter))]
		public Uri? UrlResolved { get; set; }

		/// <summary>
		///     URL to the homepage of the stream,
		///     so you can direct the user to a page with more information about the stream.
		/// </summary>
		[JsonConverter(typeof(UriConverter))]
		public Uri? Homepage { get; set; }

		/// <summary>
		///     URL to an icon or picture that represents the stream. (PNG, JPG)
		/// </summary>
		[JsonConverter(typeof(UriConverter))]
		public Uri? Favicon { get; set; }

		/// <summary>
		///     Tags of the stream with more information about it.
		/// </summary>
		[JsonConverter(typeof(ListConverter))]
		public List<string> Tags { get; set; } = new List<string>();

		/// <summary>
		///     Official country codes as in ISO 3166-1 alpha-2.
		/// </summary>
		public string? CountryCode { get; set; }

		/// <summary>
		///     Languages that are spoken in this stream.
		/// </summary>
		[JsonConverter(typeof(ListConverter))]
		public List<string> Language { get; set; } = new List<string>();

		/// <summary>
		///     The codec of this stream recorded at the last check.
		/// </summary>
		public string? Codec { get; set; }

	}
}
