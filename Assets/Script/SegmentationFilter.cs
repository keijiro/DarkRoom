using UnityEngine;
using Unity.Barracuda;

namespace DarkRoom {

public sealed class SegmentationFilter : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamSelector _webcam = null;
    [SerializeField, HideInInspector] Unity.Barracuda.NNModel _model = null;
    [SerializeField, HideInInspector] ComputeShader _compute = null;

    #endregion

    #region Compile-time constants

    // We use a bit strange aspect ratio (20:11) because we have to use 16n+1
    // for these dimension values. It may distort input images a bit, but it
    // might not be a problem for the segmentation models.
    public const int Width = 640 + 1;
    public const int Height = 352 + 1;

    public const int Width_8 = Width / 8 + 1;
    public const int Height_8 = Height / 8 + 1;

    public const int Width_8_8 = Width_8 / 8 + 1;
    public const int Height_8_8 = Height_8 / 8 + 1;

    #endregion

    #region Internal objects

    RenderTexture _webcamBuffer;
    ComputeBuffer _preprocessed;
    RenderTexture _generated;
    RenderTexture _postprocessed;
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
        _generated = RTUtil.NewSingleChannelHalf(Width_8, Height_8);
        _postprocessed = RTUtil.NewSingleChannelUav(Width_8, Height_8);
    }

    void OnEnable()
    {
        _preprocessed = new ComputeBuffer(Width * Height * 3, sizeof(float));
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
    }

    void Update()
    {
        if (!_webcam.Ready) return;

        // Input buffer update
        Graphics.Blit(_webcam.Texture, _webcamBuffer);

        // Preprocessing
        _compute.SetInt("_Width", Width);
        _compute.SetInt("_Height", Height);
        _compute.SetTexture(0, "_InSampler", _webcamBuffer);
        _compute.SetBuffer(0, "_OutTensor", _preprocessed);
        _compute.Dispatch(0, Width_8, Height_8, 1);

        // BodyPix invocation
        using (var tensor = new Tensor(1, Height, Width, 3, _preprocessed))
            _worker.Execute(tensor);

        // Output retrieval
        _worker.PeekOutput("float_segments").ToRenderTexture(_generated);

        // Postprocessing
        _compute.SetInt("_Width", Width_8);
        _compute.SetInt("_Height", Height_8);
        _compute.SetTexture(1, "_InTexture", _generated);
        _compute.SetTexture(1, "_OutTexture", _postprocessed);
        _compute.Dispatch(1, Width_8_8, Height_8_8, 1);
    }

    #endregion
}

} // namespace DarkRoom
