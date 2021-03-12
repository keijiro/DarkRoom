Shader "Hidden/DarkRoom/Blit"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise2D.hlsl"

    sampler2D _FeedbackTexture;
    sampler2D _CameraTexture;
    sampler2D _MaskTexture;

    float4 _FeedbackTexture_TexelSize;
    float4 _CameraTexture_TexelSize;
    float4 _MaskTexture_TexelSize;

    // Feedback amount, Blend ratio
    float2 _FeedbackParams;

    // Frequency, Exponent
    float2 _NoiseParams;

    // To brightness, To RGB shake, To masked shake, To stretch
    float4 _EffectParams;

    float RTime(float multiplier)
    {
        return (_Time.y * multiplier) % 1000;
    }

    void Vertex(uint vid : SV_VertexID,
                out float4 position : SV_Position,
                out float2 uv : TEXCOORD0)
    {
        float x = vid & 1;
        float y = vid < 2 || vid == 5;
        position = float4(x * 2 - 1, 1 - y * 2, 1, 1);
        uv = float2(x, y);
    }

    float4 Fragment(float4 position : SV_Position,
                    float2 uv : TEXCOORD0) : SV_Target
    {
        // Parameter extraction
        float n_freq       = _NoiseParams.x;
        float n_exp        = _NoiseParams.y;
        float fb_blend     = _FeedbackParams.y;
        float n_flicker    = _EffectParams.x;
        float n_shake_rgb  = _EffectParams.y;
        float n_shake_mask = _EffectParams.z;
        float n_stretch    = _EffectParams.w;

        // Glitch amount
        float n = snoise(float2(uv.y * n_freq, RTime(6)));
        n = pow(abs(n), n_exp);

        // Stretch (vertical displacement)
        uv.y += n * n_stretch * 0.2;

        // Per line random displacement
        uint ln = (uv.y + RTime(60)) * _CameraTexture_TexelSize.w;
        float disp = GenerateHashedRandomFloat(ln);

        // Masked shake (horizontal displacement with mask)
        float disp_sm = (disp - 0.5) * n * 0.2 * n_shake_mask;
        float mask = tex2D(_MaskTexture, uv + float2(disp_sm, 0)).r;
        if (mask > 0.4) uv.x += disp_sm;

        // Webcam input samples with RGB horizontal displacement
        float disp_rgb = disp * n * 0.2 * n_shake_rgb;
        float c_r = tex2D(_CameraTexture, uv - float2(disp_rgb, 0)).r;
        float c_g = tex2D(_CameraTexture, uv                      ).g;
        float c_b = tex2D(_CameraTexture, uv + float2(disp_rgb, 0)).b;
        float3 c_in = float3(c_r, c_g, c_b);

        // Blend with a feedback sample
        float3 c_fb = tex2D(_FeedbackTexture, uv).rgb;
        float3 c_out = lerp(c_in, max(c_in, c_fb), fb_blend);

        // Flicker (noise to brightness)
        c_out *= 1 - n * n_flicker;

        return float4(c_out, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }
}
