using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    public class RapidReviewDiagram
    {
        public int Id { get; set; }
        public string DiagramType { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<object> Shapes { get; set; }
        public List<object> Connections { get; set; }
        public int LibraryVersion { get; set; }
    }

    public class RapidReviewGlossary
    {
        public int Id { get; set; }
        public List<object> Terms { get; set; }
    }

    public class RapidReviewProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Format { get; set; }
        public int PropertyTypeId { get; set; }
    }

    public class RapidReviewAuthorHistory
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Format { get; set; }
        public int PropertyTypeId { get; set; }
    }

    public class RapidReviewDescription
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public int Format { get; set; }
        public int PropertyTypeId { get; set; }
    }
    public class RapidReviewProperties
    {
        public int ArtifactId { get; set; }
        public List<RapidReviewProperty> Properties { get; set; }
        public List<RapidReviewAuthorHistory> AuthorHistory { get; set; }
        public RapidReviewDescription Description { get; set; }
    }

    public class RapidReviewUseCasePreCondition
    {
        public string Description { get; set; }
        public int StepOf { get; set; }
        public List<object> Flows { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public double OrderIndex { get; set; }
    }

    public class RapidReviewUseCaseStep
    {
        public string Description { get; set; }
        public int StepOf { get; set; }
        public List<object> Flows { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public double OrderIndex { get; set; }
    }

    public class RapidReviewUseCasePostCondition
    {
        public string Description { get; set; }
        public int StepOf { get; set; }
        public List<object> Flows { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public double OrderIndex { get; set; }
    }

    public class RapidReviewUseCase
    {
        public int Id { get; set; }
        public RapidReviewUseCasePreCondition PreCondition { get; set; }
        public List<RapidReviewUseCaseStep> Steps { get; set; }
        public RapidReviewUseCasePostCondition PostCondition { get; set; }
    }
}
