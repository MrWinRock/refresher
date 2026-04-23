Shader "Custom/WaterFill"
{
    Properties
    {
        _MainTex    ("Sprite Texture", 2D) = "white" {}
        // PerRendererData lets SpriteRenderer tint override Color per-instance without a new material.
        [PerRendererData] _Color ("Tint", Color) = (1, 1, 1, 1)
        // 0 = empty, 1 = full. Drive this from WaterFillController via MaterialPropertyBlock.
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"          = "Transparent"
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        // Universal2D is the correct LightMode for the URP 2D Renderer.
        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                float  _FillAmount;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv          = TRANSFORM_TEX(v.uv, _MainTex);
                o.color       = v.color * _Color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // UV.y == 0 at sprite bottom, 1 at top.
                // clip(x) discards when x < 0, so pixels above the fill line disappear.
                clip(_FillAmount - i.uv.y);

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return col * i.color;
            }
            ENDHLSL
        }
    }
}
