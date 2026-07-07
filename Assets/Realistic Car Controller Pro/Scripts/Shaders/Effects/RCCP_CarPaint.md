# RCCP Car paint (lit, metallic flake + clearcoat)

Shader name: `BoneCracker Games/RCCP/Effects/CarPaint` (menu: **BoneCracker Games ▸ RCCP ▸ Effects ▸ CarPaint**). **Lit** car paint with a physically-grounded *layered* reflection model: a **paint-tinted metallic** environment reflection plus an **untinted dielectric clearcoat** reflection (Schlick Fresnel, F0≈0.04 head-on → ~100% at grazing), fresnel flip-flop color, base metallic spec, a sharper clearcoat highlight, and metal-flake sparkle. Lit by the main directional light (with shadows), additional point/spot lights (`ForwardAdd`), the active reflection probe / skybox, and the **skybox-derived ambient (SH) probe** for diffuse. Fog-aware.

> **Pipeline scope — this file is BUILT-IN ONLY.** It contains no URP package includes, so it compiles cleanly in *any* project (with or without the URP package installed). The URP version is a separate shader, **`BoneCracker Games/RCCP/Effects/CarPaint_URP`**, delivered via the RCCP URP shader package and applied by the **Render Pipeline Converter** (which swaps body materials Built-in↔URP). This split exists specifically so a pure Built-in project never errors on a missing `Packages/com.unity.render-pipelines.universal/...` include. HDRP keeps the legacy `RCCP_Shader_Body_HDRP` for now (de-scoped). The Built-in and URP variants keep identical fragment math; only the light-fetch / reflection-probe / SH / fog API differ.

| Property | Type | Default | Notes |
|----------|------|---------|-------|
| `_BaseColor` | Color | deep red | Paint color seen head-on |
| `_FlipColor` | Color | violet | Color seen at grazing angle (flip-flop / chameleon paint) |
| `_FlipPower` | 0.5..8 | 3.0 | Fresnel falloff between base and flip |
| `_MainTex` | 2D | white | Optional albedo detail (dirt, livery mask) |
| `_Metallic` | 0..1 | 0.8 | Higher = more reflective, less diffuse |
| `_Smoothness` | 0..1 | 0.5 | Base coat highlight tightness |
| `_ClearcoatColor` | HDR Color | white | Clearcoat highlight color |
| `_ClearcoatSmoothness` | 0..1 | 0.9 | Clearcoat tightness; also drives reflection sharpness |
| `_ClearcoatStrength` | 0..4 | 1.5 | Clearcoat highlight intensity |
| `_FlakeMap` | 2D (R) | black | High-frequency sparkle noise. Generate with `/texture noise --type white` or `worley`. |
| `_FlakeColor` | HDR Color | white | Sparkle tint |
| `_FlakeTiling` | 1..200 | 60 | Flake density (UV multiplier) |
| `_FlakeSharpness` | 1..64 | 24 | Individual sparkle tightness |
| `_FlakeStrength` | 0..4 | 1.2 | Sparkle intensity |
| `_ReflectionStrength` | 0..2 | 1.0 | Environment reflection amount |

- **Lighting model:** main directional light (shadowed) + additional point/spot lights + reflection probe + ambient SH. URP uses `GetMainLight(shadowCoord)`, the Forward+ `LIGHT_LOOP_BEGIN/END` clustered additional-light loop, `SampleSH(N)` for ambient diffuse, and `GlossyEnvironmentReflection(reflDir, roughness, 1)` for reflection; Built-in uses `_WorldSpaceLightPos0`/`_LightColor0` + `SHADOW_ATTENUATION` (ForwardBase) and `ForwardAdd` for extra lights, `ShadeSH9(float4(N,1))` for ambient, and `DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, ...))` for reflection.
- **Layered reflection (the "reflects the skybox" behaviour):** two environment samples — the **base coat** is sampled at `1 - _Smoothness` and tinted by paint color (`paint * _Metallic`), the **clearcoat** is sampled at `1 - _ClearcoatSmoothness`, kept **untinted/white**, and weighted by a Schlick Fresnel term (`0.04 + 0.96·(1-N·V)⁴`). The clearcoat term is what makes the sky read on the body even head-on (~4%) and sweep across the silhouette at grazing angles — independent of how dark the paint is.
- **Ambient:** diffuse picks up the skybox-derived SH probe, so shadowed sides aren't crushed to black and the car integrates with scene lighting. Set Lighting → Environment → Ambient Source = **Skybox** (and Reflections = **Skybox** or a probe) for the strongest effect.
- **Shadows (Built-in):** the Built-in SubShader includes a `ShadowCaster` pass, so the body/wheels **cast** shadows AND write into `_CameraDepthTexture`. This is REQUIRED for correct screen-space directional shadow **receiving** — without it, the surface samples the screen-space shadow buffer at the wrong depth and the car's own ground shadow gets projected back onto the body as dark diagonal smears (the classic "shadow artifact on the paint"). Do not remove the `ShadowCaster` pass.
- **Shadows + rendering paths (URP):** the URP variant (`CarPaint_URP`, in the URP shader package) ships four passes — the lit pass tagged **`UniversalForwardOnly`**, plus **`ShadowCaster`**, **`DepthOnly`**, and **`DepthNormalsOnly`** — so the body casts shadows, writes the depth prepass, and participates in SSAO/decals. `UniversalForwardOnly` (not `UniversalForward`) is what makes the body render in **all four URP rendering paths: Forward, Forward+, Deferred, and Deferred+** — the deferred renderers skip `UniversalForward`-tagged passes (they expect a GBuffer pass), and clearcoat layering can't be encoded in URP's GBuffer, so the shader renders forward-only under deferred exactly like URP's own ComplexLit. Verified pixel-level across all four paths (Unity 6000.3 / URP 17.3). Authoring note: the `ShadowCaster` pass needs `CommonMaterial.hlsl` included before `Shadows.hlsl` (`LerpWhiteTo` is used but not included by `Shadows.hlsl`), and URP-include passes must be authored/validated in a URP project — the shader stays package-only so Built-in projects never import it.
- **Reflection probe** — put a Reflection Probe near the car for accurate local reflections; otherwise it falls back to the skybox/ambient probe.
- **Flakes:** driven by a noise texture × `N·H`, so they vary across the surface and twinkle with view/light angle without needing tangents. Assign a fine white/worley noise to `_FlakeMap`. At high `_FlakeTiling` on small/distant triangles flakes can temporally alias (crawl); lower the tiling or the strength if it's distracting on moving vehicles.
- **SRP Batcher safe:** every per-material float/color (including the `[Toggle]` backing floats `_UseNormalMap`/`_UseSpecGloss`) lives inside `CBUFFER_START(UnityPerMaterial)` in the URP pass; textures and their samplers are declared outside the CBUFFER. The Built-in SubShader uses plain globals — the Built-in pipeline has no SRP Batcher, so that is expected and not a defect.

### Pipeline scope
RCC PRO ships Built-in + URP — both SubShaders here are first-class. No HDRP SubShader (full HDRP lit needs ShaderGraph).

### Upgrade: tangent-space flake normals
For flakes that catch light individually (true sparkle), sample a flake **normal** map in tangent space and perturb `N` per-flake before the spec dot. That needs tangent/bitangent in the vertex struct (`float4 tangentOS : TANGENT` → build TBN). Adds realism at the cost of the tangent plumbing this version deliberately avoids.
