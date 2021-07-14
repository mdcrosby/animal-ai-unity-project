// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ZoneShader"
{
	Properties
	{
		[Header(Textures and color)]
		[Space]
		_MainTex("Fog Texture:", 2D) = "white" {}
		_Mask("Mask Layer:", 2D) = "white" {}
		_Colour("Base Colour:", color) = (1., 1., 1., 1.)
		_AdditiveAlpha("Additive Alpha Constant:", Range(0.,1.)) = 1.
		_MinAlpha("Minimum Internal Alpha:", Range(0.,1.)) = 0.2
		_UVScale("UV Scale Factor:", float) = 1.
		_ObjScale("Object Scale Factor:", Vector) = (1.,1.,1.,1.)
		_Cull("Face Culling Mode", float) = 0.0

		[Header(Behaviour)]
		[Space]
		_ScrollDirX("Scroll X", Range(-1., 1.)) = 1.
		_ScrollDirY("Scroll Y", Range(-1., 1.)) = 1.
		_Speed("Speed", float) = 1.

		[Header(Emission)]
		[Space]
		[HDR] _EmissionColour("Emission Colour", Color) = (0,0,0)
	}

		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "LightMode" = "UniversalForward"}
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull[_Cull]

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma shader_feature_local_fragment _EMISSION

				#include "UnityCG.cginc"

				static const float one_over_root2 = 0.70710678;

				struct v2f {
					float4 pos : SV_POSITION;
					fixed4 vertCol : COLOR0;
					float2 uv : TEXCOORD0;
					float2 uv2 : TEXCOORD1;
					float3 normal : NORMAL;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;

				float2 calcPlane(float3 w, float3 n)
				{
					
					float2 p = float2(0.,0.);
					if (abs(n.x) > one_over_root2) {
						p = w.yz;
					}
					else if (abs(n.y) > one_over_root2) {
						p = w.xz;
					}
					else if (abs(n.z) > one_over_root2) {
						p = w.xy;
					}
					return p;
					//float2 p = w.xz;
					//return p;
				}

				v2f vert(appdata_full v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);

					// Gets the position of the vertex in worldspace.
					float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz/* + mul(unity_ObjectToWorld, v.vertex).yy*/;
					float2 planePos = calcPlane(worldPos, UnityObjectToWorldNormal(v.normal));
					
					// To use the worldspace coords instead of the mesh's UVs for main noisy/foggy texture, substitute v.texcoord for planePos
					o.uv = TRANSFORM_TEX(planePos, _MainTex);
					o.uv2 = v.texcoord;
					o.vertCol = v.color;

					//o.localPos = worldPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
					
					return o;
				}

				sampler2D _Mask;
				float _Speed;
				fixed _ScrollDirX;
				fixed _ScrollDirY;
				fixed4 _EmissionColour;
				float _AdditiveAlpha;
				float _MinAlpha;
				float _UVScale;
				float3 _ObjScale;

				fixed4 frag(v2f i) : SV_Target
				{
					float2 uv = i.uv * _UVScale /** _ObjScale.xz*/ + fixed2(_ScrollDirX, _ScrollDirY) * _Speed * _Time.x;
					fixed4 col = tex2D(_MainTex, uv) * _EmissionColour * i.vertCol;
					col.a *= tex2D(_Mask, i.uv2).r;
					col.a *= max(1 - (i.pos.z / i.pos.w), _MinAlpha);
					col.a = max(min(1.0, col.a + _AdditiveAlpha), _MinAlpha);
					return col;
				}
				ENDCG

			}
		}
}