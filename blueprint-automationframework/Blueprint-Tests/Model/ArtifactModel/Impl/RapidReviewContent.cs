using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    //To test that JSON can be deseriallized as a diagram representation for Rapid Review
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

    //To test that JSON can be deseriallized as a glossary representation for Rapid Review
    public class RapidReviewGlossary
    {
        public int Id { get; set; }
        public List<object> Terms { get; set; }
    }

    //auxilary class for RapidReviewProperties
    public class RapidReviewProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Format { get; set; }
        public int PropertyTypeId { get; set; }
    }

    //auxilary class for RapidReviewProperties
    public class RapidReviewAuthorHistory
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Format { get; set; }
        public int PropertyTypeId { get; set; }
    }

    //auxilary class for RapidReviewProperties
    public class RapidReviewDescription
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public int Format { get; set; }
        public int PropertyTypeId { get; set; }
    }

    //To test that JSON can be deseriallized as a list of properties
    public class RapidReviewProperties
    {
        public int ArtifactId { get; set; }
        public List<RapidReviewProperty> Properties { get; set; }
        public List<RapidReviewAuthorHistory> AuthorHistory { get; set; }
        public RapidReviewDescription Description { get; set; }
    }

    //auxilary class for RapidReviewUseCase
    public class RapidReviewUseCasePreCondition
    {
        public string Description { get; set; }
        public int StepOf { get; set; }
        public List<object> Flows { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public double OrderIndex { get; set; }
    }

    //auxilary class for RapidReviewUseCase
    public class RapidReviewUseCaseStep
    {
        public string Description { get; set; }
        public int StepOf { get; set; }
        public List<object> Flows { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public double OrderIndex { get; set; }
    }

    //auxilary class for RapidReviewUseCase
    public class RapidReviewUseCasePostCondition
    {
        public string Description { get; set; }
        public int StepOf { get; set; }
        public List<object> Flows { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public double OrderIndex { get; set; }
    }

    //To test that JSON can be deseriallized as Use Case representation for Rapid Review
    public class RapidReviewUseCase
    {
        public int Id { get; set; }
        public RapidReviewUseCasePreCondition PreCondition { get; set; }
        public List<RapidReviewUseCaseStep> Steps { get; set; }
        public RapidReviewUseCasePostCondition PostCondition { get; set; }
    }
}
