using UnityEngine;

namespace LineScripts
{
    public class LineFactory : MonoBehaviour
    {
        [SerializeField] private InputController _inputController = null;

        [SerializeField] private Line _referenceLine = null;

        private Line _creatingLine = null;
        
        private void Awake()
        {
            _inputController.OnCreatedObstacleStartingPoint += OnCreatedObstacleStartingPoint;
            _inputController.OnObstacleEndPointUpdated += OnEndPointUpdated;
            _inputController.OnObstacleCreated += OnObstacleCreated;
        }

        private void OnDestroy()
        {
            _inputController.OnCreatedObstacleStartingPoint -= OnCreatedObstacleStartingPoint;
            _inputController.OnObstacleEndPointUpdated -= OnEndPointUpdated;
            _inputController.OnObstacleCreated -= OnObstacleCreated;
        }

        private void OnCreatedObstacleStartingPoint(Vector3 startingPoint)
        {
            _creatingLine = CreateLine(startingPoint, startingPoint);
        }
        
        private void OnEndPointUpdated(Vector3 endPoint)
        {
            _creatingLine.UpdateLine(endPoint);
        }
        
        private void OnObstacleCreated(Vector3 startingPoint, Vector3 endPoint)
        {
            _creatingLine.UpdateLine(startingPoint, endPoint);
        }

        private Line CreateLine(Vector3 startPoint, Vector3 endPoint)
        {
            Line line = Instantiate(_referenceLine.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Line>();
            
            line.ActivateLine(startPoint, endPoint);

            return line;
        }
    }
}