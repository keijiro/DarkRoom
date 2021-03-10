Shader "Hidden/DarkRoom/Blit"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    sampler2D _FeedbackBuffer;
    sampler2D _WebcamInput;

    float3 _EffectParams; // feedback, brightness, glitch

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
        float feedback = _EffectParams.x;
        float3 c0 = tex2D(_FeedbackBuffer, uv).rgb;
        float3 c1 = tex2D(_WebcamInput, uv).rgb;
        return float4(lerp(c1, max(c0, c1), feedback), 1);
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
