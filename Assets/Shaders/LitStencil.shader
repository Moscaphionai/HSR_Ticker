Shader "Custom/LitStencil"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color",Color)=(1,1,1,1)
        [MainTexture] _BaseMap("Base Map",2D)="white"{}

        _HighLightColor("High Light Color",Color)=(1,1,1,1)
        _HighLightMix("High Light Mix",Range(0,1))=0

        [Header(Option)]
        [Enum(off,0,On,1)]_ZWriteMode("ZWriteMode",Float)=1
        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode",Float)=2
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("ZTestMdoe",Float)=4
        [Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask("ColorMask",Float)=15

        [Header(Blend)]
        [Enum(UnityEngine.Rendering.BlendOp)]_BlendOp("BlendOp",Float)=0
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",Float)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend",Float)=0

        [Header(Stencil)]
        _StencilRef("Stencil Ref",Float)=0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comp",Float)=8
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilPass("Stencil Pass",Float)=0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail("Stencil Fail",Float)=0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry"
        }
        Pass
        {
            BlendOp [_BlendOp]
            Blend [_SrcBlend] [_DstBlend]

            ZWrite [_ZWriteMode]
            Cull [_CullMode]
            ZTest [_ZTestMode]
            ColorMask [_ColorMask]

            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilComp]
                Pass [_StencilPass]
                Fail [_StencilFail]
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _HighLightColor;
                float _HighLightMix;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS:POSITION;
                float3 normalOS:NORMAL;
                float2 uv:TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS:SV_POSITION;
                float3 normalWS:NORMAL;
                float2 uv:TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 frag(Varyings IN):SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                Light light = GetMainLight();
                float halfLambert = pow(0.5 * dot(IN.normalWS, light.direction) + 0.5, 2);
                color.rgb *= halfLambert;
                return color * lerp(_BaseColor, _HighLightColor, _HighLightMix);
            }
            ENDHLSL
        }
    }
}