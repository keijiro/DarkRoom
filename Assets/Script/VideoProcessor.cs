using UnityEngine;
using Unity.Mathematics;

namespace DarkRoom {

public sealed class VideoProcessor : MonoBehaviour
{
    #region Editable attributes

    [SerializeField, Range(0, 1)] float _feedbackAmount = 0.5f;
    [SerializeField, Range(0, 1)] float _feedbackBlend = 0.5f;
    [SerializeField, Range(0, 1)] float _noiseFrequency = 0.0f;
    [SerializeField, Range(0, 1)] float _noiseToFlicker = 0.0f;
    [SerializeField, Range(0, 1)] float _noiseToShake = 0.0f;
    [SerializeField, Range(0, 1)] float _noiseToStretch = 0.0f;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] Shader _fxShader = null;
    [SerializeField, HideInInspector] Shader _blitShader = null;

    #endregion

    #region Private variables

    SegmentationFilter _source;
    (Material fx, Material blit) _materials;
    (RenderTexture, RenderTexture) _feedback;

    #endregion

    #region Private properties and methods

    Bounds BigBounds
      => new Bounds(Vector3.zero, Vector3.one * 1000);

    RenderTexture NewFeedbackRT()
      => new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGBFloat);

    float DynamicNoiseFrequency
      => noise.snoise(math.float2(0.234f, (Time.time % 1000) * 4)) * 2;

    float NoiseExponentValue
      => math.pow(50, 1 - _noiseFrequency);

    Vector2 NoiseParamVector
      => new Vector2(DynamicNoiseFrequency, NoiseExponentValue);

    Vector4 EffectParamVector
      => new Vector4(_feedbackBlend, _noiseToFlicker,
                     _noiseToShake, _noiseToStretch);

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _source = GetComponent<SegmentationFilter>();
        _materials = (new Material(_fxShader), new Material(_blitShader));
        _feedback = (NewFeedbackRT(), NewFeedbackRT());
    }

    void OnDestroy()
    {
        Destroy(_materials.fx);
        Destroy(_materials.blit);
        Destroy(_feedback.Item1);
        Destroy(_feedback.Item2);
    }

    void LateUpdate()
    {
        // Feedback with effects
        var m1 = _materials.fx;
        m1.SetTexture("_FeedbackTexture", _feedback.Item1);
        m1.SetTexture("_CameraTexture", _source.CameraTexture);
        m1.SetTexture("_MaskTexture", _source.MaskTexture);
        m1.SetFloat("_Feedback", _feedbackAmount);
        m1.SetPass(0);
        RenderTexture.active = _feedback.Item2;
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, 1);

        // Blit to screen
        var m2 = _materials.blit;
        m2.SetTexture("_FeedbackTexture", _feedback.Item2);
        m2.SetTexture("_CameraTexture", _source.CameraTexture);
        m2.SetTexture("_MaskTexture", _source.MaskTexture);
        m2.SetVector("_NoiseParams", NoiseParamVector);
        m2.SetVector("_EffectParams", EffectParamVector);
        Graphics.DrawProcedural(m2, BigBounds, MeshTopology.Triangles, 6, 1);

        // Swap
        _feedback = (_feedback.Item2, _feedback.Item1);
    }

    #endregion
}

} // namespace DarkRoom
