// Car paint - BUILT-IN render pipeline. LIT (main dir light + additional per-pixel lights + reflection probe + ambient).
// LAYERED model: paint-tinted metallic reflection + UNTINTED dielectric clearcoat reflection (Schlick Fresnel, F0=0.04),
// fresnel flip-flop color, base spec, sharper clearcoat highlight, flake sparkle, skybox/ambient SH diffuse.
// Optional tangent-space NORMAL map and SPECULAR/GLOSS map (RGB spec tint, A smoothness), keyword-gated.
// ForwardBase (main light + shadows + ambient + reflection) + ForwardAdd (per-pixel extra lights) + ShadowCaster, fog.
// URP variant ships separately as 'BoneCracker Games/RCCP/Effects/CarPaint_URP' (delivered via the URP shader package +
// applied by the RCCP Render Pipeline Converter) so a pure Built-in project never references URP package includes.
Shader "BoneCracker Games/RCCP/Effects/CarPaint"
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

    // ========================== Built-in ===========================
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="ForwardBase" }
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            float4 _BaseColor, _FlipColor, _MainTex_ST, _ClearcoatColor, _FlakeColor;
            sampler2D _MainTex, _BumpMap, _SpecGlossMap, _FlakeMap;
            float _FlipPower, _Metallic, _Smoothness;
            float _ClearcoatSmoothness, _ClearcoatStrength;
            float _UseNormalMap, _UseSpecGloss, _BumpScale;
            float _FlakeTiling, _FlakeSharpness, _FlakeStrength, _ReflectionStrength;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float3 normal : NORMAL; float4 tangent : TANGENT; };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
                SHADOW_COORDS(4)
                UNITY_FOG_COORDS(5)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                o.tangentWS = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w * unity_WorldTransformParams.w);
                o.viewWS   = WorldSpaceViewDir(v.vertex);   // normalized per-fragment
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 nWS = normalize(i.normalWS);
                #ifdef _NORMALMAP
                    half3 nTS = UnpackNormal(tex2D(_BumpMap, i.uv));
                    nTS.xy *= _BumpScale;
                    nTS = normalize(nTS);
                    float3 tWS = normalize(i.tangentWS.xyz);
                    float3 bWS = normalize(cross(nWS, tWS) * i.tangentWS.w);
                    float3 N   = normalize(mul(nTS, float3x3(tWS, bWS, nWS)));
                #else
                    float3 N = nWS;
                #endif
                float3 Vd = normalize(i.viewWS);
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float3 H = normalize(L + Vd);
                half ndv = saturate(dot(N, Vd));
                half ndl = saturate(dot(N, L));
                half ndh = saturate(dot(N, H));
                half shadow = SHADOW_ATTENUATION(i);
                half3 mainRadiance = _LightColor0.rgb * (ndl * shadow);

                half  fresColor = pow(1.0 - ndv, _FlipPower);
                half3 albedo = tex2D(_MainTex, i.uv).rgb;
                half3 paint  = lerp(_BaseColor.rgb, _FlipColor.rgb, fresColor) * albedo;

                half3 specTint = 1.0;
                half  smooth   = _Smoothness;
                #ifdef _SPECGLOSSMAP
                    half4 sg = tex2D(_SpecGlossMap, i.uv);
                    specTint = sg.rgb;
                    smooth   = _Smoothness * sg.a;
                #endif

                // Diffuse = direct (shadowed) + ambient SH (skybox-derived) probe.
                half3 ambient = ShadeSH9(float4(N, 1.0));
                half3 diffuse = paint * (1.0 - _Metallic) * (mainRadiance + ambient);

                half  baseSpec = pow(ndh, exp2(smooth * 10.0) + 1.0);
                half  coat     = pow(ndh, exp2(_ClearcoatSmoothness * 10.0) + 1.0) * _ClearcoatStrength;
                half3 directSpec = _ClearcoatColor.rgb * specTint * (baseSpec + coat) * mainRadiance;

                half3 flakeRGB = tex2D(_FlakeMap, i.uv * _FlakeTiling).rgb;
                half  flakeTex = max(flakeRGB.r, max(flakeRGB.g, flakeRGB.b)); // intensity = peak channel (== old .r for a gray map)
                half3 flakeHue = flakeRGB / max(flakeTex, 1e-4);               // per-flake tint; (1,1,1) for a gray map
                half  flake    = pow(saturate(flakeTex * ndh), _FlakeSharpness) * _FlakeStrength;

                // ---- LAYERED ENVIRONMENT REFLECTION ----
                half3 reflDir = reflect(-Vd, N);
                half mipBase = (1.0 - smooth) * 6.0;               // UNITY_SPECCUBE_LOD_STEPS = 6
                half mipCoat = (1.0 - _ClearcoatSmoothness) * 6.0;
                half3 envBase = DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, mipBase), unity_SpecCube0_HDR);
                half3 envCoat = DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, mipCoat), unity_SpecCube0_HDR);
                half3 metalRefl = paint * _Metallic * envBase * _ReflectionStrength;
                half  om = 1.0 - ndv;
                half  fresCoat = 0.04 + 0.96 * (om * om * om * om);
                half3 coatRefl = _ClearcoatColor.rgb * envCoat * (fresCoat * _ReflectionStrength);

                half3 rgb = diffuse
                          + metalRefl
                          + coatRefl
                          + directSpec
                          + _FlakeColor.rgb * flakeHue * flake * ndl;
                UNITY_APPLY_FOG(i.fogCoord, rgb);
                return fixed4(rgb, 1.0);
            }
            ENDCG
        }

        // ---- Additional per-pixel lights (point / spot / extra directional) ----
        Pass
        {
            Name "ForwardAdd"
            Tags { "LightMode"="ForwardAdd" }
            Blend One One
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _SPECGLOSSMAP
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            float4 _BaseColor, _FlipColor, _MainTex_ST, _ClearcoatColor, _FlakeColor;
            sampler2D _MainTex, _BumpMap, _SpecGlossMap, _FlakeMap;
            float _FlipPower, _Metallic, _Smoothness;
            float _ClearcoatSmoothness, _ClearcoatStrength;
            float _UseNormalMap, _UseSpecGloss, _BumpScale;
            float _FlakeTiling, _FlakeSharpness, _FlakeStrength, _ReflectionStrength;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float3 normal : NORMAL; float4 tangent : TANGENT; };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewWS : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float4 tangentWS : TEXCOORD4;
                SHADOW_COORDS(5)
                UNITY_FOG_COORDS(6)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                o.tangentWS = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w * unity_WorldTransformParams.w);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewWS   = WorldSpaceViewDir(v.vertex);   // normalized per-fragment
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 nWS = normalize(i.normalWS);
                #ifdef _NORMALMAP
                    half3 nTS = UnpackNormal(tex2D(_BumpMap, i.uv));
                    nTS.xy *= _BumpScale;
                    nTS = normalize(nTS);
                    float3 tWS = normalize(i.tangentWS.xyz);
                    float3 bWS = normalize(cross(nWS, tWS) * i.tangentWS.w);
                    float3 N   = normalize(mul(nTS, float3x3(tWS, bWS, nWS)));
                #else
                    float3 N = nWS;
                #endif
                float3 Vd = normalize(i.viewWS);
                // Directional (w==0) → direction; Point/Spot (w==1) → position − worldPos
                float3 L = normalize(_WorldSpaceLightPos0.xyz - i.worldPos * _WorldSpaceLightPos0.w);
                float3 H = normalize(L + Vd);
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);

                half ndv  = saturate(dot(N, Vd));
                half fresColor = pow(1.0 - ndv, _FlipPower);
                half3 albedo = tex2D(_MainTex, i.uv).rgb;
                half3 paint  = lerp(_BaseColor.rgb, _FlipColor.rgb, fresColor) * albedo;

                half3 specTint = 1.0;
                half  smooth   = _Smoothness;
                #ifdef _SPECGLOSSMAP
                    half4 sg = tex2D(_SpecGlossMap, i.uv);
                    specTint = sg.rgb;
                    smooth   = _Smoothness * sg.a;
                #endif

                half ndl = saturate(dot(N, L));
                half ndh = saturate(dot(N, H));
                half3 lc = _LightColor0.rgb * atten;

                half3 diffuse  = paint * (1.0 - _Metallic) * lc * ndl;
                half  baseSpec = pow(ndh, exp2(smooth * 10.0) + 1.0);
                half  coat     = pow(ndh, exp2(_ClearcoatSmoothness * 10.0) + 1.0) * _ClearcoatStrength;

                half3 flakeRGB = tex2D(_FlakeMap, i.uv * _FlakeTiling).rgb;
                half  flakeTex = max(flakeRGB.r, max(flakeRGB.g, flakeRGB.b)); // intensity = peak channel (== old .r for a gray map)
                half3 flakeHue = flakeRGB / max(flakeTex, 1e-4);               // per-flake tint; (1,1,1) for a gray map
                half  flake    = pow(saturate(flakeTex * ndh), _FlakeSharpness) * _FlakeStrength;

                // No env reflection here — the additive pass contributes only light-driven terms (gated by N·L).
                half3 rgb = diffuse
                          + _ClearcoatColor.rgb * specTint * (baseSpec + coat) * lc * ndl
                          + _FlakeColor.rgb * flakeHue * flake * (ndl * atten);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, rgb, fixed4(0,0,0,0));  // additive: fade to black
                return fixed4(rgb, 1.0);
            }
            ENDCG
        }

        // ---- ShadowCaster (Built-in): writes depth + casts shadows.
        // Without this, the body is absent from _CameraDepthTexture, so screen-space directional
        // shadow receiving samples the WRONG depth (the car's own ground shadow projected onto the body). ----
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual Cull Back
            CGPROGRAM
            #pragma vertex vertSC
            #pragma fragment fragSC
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            struct v2fSC { V2F_SHADOW_CASTER; };
            v2fSC vertSC (appdata_base v)
            {
                v2fSC o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }
            fixed4 fragSC (v2fSC i) : SV_Target { SHADOW_CASTER_FRAGMENT(i) }
            ENDCG
        }
    }

    Fallback Off
}
