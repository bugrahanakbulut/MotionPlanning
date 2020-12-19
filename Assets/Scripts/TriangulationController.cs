using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Delaunay;
using Delaunay.Geo;
using Helpers;
using JetBrains.Annotations;
using LineScripts;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class TriangulationController : MonoBehaviour
{
    [SerializeField] private ObstacleController _obstacleController = null;

    [SerializeField] private LineFactory _lineFactory = null;

    public List<Line> TriangulationLines = new List<Line>();

    public List<Vector2[]> Triangles = new List<Vector2[]>();

    private List<Vector2> _siteCoords = new List<Vector2>();

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

        List<LineSegment> delaunayTriangulation = v.DelaunayTriangulation();

        Dictionary<Line, List<LineSegment>> intersections =
            CheckTriangulationIntersectionWithObstacles(delaunayTriangulation, _obstacleController.Obstacles);

        delaunayTriangulation = RemoveInterSectionsAndUpdateTriangulation(intersections, delaunayTriangulation);

        foreach (Line obstacle in _obstacleController.Obstacles)
            delaunayTriangulation.Remove(obstacle.LineSegment);
        
        foreach (LineSegment lineSegment in delaunayTriangulation)
        {
            Line line = _lineFactory.CreateDashedLine(new LineCreationData((Vector2) lineSegment.p0,
                (Vector2) lineSegment.p1,
                Color.cyan));

            TriangulationLines.Add(line);
        }
        
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

    private List<LineSegment> RemoveInterSectionsAndUpdateTriangulation(
        Dictionary<Line, List<LineSegment>> intersections, List<LineSegment> delaunay)
    {
        foreach (KeyValuePair<Line, List<LineSegment>> keyValuePair in intersections)
        {
            List<Vector2> leftSites = new List<Vector2>()
            {
                (Vector2) keyValuePair.Key.LineSegment.p0,
                (Vector2) keyValuePair.Key.LineSegment.p1
            };

            List<Vector2> rightSites = new List<Vector2>()
            {
                (Vector2) keyValuePair.Key.LineSegment.p0,
                (Vector2) keyValuePair.Key.LineSegment.p1
            };

            foreach (LineSegment lineSegment in keyValuePair.Value)
            {
                if (delaunay.Contains(lineSegment))
                    delaunay.Remove(lineSegment);

                if (keyValuePair.Key.LineSegment.IsInLeft((Vector2) lineSegment.p0))
                    leftSites.Add((Vector2) lineSegment.p0);
                else
                    rightSites.Add((Vector2) lineSegment.p0);

                if (keyValuePair.Key.LineSegment.IsInLeft((Vector2) lineSegment.p1))
                    leftSites.Add((Vector2) lineSegment.p1);
                else
                    rightSites.Add((Vector2) lineSegment.p1);
            }

            List<LineSegment> leftTriangulation = TriangulateFromSiteList(leftSites);
            List<LineSegment> rightTriangulation = TriangulateFromSiteList(rightSites);

            leftTriangulation.AddRange(rightTriangulation);

            foreach (LineSegment segment in leftTriangulation)
            {
                if (!delaunay.Contains(segment))
                    delaunay.Add(segment);
            }

        }

        return delaunay;
    }

    private List<LineSegment> TriangulateFromSiteList(List<Vector2> sites)
    {
        Vector3 leftBot = _MainCamera.ScreenToWorldPoint(Vector2.zero);
        leftBot.z = Constants.Z_DEPTH;

        Vector3 rightUp = _MainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        rightUp.z = Constants.Z_DEPTH;

        Voronoi v = new Voronoi(sites, null,
            new Rect(leftBot.x, leftBot.y, rightUp.x, rightUp.y));

        return v.DelaunayTriangulation();
    }

    private void InitTriangles()
    {
        List<Line> allLines = new List<Line>(TriangulationLines);
        allLines.AddRange(_obstacleController.Obstacles);

        Dictionary<Vector2, List<Vector2>> adjacencyGraph = GetAdjacencyGraph(allLines);

        List<Vector2[]> triangles = FindAllTrianglesInAdjacencyGraph(adjacencyGraph);

        Triangles = EliminateInvalidTriangles(triangles, allLines);
        
        Debug.Log(Triangles.Count);
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
                    bool canAdd = true;

                    Vector2[] tri = new []{ kvp.Key, adjNode, commonVertex };
                    
                    foreach (Vector2[] triangle in triangles)
                    {
                        IEnumerable<Vector2> commPoints = triangle.Intersect(tri);

                        canAdd = canAdd && (commPoints.Count() != 3);
                    }

                    if (canAdd)
                        triangles.Add(tri);
                }
            }
        }

        return triangles;
    }

    [ItemCanBeNull]
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
