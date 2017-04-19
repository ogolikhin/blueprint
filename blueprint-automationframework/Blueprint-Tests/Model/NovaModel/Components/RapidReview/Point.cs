namespace Model.NovaModel.Components.RapidReview
{
    // Taken from blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/Diagram.cs
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
