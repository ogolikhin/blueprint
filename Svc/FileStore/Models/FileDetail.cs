namespace FileStore.Models
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Data.Entity.Spatial;

	public partial class FileDetail
	{
		[Key]
		[Column(Order = 0)]
		public Guid FileId { get; set; }

		[Column(Order = 1)]
		public DateTime StoredTime { get; set; }

		[Column(Order = 2)]
		[StringLength(256)]
		public string FileName { get; set; }

		[Column(Order = 3)]
		[StringLength(64)]
		public string FileType { get; set; }
	}
}
