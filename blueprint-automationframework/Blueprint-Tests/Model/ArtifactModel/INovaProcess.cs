using Model.StorytellerModel.Impl;

namespace Model.ArtifactModel
{
    public interface INovaProcess : INovaArtifactDetails
    {
        Process Process { get; set; }
    }
}
