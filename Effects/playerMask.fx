#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

DECLARE_TEXTURE(text, 0);
DECLARE_TEXTURE(buh, 1);

float2 CamPos;
float2 TextureSize;
float4 HairColor;
float Time;
bool KeepOutlines;

float2 WrapUv(float2 uv) {
    return (uv % 1.0 + 1.0) % 1.0;
}

float4 Buh(float4 inPosition : SV_POSITION, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
    float4 tex = SAMPLE_TEXTURE(text, float2(uv.x, uv.y));

    if(KeepOutlines && inColor.r + inColor.g + inColor.b == 0) return inColor * tex.a;

    float2 screenUv = (CamPos + inPosition.xy) / TextureSize;

    // return float4(textureUv, 0, 1);

    // return float4(tex.r, tex.g, 1, 0);

    float4 layer1 = SAMPLE_TEXTURE(buh, WrapUv(screenUv - Time * 0.1));
    float4 layer2 = SAMPLE_TEXTURE(buh, WrapUv(screenUv + Time * 0.141592653589));

    float4 overlay = lerp(1.0, HairColor, 0.5);

	float4 final = abs(layer1 - layer2) * 4.0;

    final.w = 1.0;

    return lerp(min(final * overlay, 1.0), HairColor, 0.5) * floor(tex.w);
}

technique Grongle
{
	pass { PixelShader = compile ps_3_0 Buh(); }
}