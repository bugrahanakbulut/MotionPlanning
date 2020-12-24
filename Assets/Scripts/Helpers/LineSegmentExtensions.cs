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

        private static bool CheckIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
            float numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            float numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));
            
            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }
    }
}