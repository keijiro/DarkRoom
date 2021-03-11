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

    // Frequency, Exponent
    float2 _NoiseParams;

    // Feedback, Noise to brightness, Noise to H/V displacement
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
        float nfreq    = _NoiseParams.x;
        float nexp     = _NoiseParams.y;
        float feedback = _EffectParams.x;
        float n2br     = _EffectParams.y;
        float n2hdisp  = _EffectParams.z;
        float n2vdisp  = _EffectParams.w;

        // Glitch amount
        float n = snoise(float2(uv.y * nfreq, RTime(6)));
        n = pow(abs(n), nexp);

        // Vertical displacement
        uv.y += n * n2vdisp * 0.2;

        // Horizontal displacement
        uint ln = (uv.y + RTime(60)) * _CameraTexture_TexelSize.w;
        float disp = GenerateHashedRandomFloat(ln) * n * n2hdisp * 0.2;

        // Webcam input samples with R/B displacement
        float c_r = tex2D(_CameraTexture, uv - float2(disp, 0)).r;
        float c_g = tex2D(_CameraTexture, uv                  ).g;
        float c_b = tex2D(_CameraTexture, uv + float2(disp, 0)).b;
        float3 c_in = float3(c_r, c_g, c_b);

        // Blend with a feedback sample
        float3 c_fb = tex2D(_FeedbackTexture, uv).rgb;
        float3 c_out = lerp(c_in, max(c_in, c_fb), feedback);;

        // Noise to brightness
        c_out *= 1 - n * n2br;

        // Temp: Desaturation with mask
        float mask = tex2D(_MaskTexture, uv).r;
        mask = smoothstep(0.4, 0.5, mask);
        c_out = lerp(dot(c_out, 0.2), c_out, mask);

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
