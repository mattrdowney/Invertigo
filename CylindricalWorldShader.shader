Shader "Custom/CylindricalWorldShader"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "surface" {}
		_Scale ("Integer Scale", Range(1,32)) = 16
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		CGPROGRAM
		#pragma surface surf Lambert

		struct Input
		{
			float3 worldPos;
		};

		sampler2D _MainTex; 
		
		float4 _Color;
		float _Scale;

		void surf (Input IN, inout SurfaceOutput o)
		{
			float2 UV;
			fixed4 c;
			
			const float LUT[32] = {0.0, 0.25, 0.16666667, 0.83748244022094675475,0.70040887047351233179, 0.89427984573910490495, 0.72446821042683576725, 0.55090653657541717022, 0.77894336519786293325, 0.94498292373434700165, 0.09326331055309348406, 0.11919822451704334456, 0.52214828924643836732, 0.38663858211813067045, 0.47567434695473223464, 0.76907512821817274570, 0.67319034301116728865, 0.34811141277553278541, 0.19920965208070581314, 0.65330733499269694548, 0.24276923811063583332, 0.57746855158028154932, 0.03147179156699033421, 0.61499091355855781211, 0.81799124119967109306, 0.50841832182174776655, 0.06298988981552154633, 0.34572590667407038058, 0.92768020528099852092, 0.61194332669122463635, 0.93163069899331044561, 0.23589591741810465564}; //lookupTable
			
			#pragma target 3.0	
			
			IN.worldPos = normalize(IN.worldPos); //could use normals instead
			
			UV = float2(atan2(IN.worldPos.z,IN.worldPos.x), 2*asin(IN.worldPos.y));
			UV += float2(3.14159265359,3.14159265359);
			UV /= 6.283185307179586;
			
			UV.y *= _Scale*2;
			
			float _XScale; 
			
			if(UV.y > _Scale) _XScale = ceil(2*_Scale - UV.y); //scale is based on proximity to poles
			else 			  _XScale = ceil(UV.y);
			
			UV.x *= _XScale;
			UV.x += LUT[_XScale-1];
			
			//0
			//.25->.75 //.25 + .25 distance sum 
			//.166->.5->.833 //.0833 + .25 + .0833 distance sum
			//0.041666->0.291666->0.541666->0.791666 //0.124674 + 0.125326 + 0.041666 + 0.041666
			//0->.2->.4->.6->.8
			
			UV = frac(UV);
			
			c = tex2D(_MainTex, UV);

			o.Albedo = c.rgb;
		}
		ENDCG
	} 
	Fallback "VertexLit"
}

//Scratch work for figuring out optimal offset configuration for LUT:

//#include <iostream>
//#include <string>
//#include <algorithm>
//
//float dist(float a, float b)
//{
//    float linearDist = std::abs(b - a);
//    return std::min(linearDist, 1 - linearDist);
//}
//
//float minDist(
//
//int main()
//{
//    float offset[16];
//    float buffer1[16];
//    float buffer2[16];
//    
//    offset[0] = 0;
//    buffer2[0] = 0;
//    
//    for(int cuts = 1; cuts < 16; ++cuts)
//    {
//        for(int cut = 1; cut <= cuts; ++cut)
//        {
//            buffer1[cut-1] = buffer2[cut-1]; //save old vector
//            buffer2[cut-1] = ((float)cut)/cuts; //calculate new vector
//        }
//        
//    }
//}
