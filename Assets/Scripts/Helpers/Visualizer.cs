using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using LineScripts;
using UnityEngine;

namespace Helpers
{
    public class Visualizer : MonoBehaviour
    {
        [SerializeField] private ObstacleController _obstacleController = null;

        [SerializeField] private LineFactory _lineFactory = null;
        
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
        
        public void Visualize()
        {
            
            AddScreenCornersIntoSiteCoords();
            
            foreach (Line obstacle in _obstacleController.Obstacles)
            {
                _siteCoords.Add((Vector3)obstacle.LineSegment.p0);
                _siteCoords.Add((Vector3)obstacle.LineSegment.p1);
            }
            
            Delaunay.Voronoi v = new Delaunay.Voronoi (_siteCoords, null, 
                new Rect (_siteCoords[0].x, _siteCoords[0].y, _siteCoords[1].x, _siteCoords[1].y));
            
            List<LineSegment> m_edges = v.VoronoiDiagram ();
			
            List<LineSegment> m_spanningTree = v.SpanningTree (KruskalType.MINIMUM);
            List<LineSegment> m_delaunayTriangulation = v.DelaunayTriangulation ();

            foreach (LineSegment segment in m_delaunayTriangulation)
                _lineFactory.CreateDashedLine(new LineCreationData((Vector2)segment.p0, (Vector2) segment.p1, Color.cyan));
            
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
    }
}