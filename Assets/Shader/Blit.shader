Shader "Hidden/DarkRoom/Blit"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise2D.hlsl"

    sampler2D _FeedbackBuffer;
    float4 _FeedbackBuffer_TexelSize;

    sampler2D _WebcamInput;
    float4 _WebcamInput_TexelSize;

    float2 _NoiseParams; // Frequency, Exponent
    float3 _EffectParams; // Feedback, Noise to brightness, Noise to displace

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
        float n2disp   = _EffectParams.z;

        // Glitch amount
        float n = snoise(float2(uv.y * nfreq, RTime(6)));
        n = pow(abs(n), nexp);

        // Horizontal displacement
        uint ln = (uv.y + RTime(60)) * _WebcamInput_TexelSize.w;
        float disp = GenerateHashedRandomFloat(ln) * n * n2disp * 0.2;

        // Webcam input samples with R/B displacement
        float c_r = tex2D(_WebcamInput, uv - float2(disp, 0)).r;
        float c_g = tex2D(_WebcamInput, uv                  ).g;
        float c_b = tex2D(_WebcamInput, uv + float2(disp, 0)).b;
        float3 c_in = float3(c_r, c_g, c_b);

        // Blend with a feedback sample
        float3 c_fb = tex2D(_FeedbackBuffer, uv).rgb;
        float3 c_out = lerp(c_in, max(c_in, c_fb), feedback);;

        // Noise to brightness
        c_out *= 1 - n * n2br;

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
