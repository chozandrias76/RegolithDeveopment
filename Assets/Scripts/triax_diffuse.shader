  Shader "Custom/triax_diffuse" {
    Properties {
      _MainTex ("Texture1", 2D) = "white" {}
      _YTex ("Texture2", 2D) = "white" {}
      _ZTex ("Texture3", 2D) = "white" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf SimpleLambert
      
      sampler2D _MainTex;
      sampler2D _YTex;
      sampler2D _ZTex;
      
      half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
          half NdotL = dot (s.Normal, lightDir);
          half4 c;
          c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2);
          c.a = s.Alpha;
          return c;
      }
      
      struct Input {
          float2 uv_MainTex;
      	  float3 worldPos;
      };
      
      void surf (Input IN, inout SurfaceOutput o) {
          float2 tc0=IN.worldPos.xz*0.25;
          float2 tc1=IN.worldPos.xy*0.25;
          float2 tc2=IN.worldPos.zy*0.25;
          o.Albedo = (max(o.Normal.y,0.0)*tex2D (_MainTex, tc0).rgb)+
           			 (max(o.Normal.y*-1.0,0.0f)*tex2D (_ZTex, tc0).rgb);
           
          o.Albedo += tex2D (_YTex, tc1).rgb*abs(o.Normal.x);
          o.Albedo += tex2D (_YTex, tc2).rgb*abs(o.Normal.z);
          
          //o.Albedo.rgb=o.Normal.xyz;
      }
      ENDCG
    }
    Fallback "Diffuse"
  }