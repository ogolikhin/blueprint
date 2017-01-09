namespace Model
{
    public interface ILicenseUsageInfo
    {
        #region Properties

        int activityMonth { get; set; }
        int activityYear { get; set; }
        int authorsActive { get; set; }
        int authorsActiveLoggedOn { get; set; }
        int authorsCreatedtodate { get; set; }
        int collaboratorsActive { get; set; }
        int collaboratorsActiveLoggedOn { get; set; }
        int collaboratorsActivetodate { get; set; }
        int maxConCurrentAuthors { get; set; }
        int maxConCurrentCollaborators { get; set; }
        int maxConCurrentViewers { get; set; }
        int registeredAuthorsCreated { get; set; }
        int registeredCollaboratorsCreated { get; set; }
        int uniqueAuthors { get; set; }
        int uniqueCollaborators { get; set; }
        int uniqueViewers { get; set; }
        int usersFromAnalytics { get; set; }
        int usersFromRestApi { get; set; }

        #endregion Properties
    }
}
