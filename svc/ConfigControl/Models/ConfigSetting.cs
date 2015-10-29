using Newtonsoft.Json;
using System;

namespace ConfigControl.Models
{
	[JsonObject]
	public class ConfigSetting
	{
		[JsonProperty]
		public string Key { get; set; }
		[JsonProperty]
		public string Value { get; set; }
		[JsonProperty]
		public string Group { get; set; }
		[JsonProperty]
		public bool IsRestricted { get; set; }
	}
}
