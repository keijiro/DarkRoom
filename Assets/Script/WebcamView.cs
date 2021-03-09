using UnityEngine;

namespace DarkRoom {

public sealed class WebcamView : MonoBehaviour
{
    [SerializeField] WebcamSelector _webcam = null;

    [SerializeField, HideInInspector] Mesh _mesh = null;
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;

    void Start()
      => _material = new Material(_shader);

    void LateUpdate()
    {
        _material.SetTexture("_BaseMap", _webcam.Texture);
        var mtx = Matrix4x4.Scale(new Vector3(16.0f / 9, 1, 1));
        Graphics.DrawMesh(_mesh, mtx, _material, 0);
    }
}

} // namespace DarkRoom
