using UnityEngine;

public class DashBehaviour : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer = null;
    
    public void InitDashes()
    {
        float distance = Vector3.Distance(_lineRenderer.GetPosition(0), _lineRenderer.GetPosition(1));
        
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        
        block.SetFloatArray("_MainTex_ST", new []{distance * 10, 1, 0, 0});
    }
}
