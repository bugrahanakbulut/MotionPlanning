using Delaunay.Geo;
using UnityEngine;

namespace Helpers
{
    public static class LineSegmentExtensions
    {
        public static bool IsIntersectingWith(this LineSegment lineSegment, LineSegment other)
        {
            if (lineSegment.p0 == other.p0 ||
                lineSegment.p0 == other.p1 ||
                lineSegment.p1 == other.p0 ||
                lineSegment.p1 == other.p1)
                return false;
            
            return CheckIntersection(
                (Vector2) lineSegment.p0, (Vector2) lineSegment.p1,
                (Vector2) other.p0, (Vector2) other.p1);
        }
        
        public static bool IsInLeft(this LineSegment lineSegment, Vector2 point)
        {
            Vector2 a = (Vector2)lineSegment.p0;
            Vector2 b = (Vector2)lineSegment.p1;

            float area = ((b[0] - a[0]) * (point[1] - a[1])) - ((b[1] - a[1]) * (point[0] - a[0]));

            return area > 0;
        }

        private static bool CheckIntersection(Vector2 lineOneA, Vector2 lineOneB, Vector2 lineTwoA, Vector2 lineTwoB)
        {
            return 
                 (lineTwoB.y - lineOneA.y) * (lineTwoA.x - lineOneA.x) > (lineTwoA.y - lineOneA.y) * (lineTwoB.x - lineOneA.x) != 
                 (lineTwoB.y - lineOneB.y) * (lineTwoA.x - lineOneB.x) > (lineTwoA.y - lineOneB.y) * (lineTwoB.x - lineOneB.x) && 
                 (lineTwoA.y - lineOneA.y) * (lineOneB.x - lineOneA.x) > (lineOneB.y - lineOneA.y) * (lineTwoA.x - lineOneA.x) != 
                 (lineTwoB.y - lineOneA.y) * (lineOneB.x - lineOneA.x) > (lineOneB.y - lineOneA.y) * (lineTwoB.x - lineOneA.x);
        }

        
    }
}