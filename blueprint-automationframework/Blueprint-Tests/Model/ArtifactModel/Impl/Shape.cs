using Model.ArtifactModel.Enums;
using System.Collections.Generic;
using System.Linq;


namespace Model.ArtifactModel.Impl
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

        private PropCollection _props;
        public PropCollection Props
        {
            get
            {
                if (_props == null)
                {
                    _props = new PropCollection();
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

    public class LabelStyle
    {
        //UNDONE: Not used yet
        public string TextAlignment { get; set; }

        public string FontFamily { get; set; }

        public string FontSize { get; set; }

        public bool IsItalic { get; set; }

        public bool IsBold { get; set; }

        public bool IsUnderline { get; set; }

        public string Foreground { get; set; }
    }

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

        PropCollection Props { get; }

        double ZIndex { get; set; }
    }

    public class PropCollection : List<Prop>
    {
    }

    public class Prop
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }

    public struct Point
    {
        public double Y { get; set; }

        public double X { get; set; }

        public static bool operator ==(Point point1, Point point2)
        {
            if (point1.X == point2.X)
            {
                return point1.Y == point2.Y;
            }
            return false;
        }

        public static bool operator !=(Point point1, Point point2)
        {
            return !(point1 == point2);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Point))
            {
                return false;
            }
            return this == (Point)obj;
        }

        public bool Equals(Point value)
        {
            return this == value;
        }
    }
}



