using System;
using System.Collections;
using System.Collections.Generic;
using Helpers;
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
    
    public EInputState EInputState { get; private set; } = EInputState.CreatingObstacles;
    
    private List<Vector3> _obstacle = new List<Vector3>();

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
    
    public Action<Vector3> OnCreatedObstacleStartingPoint { get; set; }
    public Action<Vector3> OnObstacleEndPointUpdated { get; set; }
    public Action<Vector3, Vector3> OnObstacleCreated { get; set; }
    
    private void Update()
    {
        CheckObstacleInput();

        CheckVisualizeInput();
    }

    private void CheckVisualizeInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _visualizer.Visualize();
    }

    private void CheckObstacleInput()
    {
        if (!Input.GetMouseButtonDown(0) ||
            EInputState != EInputState.CreatingObstacles)
            return;

        Vector3 mouseWorldPos = _MainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = Constants.Z_DEPTH;
        
        _obstacle.Add(mouseWorldPos);

        if (_obstacle.Count == 2)
        {
            OnObstacleCreated?.Invoke(_obstacle[0], _obstacle[1]);
            
            _obstacle.Clear();

            StopUpdateEndPointRoutine();
        }
        else if (_obstacle.Count == 1)
        {
            OnCreatedObstacleStartingPoint?.Invoke(_obstacle[0]);
            
            StartUpdateEndPointRoutine();
        }
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
