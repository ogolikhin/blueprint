namespace ServiceLibrary.Models.Workflow
{
    public static class WorkflowConstants
    {
        public const int MinNameLength = 1;
        public const int MaxNameLength = 128;

        public const int MinDescriptionLength = 0;
        public const int MaxDescriptionLength = 4000;

        // For Text Property Change action
        public const string PropertyNameName = "Name";
        public const int PropertyTypeFakeIdName = -1;
        public const string PropertyNameDescription = "Description";
        public const int PropertyTypeFakeIdDescription = -2;

    }
}