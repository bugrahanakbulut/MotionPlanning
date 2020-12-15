using System.Net.NetworkInformation;
using LineScripts;
using Unity.Mathematics;
using UnityEngine;

namespace RoadPointScripts
{
    public class RoadPointController : MonoBehaviour
    {
        [SerializeField] private TriangulationController _triangulationController;

        [SerializeField] private ObstacleController _obstacleController;

        [SerializeField] private RoadPoint _roadPointReference;

        [SerializeField] private LineFactory _lineFactory;
        
        public void CreateRoadPoint(Vector3 pos, bool isStartPoint)
        {
            RoadPoint rp = Instantiate(_roadPointReference.gameObject, pos, quaternion.identity).GetComponent<RoadPoint>();

            rp.InitRoadPoint(isStartPoint);
        }

        public void InitRoadGraph()
        {
            foreach (Vector2[] triangle in _triangulationController.Triangles)
            {
                bool hasContainsObstacleLine = CheckTriangleHasObstacleEdge(triangle, out Line intersectedObstacle);

                if (!hasContainsObstacleLine)
                {
                    Vector2 a = (triangle[0] + triangle[1]) / 2;
                    Vector2 b = (triangle[1] + triangle[2]) / 2;
                    Vector2 c = (triangle[0] + triangle[2]) / 2;

                    _lineFactory.CreateDashedLine(new LineCreationData(a, b, Color.red));
                    _lineFactory.CreateDashedLine(new LineCreationData(a, c, Color.red));
                    _lineFactory.CreateDashedLine(new LineCreationData(b, c, Color.red));
                }

                else
                {
                    Vector2 nonObsVertex = Vector2.zero;

                    foreach (Vector2 vertex in triangle)
                        if (vertex != intersectedObstacle.LineSegment.p0 &&
                            vertex != intersectedObstacle.LineSegment.p1)

                            nonObsVertex = vertex;
                    
                    Vector2 a = (nonObsVertex + (Vector2)intersectedObstacle.LineSegment.p0) / 2;
                    Vector2 b = (nonObsVertex + (Vector2)intersectedObstacle.LineSegment.p1) / 2;
                    
                    _lineFactory.CreateDashedLine(new LineCreationData(a, b, Color.red));
                }
            }
        }
        
        private bool CheckTriangleHasObstacleEdge(Vector2[] triangle, out Line intersectedObstacle)
        {
            foreach (Line obstacle in _obstacleController.Obstacles)
            {
                Vector2 v1 = (Vector2) obstacle.LineSegment.p0;
                Vector2 v2 = (Vector2) obstacle.LineSegment.p1;

                bool hasV1 = v1 == triangle[0] || v1 == triangle[1] || v1 == triangle[2];
                bool hasV2 = v2 == triangle[0] || v2 == triangle[1] || v2 == triangle[2];

                intersectedObstacle = obstacle;
                
                if (hasV1 && hasV2)
                    return true; 
            }

            intersectedObstacle = null;

            return false;
        }
    }
}