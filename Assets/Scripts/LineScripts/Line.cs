using UnityEngine;

namespace LineScripts
{
    public class Line : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer = null;
        
        public void ActivateLine(Vector3 startingPoint, Vector3 endPoint)
        {
            UpdateLine(startingPoint, endPoint);
        }

        public void UpdateLine(Vector3 startingPoint, Vector3 endPoint)
        {
            _lineRenderer.SetPosition(0, startingPoint);
            _lineRenderer.SetPosition(1, endPoint);
        }
        
        public void UpdateLine(Vector3 endPoint)
        {
            _lineRenderer.SetPosition(1, endPoint);
        }
    }
}