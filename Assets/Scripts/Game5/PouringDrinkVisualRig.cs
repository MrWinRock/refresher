using UnityEngine;

namespace Game5
{
    [DisallowMultipleComponent]
    public class PouringDrinkVisualRig : MonoBehaviour
    {
        [Header("Required")]
        [SerializeField] private Transform waterObject;

        [Header("Fill Range Override")]
        [SerializeField] private bool overrideFillRange = true;
        [SerializeField] private float fillEmptyY = -1f;
        [SerializeField] private float fillFullY = 1f;

        public Transform WaterObject => waterObject;
        public bool OverrideFillRange => overrideFillRange;
        public float FillEmptyY => fillEmptyY;
        public float FillFullY => fillFullY;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (fillFullY < fillEmptyY)
            {
                fillFullY = fillEmptyY;
            }
        }
#endif
    }
}


