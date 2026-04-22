using UnityEngine;

namespace Game5
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class WaterFillController : MonoBehaviour
    {
        private static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");

        private SpriteRenderer _renderer;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _mpb = new MaterialPropertyBlock();
        }

        // normalized: 0 = empty, 1 = full.
        public void SetFillAmount(float normalized)
        {
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(FillAmountId, Mathf.Clamp01(normalized));
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}
