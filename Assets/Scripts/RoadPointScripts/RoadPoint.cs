using TMPro;
using UnityEngine;

namespace RoadPointScripts
{
    public class RoadPoint : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;

        public void InitRoadPoint(bool isStartPoint)
        {
            string t = isStartPoint ? "s" : "t";
            
            _text.SetText(t);
        }
    }
}