namespace FileStore.Models
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Data.Entity.Spatial;

	public partial class File
	{
		[Key]
		public Guid FileId { get; set; }

		public DateTime StoredTime { get; set; }

		[Required]
		[StringLength(256)]
		public string FileName { get; set; }

		[Required]
		[StringLength(64)]
		public string FileType { get; set; }

		public byte[] FileContent { get; set; }
	}
}
