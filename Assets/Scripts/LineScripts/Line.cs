using System;
using Delaunay.Geo;
using UnityEngine;

namespace LineScripts
{
    public class Line : MonoBehaviour, IComparable<Line>
    {
        [SerializeField] private LineRenderer _lineRenderer = null;

        private LineSegment _lineSegment;
        
        public LineSegment LineSegment
        {
            get
            {
                if (_lineSegment == null)
                    _lineSegment = new LineSegment(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1));

                return _lineSegment;
            }
        }

        public bool IsObstacle { get; set; }

        public void ActivateLine(LineCreationData lineCreationData)
        {
            _lineSegment = new LineSegment(lineCreationData.StartingPoint, lineCreationData.EndPoint);
            
            _lineRenderer.SetPosition(0, lineCreationData.StartingPoint);
            _lineRenderer.SetPosition(1, lineCreationData.EndPoint);

            _lineRenderer.startColor = lineCreationData.LineColor;
            _lineRenderer.endColor = lineCreationData.LineColor;
            _lineRenderer.sortingOrder = lineCreationData.OrderInLayer;

        }

        public void UpdateLine(LineCreationData lineCreationData)
        {
            _lineSegment = new LineSegment(lineCreationData.StartingPoint, lineCreationData.EndPoint);
            
            _lineRenderer.SetPosition(0, lineCreationData.StartingPoint);
            _lineRenderer.SetPosition(1, lineCreationData.EndPoint);
        }

        public bool IsInLeft(Line other)
        {
            bool startingAtLeft = IsInLeft((Vector2) other.LineSegment.p0);
            bool finishAtLeft = IsInLeft((Vector2) other.LineSegment.p1);
            
            return startingAtLeft && finishAtLeft;
        }

        public bool IsInLeft(Vector2 c)
        {
            Vector2 a = (Vector2) LineSegment.p0;
            Vector2 b = (Vector2) LineSegment.p1;

            float area = ((b[0] - a[0]) * (c[1] - a[1])) - ((b[1] - a[1]) * (c[0] - a[0]));
            
            area /= 2;
            
            return area > 0;
        }

        public override bool Equals(object other)
        {
            Line otherLine = (Line) other;

            if ((otherLine.LineSegment.p0 == LineSegment.p0 || otherLine.LineSegment.p0 == LineSegment.p1) &&
                (otherLine.LineSegment.p1 == LineSegment.p0 || otherLine.LineSegment.p1 == LineSegment.p1))
                return true;

            return false;
        }

        public int CompareTo(Line other)
        {
            if (this == other) return 0;

            bool isInLeft = IsInLeft(other);

            if (isInLeft)
                return 1;

            return -1;
        }
    }
}