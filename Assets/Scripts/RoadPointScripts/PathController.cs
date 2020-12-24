using System.Collections.Generic;
using Delaunay.Geo;
using LineScripts;
using Unity.Mathematics;
using UnityEngine;

namespace RoadPointScripts
{
    public class PathController : MonoBehaviour
    {
        [SerializeField] private TriangulationController _triangulationController;

        [SerializeField] private ObstacleController _obstacleController;

        [SerializeField] private RoadPoint _roadPointReference;

        [SerializeField] private LineFactory _lineFactory;
        
        private Dictionary<Vector2, List<Vector2>> _adjacencyGraph;

        private List<Line> _pathLines = new List<Line>();
        
        private Vector2 _startingPos, _targetPosition;
        
        public void CreateRoadPoint(Vector3 pos, bool isStartPoint)
        {
            RoadPoint rp = Instantiate(_roadPointReference.gameObject, pos, quaternion.identity).GetComponent<RoadPoint>();

            if (isStartPoint)
                _startingPos = pos;
            else
                _targetPosition = pos;
            
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
                    
                    _pathLines.Add(_lineFactory.CreateDashedLine(new LineCreationData(a, b, Color.blue, 3)));
                    _pathLines.Add(_lineFactory.CreateDashedLine(new LineCreationData(a, c, Color.blue, 3)));
                    _pathLines.Add(_lineFactory.CreateDashedLine(new LineCreationData(b, c, Color.blue, 3)));
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
                    
                    _pathLines.Add(_lineFactory.CreateDashedLine(new LineCreationData(a, b, Color.blue, 3)));
                }
            }
            
            List<Line> distinctLines = new List<Line>();

            foreach (Line line in _pathLines)
            {
                bool isDuplicated = false;
                
                distinctLines.ForEach(i => isDuplicated = isDuplicated || line.Equals(i));
                
                if (!isDuplicated)
                    distinctLines.Add(line);
            }

            foreach (Line line in _pathLines)
            {
                bool isDistinct = false;
                
                foreach (Line distinctLine in distinctLines)
                {
                    if (distinctLine == line)
                        isDistinct = true;
                }
                
                if (!isDistinct) 
                    DestroyImmediate(line.gameObject);
            }
            
            _pathLines = distinctLines;
        }

        public void FindWayThrough()
        {
            Vector2 startingNode = FindTheClosestGraphNode(_startingPos);
            Vector2 targetNode = FindTheClosestGraphNode(_targetPosition);

            List<Vector2> nodes = new List<Vector2>();
            List<List<int>> values = new List<List<int>>();

            foreach (KeyValuePair<Vector2,List<Vector2>> keyValuePair in _adjacencyGraph)
                nodes.Add(keyValuePair.Key);

            foreach (KeyValuePair<Vector2, List<Vector2>> keyValuePair in _adjacencyGraph)
            {
                List<int> adj = new List<int>();
                
                foreach (Vector2 vector2 in keyValuePair.Value)
                    adj.Add(nodes.IndexOf(vector2));
                
                values.Add(adj);
            }

            int startingNodeIndex = nodes.IndexOf(startingNode);
            int targetNodeIndex = nodes.IndexOf(targetNode);

            int nVertices = nodes.Count;  
            float[] shortestDistances = new float[nVertices];
            bool[] added = new bool[nVertices];  
            
            for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)  
            {  
                shortestDistances[vertexIndex] = float.MaxValue;  
                added[vertexIndex] = false;  
            }
            
            shortestDistances[startingNodeIndex] = 0;  
            int[] parents = new int[nVertices];  
            
            parents[startingNodeIndex] = -1;
            
            for (int i = 1; i < nVertices; i++)
            {
                int nearestVertex = -1;  
                float shortestDistance = float.MaxValue;  
                
                for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)  
                {  
                    if (!added[vertexIndex] && shortestDistances[vertexIndex] < shortestDistance)  
                    {  
                        nearestVertex = vertexIndex;  
                        shortestDistance = shortestDistances[vertexIndex];  
                    }
                }
                
                added[nearestVertex] = true;

                for (int vertexIndex = 0; vertexIndex < nVertices; vertexIndex++)
                {
                    float edgeDistance = Vector2.Distance(nodes[nearestVertex], nodes[vertexIndex]);
                    
                    if (values[vertexIndex].Contains(nearestVertex) && shortestDistance + edgeDistance < shortestDistances[vertexIndex])
                    {
                        parents[vertexIndex] = nearestVertex;  
                        shortestDistances[vertexIndex] = shortestDistance + edgeDistance;  
                    }
                }
            }
            
            // PrintSolution(startingNodeIndex, shortestDistances, parents);

            List<int> vertexList = GetPath(targetNodeIndex, parents);

            vertexList.Insert(0, startingNodeIndex);

            int tmp = vertexList[0];

            for (int i = 1; i < vertexList.Count; i++)
            {
                _lineFactory.CreateLine(new LineCreationData(nodes[tmp], nodes[vertexList[i]], Color.green, 5));

                tmp = vertexList[i];
            }
        }
        
        private void PrintSolution(int startVertex, float[] distances, int[] parents)  
        {  
            int nVertices = distances.Length;  
            Debug.Log("Vertex\t Distance\tPath");  
          
            for (int vertexIndex = 0;  
                vertexIndex < nVertices;  
                vertexIndex++)  
            {  
                if (vertexIndex != startVertex)
                {
                    string msg = "";
                    
                    msg += startVertex + " -> ";  
                    msg += (vertexIndex + " \t\t ");  
                    msg += (distances[vertexIndex] + "\t\t");  
                    msg += (vertexIndex.ToString() + " -- ");

                    msg += printPath(vertexIndex, parents);

                    Debug.Log(msg);
                }  
            }  
        }  
        
        private string printPath(int currentVertex, int[] parents)  
        {  
            if (parents[currentVertex] == -1) return "";

            return printPath(parents[currentVertex], parents) + currentVertex + " ";
        }  
        
        private List<int> GetPath(int targetVertex, int[] parents)
        {
            if (parents[targetVertex] == -1) return new List<int>();

            List<int> recVal = GetPath(parents[targetVertex], parents);
            
            recVal.Add(targetVertex);
            
            return recVal;
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