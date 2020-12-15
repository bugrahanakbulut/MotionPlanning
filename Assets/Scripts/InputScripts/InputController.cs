using System;
using System.Collections;
using System.Collections.Generic;
using Helpers;
using RoadPointScripts;
using UnityEngine;

public enum EInputState
{
    None = 0,
    CreatingObstacles = 1,
    CreatingStartFinishPosition = 2,
    CreatingOutput = 3
}

public class InputController : MonoBehaviour
{
    [SerializeField] private Visualizer _visualizer;

    [SerializeField] private TriangulationController _triangulationController = null;

    [SerializeField] private RoadPointController _roadPointController = null;
    public EInputState EInputState { get; private set; } = EInputState.CreatingObstacles;
    
    private List<Vector3> _obstaclePointBuffer = new List<Vector3>();

    private IEnumerator _updateEndPointRoutine;

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

    private int _roadPointCount;

    public Action<Vector3> OnCreatedObstacleStartingPoint { get; set; }
    public Action<Vector3> OnObstacleEndPointUpdated { get; set; }
    public Action<Vector3, Vector3> OnObstacleCreated { get; set; }
    
    private void Update()
    {
        CheckUpdateStateInput();
        
        CheckObstacleInput();

        CheckStartFinishInput();
    }

    private void CheckUpdateStateInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (EInputState == EInputState.CreatingObstacles)
                EInputState = EInputState.CreatingStartFinishPosition;
        }
    }

    private void CheckStartFinishInput()
    {
        if (!Input.GetMouseButtonDown(0) ||
            EInputState != EInputState.CreatingStartFinishPosition)
            return;
        
        if (!CheckPositionInScreen(Input.mousePosition))
            return;
        
        Vector3 mouseWorldPos = _MainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = Constants.Z_DEPTH;
        
        _roadPointController.CreateRoadPoint(mouseWorldPos, _roadPointCount == 0);

        _roadPointCount++;

        if (_roadPointCount == 2)
        {
            EInputState = EInputState.CreatingOutput;

            _triangulationController.Triangulate();
            
            _roadPointController.InitRoadGraph();
        }
    }

    private void CheckObstacleInput()
    {
        if (!Input.GetMouseButtonDown(0) ||
            EInputState != EInputState.CreatingObstacles)
            return;
        
        if (!CheckPositionInScreen(Input.mousePosition))
            return;

        Vector3 mouseWorldPos = _MainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = Constants.Z_DEPTH;
        
        _obstaclePointBuffer.Add(mouseWorldPos);

        if (_obstaclePointBuffer.Count == 2)
        {
            OnObstacleCreated?.Invoke(_obstaclePointBuffer[0], _obstaclePointBuffer[1]);
            
            _obstaclePointBuffer.Clear();

            StopUpdateEndPointRoutine();
        }
        else if (_obstaclePointBuffer.Count == 1)
        {
            OnCreatedObstacleStartingPoint?.Invoke(_obstaclePointBuffer[0]);
            
            StartUpdateEndPointRoutine();
        }
    }

    private bool CheckPositionInScreen(Vector2 position)
    {
        if (position.x > Screen.width || position.x < 0)
            return false;
        if (position.y > Screen.height || position.y < 0)
            return false;
        
        return true;
    }

    private void StartUpdateEndPointRoutine()
    {
        StopUpdateEndPointRoutine();

        _updateEndPointRoutine = UpdateEndPointRoutine();

        StartCoroutine(_updateEndPointRoutine);
    }

    private void StopUpdateEndPointRoutine()
    {
        if (_updateEndPointRoutine != null)
            StopCoroutine(_updateEndPointRoutine);
    }

    private IEnumerator UpdateEndPointRoutine()
    {
        while (true)
        {
            Vector3 mouseWorldPos = _MainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
        
            OnObstacleEndPointUpdated?.Invoke(mouseWorldPos);
            
            yield return null;
        }
    }
}
