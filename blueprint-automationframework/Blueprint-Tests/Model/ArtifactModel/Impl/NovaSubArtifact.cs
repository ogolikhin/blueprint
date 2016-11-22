namespace Model.ArtifactModel.Impl
{
    // Taken from:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Nova/Models/NovaSubArtifact.cs
    public class NovaSubArtifact : NovaItem
    {
        #region Serialized JSON Properties

        public bool? IsDeleted { get; set; }

        #endregion Serialized JSON Properties

        // NovaSubArtifacts always have the CustomPropertyValues and SpecificPropertyValues properties, even if they're empty.
        // TODO: Check why this happens, because based on the dev code, it looks impossible.
        public override bool ShouldSerializeCustomPropertyValues()
        {
            return true;
        }

        public override bool ShouldSerializeSpecificPropertyValues()
        {
            return true;
        }
    }
}
