namespace AdminStore.Models.DTO
{
    public class FolderDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }      
        public int? ParentFolderId { get; set; }
    }
}