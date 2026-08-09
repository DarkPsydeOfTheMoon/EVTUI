#version 330 core

//#define FLAG0_HDR (1)
//#define FLAG0_LIGHT0_DIRECTION (1)
//#define FLAG0_CONSTANTCOLOR (1)
//#define FLAG0_TEXCOORD0IN (1)
//#define FLAG0_TEXCOORD0OUT (1)
//#define FLAG0_DEFERRED (1)
//#define FLAG0_OUTLINE (1)
//#define FLAG1_MATERIAL_AMBDIFF (1)
//#define FLAG1_MATERIAL_FOG (1)
//#define FLAG1_MATERIAL_REFLECTION (1)
//#define FLAG1_MATERIAL_REFLECTION_LERP (1)
//#define FLAG1_MATERIAL_LIGHT (1)
//#define FLAG1_MATERIAL_HEIGHTFOG (1)
//#define FLAG1_EDGE_REFERENCE_LIGHTALPHA (1)
//#define FLAG1_LIGHTMAP_MODULATE2 (1)
//#define FLAG1_TEXTURE1 (1)
//#define FLAG1_TEXTURE2 (1)
//#define FLAG1_TEXTURE3 (1)
//#define FLAG1_TEXTURE4 (1)
//#define FLAG1_TEXTURE9 (1)
//#define FLAG2_FAKE_REFLECTION (1)
//#define FLAG2_EDGE_REFERENCE_NORMALMAP (1)
//#define FLAG2_TOON_REFERENCE_NORMALMAP (1)

in vec4 position;
#if ((FLAG1_MATERIAL_LIGHT && (!FLAG1_TEXTURE2)) || (!FLAG2_EDGE_REFERENCE_NORMALMAP) || (!FLAG2_TOON_REFERENCE_NORMALMAP) || FLAG1_MATERIAL_REFLECTION || FLAG0_OUTLINE || FLAG1_MATERIAL_HEIGHTFOG)
	#if (FLAG1_MATERIAL_HEIGHTFOG)
in vec4 normal;
	#else
in vec3 normal;
	#endif
#endif

#if (FLAG1_MATERIAL_LIGHT)
	#if (FLAG0_LIGHT0_DIRECTION || FLAG0_LIGHT0_POINT || FLAG0_LIGHT0_SPOT)
in vec4 light0Vec;
		#if (FLAG1_TEXTURE2)
in vec4 light0TangentDir;
		#endif
	#endif
	#if (FLAG0_LIGHT1_DIRECTION || FLAG0_LIGHT1_POINT || FLAG0_LIGHT1_SPOT)
in vec4 light1Vec;
	#endif
	#if (FLAG0_LIGHT2_DIRECTION || FLAG0_LIGHT2_POINT || FLAG0_LIGHT2_SPOT)
in vec4 light2Vec;
	#endif
#endif

#if (FLAG1_TEXTURE2)
in vec3 viewTangentDir;
	#if (!FLAG2_EDGE_REFERENCE_NORMALMAP)
in vec3 inviewDir;
	#endif
#else
in vec3 inviewDir;
#endif

#if (!FLAG2_REFLECTION_CASTER)
	#if (FLAG1_MATERIAL_SHADOW)
		#if (FLAG2_CSM)
in vec4 lightcoord[3];
		#else
in vec4 lightcoord;
		#endif
	#endif
#else
in vec4 worldPos;
#endif

#if (FLAG1_MATERIAL_VERTEXCOLOR || FLAG0_CONSTANTCOLOR)
in vec4 color;
#endif

#if (FLAG0_TEXCOORD0OUT)
in vec2 texcoord0;
#endif

#if (FLAG0_TEXCOORD1OUT)
in vec2 texcoord1;
#endif

#if (FLAG0_TEXCOORD2OUT || FLAG2_FAKE_REFLECTION)
in vec2 texcoord2;
#endif

#if (FLAG1_MATERIAL_FOG)
in vec4 misc;
#endif

out vec4 oColor;

/*---------------------------------------------------------------------------*/

layout (std140) uniform GFD_PSCONST_SYSTEM {
	vec4 clearColor;		// 画面クリアカラー
	vec2 resolution;		// 画面解像度
	vec2 resolutionRev;	// 解像度逆数
};

layout (std140) uniform GFD_PSCONST_MATERIAL {
	vec4 matAmbient;
	vec4 matDiffuse;
	vec4 matSpecular;
	vec4 matEmissive;
	float matReflectivity;
	float matOutlineIndex;
	float shadowDisable;
	float fogDisable;
};

layout (std140) uniform GFD_PSCONST_LIGHT0_PS {
	vec4 light0Ambient;
	vec4 light0Diffuse;
	vec4 light0Specular;
	vec3 light0Attenuation;
	vec2 light0Spot;
	vec2 _reserved_b2;
};

layout (std140) uniform GFD_PSCONST_LIGHT1_PS {
	vec4 light1Ambient;
	vec4 light1Diffuse;
	vec4 light1Specular;
	vec3 light1Attenuation;
	vec2 light1Spot;
	vec2 _reserved_b3;
};

layout (std140) uniform GFD_PSCONST_LIGHT2_PS {
	vec4 light2Ambient;
	vec4 light2Diffuse;
	vec4 light2Specular;
	vec3 light2Attenuation;
	vec2 light2Spot;
	vec2 _reserved_b4;
};

layout (std140) uniform GFD_PSCONST_ENV_COLORS {
	vec4 fogColor;
	vec4 heightFogColor;
	vec3 lmapAmbient;
	float atestRef;
};

#if (FLAG2_PCF)
layout (std140) uniform GFD_PSCONST_SHADOW {
	float dimmerDif;
	float dimmerAmb;
	float depthBias;
	float shiftPCF;
	vec4 shadowColor;
	vec4 csmDebugColor[3];
};
#endif

#if (FLAG2_VSM)
layout (std140) uniform GFD_PSCONST_SHADOW {
	vec4 vsm;
};
#endif

layout (std140) uniform GFD_PSCONST_TOON {
	vec4 toonLightColor;
	float toonLightThreshold;
	float toonLightFactor;
	float toonShadowBrightness;
	float toonShadowThreshold;
	float toonShadowFactor;
};

/*--------------------------------------------------
 * テクスチャ定義
--------------------------------------------------*/
#if (FLAG1_TEXTURE1)
uniform sampler2D diffuseTexture;
#endif

#if (FLAG1_TEXTURE2)
uniform sampler2D normalTexture;
#endif

#if (FLAG1_TEXTURE3)
uniform sampler2D specularTexture;
#endif

#if (FLAG1_TEXTURE4 && FLAG1_MATERIAL_REFLECTION)
	#if (FLAG2_FAKE_REFLECTION)
uniform sampler2D reflectionTexture;
	#else
uniform sampler3D reflectionTexture;
	#endif
#endif

#if (FLAG1_TEXTURE5)
uniform sampler2D multipleTexture;
#endif

#if (FLAG1_TEXTURE6)
uniform sampler2D glowTexture;
#endif

#if (FLAG1_TEXTURE7)
uniform sampler2D darkTexture;
#endif

#if (FLAG1_TEXTURE8)
uniform sampler2D detailTexture;
#endif

#if (FLAG1_TEXTURE9)
uniform sampler2D lightTexture;
#endif

#if (FLAG1_MATERIAL_SHADOW && (!FLAG2_REFLECTION_CASTER))
uniform sampler2DArrayShadow shadowSampler;
	#if (FLAG2_CSM)
uniform sampler2D shadowTexture0;
uniform sampler2D shadowTexture1;
uniform sampler2D shadowTexture2;	
	#else
uniform sampler2D shadowTexture;
#endif

#endif

#define gfdDepthCmp( _texture, _sampler, _uv, _depth ) ( (_texture).SampleCmp( _sampler, _uv, _depth ) )

#define gfdDepthSampleCmp( _output, _texture, _sampler, _uv, _depth ) {(_output) = (_texture).SampleCmp( _sampler, _uv, _depth );}

#define gfdDepthGatherCmp( _output, _texture, _sampler, _uv, _depth ) {vec4 s4 = ( (_texture).GatherCmp( _sampler, _uv, _depth ) ); (_output) = (s4.x + s4.y + s4.z + s4.w) * (1.0f / 4.0f);}

#if (FLAG0_DEFERRED)
uniform sampler2D lbufferTexture;
#endif

/*---------------------------------------------------------------------------*/
#define TOONLIGHT_SHADOW_MIN (0.4)

/*---------------------------------------------------------------------------*/

float saturate(float inFloat) {
	return clamp(inFloat, 0.0, 1.0);
}

void main() {
	#if (FLAG1_TEXTURE1)
		#if (CHANNEL_TEX1 == 0)
	oColor = texture(diffuseTexture, texcoord0.xy);
		#elif (CHANNEL_TEX1 == 1)
	oColor = texture(diffuseTexture, texcoord1.xy);
		#elif (CHANNEL_TEX1 == 2)
	oColor = texture(diffuseTexture, texcoord2.xy);
		#endif
	#else
	oColor = vec4(1, 1, 1, 1); 
	#endif

	#if (FLAG1_MATERIAL_AMBDIFF)
	oColor.a *= matDiffuse.a;
	#endif

	#if (!(FLAG1_MATERIAL_VERTEXCOLOR) && !(FLAG0_CONSTANTCOLOR))
		#if (FLAG2_ATEST_NEVER)
	clip(-1);
		#elif (FLAG2_ATEST_LESS_LEQUAL)
	clip(atestRef - oColor.a);
		#elif (FLAG2_ATEST_EQUAL)
	clip(-(oColor.a != atestRef));
		#elif (FLAG2_ATEST_GREATER_GEQUAL)
	clip(oColor.a - atestRef);
		#elif (FLAG2_ATEST_NOTEQUAL)
	clip(-(oColor.a == atestRef));
		#endif
	#endif

	#if (FLAG1_TEXTURE5)
		#if (CHANNEL_TEX5 == 0)
	vec4 multipleCol = texture(multipleTexture, texcoord0.xy);
		#elif (CHANNEL_TEX5 == 1)
	vec4 multipleCol = texture(multipleTexture, texcoord1.xy);
		#elif (CHANNEL_TEX5 == 2)
	vec4 multipleCol = texture(multipleTexture, texcoord2.xy);
		#endif

		#if (FLAG2_MATERIAL_MULTIPLE_SEMI)
	oColor.rgb = mix(oColor.rgb, multipleCol.rgb, multipleCol.a * matAmbient.a);
		#elif (FLAG2_MATERIAL_MULTIPLE_ADD)
	oColor.rgb += multipleCol.rgb * multipleCol.a * matAmbient.a;
		#elif (FLAG2_MATERIAL_MULTIPLE_SUB)
	oColor.rgb -= multipleCol.rgb * multipleCol.a * matAmbient.a;
		#elif (FLAG3_MATERIAL_MULTIPLE_MODULATE)
	oColor.rgb *= multipleCol.rgb * multipleCol.a * matAmbient.a;
		#endif
	#endif

	#if (!FLAG2_SPECULAR_NORMALMAPALPHA)
		#if (FLAG1_TEXTURE3)
			#if (CHANNEL_TEX3 == 0)
	vec3 glossCol = texture(specularTexture, texcoord0.xy).rgb;
			#elif (CHANNEL_TEX3 == 1)
	vec3 glossCol = texture(specularTexture, texcoord1.xy).rgb;
			#elif (CHANNEL_TEX3 == 2)
	vec3 glossCol = texture(specularTexture, texcoord2.xy).rgb;
			#endif
		#endif
	#else
	float glossCol;
	#endif

	#if (FLAG1_TEXTURE6)
		#if (CHANNEL_TEX6 == 0)
	vec3 glowCol = texture(glowTexture, texcoord0.xy).rgb;
		#elif (CHANNEL_TEX6 == 1)
	vec3 glowCol = texture(glowTexture, texcoord1.xy).rgb;
		#elif (CHANNEL_TEX6 == 2)
	vec3 glowCol = texture(glowTexture, texcoord2.xy).rgb;
		#endif
	#endif

	#if (FLAG1_TEXTURE7)
		#if (CHANNEL_TEX7 == 0)
	vec3 darkCol = texture(darkTexture, texcoord0.xy).rgb;
		#elif (CHANNEL_TEX7 == 1)
	vec3 darkCol = texture(darkTexture, texcoord1.xy).rgb;
		#elif (CHANNEL_TEX7 == 2)
	vec3 darkCol = texture(darkTexture, texcoord2.xy).rgb;
		#endif
	#endif

	#if (FLAG1_TEXTURE8)
		#if (CHANNEL_TEX8 == 0)
	vec3 detailCol = texture(detailTexture, texcoord0.xy).rgb;
		#elif (CHANNEL_TEX8 == 1)
	vec3 detailCol = texture(detailTexture, texcoord1.xy).rgb;
		#elif (CHANNEL_TEX8 == 2)
	vec3 detailCol = texture(detailTexture, texcoord2.xy).rgb;
		#endif
	detailCol *= 2.0;
	#endif

	#if (FLAG1_TEXTURE9)
		#if (CHANNEL_TEX9 == 0)
	vec4 lightCol = texture(lightTexture, texcoord0.xy);
		#elif (CHANNEL_TEX9 == 1)
	vec4 lightCol = texture(lightTexture, texcoord1.xy);
		#elif (CHANNEL_TEX9 == 2)
	vec4 lightCol = texture(lightTexture, texcoord2.xy);
		#endif
	#endif

	#if (FLAG1_TEXTURE2)
	vec4 normalCol;
	vec3 normalVec;
		#if (CHANNEL_TEX2 == 0)
	normalCol = texture(normalTexture, texcoord0.xy);
		#elif (CHANNEL_TEX2 == 1)
	normalCol = texture(normalTexture, texcoord1.xy);
		#elif (CHANNEL_TEX2 == 2)
	normalCol = texture(normalTexture, texcoord2.xy);
		#endif
	normalVec = normalize(2.0 * normalCol.xyz - 1.0);
		#if (FLAG2_SPECULAR_NORMALMAPALPHA)
	glossCol = normalCol.a;
		#endif
	#endif

	#if (FLAG1_MATERIAL_HEIGHTFOG)
	float hfAlpha = saturate(normal.w * heightFogColor.a * (1.0 - fogDisable));
	#endif

	#if (FLAG1_MATERIAL_LIGHT)
	vec3 L;
	vec3 N;
	float NL;
	float D = 0.0;
	float E;
	float NVW = 0.0;
	vec3 lightAmbient = vec3(0.0);
	vec3 lightDiffuse = vec3(0.0);
	vec3 toonLightRGB = toonLightColor.rgb;

		#if (FLAG1_TEXTURE2)
	vec3 viewDir = normalize(viewTangentDir);
		#else
	vec3 viewDir = normalize(inviewDir);
		#endif

		#if (FLAG1_TEXTURE2)
	N = normalVec;
		#else
	N = normalize(normal.xyz);
		#endif
		#if (FLAG0_DEFERRED)
	vec4 lbuffCol;
	vec2 bufcoord;
	bufcoord = position.xy * resolutionRev.xy;
	lbuffCol = texture(lbufferTexture, bufcoord.xy);
	toonLightRGB = mix(toonLightRGB, lbuffCol.rgb, min(max(lbuffCol.r, max(lbuffCol.g, lbuffCol.b)) * 2.0, 1.0));
		#endif

		#if (FLAG1_MATERIAL_SPECULAR)
			#if (!(FLAG0_DEFERRED))
	vec3 lightSpecular = vec3(0.0);
			#else
				#if (FLAG0_LIGHT0_DIRECTION || FLAG0_LIGHT0_POINT || FLAG0_LIGHT0_SPOT)
	vec3 lightSpecular = light0Specular.rgb * lbuffCol.w;
				#else
	vec3 lightSpecular = vec3(lbuffCol.w);
				#endif
			#endif
	float P = matSpecular.w;
	vec3 H;
		#endif

		/*#if (FLAG0_OUTLINE)
			vec3 outlineN;
			#if (FLAG1_TEXTURE2)
				outlineN = normalize(normal.xyz);
			#else
				outlineN = N;
			#endif
			#if (FLAG1_MATERIAL_HEIGHTFOG)
				result.outline.rgb = 0.5f * outlineN * (1.f - hfAlpha) + 0.5f;
			#else
				result.outline.rgb = 0.5f * outlineN + 0.5f;
			#endif
			result.outline.a = matOutlineIndex;
		#endif*/

		#if (FLAG0_LIGHT0_DIRECTION)
			#if (FLAG1_TEXTURE2)
	L = normalize(light0TangentDir.xyz);
			#else
	L = normalize(light0Vec.xyz);
			#endif

	E = dot(L, N);
			#if (FLAG2_EDGE_REFERENCE_NORMALMAP)
	NVW = saturate(dot(N, mix(viewDir, N, -min(E, 0.0))));
			#else
				#if (FLAG1_TEXTURE2)
	vec3 refDir = normalize(inviewDir);
	vec3 refN = normalize(normal.xyz);
	float refE = dot(normalize(light0Vec.xyz), refN);
	NVW = saturate(dot(refN, mix(refDir, refN, -min(refE, 0.0))));
				#else
	NVW = saturate(dot(N, mix(viewDir, N, -min(E, 0.0))));
				#endif
			#endif

			#if (FLAG2_TOON_REFERENCE_NORMALMAP)
	NL = saturate(E);
	D = saturate(max(NL - pow(toonShadowThreshold, 1.8), 0.0) * toonShadowFactor);
			#else
				#if (FLAG1_TEXTURE2)
					#if (FLAG2_EDGE_REFERENCE_NORMALMAP)
	float refE = dot(normalize(light0Vec.xyz), normalize(normal.xyz));
					#endif
	NL = saturate(refE);
	D = saturate(max(NL - pow(toonShadowThreshold, 1.8), 0.0) * toonShadowFactor);
				#else
	NL = saturate(E);
	D = saturate(max(NL - pow(toonShadowThreshold, 1.8), 0.0) * toonShadowFactor);
				#endif
			#endif

	lightAmbient += light0Ambient.rgb;
	lightDiffuse += light0Diffuse.rgb;
			#if (FLAG1_MATERIAL_SPECULAR)
	H = normalize(L + viewDir);
	lightSpecular += light0Specular.rgb * pow(max(dot(H, N), 0.0), P);
			#endif

		#elif (FLAG0_LIGHT0_POINT)
	float K;
	float A;
	K = length(light0Vec.xyz);
	A = 1.0 / (light0Attenuation.x + light0Attenuation.y * K + light0Attenuation.z * K * K);
			#if (FLAG1_TEXTURE2)
	L = normalize(light0TangentDir.xyz);
			#else
	L = normalize(light0Vec.xyz);
			#endif

	NL = saturate(dot(L, N));
	D = saturate(max(NL - pow(toonShadowThreshold, 1.8), 0.0) * toonShadowFactor) * A;

	lightAmbient += light0Ambient.rgb * A;
	lightDiffuse += light0Diffuse.rgb;
			#if (FLAG1_MATERIAL_SPECULAR)
	H = normalize(L + viewDir);
	lightSpecular += light0Specular.rgb * pow(max(dot(H, N), 0.0), P) * A;
			#endif
		#elif (FLAG0_LIGHT0_SPOT)
			#if (FLAG1_TEXTURE2)
	L = normalize(light0TangentDir.xyz);
			#else
	L = normalize(light0Vec.xyz);
			#endif

	NL = saturate(dot(L, N));
	D = saturate(max(NL - pow(toonShadowThreshold, 1.8), 0.0) * toonShadowFactor);

	lightAmbient += light0Ambient.rgb;
	lightDiffuse += light0Diffuse.rgb;
			#if (FLAG1_MATERIAL_SPECULAR)
	H = normalize(L + viewDir);
	lightSpecular += light0Specular.rgb * pow(max(dot(H, N), 0.0), P);
			#endif
		#endif

		#if (FLAG0_LIGHT1_DIRECTION)
	L = normalize(light1Vec.xyz);
	D = saturate(dot(L, N));
	lightAmbient += light1Ambient.rgb;
	lightDiffuse += light1Diffuse.rgb * D;
			#if (FLAG1_MATERIAL_SPECULAR)
	H = normalize(L + viewDir);
	lightSpecular += light1Specular.rgb * pow(max(dot(H, N), 0.0), P);
			#endif
		#elif (FLAG0_LIGHT1_POINT)
	float K;
	float A;
	K = length(light1Vec.xyz);
	A = 1.0 / (light1Attenuation.x + light1Attenuation.y * K + light1Attenuation.z * K * K);
	L = normalize(light1Vec.xyz);
	D = saturate(dot(L, N)) * D;
	lightAmbient += light1Ambient.rgb * A;
	lightDiffuse += light1Diffuse.rgb * D;
			#if (FLAG1_MATERIAL_SPECULAR)
	H = normalize(L + viewDir);
	lightSpecular += light1Specular.rgb * pow(max(dot(H, N), 0.0), P) * A;
			#endif
		#elif (FLAG0_LIGHT1_SPOT)
	L = normalize(light1Vec.xyz);
	D = saturate(dot(L, N));
	lightAmbient += light1Ambient.rgb;
	lightDiffuse += light1Diffuse.rgb * D;
			#if (FLAG1_MATERIAL_SPECULAR)
	H = normalize(L + viewDir);
	lightSpecular += light1Specular.rgb * pow(max(dot(H, N), 0.0), P);
			#endif
		#endif

		#if (FLAG0_LIGHT2_DIRECTION)
	L = normalize(light2Vec.xyz);
	D = saturate(dot(L, N));
	lightAmbient += light2Ambient.rgb;
	lightDiffuse += light2Diffuse.rgb * D;
			#if (FLAG1_MATERIAL_SPECULAR)
	H = normalize(L + viewDir);
	lightSpecular += light2Specular.rgb * pow(max(dot(H, N), 0.0), P);
			#endif
		#elif (FLAG0_LIGHT2_POINT)
	float K;
	float A;
	K = length(light2Vec.xyz);
	A = 1.0 / (light2Attenuation.x + light2Attenuation.y * K + light2Attenuation.z * K * K);
	L = normalize(light2Vec.xyz);
	D = saturate(dot(L, N)) * D;
	lightAmbient += light2Ambient.rgb * A;
	lightDiffuse += light2Diffuse.rgb * D;
			#if (FLAG1_MATERIAL_SPECULAR)
	H = normalize(L + viewDir);
	lightSpecular += light2Specular.rgb * pow(max(dot(H, N), 0.0), P) * A;
			#endif
		#elif (FLAG0_LIGHT2_SPOT)
	L = normalize(light2Vec.xyz);
	D = saturate(dot(L, N));
	lightAmbient += light2Ambient.rgb;
	lightDiffuse += light2Diffuse.rgb * D;
			#if (FLAG1_MATERIAL_SPECULAR)
	H = normalize(L + viewDir);
	lightSpecular += light2Specular.rgb * pow(max(dot(H, N), 0.0), P);
			#endif
		#endif

		#if (FLAG1_TEXTURE9)
			#if (FLAG2_LIGHTMAP_MODULATE)
			#elif (FLAG1_LIGHTMAP_MODULATE2)
			#else
	lightDiffuse += lightCol.rgb;
			#endif
		#endif

		#if ((FLAG1_MATERIAL_SHADOW) && (!FLAG2_REFLECTION_CASTER))
			/*#if (FLAG2_PCF)
				float  shadow;
				#if (FLAG2_CSM)
					float  w2     = shiftPCF * 2.0f;
					vec3 tx     = lightcoord[2].xyz / lightcoord[2].w;
					vec2 border = saturate( clamp(tx.xy, w2, 1.0f - w2) ) - tx.xy;
					#ifndef NEEDLE_SHADER
						if( any(bit_cast<bool2>(border)) ) {
					#else
						if( (abs(border.x) + abs(border.y)) > CSM_EPS ) {
					#endif

						tx     = fragment.lightcoord[1].xyz / fragment.lightcoord[1].w;
						border = saturate( clamp(tx.xy, w2, 1.0f - w2) ) - tx.xy;
						#ifndef NEEDLE_SHADER
							if( any(bit_cast<bool2>(border)) ) {
						#else
							if( (abs(border.x) + abs(border.y)) > CSM_EPS ) {
						#endif

							//---------- CSM遠景用処理[0] ----------

							tx = fragment.lightcoord[0].xyz / fragment.lightcoord[0].w;

							gfdDepthSampleCmp( shadow, shadowTexture0, shadowSampler, tx.xy, tx.z + depthBias );

							oColor.rgb *= csmDebugColor[0].rgb;		// 青

						} else {

							//---------- CSM中景用処理[1] ----------

							tx = fragment.lightcoord[1].xyz / fragment.lightcoord[1].w;

							gfdDepthGatherCmp( shadow, shadowTexture1, shadowSampler, tx.xy, tx.z + depthBias );

							oColor.rgb *= csmDebugColor[1].rgb;		// 緑
						}

					} else {

						//---------- CSM近景用処理[2] ----------

						tx.z += depthBias;
						#if (FLAG2_PCF_4x4)
							float shade;
							shade   = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(-shiftPCF,-shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(     0.0f,-shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF,-shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(-shiftPCF,     0.0f), tx.z );
							shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(     0.0f,     0.0f), tx.z );
							shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF,     0.0f), tx.z );
							shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(-shiftPCF, shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(     0.0f, shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF, shiftPCF), tx.z );
							shadow  = shade * (1.0f / 9.0f);
						#elif (FLAG2_PCF_3x3)
							gfdDepthGatherCmp( shadow, shadowTexture2, shadowSampler, tx.xy, tx.z );
							//	vec4 shade;
							//	shade.x = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy, tx.z );
							//	shade.y = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF,     0.0f), tx.z );
							//	shade.z = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(     0.0f, shiftPCF), tx.z );
							//	shade.w = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF, shiftPCF), tx.z );
							//	shadow  = (shade.x + shade.y + shade.z + shade.w) * (1.0f / 4.0f);
						#else
							gfdDepthSampleCmp( shadow, shadowTexture2, shadowSampler, tx.xy, tx.z );
						#endif

						oColor.rgb *= csmDebugColor[2].rgb;	// 赤
					}

				#else	// ------------------------------------ FLAG2_CSM ------------------------------------

					vec3 tx = fragment.lightcoord.xyz / fragment.lightcoord.w;
					tx.z += depthBias;
					#if (FLAG2_PCF_4x4)
						float shade;
						shade   = gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(-shiftPCF,-shiftPCF), tx.z );
						shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(     0.0f,-shiftPCF), tx.z );
						shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2( shiftPCF,-shiftPCF), tx.z );
						shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(-shiftPCF,     0.0f), tx.z );
						shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(     0.0f,     0.0f), tx.z );
						shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2( shiftPCF,     0.0f), tx.z );
						shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(-shiftPCF, shiftPCF), tx.z );
						shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(     0.0f, shiftPCF), tx.z );
						shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2( shiftPCF, shiftPCF), tx.z );
						shadow  = shade * (1.0f / 9.0f);
					#elif (FLAG2_PCF_3x3)
						gfdDepthGatherCmp( shadow, shadowTexture, shadowSampler, tx.xy, tx.z );
					#else
						gfdDepthSampleCmp( shadow, shadowTexture, shadowSampler, tx.xy, tx.z );
					#endif

				#endif	// ------------------------------------ FLAG2_CSM ------------------------------------

	            #ifdef NEEDLE_SHADER
				vec3 lightCoord = 0.f;
				// 簡単のために前の処理と重複するが、コンパイラの最適化に期待.
				#if FLAG2_CSM
					lightCoord = fragment.lightcoord[0].xyz / fragment.lightcoord[0].w;
				#else
					lightCoord = fragment.lightcoord.xyz / fragment.lightcoord.w;
				#endif
				vec2 _border = saturate(lightCoord.xy) - lightCoord.xy;
				if( (abs(_border.x) + abs(_border.y)) > CSM_EPS ) {
					shadow = 1.f;
				}
				#endif

				float shadowDif = min( shadow + dimmerDif + shadowDisable, 1.f);

				D *= shadow;
				#if (FLAG1_MATERIAL_SPECULAR)
					lightSpecular *= shadowDif;
				#endif
				toonLightRGB *= max( shadowDif, TOONLIGHT_SHADOW_MIN);
			#else
			#endif*/
		#endif

		#if (FLAG1_MATERIAL_AMBDIFF)
			#if (FLAG1_TEXTURE9 && FLAG2_LIGHTMAP_MODULATE)
	float lightMapMod;
	lightMapMod = 1.0 - lightCol.r;
				#if (FLAG1_MATERIAL_EMISSIVE)
	oColor.rgb *= matEmissive.rgb + mix(matAmbient.rgb * lightAmbient, lightAmbient + matDiffuse.rgb * lightDiffuse, saturate(toonShadowBrightness + D + lightMapMod));
				#else
	oColor.rgb *= mix(matAmbient.rgb * lightAmbient, lightAmbient + matDiffuse.rgb * lightDiffuse, saturate(toonShadowBrightness + D + lightMapMod));
				#endif
			#elif (FLAG1_TEXTURE9 && FLAG1_LIGHTMAP_MODULATE2)
	float lightMapMod;
	lightMapMod = 1.0 - lightCol.r;
				#if (FLAG1_MATERIAL_EMISSIVE)
	oColor.rgb *= matEmissive.rgb + mix(matAmbient.rgb * lightAmbient, lightAmbient + matDiffuse.rgb * lightDiffuse, saturate(toonShadowBrightness + D * lightMapMod));
				#else
	oColor.rgb *= mix(matAmbient.rgb * lightAmbient, lightAmbient + matDiffuse.rgb * lightDiffuse, saturate(toonShadowBrightness + D * lightMapMod));
				#endif
			#else
				#if (FLAG1_MATERIAL_EMISSIVE)
	oColor.rgb *= matEmissive.rgb + mix(matAmbient.rgb * lightAmbient, lightAmbient + matDiffuse.rgb * lightDiffuse, saturate(toonShadowBrightness + D));
				#else
	oColor.rgb *= mix(matAmbient.rgb * lightAmbient, lightAmbient + matDiffuse.rgb * lightDiffuse, saturate(toonShadowBrightness + D));
				#endif
			#endif
		#endif

		#if (FLAG1_MATERIAL_SPECULAR)
			#if (FLAG1_TEXTURE3 || FLAG2_SPECULAR_NORMALMAPALPHA)
	oColor.rgb += matSpecular.rgb * lightSpecular * glossCol;
			#else
	oColor.rgb += matSpecular.rgb * lightSpecular;
			#endif
		#endif

	#else

		/*#if (FLAG0_OUTLINE)
			result.outline.rgb = 0;
			result.outline.a   = matOutlineIndex;
		#endif*/

		#if (FLAG2_SPECULAR_NORMALMAPALPHA)
	glossCol = 0;
		#endif

		#if (FLAG1_MATERIAL_AMBDIFF)
			#if ((FLAG1_MATERIAL_SHADOW) && (!FLAG2_REFLECTION_CASTER))
				/*#if (FLAG2_PCF)
					float  shadow;

					#if (FLAG2_CSM)
						// ------------------------------------ FLAG2_CSM ------------------------------------
						float  w2     = shiftPCF * 2.0f;
						vec3 tx     = fragment.lightcoord[2].xyz / fragment.lightcoord[2].w;
						vec2 border = saturate( clamp(tx.xy, w2, 1.0f - w2) ) - tx.xy;
						#ifndef NEEDLE_SHADER
							if( any(bit_cast<bool2>(border)) ) {
						#else
							if( (abs(border.x) + abs(border.y)) > CSM_EPS ) {
						#endif

							tx     = fragment.lightcoord[1].xyz / fragment.lightcoord[1].w;
							border = saturate( clamp(tx.xy, w2, 1.0f - w2) ) - tx.xy;
							#ifndef NEEDLE_SHADER
								if( any(bit_cast<bool2>(border)) ) {
							#else
								if( (abs(border.x) + abs(border.y)) > CSM_EPS ) {
							#endif

								//---------- CSM遠景用処理[0] ----------

								tx = fragment.lightcoord[0].xyz / fragment.lightcoord[0].w;

								gfdDepthSampleCmp( shadow, shadowTexture0, shadowSampler, tx.xy, tx.z + depthBias );

								oColor.rgb *= csmDebugColor[0].rgb;		// 青

							} else {

								//---------- CSM中景用処理[1] ----------

								tx = fragment.lightcoord[1].xyz / fragment.lightcoord[1].w;

								gfdDepthGatherCmp( shadow, shadowTexture1, shadowSampler, tx.xy, tx.z + depthBias );

								oColor.rgb *= csmDebugColor[1].rgb;		// 緑
							}

						} else {

							//---------- CSM近景用処理[2] ----------

							tx.z += depthBias;
							#if (FLAG2_PCF_4x4)
								float shade;
								shade   = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(-shiftPCF,-shiftPCF), tx.z );
								shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(     0.0f,-shiftPCF), tx.z );
								shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF,-shiftPCF), tx.z );
								shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(-shiftPCF,     0.0f), tx.z );
								shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(     0.0f,     0.0f), tx.z );
								shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF,     0.0f), tx.z );
								shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(-shiftPCF, shiftPCF), tx.z );
								shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(     0.0f, shiftPCF), tx.z );
								shade  += gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF, shiftPCF), tx.z );
								shadow  = shade * (1.0f / 9.0f);
							#elif (FLAG2_PCF_3x3)
								gfdDepthGatherCmp( shadow, shadowTexture2, shadowSampler, tx.xy, tx.z );
								//	vec4 shade;
								//	shade.x = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy, tx.z );
								//	shade.y = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF,     0.0f), tx.z );
								//	shade.z = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2(     0.0f, shiftPCF), tx.z );
								//	shade.w = gfdDepthCmp( shadowTexture2, shadowSampler, tx.xy + vec2( shiftPCF, shiftPCF), tx.z );
								//	shadow  = (shade.x + shade.y + shade.z + shade.w) * (1.0f / 4.0f);
							#else
								gfdDepthSampleCmp( shadow, shadowTexture2, shadowSampler, tx.xy, tx.z );
							#endif

							oColor.rgb *= csmDebugColor[2].rgb;	// 赤
						}

					#else	// ------------------------------------ FLAG2_CSM ------------------------------------

						vec3 tx = fragment.lightcoord.xyz / fragment.lightcoord.w;
						tx.z += depthBias;
						#if (FLAG2_PCF_4x4)
							float shade;
							shade   = gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(-shiftPCF,-shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(     0.0f,-shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2( shiftPCF,-shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(-shiftPCF,     0.0f), tx.z );
							shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(     0.0f,     0.0f), tx.z );
							shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2( shiftPCF,     0.0f), tx.z );
							shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(-shiftPCF, shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2(     0.0f, shiftPCF), tx.z );
							shade  += gfdDepthCmp( shadowTexture, shadowSampler, tx.xy + vec2( shiftPCF, shiftPCF), tx.z );
							shadow  = shade * (1.0f / 9.0f);
						#elif (FLAG2_PCF_3x3)
							gfdDepthGatherCmp( shadow, shadowTexture, shadowSampler, tx.xy, tx.z );
						#else
							gfdDepthSampleCmp( shadow, shadowTexture, shadowSampler, tx.xy, tx.z );
						#endif

					#endif	// ------------------------------------ FLAG2_CSM ------------------------------------

		            #ifdef NEEDLE_SHADER
					vec3 lightCoord = 0.f;
					// 簡単のために前の処理と重複するが、コンパイラの最適化に期待.
					#if FLAG2_CSM
						lightCoord = fragment.lightcoord[0].xyz / fragment.lightcoord[0].w;
					#else
						lightCoord = fragment.lightcoord.xyz / fragment.lightcoord.w;
					#endif
					vec2 _border = saturate(lightCoord.xy) - lightCoord.xy;
					if( (abs(_border.x) + abs(_border.y)) > CSM_EPS ) {
						shadow = 1.f;
					}
					#endif

					float shadowAmb = min( shadow + dimmerAmb + shadowDisable, 1.f);
					float shadowDif = min( shadow + dimmerDif + shadowDisable, 1.f);

					#if (FLAG1_MATERIAL_EMISSIVE)
						oColor.rgb *= matEmissive.rgb + matAmbient.rgb * shadowAmb + matDiffuse.rgb * shadowDif;
					#else
						oColor.rgb *= matAmbient.rgb * shadowAmb + matDiffuse.rgb * shadowDif;
					#endif
				#else
				#endif
			#else
				#if (FLAG1_MATERIAL_EMISSIVE)
					oColor.rgb *= matEmissive.rgb + matAmbient.rgb + matDiffuse.rgb;
				#else
					oColor.rgb *= matAmbient.rgb + matDiffuse.rgb;
				#endif*/
			#endif
		#endif

	#endif

	#if (FLAG1_TEXTURE4 && FLAG1_MATERIAL_REFLECTION)
		#if ((!FLAG0_TEXCOORD2OUT) && FLAG2_FAKE_REFLECTION && FLAG3_REFLECT_TYPE1)
	vec3 reflectCol = texture(reflectionTexture, texcoord2.xy * 0.5 + 0.5);
		#else
	vec3 R;
			#if (FLAG1_TEXTURE2)
	R = reflect(viewTangentDir, normalVec);
			#else
				#if (FLAG1_MATERIAL_LIGHT)
	R = reflect(-inviewDir, N);
				#else
	R = reflect(-inviewDir, normalize(normal.xyz));
				#endif
			#endif
			#if (FLAG2_FAKE_REFLECTION)
	vec3 reflectCol = texture(reflectionTexture, R.xy).rgb;
			#else
	vec3 reflectCol = texture(reflectionTexture, R).rgb;
			#endif
		#endif
		#if (FLAG1_MATERIAL_REFLECTION_ADD)
			#if (FLAG1_MATERIAL_REFLECTION_LERP)
				#if (FLAG1_TEXTURE3 || FLAG2_SPECULAR_NORMALMAPALPHA)
	oColor.rgb += reflectCol * matReflectivity * glossCol.r;
				#else
	oColor.rgb += reflectCol * matReflectivity;
				#endif
			#else
				#if (FLAG1_TEXTURE3 || FLAG2_SPECULAR_NORMALMAPALPHA)
	oColor.rgb += reflectCol * glossCol.r;
				#else
	oColor.rgb += reflectCol;
				#endif
			#endif
		#else
			#if (FLAG1_MATERIAL_REFLECTION_LERP)
				#if (FLAG1_TEXTURE3 || FLAG2_SPECULAR_NORMALMAPALPHA)
	oColor.rgb = mix(oColor.rgb, reflectCol, matReflectivity * glossCol.r);
				#else
	oColor.rgb = mix(oColor.rgb, reflectCol, matReflectivity);
				#endif
			#else
				#if (FLAG1_TEXTURE3 || FLAG2_SPECULAR_NORMALMAPALPHA)
	oColor.rgb = mix(oColor.rgb, reflectCol, glossCol.r);
				#else
	oColor.rgb = reflectCol;
				#endif
			#endif
		#endif
	#endif

	#if (FLAG1_TEXTURE7)
	oColor.rgb *= darkCol;
	#endif

	#if (FLAG1_TEXTURE8)
	oColor.rgb *= detailCol;
	#endif

	#if (FLAG1_TEXTURE6)
	oColor.rgb += glowCol;
	#endif

	#if (FLAG1_MATERIAL_LIGHT)
	E = pow((min(1.0 - NVW, toonLightThreshold) / toonLightThreshold), toonLightFactor);
		#if (!FLAG2_EDGE_SEMITRANS)
			#if (FLAG0_LIGHT0_DIRECTION || FLAG0_LIGHT0_POINT || FLAG0_LIGHT0_SPOT)
				#if (FLAG2_EDGE_REFERENCE_NORMALALPHA)
	oColor.rgb += toonLightRGB * light0Diffuse.rgb * E * toonLightColor.a * normalCol.a;
				#elif (FLAG2_EDGE_REFERENCE_DIFFUSEALPHA)
	oColor.rgb += toonLightRGB * light0Diffuse.rgb * E * toonLightColor.a * oColor.a;
				#elif (FLAG1_EDGE_REFERENCE_LIGHTALPHA)
	oColor.rgb += toonLightRGB * light0Diffuse.rgb * E * toonLightColor.a * lightCol.a;
				#else
	oColor.rgb += toonLightRGB * light0Diffuse.rgb * E * toonLightColor.a;
				#endif
			#else
				#if (FLAG2_EDGE_REFERENCE_NORMALALPHA)
	oColor.rgb += toonLightRGB * E * toonLightColor.a * normalCol.a;
				#elif (FLAG2_EDGE_REFERENCE_DIFFUSEALPHA)
	oColor.rgb += toonLightRGB * E * toonLightColor.a * oColor.a;
				#elif (FLAG1_EDGE_REFERENCE_LIGHTALPHA)
	oColor.rgb += toonLightRGB * E * toonLightColor.a * lightCol.a;
				#else
	oColor.rgb += toonLightRGB * E * toonLightColor.a;
				#endif
			#endif
		#else
			#if (FLAG0_LIGHT0_DIRECTION || FLAG0_LIGHT0_POINT || FLAG0_LIGHT0_SPOT)
				#if (FLAG2_EDGE_REFERENCE_NORMALALPHA)
	oColor.rgb = mix(oColor.rgb, toonLightRGB * light0Diffuse.rgb, E * toonLightColor.a * normalCol.a);
				#elif (FLAG2_EDGE_REFERENCE_DIFFUSEALPHA)
	oColor.rgb = mix(oColor.rgb, toonLightRGB * light0Diffuse.rgb, E * toonLightColor.a * oColor.a);
				#elif (FLAG1_EDGE_REFERENCE_LIGHTALPHA)
	oColor.rgb = mix(oColor.rgb, toonLightRGB * light0Diffuse.rgb, E * toonLightColor.a * lightCol.a);
				#else
	oColor.rgb = mix(oColor.rgb, toonLightRGB * light0Diffuse.rgb, E * toonLightColor.a);
				#endif
			#else
				#if (FLAG2_EDGE_REFERENCE_NORMALALPHA)
	oColor.rgb = mix(oColor.rgb, toonLightRGB, E * toonLightColor.a * normalCol.a);
				#elif (FLAG2_EDGE_REFERENCE_DIFFUSEALPHA)
	oColor.rgb = mix(oColor.rgb, toonLightRGB, E * toonLightColor.a * oColor.a);
				#elif (FLAG1_EDGE_REFERENCE_LIGHTALPHA)
	oColor.rgb = mix(oColor.rgb, toonLightRGB, E * toonLightColor.a * lightCol.a);
				#else
	oColor.rgb = mix(oColor.rgb, toonLightRGB, E * toonLightColor.a);
				#endif
			#endif
		#endif
	#endif

	#if (FLAG1_MATERIAL_VERTEXCOLOR || FLAG0_CONSTANTCOLOR)
		#if (FLAG0_GRAYSCALE)
	float gray = (oColor.r + oColor.g + oColor.b) / 3.;
	oColor.rgb = mix(oColor.rgb, vec3(gray, gray, gray), color.a);
		#endif
		#if (FLAG0_COLOR_OP_mix)
	oColor.rgb = mix(oColor.rgb, color.rgb, color.a);
		#else
	oColor *= color;
		#endif
		#if (FLAG2_EDGE_REFERENCE_DIFFUSEALPHA)
	oColor.a = color.a;
		#endif
	#endif

	#if (FLAG1_MATERIAL_VERTEXCOLOR || FLAG0_CONSTANTCOLOR || FLAG0_HDR || FLAG0_OUTLINE)
		#if (FLAG2_ATEST_NEVER)
	clip(-1);
		#elif (FLAG2_ATEST_LESS_LEQUAL)
	clip(atestRef - oColor.a);
		#elif (FLAG2_ATEST_EQUAL)
	clip(-(oColor.a != atestRef));
		#elif (FLAG2_ATEST_GREATER_GEQUAL)
	clip(oColor.a - atestRef);
		#elif (FLAG2_ATEST_NOTEQUAL)
	clip(-(oColor.a == atestRef));
		#endif
	#endif

	#if (FLAG1_MATERIAL_HEIGHTFOG)
		#if (FLAG2_MATERIAL_MODULATE_FOG)
	oColor.rgb *= (1.0 - hfAlpha) + heightFogColor.rgb * hfAlpha;
		#else
	oColor.rgb = mix(oColor.rgb, heightFogColor.rgb, hfAlpha);
		#endif
	#endif

	#if (FLAG1_MATERIAL_FOG)
		#if (FLAG2_MATERIAL_MODULATE_FOG)
	float fs = (1.0 - misc.x) * fogColor.a * (1.0 - fogDisable);
	oColor.rgb *= (1.0 - fs) + fogColor.rgb * fs;
		#else
	float fs = (1.0 - misc.x) * fogColor.a * (1.0 - fogDisable);
	oColor.rgb = mix(oColor.rgb, fogColor.rgb, fs);
		#endif
	#endif

	#if (FLAG0_FLOAT_RENDER_TARGET)
		// 浮動小数点フレームバッファに負の値は出力しないようにする
	oColor.rgb = max(0.0, oColor.rgb);
	#endif

	#if (FLAG0_HDR) && (!FLAG0_FLOAT_RENDER_TARGET)	&& (!FLAG2_REFLECTION_CASTER)	//	#if (FLAG0_HDR_PS3)
		#if (FLAG0_BLOOM)
	float tmp, scl;
	tmp = max(max(oColor.r, oColor.g), max(oColor.b, 0.25));
	scl = 1.0 / tmp;
	oColor = vec4(oColor.rgb * scl, 0.2475 * scl);
		#else
	oColor.a = 1.0;
		#endif
	#elif (FLAG0_HDR) && (FLAG0_FLOAT_RENDER_TARGET) && (!FLAG2_REFLECTION_CASTER) // 16/12/13
		// 小数バッファとマテリアルのブルームフラグに対応
		#if (FLAG0_BLOOM)
	oColor.a = 0.0;// 本当は0.998f等にしたいが精度が2bitしかないので
		#else
	oColor.a = 1.0;
		#endif
	#elif (FLAG0_OPAQUE_ALPHA1)
	oColor.a = 1.0;
	#endif

	//	result.depth = fragment.proj.z / fragment.proj.w;
}
