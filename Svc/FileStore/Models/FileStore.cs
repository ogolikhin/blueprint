namespace FileStore.Models
{
	using System;
	using System.Data.Entity;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Linq;

	public partial class FileStoreContext : DbContext
	{
		public FileStoreContext()
			 : base("name=FileStoreDatabase")
		{
		}

		public virtual DbSet<File> Files { get; set; }
		public virtual DbSet<FileDetail> FileDetails { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
		}
	}
}
