
/*---------------------------------------------------------------------------*/
#version 330 core
	in vec3 inposition;

#if (FLAG1_MATERIAL_VERTEXCOLOR)
	in vec4 incolor;
#endif

#if (FLAG0_TEXCOORD0IN)
	in vec2 intexcoord0;
#endif

#if (FLAG0_TEXCOORD1IN)
	in vec2 intexcoord1;
#endif

#if (FLAG0_TEXCOORD2IN)
	in vec2 intexcoord2;
#endif

// ストリームに法線があったりなかったりするため問題あるので一番下へ移動して対処
#if (FLAG1_MATERIAL_LIGHT  || (!FLAG2_EDGE_REFERENCE_NORMALMAP) || (!FLAG2_TOON_REFERENCE_NORMALMAP) || FLAG1_MATERIAL_REFLECTION || FLAG0_OUTLINE || FLAG1_TEXTURE2)
	in vec3 innormal;
#endif
#if (FLAG1_TEXTURE2)
	in vec3 inbinormal;
#endif


/*---------------------------------------------------------------------------*/

	out vec4 position;

#if ((FLAG1_MATERIAL_LIGHT && (!FLAG1_TEXTURE2)) || (!FLAG2_EDGE_REFERENCE_NORMALMAP) || (!FLAG2_TOON_REFERENCE_NORMALMAP) || FLAG1_MATERIAL_REFLECTION || FLAG0_OUTLINE || FLAG1_MATERIAL_HEIGHTFOG)
	#if (FLAG1_MATERIAL_HEIGHTFOG)
	out vec4 normal;
	#else
	out vec3 normal;
	#endif
#endif

#if (FLAG1_MATERIAL_LIGHT)
	#if (FLAG0_LIGHT0_DIRECTION || FLAG0_LIGHT0_POINT || FLAG0_LIGHT0_SPOT)
	out vec4 outlight0Vec;
		#if (FLAG1_TEXTURE2)
		out vec4 light0TangentDir;
		#endif
	#endif

	#if (FLAG0_LIGHT1_DIRECTION || FLAG0_LIGHT1_POINT || FLAG0_LIGHT1_SPOT)
	out vec4 outlight1Vec;
	#endif

	#if (FLAG0_LIGHT2_DIRECTION || FLAG0_LIGHT2_POINT || FLAG0_LIGHT2_SPOT)
	out vec4 outlight2Vec;
	#endif
#endif

#if (FLAG1_TEXTURE2)
	out vec3 viewTangentDir;
	#if (!FLAG2_EDGE_REFERENCE_NORMALMAP)
	out vec3 inviewDir;
	#endif
#else
	out vec3 inviewDir;
#endif

#if (!FLAG2_REFLECTION_CASTER)
	#if (FLAG1_MATERIAL_SHADOW)
		#if (FLAG2_CSM)
		out vec4 lightcoord[3];	// TEXCOORD5,6,7 TEXCOORD8予備
		#else
		out vec4 lightcoord;
		#endif
	#endif
#else
	out vec4 worldPos;
#endif

#if (FLAG1_MATERIAL_VERTEXCOLOR || FLAG0_CONSTANTCOLOR)
	out vec4 color;
#endif

#if (FLAG0_TEXCOORD0OUT)
	out vec2 texcoord0;
#endif

#if (FLAG0_TEXCOORD1OUT)
	out vec2 texcoord1;
#endif

#if (FLAG0_TEXCOORD2OUT || FLAG2_FAKE_REFLECTION)
	out vec2 texcoord2;
#endif

#if (FLAG1_MATERIAL_FOG)
	out vec4 misc; // COLOR1.x = fog
#endif

/*---------------------------------------------------------------------------*/

layout (std140) uniform GFD_VSCONST_SYSTEM
{
	vec4		zeroVec;
	vec4		constants;
	vec3		scale2D;
};

layout (std140) uniform GFD_VSCONST_TRANSFORM
{
	mat4	mtxLocalToWorld;
	// mat3	mtxNormal;
};

layout (std140) uniform GFD_VSCONST_VIEWPROJ
{
	mat4	mtxViewProj;
	mat4	mtxView;
	vec3		eyePosition;
	float		_reserved_b2;
};

layout (std140) uniform GFD_VSCONST_SHADOW
{
	mat4	mtxLightViewProj[4];
};

layout (std140) uniform GFD_VSCONST_LIGHT_VEC
{
	vec4		light0Vec;
	vec4		light1Vec;
	vec4		light2Vec;
};

layout (std140) uniform GFD_VSCONST_FOGFACTOR
{
	vec4		fog;
	vec2		heightFog;
	vec2		_reserved_b5;
};

layout (std140) uniform GFD_VSCONST_COLORS
{
	vec4		constantColor;
};

layout (std140) uniform GFD_VSCONST_UV0_TRANSFORM
{
	mat4	mtxUV0Transform;
};

layout (std140) uniform GFD_VSCONST_UV1_TRANSFORM
{
	mat4	mtxUV1Transform;
};

layout (std140) uniform GFD_VSCONST_UV2_TRANSFORM
{
	mat4	mtxUV2Transform;
};

/*---------------------------------------------------------------------------*/
void main()
{
	vec4		tempposition;

#if defined(NEEDLE_SHADER)
#if (FLAG1_MATERIAL_VERTEXCOLOR)
	incolor.xyzw = incolor.wzyx;
#endif
#if (FLAG1_MATERIAL_LIGHT  || (!FLAG2_EDGE_REFERENCE_NORMALMAP) || (!FLAG2_TOON_REFERENCE_NORMALMAP) || FLAG1_MATERIAL_REFLECTION || FLAG0_OUTLINE || FLAG1_TEXTURE2)
	innormal = UnpackVSNormal(innormal);
#endif
#if  (FLAG1_TEXTURE2)
	inbinormal = UnpackVSNormal(inbinormal);
#endif
#endif

	tempposition = vec4(inposition, 1.0f) * mtxLocalToWorld;

	position = tempposition * mtxViewProj;

#if (FLAG2_REFLECTION_CASTER)
	result.worldPos = tempposition;
#endif

#if (FLAG1_MATERIAL_LIGHT || FLAG1_MATERIAL_REFLECTION || FLAG0_OUTLINE || FLAG1_TEXTURE2 || (!FLAG2_EDGE_REFERENCE_NORMALMAP) || (!FLAG2_TOON_REFERENCE_NORMALMAP))
	vec3 tempnormal = innormal.xyz * mat3(mtxLocalToWorld);
#endif

#if ((FLAG1_MATERIAL_LIGHT && (!FLAG1_TEXTURE2)) || (!FLAG2_EDGE_REFERENCE_NORMALMAP) || (!FLAG2_TOON_REFERENCE_NORMALMAP) || FLAG1_MATERIAL_REFLECTION || FLAG0_OUTLINE)
	normal.xyz = tempnormal;
#elif (FLAG1_MATERIAL_HEIGHTFOG)
	normal.xyz = 0.f;
#endif

#if ((FLAG1_MATERIAL_SHADOW) && (!FLAG2_REFLECTION_CASTER))
	#if (FLAG2_CSM)
	result.lightcoord[0] = tempposition * mtxLightViewProj[0];
	result.lightcoord[1] = tempposition * mtxLightViewProj[1];
	result.lightcoord[2] = tempposition * mtxLightViewProj[2];
	#else
	result.lightcoord = tempposition * mtxLightViewProj[0];
	#endif
#endif

#if (FLAG1_TEXTURE2)
	vec3   binormal   = inbinormal.xyz * mat3(mtxLocalToWorld);
	vec3   tangent    = cross( tempnormal, binormal);
	mat3 tangentMtx = { tangent, binormal, tempnormal};
#endif

#if (FLAG1_MATERIAL_LIGHT)

	#if (FLAG0_LIGHT0_DIRECTION || FLAG0_LIGHT0_POINT || FLAG0_LIGHT0_SPOT)
		#if (FLAG0_LIGHT0_DIRECTION)
			#if (FLAG2_TOON_REMOVAL_LIGHT_YAXIS)
				vec3 rYn = normalize( vec3( light0Vec.x, 0.0f, light0Vec.z ) );
				outlight0Vec = vec4( rYn.xyz, light0Vec.w);
				#if (FLAG1_TEXTURE2)
				light0TangentDir = vec4( rYn.xyz * transpose(tangentMtx), 1.f);
				#endif
			#else
				outlight0Vec = light0Vec;
				#if (FLAG1_TEXTURE2)
				light0TangentDir = vec4( light0Vec.xyz * transpose(tangentMtx), 1.f);
				#endif
			#endif
		#else
			outlight0Vec = light0Vec - tempposition;
			#if (FLAG1_TEXTURE2)
			light0TangentDir = vec4( normalize(light0Vec.xyz) * transpose(tangentMtx), 1.f);
			#endif
		#endif
	#endif

	#if (FLAG0_LIGHT1_DIRECTION || FLAG0_LIGHT1_POINT || FLAG0_LIGHT1_SPOT)
		#if (FLAG0_LIGHT1_DIRECTION)
		result.light1Vec = light1Vec;
		#else
		result.light1Vec = light1Vec - tempposition;
		#endif
	#endif

	#if (FLAG0_LIGHT2_DIRECTION || FLAG0_LIGHT2_POINT || FLAG0_LIGHT2_SPOT)
		#if (FLAG0_LIGHT2_DIRECTION)
		result.light2Vec = light2Vec;
		#else
		result.light2Vec = light2Vec - tempposition;
		#endif
	#endif
#endif

#if (FLAG1_TEXTURE2)
	#if (FLAG3_REFLECT_TYPE1)
		viewTangentDir = normalize( mul( tangentMtx, normalize( eyePosition - position.xyz ) ) );
	#else
		viewTangentDir = mul( normalize(eyePosition - tempposition.xyz), transpose(tangentMtx) );
	#endif
	#if (!FLAG2_EDGE_REFERENCE_NORMALMAP)
		inviewDir = normalize(eyePosition - tempposition.xyz);
	#endif
#else
	inviewDir = normalize(eyePosition - tempposition.xyz);
#endif

#if (FLAG1_MATERIAL_VERTEXCOLOR)
	#if (FLAG0_CONSTANTCOLOR)
		color = incolor * constantColor;
	#else
		color = incolor;
	#endif
#else
	#if (FLAG0_CONSTANTCOLOR)
		color = constantColor;
	#endif
#endif

#if (FLAG0_TEXCOORD0OUT)
	#if (IN_TEXCOORD0 == 0)
		#if (FLAG1_MATERIAL_UV0TRANSFORM)
			texcoord0 = mul( vec4( intexcoord0.x, intexcoord0.y, 0.0, 1.0), mtxUV0Transform);
		#else
			texcoord0 = intexcoord0;
		#endif
	#elif (IN_TEXCOORD0 == 1)
		#if (FLAG1_MATERIAL_UV0TRANSFORM)
			texcoord0 = mul( vec4( intexcoord1.x, intexcoord1.y, 0.0, 1.0), mtxUV0Transform);
		#else
			texcoord0 = intexcoord1;
		#endif
	#elif (IN_TEXCOORD0 == 2)
		#if (FLAG1_MATERIAL_UV0TRANSFORM)
			texcoord0 = mul( vec4( intexcoord2.x, intexcoord2.y, 0.0, 1.0), mtxUV0Transform);
		#else
			texcoord0 = intexcoord2;
		#endif
	#endif
#endif

#if (FLAG0_TEXCOORD1OUT)
	#if (IN_TEXCOORD1 == 0)
		#if (FLAG1_MATERIAL_UV1TRANSFORM)
			texcoord1 = mul( vec4( intexcoord0.x, intexcoord0.y, 0.0, 1.0), mtxUV1Transform);
		#else
			texcoord1 = intexcoord0;
		#endif
	#elif (IN_TEXCOORD1 == 1)
		#if (FLAG1_MATERIAL_UV1TRANSFORM)
			texcoord1 = mul( vec4( intexcoord1.x, intexcoord1.y, 0.0, 1.0), mtxUV1Transform);
		#else
			texcoord1 = intexcoord1;
		#endif
	#elif (IN_TEXCOORD1 == 2)
		#if (FLAG1_MATERIAL_UV1TRANSFORM)
			texcoord1 = mul( vec4( intexcoord2.x, intexcoord2.y, 0.0, 1.0), mtxUV1Transform);
		#else
			texcoord1 = intexcoord2;
		#endif
	#endif
#endif

#if (FLAG0_TEXCOORD2OUT)
	#if (IN_TEXCOORD2 == 0)
		#if (FLAG1_MATERIAL_UV2TRANSFORM)
			texcoord2 = mul( vec4( intexcoord0.x, intexcoord0.y, 0.0, 1.0), mtxUV2Transform);
		#else
			texcoord2 = intexcoord0;
		#endif
	#elif (IN_TEXCOORD2 == 1)
		#if (FLAG1_MATERIAL_UV2TRANSFORM)
			texcoord2 = mul( vec4( intexcoord1.x, intexcoord1.y, 0.0, 1.0), mtxUV2Transform);
		#else
			texcoord2 = intexcoord1;
		#endif
	#elif (IN_TEXCOORD2 == 2)
		#if (FLAG1_MATERIAL_UV2TRANSFORM)
			texcoord2 = mul( vec4( intexcoord2.x, intexcoord2.y, 0.0, 1.0), mtxUV2Transform);
		#else
			texcoord2 = intexcoord2;
		#endif
	#endif
#else
	#if (FLAG2_FAKE_REFLECTION)
		texcoord2.xy = mul( tempnormal.xyz, (mat3)mtxView );
		texcoord2.y *= -1.f;
	#endif
#endif

#if (FLAG1_MATERIAL_FOG)
	vec4 view = mul( tempposition, mtxView);

#if 1 // 2015/09/04 niida
	float z      = -view.z;
	float param0 = fog.y;
	float param1 = fog.z;
	float fraction;

	if( fog.w == 0.f )
	{
		// linear
		fraction = z * param1 + ( param0 - 1.0f );
	}else
	if( fog.w == 1.f )
	{
		// exponential
		float dist = 11.084f * ( param1 * z + param0 - 1.5f );
		fraction   = exp( dist );
	}else{
		// exponential2
		float dist = 4.709f * ( param1 * z + param0 - 1.5f );
		fraction   = exp( -( dist * dist ) );
	}

	result.misc.x   = saturate( fraction );
	result.misc.yzw = 0;
#else
	result.misc   = 0;
	result.misc.x = clamp( (fog.y + view.z) * fog.z, 0.0f, 1.0f);
#endif

#endif

#if (FLAG1_MATERIAL_HEIGHTFOG)
	normal.w = heightFog.x + tempposition.y * heightFog.y;
#endif

}


