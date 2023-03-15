
float3 offset;


float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 AmbientColor;
float AmbientIntensity;

float4 DiffuseColor;
float DiffuseIntensity;


float Shininess;
float4 SpecularColor;
float SpecularIntensity;

float3 CameraPosition;
float3 LightPosition;

sampler DiffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexInput
{
	float4 Position : POSITION;
	float4 Normal: NORMAL;
};

struct VertexShaderOutput
{
	float4 Position : POSITION;
	float4 Color : COLOR;
	float4 Normal : TEXCOORD0;
	float4 WorldPosition: TEXCOORD1;

};


// GOURAND
VertexShaderOutput GourandVertexShaderFunction(VertexInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = 0;
	output.Normal = 0;
	float3 N = normalize((mul(input.Normal, WorldInverseTranspose)).xyz);
	float3 V = normalize(CameraPosition - worldPosition.xyz);
	float3 L = normalize(LightPosition);
	float3 R = reflect(-L, N);
	float4 ambient = AmbientColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
	float4 specular = pow(max(0, dot(V, R)), Shininess) * SpecularColor * SpecularIntensity;
	output.Color = saturate(ambient + diffuse + specular);
	return output;
}
float4 GourandPixelShaderFunction(VertexShaderOutput input) : COLOR
{
	return input.Color;
}
technique MyTechnique
{
	pass pass1
	{
		VertexShader = compile vs_4_0 GourandVertexShaderFunction();
		PixelShader = compile ps_4_0 GourandPixelShaderFunction();
	}
};

// PHONG PIXEL
VertexShaderOutput PhongVertexShaderFunction(VertexInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	output.Color = 0;

	return output;
}
float4 PhongPixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float3 N = normalize(input.Normal.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 L = normalize(LightPosition);
	float3 R = reflect(-L, N);

	float4 ambient = AmbientColor * AmbientIntensity;
	float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
	float4 specular = pow(max(0, dot(V, R)), Shininess) * SpecularColor * SpecularIntensity;
	float4 color = saturate(ambient + diffuse + specular);
	
	
	color.a = 1;
	return color;
}
technique Phong
{
	pass pass1
	{
		VertexShader = compile vs_4_0 PhongVertexShaderFunction();
		PixelShader = compile ps_4_0 PhongPixelShaderFunction();
	}
};

// PhongBlinn
VertexShaderOutput PhongBlinnVertexShaderFunction(VertexInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	output.Color = 0;

	return output;

}
float4 PhongBlinnPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 N = normalize(input.Normal.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 L = normalize(LightPosition);
	float3 H = normalize(L + V);
	float4 color = DiffuseIntensity * max(0, dot(N, L)) + pow(saturate(dot(N,H)), Shininess) * SpecularColor * SpecularIntensity;

	color.a = 1;

	return color;
}

technique PhongBlinn
{
	pass pass1
	{
		VertexShader = compile vs_4_0 PhongBlinnVertexShaderFunction();
		PixelShader = compile ps_4_0 PhongBlinnPixelShaderFunction();
	}
};

// Schlick
VertexShaderOutput SchlickVertexShaderFunction(VertexInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	output.Color = 0;

	return output;
}
float4 SchlickPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 N = normalize(input.Normal.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 L = normalize(LightPosition);
	float3 R = reflect(-L, N);
	float T = max(0, dot(V,R));
	float P = T / (Shininess - T * Shininess + T);
	float4 color = DiffuseIntensity * DiffuseColor * max(0, dot(N, L)) + P * SpecularColor * SpecularIntensity;;

	color.a = 1;
	return color;

}
technique Schlick
{
	pass pass1
	{
		VertexShader = compile vs_4_0 SchlickVertexShaderFunction();
		PixelShader = compile ps_4_0 SchlickPixelShaderFunction();
	}
};

// TOON
VertexShaderOutput ToonVertexShaderFunction(VertexInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.Normal = mul(input.Normal, WorldInverseTranspose);
	output.Color = 0;

	return output;
}
float4 ToonPixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float3 N = normalize(input.Normal.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 L = normalize(LightPosition);
	float3 R = reflect(-L, N);
	float D = dot(V, R);
	if (D < -0.7)
	{
		return float4(0, 0, 0, 1);
	}
	else if (D < 0.2)
	{
		return float4(0.25, 0.25, 0.25, 1);
	}
	else if (D < 0.97)
	{
		return float4(0.5, 0.5, 0.5, 1);
	}
	else
	{
		return float4(1, 1, 1, 1);
	}
}
technique Toon
{
	pass pass1
	{
		VertexShader = compile vs_4_0 ToonVertexShaderFunction();
		PixelShader = compile ps_4_0 ToonPixelShaderFunction();
	}
};

// HalfLife UNFINISHED
VertexShaderOutput HalfLifeVertexShaderFunction(VertexInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	output.WorldPosition = worldPosition;
	output.Position = mul(worldPosition, View);
	output.Position = mul(output.Position, Projection);

	float4 normal = mul(input.Normal, WorldInverseTranspose);
	normal = normalize(normal);

	float3 L = normalize(LightPosition);
	float diffuseFactor = max(dot(normal.xyz, L), 0);
	float3 diffuse = DiffuseColor.xyz * DiffuseIntensity * diffuseFactor;

	float3 V = normalize(CameraPosition - worldPosition.xyz);
	float3 H = normalize(L + V);
	float specularFactor = pow(max(dot(normal.xyz, H), 0), Shininess);
	float3 specular = SpecularColor.xyz * SpecularIntensity * specularFactor;

	output.Color = (AmbientColor * AmbientIntensity) + float4(diffuse + specular, 1);
	output.Normal = normal;

	return output;

}
float4 HalfLifePixelShaderFunction(VertexShaderOutput input) : COLOR
{
	float3 N = normalize(input.Normal.xyz);
	float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
	float3 L = normalize(LightPosition);
	float4 color = DiffuseIntensity * DiffuseColor * pow(0.5*(dot(N,L)+1),2);	
	return color;

}
technique HalfLife
{
	pass pass1
	{
		VertexShader = compile vs_4_0 HalfLifeVertexShaderFunction();
		PixelShader = compile ps_4_0 HalfLifePixelShaderFunction();
	}
};