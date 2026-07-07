// Car paint - URP 17 (Unity 6). LIT. URP variant of "BoneCracker Games/RCCP/Effects/CarPaint".
// LAYERED model: paint-tinted metallic reflection + UNTINTED dielectric clearcoat reflection (Schlick Fresnel, F0=0.04),
// fresnel flip-flop color, base spec, sharper clearcoat highlight, flake sparkle, skybox/ambient SH diffuse.
// Forward+ clustered additional lights (LIGHT_LOOP), main-light shadow receiving, fog.
// PASSES: UniversalForwardOnly + ShadowCaster + DepthOnly + DepthNormalsOnly. Clearcoat/flip-flop/flakes can't be
// encoded in URP's GBuffer, so like URP's own ComplexLit this shader renders FORWARD-ONLY in the Deferred and
// Deferred+ paths — "UniversalForwardOnly" is drawn by all four rendering paths, while a "UniversalForward" tag is
// SKIPPED by the deferred renderers (the V2.50.0 fix: the previous single UniversalForward pass made bodies
// invisible in Deferred). DepthNormalsOnly is required so deferred's depth-normal prepass / SSAO / decals see the body.
// Every pass declares the IDENTICAL UnityPerMaterial CBUFFER — SRP Batcher compatibility is per-pass.
// SHIPPED ONLY INSIDE THE URP SHADER PACKAGE (imported by the RCCP Render Pipeline Converter when URP is active) so
// it never lands in a pure Built-in project — its URP includes would otherwise error there. Built-in uses the
// loose "BoneCracker Games/RCCP/Effects/CarPaint". Keep the Properties identical so the converter's shader swap carries values.
Shader "BoneCracker Games/RCCP/Effects/CarPaint_URP"
{
    Properties
    {
        [Header(Paint Color)]
        _BaseColor ("Base Color", Color) = (0,0,0,1)
        _FlipColor ("Flip Color (grazing)", Color) = (0.051,0,0.251,1)
        _FlipPower ("Flip Power", Range(0.5,8)) = 3.0
        _MainTex ("Albedo (optional)", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.22
        [Header(Specular and Clearcoat)]
        _Smoothness ("Base Smoothness", Range(0,1)) = 0.95
        [HDR] _ClearcoatColor ("Clearcoat Color", Color) = (1,1,1,1)
        _ClearcoatSmoothness ("Clearcoat Smoothness", Range(0,1)) = 0.8
        _ClearcoatStrength ("Clearcoat Strength", Range(0,4)) = 3
        [Header(Surface Maps)]
        [Toggle(_NORMALMAP)] _UseNormalMap ("Use Normal Map", Float) = 0
        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0,3)) = 0
        [Toggle(_SPECGLOSSMAP)] _UseSpecGloss ("Use Specular Gloss Map", Float) = 0
        _SpecGlossMap ("Specular (RGB) Smoothness (A)", 2D) = "white" {}
        [Header(Metal Flakes)]
        _FlakeMap ("Flake Noise (R)", 2D) = "black" {}
        [HDR] _FlakeColor ("Flake Color", Color) = (1,1,1,1)
        _FlakeTiling ("Flake Tiling", Range(1,200)) = 17
        _FlakeSharpness ("Flake Sharpness", Range(1,64)) = 8
        _FlakeStrength ("Flake Strength", Range(0,4)) = 4
        [Header(Reflection)]
        _ReflectionStrength ("Reflection Strength", Range(0,2)) = 1.25
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            // UniversalForwardOnly (NOT UniversalForward): drawn by Forward, Forward+, Deferred AND Deferred+
            // renderers. UniversalForward is skipped by the deferred paths (assumed GBuffer-lit) — see header.
            Name "ForwardOnly"
            Tags { "LightMode"="UniversalForwardOnly" }
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            // _FORWARD_PLUS = Unity 6000.0 clustered keyword; _CLUSTER_LIGHT_LOOP = its 6000.1+ rename (also used
            // by Deferred+ for the forward-only pass). Both variants ship so additional lights work on every 6.x.
            #pragma multi_compile _ _FORWARD_PLUS _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _FlipColor, _MainTex_ST, _ClearcoatColor, _FlakeColor;
                float  _FlipPower, _Metallic, _Smoothness;
                float  _ClearcoatSmoothness, _ClearcoatStrength;
                float  _UseNormalMap, _UseSpecGloss, _BumpScale;   // kept in CBUFFER: SRP Batcher needs every Properties float here
                float  _FlakeTiling, _FlakeSharpness, _FlakeStrength, _ReflectionStrength;
            CBUFFER_END
            TEXTURE2D(_MainTex);      SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);      SAMPLER(sampler_BumpMap);
            TEXTURE2D(_SpecGlossMap); SAMPLER(sampler_SpecGlossMap);
            TEXTURE2D(_FlakeMap);     SAMPLER(sampler_FlakeMap);

            struct A { float4 positionOS : POSITION; float2 uv : TEXCOORD0; float3 normalOS : NORMAL; float4 tangentOS : TANGENT; };
            struct V { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; float3 normalWS : TEXCOORD1; float3 viewWS : TEXCOORD2; float3 positionWS : TEXCOORD3; float4 tangentWS : TEXCOORD4; half fogFactor : TEXCOORD5; };

            V vert (A IN)
            {
                V o;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(posWS);
                o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                VertexNormalInputs n = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                o.normalWS  = n.normalWS;
                o.tangentWS = float4(n.tangentWS, IN.tangentOS.w * unity_WorldTransformParams.w);
                o.viewWS    = GetWorldSpaceViewDir(posWS);   // normalized per-fragment
                o.positionWS = posWS;
                o.fogFactor = ComputeFogFactor(o.positionHCS.z);
                return o;
            }

            half4 frag (V IN) : SV_Target
            {
                half3 nWS = normalize(IN.normalWS);
                #ifdef _NORMALMAP
                    half3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv), _BumpScale);
                    half3 tWS = normalize(IN.tangentWS.xyz);
                    half3 bWS = normalize(cross(nWS, tWS) * IN.tangentWS.w);
                    half3 N   = normalize(mul(nTS, half3x3(tWS, bWS, nWS)));
                #else
                    half3 N = nWS;
                #endif
                half3 Vd = normalize(IN.viewWS);
                half  ndv = saturate(dot(N, Vd));

                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half3 L = normalize(mainLight.direction);
                half3 H = normalize(L + Vd);
                half  ndl = saturate(dot(N, L));
                half  ndh = saturate(dot(N, H));
                half3 mainRadiance = mainLight.color * (ndl * mainLight.shadowAttenuation);

                half  fresColor = pow(1.0 - ndv, _FlipPower);
                half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;
                half3 paint  = lerp(_BaseColor.rgb, _FlipColor.rgb, fresColor) * albedo;

                half3 specTint = 1.0;
                half  smooth   = _Smoothness;
                #ifdef _SPECGLOSSMAP
                    half4 sg = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, IN.uv);
                    specTint = sg.rgb;
                    smooth   = _Smoothness * sg.a;
                #endif

                half3 ambient = SampleSH(N);
                half3 diffuse = paint * (1.0 - _Metallic) * (mainRadiance + ambient);

                half  baseSpec = pow(ndh, exp2(smooth * 10.0) + 1.0);
                half  coat     = pow(ndh, exp2(_ClearcoatSmoothness * 10.0) + 1.0) * _ClearcoatStrength;
                half3 directSpec = _ClearcoatColor.rgb * specTint * (baseSpec + coat) * mainRadiance;

                half flakeTex = SAMPLE_TEXTURE2D(_FlakeMap, sampler_FlakeMap, IN.uv * _FlakeTiling).r;
                half flake = pow(saturate(flakeTex * ndh), _FlakeSharpness) * _FlakeStrength;

                // Layered reflection: paint-tinted metal (base roughness) + untinted dielectric clearcoat (Schlick F0=0.04).
                half3 reflDir = reflect(-Vd, N);
                half3 envBase = GlossyEnvironmentReflection(reflDir, 1.0 - smooth, 1.0);
                half3 metalRefl = paint * _Metallic * envBase * _ReflectionStrength;
                half3 envCoat = GlossyEnvironmentReflection(reflDir, 1.0 - _ClearcoatSmoothness, 1.0);
                half  om = 1.0 - ndv;
                half  fresCoat = 0.04 + 0.96 * (om * om * om * om);
                half3 coatRefl = _ClearcoatColor.rgb * envCoat * (fresCoat * _ReflectionStrength);

                half3 addColor = 0;
                #if defined(_ADDITIONAL_LIGHTS)
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light al = GetAdditionalLight(lightIndex, IN.positionWS, half4(1,1,1,1));
                    half3 Lp = normalize(al.direction);
                    half3 Hp = normalize(Lp + Vd);
                    half  atten = al.distanceAttenuation * al.shadowAttenuation;
                    half3 lc    = al.color * atten;
                    half  ndlp  = saturate(dot(N, Lp));
                    half  ndhp  = saturate(dot(N, Hp));
                    half  pSpec  = pow(ndhp, exp2(smooth * 10.0) + 1.0);
                    half  pCoat  = pow(ndhp, exp2(_ClearcoatSmoothness * 10.0) + 1.0) * _ClearcoatStrength;
                    half  pFlake = pow(saturate(flakeTex * ndhp), _FlakeSharpness) * _FlakeStrength;
                    addColor += paint * (1.0 - _Metallic) * lc * ndlp
                              + _ClearcoatColor.rgb * specTint * (pSpec + pCoat) * lc * ndlp
                              + _FlakeColor.rgb * pFlake * (ndlp * atten);
                LIGHT_LOOP_END
                #endif

                half3 rgb = diffuse
                          + metalRefl
                          + coatRefl
                          + directSpec
                          + _FlakeColor.rgb * flake * ndl
                          + addColor;
                rgb = MixFog(rgb, IN.fogFactor);
                return half4(rgb, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            // Shadow casting (was missing entirely pre-V2.50.0 — URP bodies cast no shadows in any path).
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"   // LerpWhiteTo — Shadows.hlsl uses it but doesn't include it
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)   // identical layout in every pass — SRP Batcher requirement
                float4 _BaseColor, _FlipColor, _MainTex_ST, _ClearcoatColor, _FlakeColor;
                float  _FlipPower, _Metallic, _Smoothness;
                float  _ClearcoatSmoothness, _ClearcoatStrength;
                float  _UseNormalMap, _UseSpecGloss, _BumpScale;
                float  _FlakeTiling, _FlakeSharpness, _FlakeStrength, _ReflectionStrength;
            CBUFFER_END

            float3 _LightDirection;   // set globally by URP's shadow caster pass
            float3 _LightPosition;

            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct V { float4 positionHCS : SV_POSITION; };

            V ShadowPassVertex (A IN)
            {
                V o;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 nWS   = TransformObjectToWorldNormal(IN.normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 lightDir = normalize(_LightPosition - posWS);
                #else
                    float3 lightDir = _LightDirection;
                #endif
                float4 posHCS = TransformWorldToHClip(ApplyShadowBias(posWS, nWS, lightDir));
                #if UNITY_REVERSED_Z
                    posHCS.z = min(posHCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    posHCS.z = max(posHCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                o.positionHCS = posHCS;
                return o;
            }

            half4 ShadowPassFragment (V IN) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            // Depth prepass / _CameraDepthTexture / Depth Priming.
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back
            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)   // identical layout in every pass — SRP Batcher requirement
                float4 _BaseColor, _FlipColor, _MainTex_ST, _ClearcoatColor, _FlakeColor;
                float  _FlipPower, _Metallic, _Smoothness;
                float  _ClearcoatSmoothness, _ClearcoatStrength;
                float  _UseNormalMap, _UseSpecGloss, _BumpScale;
                float  _FlakeTiling, _FlakeSharpness, _FlakeStrength, _ReflectionStrength;
            CBUFFER_END

            struct A { float4 positionOS : POSITION; };
            struct V { float4 positionHCS : SV_POSITION; };

            V DepthOnlyVertex (A IN)
            {
                V o;
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return o;
            }

            half DepthOnlyFragment (V IN) : SV_Target { return IN.positionHCS.z; }
            ENDHLSL
        }

        Pass
        {
            // Deferred's depth-normal prepass + SSAO (Depth Normals source) + decals. REQUIRED for forward-only
            // materials in the Deferred / Deferred+ paths. Geometric normal only: paint ships with _BumpScale 0
            // and flake/bump micro-detail is irrelevant to SSAO.
            Name "DepthNormalsOnly"
            Tags { "LightMode"="DepthNormalsOnly" }
            ZWrite On
            Cull Back
            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

            CBUFFER_START(UnityPerMaterial)   // identical layout in every pass — SRP Batcher requirement
                float4 _BaseColor, _FlipColor, _MainTex_ST, _ClearcoatColor, _FlakeColor;
                float  _FlipPower, _Metallic, _Smoothness;
                float  _ClearcoatSmoothness, _ClearcoatStrength;
                float  _UseNormalMap, _UseSpecGloss, _BumpScale;
                float  _FlakeTiling, _FlakeSharpness, _FlakeStrength, _ReflectionStrength;
            CBUFFER_END

            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct V { float4 positionHCS : SV_POSITION; float3 normalWS : TEXCOORD0; };

            V DepthNormalsVertex (A IN)
            {
                V o;
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return o;
            }

            half4 DepthNormalsFragment (V IN) : SV_Target
            {
                float3 nWS = normalize(IN.normalWS);
                #if defined(_GBUFFER_NORMALS_OCT)
                    // Match URP's accurate-GBuffer-normals encoding (deferred paths).
                    float2 oct = PackNormalOctQuadEncode(nWS);
                    float2 remapped = saturate(oct * 0.5 + 0.5);
                    half3 packed = PackFloat2To888(remapped);
                    return half4(packed, 0.0);
                #else
                    return half4(nWS, 0.0);
                #endif
            }
            ENDHLSL
        }
    }

    Fallback Off
}
