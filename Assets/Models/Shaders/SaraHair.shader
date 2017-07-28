// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld

// Shader created with Shader Forge v1.26
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge
// Note: Manually altering this data may prevent you from opening it in Shader Forg
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:0,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:True,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:2865,x:32719,y:32712,varname:node_2865,prsc:2|diff-6343-OUT,spec-2213-OUT,gloss-1813-OUT,normal-1643-RGB,amspl-9144-OUT,alpha-7736-A;n:type:ShaderForge.SFN_Multiply,id:6343,x:32114,y:32712,varname:node_6343,prsc:2|A-7736-RGB,B-6665-RGB;n:type:ShaderForge.SFN_Color,id:6665,x:31921,y:32805,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5019608,c2:0.5019608,c3:0.5019608,c4:1;n:type:ShaderForge.SFN_Tex2d,id:7736,x:31921,y:32620,ptovrint:True,ptlb:Base Color,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Slider,id:358,x:32250,y:32780,ptovrint:False,ptlb:Metallic,ptin:_Metallic,varname:node_358,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:1813,x:32250,y:32882,ptovrint:False,ptlb:Roughness,ptin:_Roughness,varname:_Metallic_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8,max:1;n:type:ShaderForge.SFN_Tex2d,id:5188,x:31921,y:33004,ptovrint:False,ptlb:Occlusion,ptin:_Occlusion,varname:node_5188,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:9144,x:32370,y:33062,varname:node_9144,prsc:2|A-5188-RGB,B-3418-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3418,x:32095,y:33216,ptovrint:False,ptlb:Occlusion_Amount,ptin:_Occlusion_Amount,varname:node_3418,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Tex2d,id:1643,x:32283,y:33246,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:node_1643,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Color,id:8078,x:32308,y:32609,ptovrint:False,ptlb:Spec_Color,ptin:_Spec_Color,varname:node_8078,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.6413794,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:2213,x:32562,y:32549,varname:node_2213,prsc:2|A-8078-RGB,B-358-OUT;proporder:6665-7736-8078-358-1813-5188-3418-1643;pass:END;sub:END;*

Shader "Shader Forge/SaraHair" 
    Properties 
        _Color ("Color", Color) = (0.5019608,0.5019608,0.5019608,1
        _MainTex ("Base Color", 2D) = "white" {
        _Spec_Color ("Spec_Color", Color) = (1,0.6413794,0,1
        _Metallic ("Metallic", Range(0, 1)) = 
        _Roughness ("Roughness", Range(0, 1)) = 0.
        _Occlusion ("Occlusion", 2D) = "white" {
        _Occlusion_Amount ("Occlusion_Amount", Float ) = 
        _Normal ("Normal", 2D) = "bump" {
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.
    
    SubShader 
        Tags 
            "IgnoreProjector"="True
            "Queue"="Transparent
            "RenderType"="Transparent
        
        Pass 
            Name "FORWARD
            Tags 
                "LightMode"="ForwardBase
            
            Blend SrcAlpha OneMinusSrcAlph
            Cull Of
            ZWrite Of
           
            CGPROGRA
            #pragma vertex ver
            #pragma fragment fra
            #define UNITY_PASS_FORWARDBAS
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) 
            #define _GLOSSYENV 
            #include "UnityCG.cginc
            #include "Lighting.cginc
            #include "UnityPBSLighting.cginc
            #include "UnityStandardBRDF.cginc
            #pragma multi_compile_fwdbas
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_O
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARAT
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_O
            #pragma multi_compile_fo
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2
            #pragma target 3.
            uniform float4 _Color
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST
            uniform float _Metallic
            uniform float _Roughness
            uniform sampler2D _Occlusion; uniform float4 _Occlusion_ST
            uniform float _Occlusion_Amount
            uniform sampler2D _Normal; uniform float4 _Normal_ST
            uniform float4 _Spec_Color
            struct VertexInput 
                float4 vertex : POSITION
                float3 normal : NORMAL
                float4 tangent : TANGENT
                float2 texcoord0 : TEXCOORD0
                float2 texcoord1 : TEXCOORD1
                float2 texcoord2 : TEXCOORD2
            }
            struct VertexOutput 
                float4 pos : SV_POSITION
                float2 uv0 : TEXCOORD0
                float2 uv1 : TEXCOORD1
                float2 uv2 : TEXCOORD2
                float4 posWorld : TEXCOORD3
                float3 normalDir : TEXCOORD4
                float3 tangentDir : TEXCOORD5
                float3 bitangentDir : TEXCOORD6
                UNITY_FOG_COORDS(7
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH
                    float4 ambientOrLightmapUV : TEXCOORD8
                #endi
            }
            VertexOutput vert (VertexInput v) 
                VertexOutput o = (VertexOutput)0
                o.uv0 = v.texcoord0
                o.uv1 = v.texcoord1
                o.uv2 = v.texcoord2
                #ifdef LIGHTMAP_O
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw
                    o.ambientOrLightmapUV.zw = 0
                #elif UNITY_SHOULD_SAMPLE_S
                #endi
                #ifdef DYNAMICLIGHTMAP_O
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw
                #endi
                o.normalDir = UnityObjectToWorldNormal(v.normal)
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz )
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w)
                o.posWorld = mul(unity_ObjectToWorld, v.vertex)
                float3 lightColor = _LightColor0.rgb
                o.pos = UnityObjectToClipPos(v.vertex )
                UNITY_TRANSFER_FOG(o,o.pos)
                return o
            
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR 
                float isFrontFace = ( facing >= 0 ? 1 : 0 )
                float faceSign = ( facing >= 0 ? 1 : -1 )
                i.normalDir = normalize(i.normalDir)
                i.normalDir *= faceSign
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir)
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz)
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)))
                float3 normalLocal = _Normal_var.rgb
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normal
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection )
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz)
                float3 lightColor = _LightColor0.rgb
                float3 halfDirection = normalize(viewDirection+lightDirection)
////// Lighting
                float attenuation = 1
                float3 attenColor = attenuation * _LightColor0.xyz
                float Pi = 3.141592654
                float InvPi = 0.31830988618
///////// Gloss
                float gloss = _Roughness
                float specPow = exp2( gloss * 10.0+1.0)
/////// GI Data
                UnityLight light
                #ifdef LIGHTMAP_OF
                    light.color = lightColor
                    light.dir = lightDirection
                    light.ndotl = LambertTerm (normalDirection, light.dir)
                #els
                    light.color = half3(0.f, 0.f, 0.f)
                    light.ndotl = 0.0f
                    light.dir = half3(0.f, 0.f, 0.f)
                #endi
                UnityGIInput d
                d.light = light
                d.worldPos = i.posWorld.xyz
                d.worldViewDir = viewDirection
                d.atten = attenuation
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON
                    d.ambient = 0
                    d.lightmapUV = i.ambientOrLightmapUV
                #els
                    d.ambient = i.ambientOrLightmapUV
                #endi
                d.boxMax[0] = unity_SpecCube0_BoxMax
                d.boxMin[0] = unity_SpecCube0_BoxMin
                d.probePosition[0] = unity_SpecCube0_ProbePosition
                d.probeHDR[0] = unity_SpecCube0_HDR
                d.boxMax[1] = unity_SpecCube1_BoxMax
                d.boxMin[1] = unity_SpecCube1_BoxMin
                d.probePosition[1] = unity_SpecCube1_ProbePosition
                d.probeHDR[1] = unity_SpecCube1_HDR
                Unity_GlossyEnvironmentData ugls_en_data
                ugls_en_data.roughness = 1.0 - gloss
                ugls_en_data.reflUVW = viewReflectDirection
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data )
                lightDirection = gi.light.dir
                lightColor = gi.light.color
////// Specular
                float NdotL = max(0, dot( normalDirection, lightDirection ))
                float4 _Occlusion_var = tex2D(_Occlusion,TRANSFORM_TEX(i.uv0, _Occlusion))
                float3 specularColor = (_Spec_Color.rgb*_Metallic)
                float specularMonochrome = max( max(specularColor.r, specularColor.g), specularColor.b)
                float normTerm = (specPow + 8.0 ) / (8.0 * Pi)
                float3 directSpecular = 1 * pow(max(0,dot(halfDirection,normalDirection)),specPow)*normTerm*specularColor
                float3 indirectSpecular = (gi.indirect.specular + (_Occlusion_var.rgb*_Occlusion_Amount))*specularColor
                float3 specular = (directSpecular + indirectSpecular)
/////// Diffuse
                NdotL = max(0.0,dot( normalDirection, lightDirection ))
                float3 directDiffuse = max( 0.0, NdotL) * attenColor
                float3 indirectDiffuse = float3(0,0,0)
                indirectDiffuse += gi.indirect.diffuse
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex))
                float3 diffuseColor = (_MainTex_var.rgb*_Color.rgb)
                diffuseColor *= 1-specularMonochrome
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor
/// Final Color
                float3 finalColor = diffuse + specular
                fixed4 finalRGBA = fixed4(finalColor,_MainTex_var.a)
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA)
                return finalRGBA
            
            ENDC
        
        Pass 
            Name "FORWARD_DELTA
            Tags 
                "LightMode"="ForwardAdd
            
            Blend One On
            Cull Of
            ZWrite Of
           
            CGPROGRA
            #pragma vertex ver
            #pragma fragment fra
            #define UNITY_PASS_FORWARDAD
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) 
            #define _GLOSSYENV 
            #include "UnityCG.cginc
            #include "AutoLight.cginc
            #include "Lighting.cginc
            #include "UnityPBSLighting.cginc
            #include "UnityStandardBRDF.cginc
            #pragma multi_compile_fwdad
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_O
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARAT
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_O
            #pragma multi_compile_fo
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2
            #pragma target 3.
            uniform float4 _Color
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST
            uniform float _Metallic
            uniform float _Roughness
            uniform sampler2D _Normal; uniform float4 _Normal_ST
            uniform float4 _Spec_Color
            struct VertexInput 
                float4 vertex : POSITION
                float3 normal : NORMAL
                float4 tangent : TANGENT
                float2 texcoord0 : TEXCOORD0
                float2 texcoord1 : TEXCOORD1
                float2 texcoord2 : TEXCOORD2
            }
            struct VertexOutput 
                float4 pos : SV_POSITION
                float2 uv0 : TEXCOORD0
                float2 uv1 : TEXCOORD1
                float2 uv2 : TEXCOORD2
                float4 posWorld : TEXCOORD3
                float3 normalDir : TEXCOORD4
                float3 tangentDir : TEXCOORD5
                float3 bitangentDir : TEXCOORD6
                LIGHTING_COORDS(7,8
                UNITY_FOG_COORDS(9
            }
            VertexOutput vert (VertexInput v) 
                VertexOutput o = (VertexOutput)0
                o.uv0 = v.texcoord0
                o.uv1 = v.texcoord1
                o.uv2 = v.texcoord2
                o.normalDir = UnityObjectToWorldNormal(v.normal)
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz )
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w)
                o.posWorld = mul(unity_ObjectToWorld, v.vertex)
                float3 lightColor = _LightColor0.rgb
                o.pos = UnityObjectToClipPos(v.vertex )
                UNITY_TRANSFER_FOG(o,o.pos)
                TRANSFER_VERTEX_TO_FRAGMENT(o
                return o
            
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR 
                float isFrontFace = ( facing >= 0 ? 1 : 0 )
                float faceSign = ( facing >= 0 ? 1 : -1 )
                i.normalDir = normalize(i.normalDir)
                i.normalDir *= faceSign
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir)
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz)
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(i.uv0, _Normal)))
                float3 normalLocal = _Normal_var.rgb
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normal
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w))
                float3 lightColor = _LightColor0.rgb
                float3 halfDirection = normalize(viewDirection+lightDirection)
////// Lighting
                float attenuation = LIGHT_ATTENUATION(i)
                float3 attenColor = attenuation * _LightColor0.xyz
                float Pi = 3.141592654
                float InvPi = 0.31830988618
///////// Gloss
                float gloss = _Roughness
                float specPow = exp2( gloss * 10.0+1.0)
////// Specular
                float NdotL = max(0, dot( normalDirection, lightDirection ))
                float3 specularColor = (_Spec_Color.rgb*_Metallic)
                float specularMonochrome = max( max(specularColor.r, specularColor.g), specularColor.b)
                float normTerm = (specPow + 8.0 ) / (8.0 * Pi)
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*normTerm*specularColor
                float3 specular = directSpecular
/////// Diffuse
                NdotL = max(0.0,dot( normalDirection, lightDirection ))
                float3 directDiffuse = max( 0.0, NdotL) * attenColor
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex))
                float3 diffuseColor = (_MainTex_var.rgb*_Color.rgb)
                diffuseColor *= 1-specularMonochrome
                float3 diffuse = directDiffuse * diffuseColor
/// Final Color
                float3 finalColor = diffuse + specular
                fixed4 finalRGBA = fixed4(finalColor * _MainTex_var.a,0)
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA)
                return finalRGBA
            
            ENDC
        
        Pass 
            Name "Meta
            Tags 
                "LightMode"="Meta
            
            Cull Of
           
            CGPROGRA
            #pragma vertex ver
            #pragma fragment fra
            #define UNITY_PASS_META 
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) 
            #define _GLOSSYENV 
            #include "UnityCG.cginc
            #include "Lighting.cginc
            #include "UnityPBSLighting.cginc
            #include "UnityStandardBRDF.cginc
            #include "UnityMetaPass.cginc
            #pragma fragmentoption ARB_precision_hint_fastes
            #pragma multi_compile_shadowcaste
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_O
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARAT
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_O
            #pragma multi_compile_fo
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2
            #pragma target 3.
            uniform float4 _Color
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST
            uniform float _Metallic
            uniform float _Roughness
            uniform float4 _Spec_Color
            struct VertexInput 
                float4 vertex : POSITION
                float2 texcoord0 : TEXCOORD0
                float2 texcoord1 : TEXCOORD1
                float2 texcoord2 : TEXCOORD2
            }
            struct VertexOutput 
                float4 pos : SV_POSITION
                float2 uv0 : TEXCOORD0
                float2 uv1 : TEXCOORD1
                float2 uv2 : TEXCOORD2
                float4 posWorld : TEXCOORD3
            }
            VertexOutput vert (VertexInput v) 
                VertexOutput o = (VertexOutput)0
                o.uv0 = v.texcoord0
                o.uv1 = v.texcoord1
                o.uv2 = v.texcoord2
                o.posWorld = mul(unity_ObjectToWorld, v.vertex)
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST )
                return o
            
            float4 frag(VertexOutput i, float facing : VFACE) : SV_Target 
                float isFrontFace = ( facing >= 0 ? 1 : 0 )
                float faceSign = ( facing >= 0 ? 1 : -1 )
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz)
                UnityMetaInput o
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o )
               
                o.Emission = 0
               
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex))
                float3 diffColor = (_MainTex_var.rgb*_Color.rgb)
                float3 specColor = (_Spec_Color.rgb*_Metallic)
                float roughness = 1.0 - _Roughness
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5
               
                return UnityMetaFragment( o )
            
            ENDC
        
    
    FallBack "Diffuse
    CustomEditor "ShaderForgeMaterialInspector
}
