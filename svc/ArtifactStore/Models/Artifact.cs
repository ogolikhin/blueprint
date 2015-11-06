using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ArtifactStore.Models
{
	[JsonObject]
	public class Artifact
	{
		[JsonProperty]
		public int ProjectId { get; set; }
		[JsonProperty]
		public int ArtifactId { get; set; }
		[JsonProperty]
		public int? LockedByUserId { get; set; }
		[JsonProperty]
		public DateTime? LockedByUserTime { get; set; }
		[JsonProperty]
		public IEnumerable<Element> Elements { get; set; }
	}
}
