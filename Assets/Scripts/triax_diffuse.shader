  Shader "Custom/triax_diffuse" {
    Properties {
      _XTex ("Texture1", 2D) = "white" {}
      _YTex ("Texture2", 2D) = "white" {}
      _ZTex ("Texture3", 2D) = "white" {}
      _X1Tex ("Texture4", 2D) = "white" {}
      _Y1Tex ("Texture5", 2D) = "white" {}
      _Z1Tex ("Texture6", 2D) = "white" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf SimpleLambert
      
      sampler2D _XTex;
      sampler2D _YTex;
      sampler2D _ZTex;
      sampler2D _X1Tex;
      sampler2D _Y1Tex;
      sampler2D _Z1Tex;
      
      half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
          half NdotL = dot (s.Normal, lightDir);
          half4 c;
          c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2);
          c.a = s.Alpha;
          return c;
      }
      
      float3	triplanarMap(sampler2D tx,sampler2D ty,sampler2D tz,float3 wpscl,SurfaceOutput o){
      	return
	          (max(o.Normal.y,0.0)*tex2D (tx, wpscl.xz).rgb)+
	          (max(o.Normal.y*-1.0,0.0f)*tex2D (tz, wpscl.xz).rgb)+
	           	tex2D (ty, wpscl.xy).rgb*abs(o.Normal.x)+
	          	tex2D (ty, wpscl.zy).rgb*abs(o.Normal.z);
      }
      
      struct Input {
          float2 uv_XTex;
      	  float3 worldPos;
      	  float4 screenPos;
      };
      
      void surf (Input IN, inout SurfaceOutput o) {
	      float3 wpscl=IN.worldPos.xyz*0.25;
		  float matsel=fmod(IN.uv_XTex.x,1.0);
         
	      o.Albedo = (triplanarMap(_XTex,_YTex,_ZTex,wpscl,o)*matsel)+
          // if(matsel<0.5)
           (triplanarMap(_X1Tex,_Y1Tex,_Z1Tex,wpscl,o)*(1.0-matsel));
         // else if(matsel<0.75)
         // 	o.Albedo = triplanarMap(_Z1Tex,_Y1Tex,_X1Tex,wpscl,o);
          //else
          //	o.Albedo = triplanarMap(_YTex,_YTex,_XTex,wpscl,o);
          
          //if(fmod(IN.uv_XTex.x,1.0)>0.5)
          //{
//	          o.Albedo = max(o.Normal.y,0.0)*tex2D (_XTex, tc0).rgb)+
//	           			 (max(o.Normal.y*-1.0,0.0f)*tex2D (_ZTex, tc0).rgb);	          
//	          o.Albedo += tex2D (_YTex, tc1).rgb*abs(o.Normal.x);
//	          o.Albedo += tex2D (_YTex, tc2).rgb*abs(o.Normal.z);

	          //float3 wsp=IN.screenPos.xyz/IN.screenPos.w;
	          //if(wsp.x>0.5)
	          //else
	          //if(wsp.y>0.5)
	          //	o.Albedo.rgb=o.Normal.xyz;
          //}
          //else discard;
      }
      ENDCG
    }
    Fallback "Diffuse"
  }