using TMPro;
using UnityEngine;

public class VertexVisualizer : MonoBehaviour
{
    [SerializeField] private TextMeshPro _textMesh;

    public void Init(Vector3 pos, int index)
    {
        transform.position = pos;
        _textMesh.SetText(index.ToString());
    }
}
