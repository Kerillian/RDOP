using System;
using System.Text.Json.Serialization;

namespace RDOP
{
	[Serializable]
	public class Config
	{
		[JsonPropertyName("passphrase")]
		public string? Passphrase { get; set; }
		
		[JsonPropertyName("path")]
		public string? Path { get; set; }
	}
}