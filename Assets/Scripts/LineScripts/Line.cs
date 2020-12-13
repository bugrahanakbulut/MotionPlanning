using UnityEngine;

namespace LineScripts
{
    public class Line : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer = null;

        public Vector3 StartingPoint { get; private set; }
        public Vector3 EndPoint { get; private set; }
        
        public void ActivateLine(Vector3 startingPoint, Vector3 endPoint)
        {
            StartingPoint = startingPoint;
            EndPoint = endPoint;
            
            UpdateLine(startingPoint, endPoint);
        }

        public void UpdateLine(Vector3 startingPoint, Vector3 endPoint)
        {
            StartingPoint = startingPoint;
            EndPoint = endPoint;
            
            _lineRenderer.SetPosition(0, startingPoint);
            _lineRenderer.SetPosition(1, endPoint);
        }
        
        public void UpdateLine(Vector3 endPoint)
        {
            EndPoint = endPoint;
            
            _lineRenderer.SetPosition(1, endPoint);
        }
    }
}