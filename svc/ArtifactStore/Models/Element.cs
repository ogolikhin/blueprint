using Newtonsoft.Json;
using System;

namespace ArtifactStore.Models
{
	[JsonObject]
	public class Element
	{
		[JsonProperty]
		public int ProjectId { get; set; }
		[JsonProperty]
		public int ArtifactId { get; set; }
		[JsonProperty]
		public int ElementtId { get; set; }
	}
}
