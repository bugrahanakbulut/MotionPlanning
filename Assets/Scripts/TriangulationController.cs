using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;
using Helpers;
using LineScripts;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class TriangulationController : MonoBehaviour
{
    [SerializeField] private ObstacleController _obstacleController = null;

    [SerializeField] private LineFactory _lineFactory = null;

    [SerializeField] private VertexVisualizer _referenceVisualizer = null;
    
    public List<Line> TriangulationLines = new List<Line>();

    public List<Vector2[]> Triangles = new List<Vector2[]>();

    private List<Vector2> _siteCoords = new List<Vector2>();

    private List<VertexVisualizer> _vertexVisualizers = new List<VertexVisualizer>();
    
    private Camera _mainCamera;

    private Camera _MainCamera
    {
        get
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            return _mainCamera;
        }
    }
    public void Triangulate()
    {
        AddScreenCornersIntoSiteCoords();

        foreach (Line obstacle in _obstacleController.Obstacles)
        {
            _siteCoords.Add((Vector3) obstacle.LineSegment.p0);
            _siteCoords.Add((Vector3) obstacle.LineSegment.p1);
        }

        Voronoi v = new Voronoi(_siteCoords, null, new Rect(0, 0, 0, 0));

        // complexity : n * log(n)
        List<LineSegment> delaunayTriangulation = v.DelaunayTriangulation();

        // complexity : n * v
        Dictionary<Line, List<LineSegment>> intersections =
            CheckTriangulationIntersectionWithObstacles(delaunayTriangulation, _obstacleController.Obstacles);

        // complexity : n * n * log(n)
        delaunayTriangulation = UpdateTriangulationByAddingObstacles(intersections, delaunayTriangulation);

        foreach (LineSegment lineSegment in delaunayTriangulation)
        {
            Line line = _lineFactory.CreateDashedLine(new LineCreationData((Vector2) lineSegment.p0,
                (Vector2) lineSegment.p1,
                Color.yellow, 1));

            TriangulationLines.Add(line);
        }

        int vertIndex = 0;
        
        foreach (Line line in TriangulationLines)
        {
            bool p0 = false, p1 = false;

            foreach (VertexVisualizer visualizer in _vertexVisualizers)
            {
                if (visualizer.transform.position == line.LineSegment.p0)
                    p0 = true;
                
                if (visualizer.transform.position == line.LineSegment.p1)
                    p1= true;
            }

            if (!p0)
            {
                GameObject g = Instantiate(_referenceVisualizer.gameObject, (Vector2)line.LineSegment.p0, Quaternion.identity);
                VertexVisualizer vv = g.GetComponent<VertexVisualizer>();
                vv.Init((Vector2) line.LineSegment.p0, vertIndex++);
                _vertexVisualizers.Add(vv);
            }

            if (!p1)
            {
                GameObject g = Instantiate(_referenceVisualizer.gameObject, (Vector2)line.LineSegment.p0, Quaternion.identity);
                VertexVisualizer vv = g.GetComponent<VertexVisualizer>();
                vv.Init((Vector2) line.LineSegment.p1, vertIndex++);
                _vertexVisualizers.Add(vv);
            }
        }
        
        // complexity : n * n * n
        InitTriangles();
    }

    private void AddScreenCornersIntoSiteCoords()
    {
        Vector3 leftBot = _MainCamera.ScreenToWorldPoint(Vector2.zero);
        leftBot.z = Constants.Z_DEPTH;

        Vector3 leftUp = _MainCamera.ScreenToWorldPoint(new Vector2(0, Screen.height));
        leftUp.z = Constants.Z_DEPTH;

        Vector3 rightBot = _MainCamera.ScreenToWorldPoint(new Vector2(Screen.width, 0));
        rightBot.z = Constants.Z_DEPTH;

        Vector3 rightUp = _MainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        rightUp.z = Constants.Z_DEPTH;

        _siteCoords.Add(leftBot);
        _siteCoords.Add(leftUp);
        _siteCoords.Add(rightBot);
        _siteCoords.Add(rightUp);
    }

    private Dictionary<Line, List<LineSegment>> CheckTriangulationIntersectionWithObstacles(
        List<LineSegment> delaunayTriangulation, List<Line> obstacles)
    {
        Dictionary<Line, List<LineSegment>> intersectDict = new Dictionary<Line, List<LineSegment>>();
        
        foreach (Line obstacle in obstacles)
        {
            intersectDict.Add(obstacle, new List<LineSegment>());

            foreach (LineSegment lineSegment in delaunayTriangulation)
            {
                if (obstacle.LineSegment.IsIntersectingWith(lineSegment))
                    intersectDict[obstacle].Add(lineSegment);
            }
        }

        return intersectDict;
    }

    private List<LineSegment> UpdateTriangulationByAddingObstacles(
        Dictionary<Line, List<LineSegment>> intersections,
        List<LineSegment> delaunay)
    {
        List<Line> triangulatedObstacles = new List<Line>();

        foreach (Line obstacle in intersections.Keys)
        {
            if (triangulatedObstacles.Contains(obstacle)) continue;
            
            List<Line> allIntersectedObstacles = new List<Line>() { obstacle };
            
            List<LineSegment> allIntersectedLineSegments = new List<LineSegment>(intersections[obstacle]);

            foreach (KeyValuePair<Line,List<LineSegment>> intersection in intersections)
            {
                if (intersection.Key == obstacle) continue;
                
                if (triangulatedObstacles.Contains(obstacle)) continue;

                IEnumerable<LineSegment> commonLineSegment = intersections[obstacle].Intersect(intersection.Value);

                if (commonLineSegment.Any())
                {
                    foreach (LineSegment lineSegment in intersection.Value)
                    {
                        if (!allIntersectedLineSegments.Contains(lineSegment))
                            allIntersectedLineSegments.Add(lineSegment);
                    }
                    
                    allIntersectedObstacles.Add(intersection.Key);
                
                    triangulatedObstacles.AddRange(allIntersectedObstacles);
                }
            }
            
            foreach (LineSegment segment in allIntersectedLineSegments)
                delaunay.RemoveAll(i 
                    => ( i.p0 == segment.p0 || i.p0 == segment.p1 ) &&
                       ( i.p1 == segment.p0 || i.p1 == segment.p1 ));

            List<LineSegment> candidates =
                ConstructTriangulationWithObstaclesAndSegments(allIntersectedObstacles, allIntersectedLineSegments);

            foreach (LineSegment candidate in candidates)
            {
                bool isIntersected = false;
                
                foreach (LineSegment lineSegment in delaunay)
                {
                    if (candidate.IsIntersectingWith(lineSegment))
                    {
                        isIntersected = true;
                        
                        break;
                    }
                }

                if (!isIntersected)
                {
                    foreach (Line obstacleLine in _obstacleController.Obstacles)
                    {
                        if (candidate.IsIntersectingWith(obstacleLine.LineSegment))
                        {
                            isIntersected = true;
                            
                            break;
                        }
                    }
                }
                
                if (!isIntersected)
                    delaunay.Add(candidate);
            }

            foreach (LineSegment lineSegment in allIntersectedLineSegments)
                delaunay.Remove(lineSegment);
        }

        return delaunay;
    }

    private List<LineSegment> ConstructTriangulationWithObstaclesAndSegments(List<Line> obstacles, List<LineSegment> lineSegments)
    {
        // 1. order obstacles by left to right
        // 2. categorize points by segments (p1 in between o1 - o2 etc.)
        // 3. triangulate each segment by itself

        obstacles.Sort();

        List<Vector2> allVertices =  new List<Vector2>();

        foreach (LineSegment lineSegment in lineSegments)
        {
            if (!allVertices.Contains((Vector2) lineSegment.p0))
                allVertices.Add((Vector2) lineSegment.p0);
            
            if (!allVertices.Contains((Vector2) lineSegment.p1))
                allVertices.Add((Vector2) lineSegment.p1);
        }
        
        // triangulate leftmost and rightmost segments first

        List<Vector2> leftMostSegment = new List<Vector2>()
        {
            (Vector2) obstacles[0].LineSegment.p0,
            (Vector2) obstacles[0].LineSegment.p1
        };


        List<Vector2> rightMostSegment = new List<Vector2>()
        {
            (Vector2) obstacles[obstacles.Count - 1].LineSegment.p0,
            (Vector2) obstacles[obstacles.Count - 1].LineSegment.p1
        };
        
        foreach (Vector2 vertex in allVertices)
        {
            if (obstacles[0].IsInLeft(vertex) && !leftMostSegment.Contains(vertex))
                leftMostSegment.Add(vertex);
            
            if (!obstacles[obstacles.Count - 1].IsInLeft(vertex) && !rightMostSegment.Contains(vertex))
                rightMostSegment.Add(vertex);
        }
        
        List<LineSegment> newTriangulationLineSegments = new List<LineSegment>();
        
        foreach (LineSegment segment in TriangulateVertices(leftMostSegment))
        {
            if (!newTriangulationLineSegments.Contains(segment))
                newTriangulationLineSegments.Add(segment);
        }

        foreach (LineSegment segment in TriangulateVertices(rightMostSegment))
        {
            if (!newTriangulationLineSegments.Contains(segment))
                newTriangulationLineSegments.Add(segment);
        }
        

        for (int i = 0; i < obstacles.Count - 1; i++)
        {
            List<Vector2> curSegmentVertices = new List<Vector2>();
            
            curSegmentVertices.Add((Vector2) obstacles[i].LineSegment.p0);
            curSegmentVertices.Add((Vector2) obstacles[i].LineSegment.p1);
            curSegmentVertices.Add((Vector2) obstacles[i + 1].LineSegment.p0);
            curSegmentVertices.Add((Vector2) obstacles[i + 1].LineSegment.p1);
            
            foreach (Vector2 vertex in allVertices)
            {
                if (!obstacles[i].IsInLeft(vertex) && obstacles[i + 1].IsInLeft(vertex))
                {
                    if (!curSegmentVertices.Contains(vertex))
                        curSegmentVertices.Add(vertex);
                }
            }
            
            foreach (LineSegment segment in TriangulateVertices(curSegmentVertices))
            {
                if (!newTriangulationLineSegments.Contains(segment))
                    newTriangulationLineSegments.Add(segment);
            }
        }

        return newTriangulationLineSegments;
    }

    private List<LineSegment> TriangulateVertices(List<Vector2> vertices)
    {
        Voronoi v = new Voronoi(vertices, null, Rect.zero);

        return v.DelaunayTriangulation();
    }

    private void InitTriangles()
    {
        List<Line> allLines = new List<Line>(TriangulationLines);
        allLines.AddRange(_obstacleController.Obstacles);
        
        // complexity : n
        Dictionary<Vector2, List<Vector2>> adjacencyGraph = GetAdjacencyGraph(allLines);

        // complexity : n * n * n
        List<Vector2[]> triangles = FindAllTrianglesInAdjacencyGraph(adjacencyGraph);

        // complexity : n * n
        Triangles = EliminateInvalidTriangles(triangles, allLines);
    }

    private Dictionary<Vector2, List<Vector2>> GetAdjacencyGraph(List<Line> lines)
    {
        Dictionary<Vector2, List<Vector2>> adjacencyGraph = new Dictionary<Vector2, List<Vector2>>();

        foreach (Line line in lines)
        {
            Vector2 s = (Vector2) line.LineSegment.p0;
            Vector2 f = (Vector2) line.LineSegment.p1;

            if (adjacencyGraph.ContainsKey(s))
                adjacencyGraph[s].Add(f);
            else
                adjacencyGraph.Add(s, new List<Vector2>() { f });

            if (adjacencyGraph.ContainsKey(f))
                adjacencyGraph[f].Add(s);
            else
                adjacencyGraph.Add(f, new List<Vector2>() { s });
        }

        return adjacencyGraph;
    }

    private List<Vector2[]> FindAllTrianglesInAdjacencyGraph(Dictionary<Vector2, List<Vector2>> adjacencyGraph)
    {
        List<Vector2[]> triangles = new List<Vector2[]>();

        foreach (KeyValuePair<Vector2,List<Vector2>> kvp in adjacencyGraph)
        {
            foreach (Vector2 adjNode in kvp.Value)
            {
                if (!adjacencyGraph.ContainsKey(adjNode)) continue;

                IEnumerable<Vector2> commons = kvp.Value.Intersect(adjacencyGraph[adjNode]);

                foreach (Vector2 commonVertex in commons)
                {
                    Vector2[] tri = new []{ kvp.Key, adjNode, commonVertex };
                    
                    triangles.Add(tri);
                }
            }
        }

        List<Vector2[]> uniqueTriangles = new List<Vector2[]>(triangles);

        for (int i = 0; i < triangles.Count; i++)
        {
            for (int j = i + 1; j < uniqueTriangles.Count; j++)
            {
                if (triangles[i].Intersect(uniqueTriangles[j]).Count() == 3)
                    uniqueTriangles.RemoveAt(j);
            }
        }
        
        return uniqueTriangles;
    }

    private List<Vector2[]> EliminateInvalidTriangles(List<Vector2[]> triangles, List<Line> allLines)
    {
        List<Vector2> allVertices = new List<Vector2>();
        foreach (Line line in allLines)
        {
            if (!allVertices.Contains((Vector2)line.LineSegment.p0))
                allVertices.Add((Vector2)line.LineSegment.p0);
            
            if (!allVertices.Contains((Vector2)line.LineSegment.p1))
                allVertices.Add((Vector2)line.LineSegment.p1);
        }

        List<Vector2[]> invalidTriangles = new List<Vector2[]>();

        foreach (Vector2[] triangle in triangles)
        {
            foreach (Vector2 vertex in allVertices)
            {
                if (IsPointInTriangle(triangle, vertex))
                {
                    if (!invalidTriangles.Contains(triangle))
                        invalidTriangles.Add(triangle);
                    
                    break;
                }
            }
        }

        foreach (Vector2[] invalidTriangle in invalidTriangles)
            triangles.Remove(invalidTriangle);

        return triangles;
    }

    private bool IsPointInTriangle(Vector2[] triangle, Vector2 point)
    {
        if (point == triangle[0] ||
            point == triangle[1] ||
            point == triangle[2])
            return false;
        
        float d1, d2, d3;

        bool neg, pos;

        d1 = Sign(point, triangle[0], triangle[1]);
        d2 = Sign(point, triangle[1], triangle[2]);
        d3 = Sign(point, triangle[2], triangle[0]);
        
        neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        bool res = !(neg && pos);

        return res;
    }

    private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}
