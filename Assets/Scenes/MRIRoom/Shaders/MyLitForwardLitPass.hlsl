// MIT License

// Copyright (c) 2023 NedMakesGames

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// This file contains the vertex and fragment functions for the forward lit pass
// This is the shader pass that computes visible colors for a material
// by reading material, light, shadow, etc. data

// Pull in URP library functions and our own common functions
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// Textures
TEXTURE2D(_ColorMap); SAMPLER(sampler_ColorMap); // RGB = albedo, A = alpha

float4 _ColorMap_ST; // This is automatically set by Unity. Used in TRANSFORM_TEX to apply UV tiling
float4 _ColorTint;
float _Smoothness;

// This attributes struct receives data about the mesh we're currently rendering
// Data is automatically placed in fields according to their semantic
struct Attributes {
	float3 positionOS : POSITION; // Position in object space
	float3 normalOS : NORMAL; // Normal in object space
	float2 uv : TEXCOORD0; // Material texture UVs
};

// This struct is output by the vertex function and input to the fragment function.
// Note that fields will be transformed by the intermediary rasterization stage
struct Interpolators {
	// This value should contain the position in clip space (which is similar to a position on screen)
	// when output from the vertex function. It will be transformed into pixel position of the current
	// fragment on the screen when read from the fragment function
	float4 positionCS : SV_POSITION;

	// The following variables will retain their values from the vertex stage, except the
	// rasterizer will interpolate them between vertices
	float2 uv : TEXCOORD0; // Material texture UVs
	float3 positionWS : TEXCOORD1; // Position in world space
	float3 normalWS : TEXCOORD2; // Normal in world space
};

// The vertex function. This runs for each vertex on the mesh.
// It must output the position on the screen each vertex should appear at,
// as well as any data the fragment function will need
Interpolators Vertex(Attributes input) {
	Interpolators output;

	// These helper functions, found in URP/ShaderLib/ShaderVariablesFunctions.hlsl
	// transform object space values into world and clip space
	VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	// Pass position and orientation data to the fragment function
	output.positionCS = posnInputs.positionCS;
	output.uv = TRANSFORM_TEX(input.uv, _ColorMap);
	output.normalWS = normInputs.normalWS;
	output.positionWS = posnInputs.positionWS;

	return output;
}

// The fragment function. This runs once per fragment, which you can think of as a pixel on the screen
// It must output the final color of this pixel
float4 Fragment(Interpolators input) : SV_TARGET{
	float2 uv = input.uv;
	// Sample the color map
	float4 colorSample = SAMPLE_TEXTURE2D(_ColorMap, sampler_ColorMap, uv);

	// For lighting, create the InputData struct, which contains position and orientation data
	InputData lightingInput = (InputData)0; // Found in URP/ShaderLib/Input.hlsl
	lightingInput.positionWS = input.positionWS;
	lightingInput.normalWS = normalize(input.normalWS);
	lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS); // In ShaderVariablesFunctions.hlsl
	lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS); // In Shadows.hlsl
	
	// Calculate the surface data struct, which contains data from the material textures
	SurfaceData surfaceInput = (SurfaceData)0;
	surfaceInput.albedo = colorSample.rgb * _ColorTint.rgb;
	surfaceInput.alpha = colorSample.a * _ColorTint.a;
	surfaceInput.specular = 1;
	surfaceInput.smoothness = _Smoothness;

#if UNITY_VERSION >= 202120
	return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
#else
	return UniversalFragmentBlinnPhong(lightingInput, surfaceInput.albedo, float4(surfaceInput.specular, 1), surfaceInput.smoothness, surfaceInput.emission, surfaceInput.alpha);
#endif
}