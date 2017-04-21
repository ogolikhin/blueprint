using Model.ArtifactModel.Enums;
using System.Collections.Generic;

namespace Model.NovaModel.Components.RapidReview
{
    // Taken from blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/Diagram.cs
    public class Shape : IDiagramElement
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ParentId { get; set; }

        public string Type { get; set; }

        public double Height { get; set; }

        public double Width { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double ZIndex { get; set; }

        public double Angle { get; set; }

        public string Stroke { get; set; }

        public double StrokeOpacity { get; set; }

        public double StrokeWidth { get; set; }

        public string StrokeDashPattern { get; set; }

        public string Fill { get; set; }

        public string GradientFill { get; set; }

        public bool IsGradient { get; set; }

        public double FillOpacity { get; set; }

        public bool Shadow { get; set; }

        public string Label { get; set; }

        //UNDONE: should be moved into LabelStyle property
        public string LabelTextAlignment { get; set; }

        public string Description { get; set; }

        public ItemIndicatorFlags IndicatorFlags { get; set; }

        private List<Prop> _props;
        public List<Prop> Props
        {
            get
            {
                if (_props == null)
                {
                    _props = new List<Prop>();
                }
                return _props;
            }
        }

        private LabelStyle _labelStyle;
        public LabelStyle LabelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new LabelStyle();
                }

                return _labelStyle;
            }
        }
    }
}



