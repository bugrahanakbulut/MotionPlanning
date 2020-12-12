﻿using System;
using System.Collections.Generic;
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
    public EInputState EInputState { get; private set; } = EInputState.CreatingObstacles;
    
    private List<Vector3> _obstacle = new List<Vector3>();

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
    
    public Action<Vector3, Vector3> OnObstacleCreated { get; set; }

    private void Update()
    {
        CheckObstacleInput();
    }

    private void CheckObstacleInput()
    {
        if (!Input.GetMouseButtonDown(0) ||
            EInputState != EInputState.CreatingObstacles)
            return;

        Vector3 mouseWorldPos = _MainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        _obstacle.Add(mouseWorldPos);

        if (_obstacle.Count == 2)
        {
            OnObstacleCreated?.Invoke(_obstacle[0], _obstacle[1]);
            
            _obstacle.Clear();
        }
    }
}
