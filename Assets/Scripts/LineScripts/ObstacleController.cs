﻿using System.Collections.Generic;
using UnityEngine;

namespace LineScripts
{
    public class ObstacleController : MonoBehaviour
    {
        [SerializeField] private InputController _inputController = null;

        [SerializeField] private LineFactory _lineFactory = null;

        public List<Line> Obstacles { get; private set; } = new List<Line>();

        private Line _curLine = null;

        private Vector2 _startingPoint, _endPoint;

        private void Awake()
        {
            _inputController.OnCreatedObstacleStartingPoint += OnCreatedObstacleStartingPoint;
            _inputController.OnObstacleEndPointUpdated += OnObstacleEndPointUpdated;
            _inputController.OnObstacleCreated += OnObstacleCreated;
        }

        private void OnDestroy()
        {
            _inputController.OnCreatedObstacleStartingPoint -= OnCreatedObstacleStartingPoint;
            _inputController.OnObstacleEndPointUpdated -= OnObstacleEndPointUpdated;
            _inputController.OnObstacleCreated -= OnObstacleCreated;
        }

        private void OnCreatedObstacleStartingPoint(Vector3 startingPoint)
        {
            _startingPoint = startingPoint;
            
            _curLine = _lineFactory.CreateLine(new LineCreationData(_startingPoint, _startingPoint, 10));

            _curLine.IsObstacle = true;
        }

        private void OnObstacleEndPointUpdated(Vector3 endPoint)
        {
            _endPoint = endPoint;
            
            _curLine.UpdateLine(new LineCreationData(_startingPoint, _endPoint, 0));
        }

        private void OnObstacleCreated(Vector3 startingPoint, Vector3 endPoint)
        {
            _curLine.UpdateLine(new LineCreationData(startingPoint, endPoint, 0));
            
            Obstacles.Add(_curLine);

            _curLine = null;
        }
    }
}