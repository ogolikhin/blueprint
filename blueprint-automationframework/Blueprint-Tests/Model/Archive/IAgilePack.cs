using Model.ArtifactModel;

namespace Model.Archive
{
    public enum MoSCoW
    {
        Could,
        Must,
        Should,
        Wont
    }

    public enum TShirt
    {
        XS,
        S,
        M,
        L,
        XL
    }


    public interface IAgilePack : IArtifact
    {
        string AcceptanceCriteria { get; set; }
        MoSCoW MoSCoW { get; set; }
        int StoryPoints { get; set; }
        TShirt TShirt { get; set; }
    }
}
