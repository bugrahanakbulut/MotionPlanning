using System.Collections.Generic;
using LineScripts;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoadPointScripts
{
    public class RoadPointController : MonoBehaviour
    {
        [SerializeField] private TriangulationController _triangulationController;

        [SerializeField] private ObstacleController _obstacleController;

        [SerializeField] private RoadPoint _roadPointReference;

        [SerializeField] private LineFactory _lineFactory;

        private Dictionary<Vector2, List<Vector2>> _adjacencyGraph;
        
        public void CreateRoadPoint(Vector3 pos, bool isStartPoint)
        {
            RoadPoint rp = Instantiate(_roadPointReference.gameObject, pos, quaternion.identity).GetComponent<RoadPoint>();

            rp.InitRoadPoint(isStartPoint);
        }

        public void InitRoadGraph()
        {
            _adjacencyGraph = new Dictionary<Vector2, List<Vector2>>();
            
            foreach (Vector2[] triangle in _triangulationController.Triangles)
            {
                bool hasContainsObstacleLine = CheckTriangleHasObstacleEdge(triangle, out Line intersectedObstacle);

                if (!hasContainsObstacleLine)
                {
                    Vector2 a = (triangle[0] + triangle[1]) / 2;
                    Vector2 b = (triangle[1] + triangle[2]) / 2;
                    Vector2 c = (triangle[0] + triangle[2]) / 2;
                    
                    UpdateAdjacencyGraph(a, new List<Vector2>(){ b, c });
                    UpdateAdjacencyGraph(b, new List<Vector2>(){ a, c });
                    UpdateAdjacencyGraph(c, new List<Vector2>(){ a, b });

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
                    
                    UpdateAdjacencyGraph(a, b);
                    UpdateAdjacencyGraph(b, a);
                    
                    _lineFactory.CreateDashedLine(new LineCreationData(a, b, Color.red));
                }
            }
        }

        public void FindWayThrough(Vector2 a, Vector2 b)
        {
            Vector2 startingNode = FindTheClosestGraphNode(a);
            Vector2 targetNode = FindTheClosestGraphNode(b);

            List<Vector2> nodes = new List<Vector2>();
            List<List<Vector2>> values = new List<List<Vector2>>();

            foreach (KeyValuePair<Vector2,List<Vector2>> keyValuePair in _adjacencyGraph)
            {
                nodes.Add(keyValuePair.Key);
                values.Add(keyValuePair.Value);
            }

            float[] distances = new float[nodes.Count];
            bool[] visitedFlags = new bool[nodes.Count];
            
            for (int i = 0; i < _adjacencyGraph.Count; i++)
            {
                distances[i] = float.MaxValue;
                visitedFlags[i] = false;
            }
            
            distances[nodes.IndexOf(startingNode)] = 0;

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                int u = MinDistance(distances, visitedFlags);

                visitedFlags[u] = true;

                for (int v = 0; v < nodes.Count; v++)
                {
                    if (!visitedFlags[v] && distances[u] != float.MaxValue && distances[u] < distances[v]) 
                        distances[v] = distances[u] + 1; 
                }
            }
        }
        
        private int MinDistance(float[] dist, bool[] visitedArr)
        {
            float min = float.MaxValue;
            int min_index = -1; 
  
            for (int v = 0; v < dist.Length; v++) 
                if (visitedArr[v] == false && dist[v] <= min) { 
                    min = dist[v]; 
                    min_index = v; 
                } 
  
            return min_index; 
        } 

        private Vector2 FindTheClosestGraphNode(Vector2 point)
        {
            float dist = float.MaxValue;

            Vector2 closestPoint = Vector2.zero;
            
            foreach (Vector2 node in _adjacencyGraph.Keys)
            {
                float tempDist = Vector2.Distance(node, point);

                if (tempDist < dist)
                {
                    dist = tempDist;
                    closestPoint = node;
                }
            }

            return closestPoint;
        }

        private void UpdateAdjacencyGraph(Vector2 key, Vector2 adjacentNode)
        {
            if (_adjacencyGraph.ContainsKey(key))
                _adjacencyGraph[key].Add(adjacentNode);
            else
                _adjacencyGraph.Add(key, new List<Vector2>() { adjacentNode });
        }

        private void UpdateAdjacencyGraph(Vector2 key, List<Vector2> adjacentNodes)
        {
            if (_adjacencyGraph.ContainsKey(key))
            {
                foreach (Vector2 adjacentNode in adjacentNodes)
                {
                    if (!_adjacencyGraph[key].Contains(adjacentNode))
                        _adjacencyGraph[key].Add(adjacentNode);
                }
            }
            else
            {
                _adjacencyGraph[key] = new List<Vector2>();
                
                foreach (Vector2 adjacentNode in adjacentNodes)
                {
                    if (!_adjacencyGraph[key].Contains(adjacentNode))
                        _adjacencyGraph[key].Add(adjacentNode);
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