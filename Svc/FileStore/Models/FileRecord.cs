using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStore.Models
{
	public class FileRecord
	{
		[Key]
		public string FileId { get; set; }
		[Required]
		public DateTime StoredTime { get; set; }
		public string FileName { get; set; }
		public string FileType { get; set; }
	}
}
