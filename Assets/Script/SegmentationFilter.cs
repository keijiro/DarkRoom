using UnityEngine;
using Unity.Barracuda;

namespace DarkRoom {

public sealed class SegmentationFilter : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamSelector _webcam = null;
    [SerializeField, HideInInspector] Unity.Barracuda.NNModel _model = null;
    [SerializeField, HideInInspector] ComputeShader _preprocessor = null;
    [SerializeField, HideInInspector] Shader _postprocessShader = null;

    #endregion

    #region Compile-time constants

    // We use a bit strange aspect ratio (20:11) because we have to use 16n+1
    // for these dimension values. It may distort input images a bit, but it
    // might not be a problem for the segmentation models.
    public const int InWidth = 640 + 1;
    public const int InHeight = 352 + 1;
    public const int OutWidth = InWidth / 8 + 1;
    public const int OutHeight = InHeight / 8 + 1;

    #endregion

    #region Internal objects

    RenderTexture _webcamBuffer;
    ComputeBuffer _preprocessed;
    RenderTexture _generated;
    RenderTexture _postprocessed;
    Material _postprocessor;
    IWorker _worker;

    #endregion

    #region Public properties

    public Texture CameraTexture => _webcam.Ready ? _webcamBuffer : null;
    public Texture MaskTexture => _webcam.Ready ? _postprocessed : null;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _webcamBuffer = new RenderTexture(1920, 1080, 0);
        _generated = RTUtil.NewSingleChannelHalfRT(OutWidth, OutHeight);
        _postprocessed = RTUtil.NewSingleChannelRT(OutWidth, OutHeight);
        _postprocessor = new Material(_postprocessShader);
    }

    void OnEnable()
    {
        _preprocessed = new ComputeBuffer(InWidth * InHeight * 3, sizeof(float));
        _worker = ModelLoader.Load(_model).CreateWorker();
    }

    void OnDisable()
    {
        _preprocessed?.Dispose();
        _preprocessed = null;

        _worker?.Dispose();
        _worker = null;
    }

    void OnDestroy()
    {
        if (_webcamBuffer != null) Destroy(_webcamBuffer);
        if (_generated != null) Destroy(_generated);
        if (_postprocessed != null) Destroy(_postprocessed);
        if (_postprocessor != null) Destroy(_postprocessor);
    }

    void Update()
    {
        if (!_webcam.Ready) return;

        // Input buffer update
        Graphics.Blit(_webcam.Texture, _webcamBuffer);

        // Preprocessing for BodyPix
        _preprocessor.SetTexture(0, "_Texture", _webcamBuffer);
        _preprocessor.SetBuffer(0, "_Tensor", _preprocessed);
        _preprocessor.SetInt("_Width", InWidth);
        _preprocessor.SetInt("_Height", InHeight);
        _preprocessor.Dispatch(0, OutWidth, OutHeight, 1);

        // BodyPix invocation
        using (var tensor = new Tensor(1, InHeight, InWidth, 3, _preprocessed))
            _worker.Execute(tensor);

        // BodyPix output retrieval
        var output = _worker.PeekOutput("float_segments");

        // Bake into a render texture with normalizing into [0, 1].
        output.ToRenderTexture(_generated);

        // Postprocessing shader invocation
        Graphics.Blit(_generated, _postprocessed, _postprocessor);
    }

    #endregion
}

} // namespace DarkRoom
