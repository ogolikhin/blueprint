namespace Model.Impl
{
    public class Process: IProcess
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentId { get; set; }

        public int OrderIndex { get; set; }

        public int TypeId { get; set; }

        public string TypePrefix { get; set; }

        public int VersionId { get; set; }

        public string Description { get; set; }

        public ProcessType Type { get; set; }

        public string RawData { get; set; }

        public int? LockedByUserId { get; set; }
    }
}
