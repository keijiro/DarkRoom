using UnityEngine;
using Unity.Mathematics;

namespace DarkRoom {

public sealed class VideoProcessor : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamSelector _source = null;
    [SerializeField, Range(0, 1)] float _feedbackAmount = 0.5f;
    [SerializeField, Range(0, 1)] float _feedbackBlend = 0.5f;

    #endregion

    #region Project asset references

    [SerializeField, HideInInspector] Shader _fxShader = null;
    [SerializeField, HideInInspector] Shader _blitShader = null;

    #endregion

    #region Private variables

    (Material fx, Material blit) _materials;
    (RenderTexture, RenderTexture) _feedback;

    #endregion

    #region Private properties and methods

    Bounds BigBounds
      => new Bounds(Vector3.zero, Vector3.one * 1000);

    RenderTexture NewFeedbackRT()
      => new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGBFloat);

    Vector2 FeedbackParamVector
      => new Vector2(_feedbackAmount, _feedbackBlend);

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
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
        m1.SetTexture("_CameraTexture", _source.Texture);
        m1.SetVector("_FeedbackParams", FeedbackParamVector);
        m1.SetPass(0);
        RenderTexture.active = _feedback.Item2;
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, 1);

        // Blit to screen
        var m2 = _materials.blit;
        m2.SetTexture("_FeedbackTexture", _feedback.Item2);
        m2.SetTexture("_CameraTexture", _source.Texture);
        m2.SetVector("_FeedbackParams", FeedbackParamVector);
        Graphics.DrawProcedural(m2, BigBounds, MeshTopology.Triangles, 6, 1);

        // Swap
        _feedback = (_feedback.Item2, _feedback.Item1);
    }

    #endregion
}

} // namespace DarkRoom
