using UnityEngine;
using UnityEngine.UI;

namespace DarkRoom {

public sealed class WebcamSelector : MonoBehaviour
{
    WebCamTexture _webcam;

    public bool Ready => _webcam != null;
    public Texture Texture => _webcam;

    void Start()
    {
        var dropdown = GetComponent<Dropdown>();

        dropdown.ClearOptions();
        dropdown.options.Add(new Dropdown.OptionData("--"));

        foreach (var device in WebCamTexture.devices)
            dropdown.options.Add(new Dropdown.OptionData(device.name));

        dropdown.value = dropdown.options.Count > 0 ? 1 : 0;
        dropdown.RefreshShownValue();
    }

    public void OnChangeValue(int value)
    {
        if (_webcam != null)
        {
            _webcam.Stop();
            Destroy(_webcam);
        }

        if (value == 0) return;

        var deviceName = GetComponent<Dropdown>().options[value].text;
        _webcam = new WebCamTexture(deviceName, 1920, 1080, 30);
        _webcam.Play();
    }
}

} // namespace DarkRoom
