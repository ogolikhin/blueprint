namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// Describes discussion body that needs to be sent in request for discussion create/update call
    /// </summary>
    public class RaptorComment
    {
        public string Comment { get; set; }
        public int Status { get; set; }
    }
}
