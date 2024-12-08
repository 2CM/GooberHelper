#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

DECLARE_TEXTURE(text, 0);
DECLARE_TEXTURE(buh, 1);

float2 CamPos;
float4 HairColor;

float4 Buh(float4 inPosition : SV_POSITION, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
	float4 tex = SAMPLE_TEXTURE(text, float2(uv.x, uv.y));
    float2 screenUv = (CamPos + inPosition.xy) / float2(320, 180);

    // return float4(tex.r, tex.g, 1, 0);

	return floor(tex.w) * SAMPLE_TEXTURE(buh, (((screenUv + abs(floor(screenUv))) * 10.0) % 1.0)) * HairColor * 4.0;
}

technique Grongle
{
	pass { PixelShader = compile ps_3_0 Buh(); }
}