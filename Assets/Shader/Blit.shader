Shader "Hidden/DarkRoom/Blit"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    sampler2D _FeedbackTexture;
    sampler2D _CameraTexture;

    float4 _FeedbackTexture_TexelSize;
    float4 _CameraTexture_TexelSize;

    // Feedback amount, Blend ratio
    float2 _FeedbackParams;

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
        float fb_blend = _FeedbackParams.y;

        // Camera input + feedback
        float3 cam = tex2D(_CameraTexture, uv).rgb;
        float3 fb = tex2D(_FeedbackTexture, uv).rgb;
        return float4(lerp(cam, max(cam, fb), fb_blend), 1);
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
