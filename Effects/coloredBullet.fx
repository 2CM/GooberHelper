#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

DECLARE_TEXTURE(text, 0);


float4 ColoredBullet(float4 inPosition : SV_POSITION, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
    float4 tex = SAMPLE_TEXTURE(text, float2(uv.x, uv.y));
    float factor = abs(tex.r - tex.g);

    return lerp(float4(tex.g, tex.g, tex.g, tex.a), inColor, factor);
}

technique Shader {
	pass { PixelShader = compile ps_3_0 ColoredBullet(); }
}