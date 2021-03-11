using UnityEngine;

namespace DarkRoom {

static class RTUtil
{
    public static RenderTextureFormat SingleChannelFormat
      => SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)
           ? RenderTextureFormat.R8 : RenderTextureFormat.Default;

    public static RenderTextureFormat SingleChannelHalfFormat
      => SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf)
           ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGBHalf;

    public static RenderTexture NewSingleChannel(int width, int height)
      => NewRT(width, height, SingleChannelFormat);

    public static RenderTexture NewSingleChannelHalf(int width, int height)
      => NewRT(width, height, SingleChannelHalfFormat);

    public static RenderTexture NewSingleChannelUav(int width, int height)
      => NewRT(width, height, SingleChannelFormat, true);

    public static RenderTexture NewSingleChannelHalfUav(int width, int height)
      => NewRT(width, height, SingleChannelHalfFormat, true);

    public static RenderTexture NewRT
      (int width, int height, RenderTextureFormat format, bool uav = false)
    {
        var rt = new RenderTexture(width, height, 0, format);
        if (uav) rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }
}

static class ComputeShaderExtensions
{
    static int[] i2 = new int[2];

    public static void SetInts
      (this ComputeShader cs, string name, int x, int y)
    {
        i2[0] = x; i2[1] = y;
        cs.SetInts(name, i2);
    }
}

} // namespace DarkRoom
