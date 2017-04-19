using System.Collections.Generic;

namespace Model.NovaModel.Components.RapidReview
{
    // Taken from blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/Diagram.cs
    internal interface IDiagramElement
    {
        int Id { get; set; }

        string Type { get; set; }

        int? ParentId { get; set; }

        string Name { get; set; }

        string Label { get; set; }

        string Stroke { get; set; }

        double StrokeWidth { get; set; }

        string StrokeDashPattern { get; set; }

        List<Prop> Props { get; }

        double ZIndex { get; set; }
    }
}
