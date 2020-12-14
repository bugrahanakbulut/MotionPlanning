using Helpers;
using UnityEngine;

namespace LineScripts
{
    public class LineCreationData
    {
        public Vector3 StartingPoint { get; }
        public Vector3 EndPoint { get; }
        public Color LineColor { get; }
        public int OrderInLayer { get; }

        public LineCreationData(Vector3 startingPoint, Vector3 endPoint)
        {
            StartingPoint = startingPoint;
            EndPoint = endPoint;
            LineColor = Constants.DEFAULT_COLOR;
        }

        public LineCreationData(Vector3 startingPoint, Vector3 endPoint, Color lineColor)
        {
            StartingPoint = startingPoint;
            EndPoint = endPoint;
            LineColor = lineColor;
        }

        public LineCreationData(Vector3 startingPoint, Vector3 endPoint, int orderInLayer)
        {
            StartingPoint = startingPoint;
            EndPoint = endPoint;
            LineColor = Constants.DEFAULT_COLOR;
            OrderInLayer = orderInLayer;
        }
    }
    
    public class LineFactory : MonoBehaviour
    {
        [SerializeField] private Line _referenceLine = null;
        [SerializeField] private Line _dashedLine = null;
        
        public Line CreateLine(LineCreationData creationData)
        {
            Line line = Instantiate(_referenceLine.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Line>();

            line.ActivateLine(creationData);
            
            return line;
        }
        
        public Line CreateDashedLine(LineCreationData creationData)
        {
            Line line = Instantiate(_dashedLine.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Line>();

            line.ActivateLine(creationData);
            
            return line;
        }
    }
}