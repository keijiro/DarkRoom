Shader "Hidden/DarkRoom/Feedback"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    sampler2D _FeedbackBuffer;
    float4 _FeedbackBuffer_TexelSize;

    sampler2D _WebcamInput;
    float4 _WebcamInput_TexelSize;

    float _Feedback;

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
        // The gaussian filter constants came from
        // rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/
        const float w0 = 0.2270270270;
        const float w1 = 0.3162162162;
        const float w2 = 0.0702702703;

        const float2 d1 = 1.3846153846 * _FeedbackBuffer_TexelSize.xy;
        const float2 d2 = 3.2307692308 * _FeedbackBuffer_TexelSize.xy;

        float3 fb =
            tex2D(_FeedbackBuffer, uv + float2(-d2.x, -d2.y)).rgb * w2 * w2 +
            tex2D(_FeedbackBuffer, uv + float2(-d1.x, -d2.y)).rgb * w1 * w2 +
            tex2D(_FeedbackBuffer, uv + float2(    0, -d2.y)).rgb * w0 * w2 +
            tex2D(_FeedbackBuffer, uv + float2(+d1.x, -d2.y)).rgb * w1 * w2 +
            tex2D(_FeedbackBuffer, uv + float2(+d2.x, -d2.y)).rgb * w2 * w2 +

            tex2D(_FeedbackBuffer, uv + float2(-d2.x, -d1.y)).rgb * w2 * w1 +
            tex2D(_FeedbackBuffer, uv + float2(-d1.x, -d1.y)).rgb * w1 * w1 +
            tex2D(_FeedbackBuffer, uv + float2(    0, -d1.y)).rgb * w0 * w1 +
            tex2D(_FeedbackBuffer, uv + float2(+d1.x, -d1.y)).rgb * w1 * w1 +
            tex2D(_FeedbackBuffer, uv + float2(+d2.x, -d1.y)).rgb * w2 * w1 +

            tex2D(_FeedbackBuffer, uv + float2(-d2.x,     0)).rgb * w2 * w0 +
            tex2D(_FeedbackBuffer, uv + float2(-d1.x,     0)).rgb * w1 * w0 +
            tex2D(_FeedbackBuffer, uv + float2(    0,     0)).rgb * w0 * w0 +
            tex2D(_FeedbackBuffer, uv + float2(+d1.x,     0)).rgb * w1 * w0 +
            tex2D(_FeedbackBuffer, uv + float2(+d2.x,     0)).rgb * w2 * w0 +

            tex2D(_FeedbackBuffer, uv + float2(-d2.x, +d1.y)).rgb * w2 * w1 +
            tex2D(_FeedbackBuffer, uv + float2(-d1.x, +d1.y)).rgb * w1 * w1 +
            tex2D(_FeedbackBuffer, uv + float2(    0, +d1.y)).rgb * w0 * w1 +
            tex2D(_FeedbackBuffer, uv + float2(+d1.x, +d1.y)).rgb * w1 * w1 +
            tex2D(_FeedbackBuffer, uv + float2(+d2.x, +d1.y)).rgb * w2 * w1 +

            tex2D(_FeedbackBuffer, uv + float2(-d2.x, +d2.y)).rgb * w2 * w2 +
            tex2D(_FeedbackBuffer, uv + float2(-d1.x, +d2.y)).rgb * w1 * w2 +
            tex2D(_FeedbackBuffer, uv + float2(    0, +d2.y)).rgb * w0 * w2 +
            tex2D(_FeedbackBuffer, uv + float2(+d1.x, +d2.y)).rgb * w1 * w2 +
            tex2D(_FeedbackBuffer, uv + float2(+d2.x, +d2.y)).rgb * w2 * w2;

        float3 wi = tex2D(_WebcamInput, uv).rgb; 
        return float4(lerp(wi, fb, _Feedback), 1);
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
