Shader "Custom/UnlitTriplanar"
{
    Properties
    {
        // NoScaleOffset removes Unity's automatic per-texture Tiling/Offset
        // control from the Inspector. That control writes to _MainTex_ST,
        // which this shader never reads (UVs are built from object-space
        // position instead) - leaving it in was confusing, it looked like
        // a working tiling control but silently did nothing. _Tiling below
        // is the one that actually drives the triplanar UVs.
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _Tiling ("Tiling", Float) = 1
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 normalOS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _Color;
            float _Tiling;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Raw positionOS is in the mesh's unscaled local units, so
                // two blocks sharing the same base mesh at different
                // Transform Scale values (this project has blocks ranging
                // from Scale 10 to Scale 60) would sample the same UV
                // range despite being very different real-world sizes -
                // that's why the stripe pattern looked zoomed-in on some
                // blocks and tight on others. Baking the object's actual
                // scale into the position fixes that, while still keeping
                // everything locked to the object's own axes (translation
                // and rotation don't affect it), so it still won't "swim"
                // once this becomes flying debris.
                float3 objectScale = float3(
                    length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20)),
                    length(float3(unity_ObjectToWorld._m01, unity_ObjectToWorld._m11, unity_ObjectToWorld._m21)),
                    length(float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22)));

                OUT.positionOS = IN.positionOS.xyz * objectScale;
                OUT.normalOS = IN.normalOS;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Blend weight per axis based on how much the surface faces
                // that axis. A flat-topped face uses almost all Y, a wall
                // uses X or Z, corners blend smoothly between them.
                float3 blend = abs(normalize(IN.normalOS));
                blend = blend / (blend.x + blend.y + blend.z);

                float2 uvX = IN.positionOS.zy * _Tiling;
                float2 uvY = IN.positionOS.xz * _Tiling;
                float2 uvZ = IN.positionOS.xy * _Tiling;

                float4 colX = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvX);
                float4 colY = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvY);
                float4 colZ = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvZ);

                float4 result = colX * blend.x + colY * blend.y + colZ * blend.z;
                return result * _Color;
            }
            ENDHLSL
        }
    }
}