using UnityEngine;

namespace LineScripts
{
    public class LineFactory : MonoBehaviour
    {
        [SerializeField] private InputController _inputController = null;

        [SerializeField] private Line _referenceLine = null;
        
        private void Awake()
        {
            _inputController.OnObstacleCreated += OnObstacleCreated;
        }

        private void OnDestroy()
        {
            _inputController.OnObstacleCreated -= OnObstacleCreated;
            
        }

        private void OnObstacleCreated(Vector3 startPoint, Vector3 endPoint)
        {
            Line newObstacleLine = CreateLine(startPoint, endPoint);
        }

        private Line CreateLine(Vector3 startPoint, Vector3 endPoint)
        {
            Line line = Instantiate(_referenceLine.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Line>();
            
            line.ActivateLine(startPoint, endPoint);

            return line;
        }
    }
}