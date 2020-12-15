using Delaunay.Geo;
using UnityEngine;

namespace LineScripts
{
    public class Line : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer = null;

        public LineSegment LineSegment { get; private set; }
        
        public bool IsObstacle { get; set; }

        public void ActivateLine(LineCreationData lineCreationData)
        {
            LineSegment = new LineSegment(lineCreationData.StartingPoint, lineCreationData.EndPoint);
            
            _lineRenderer.SetPosition(0, lineCreationData.StartingPoint);
            _lineRenderer.SetPosition(1, lineCreationData.EndPoint);

            _lineRenderer.startColor = lineCreationData.LineColor;
            _lineRenderer.endColor = lineCreationData.LineColor;
            _lineRenderer.sortingOrder = lineCreationData.OrderInLayer;

        }

        public void UpdateLine(LineCreationData lineCreationData)
        {
            LineSegment = new LineSegment(lineCreationData.StartingPoint, lineCreationData.EndPoint);
            
            _lineRenderer.SetPosition(0, lineCreationData.StartingPoint);
            _lineRenderer.SetPosition(1, lineCreationData.EndPoint);
        }
    }
}