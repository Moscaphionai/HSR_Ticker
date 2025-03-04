Shader "Custom/Mirror"
{
    Properties
    {
        _BaseMap("Base Map",2D)="white"{}
        _BaseColor("Base Color",Color)=(1,1,1,1)
        _HighLightColor("High Light Color",Color)=(1,1,1,1)
        _HighLightMix("High Light Mix",Range(0,1))=0.2

        [Space]

        _StencilRef("Stencil Ref",Float)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comp",Float)=8
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass("Stencil Pass",Float)=0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail("Stencil Fail",Float)=0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        Pass
        {
            Name "Mirror Stencil"
            Tags
            {
                "LightMode"="MirrorStencil"
            }

            ZWrite Off
            Cull Off
            ColorMask 0

            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilComp]
                Pass [_StencilPass]
                Fail [_StencilFail]
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _HighLightColor;
                float _HighLightMix;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            float4 frag(Varyings IN): SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
        Pass
        {
            Name "Mirror Color"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            BlendOp Add
            Blend SrcAlpha OneMinusSrcAlpha

            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS:SV_POSITION;
                float2 uv:TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _HighLightColor;
                float _HighLightMix;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 frag(Varyings IN):SV_Target
            {
                float4 destColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float4 finalColor = destColor * lerp(_BaseColor, _HighLightColor, _HighLightMix);
                return finalColor;
            }
            ENDHLSL
        }
    }
}