#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

DECLARE_TEXTURE(text, 0);
DECLARE_TEXTURE(buh, 1);

float time;
// float2 uvStart;
// float2 uvSize;
// float2 atlasSize;
// float2 textureSize;

float4 Buh(float4 inPosition : SV_POSITION, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // float2 mappedUv = uvStart
    //     + float2(0, 6)/atlasSize //its offest ingame or something idk
    //     - 0.5 * (textureSize - uvSize)/atlasSize //bring it to the center
    //     + (float2(1,0) + float2(-1,1) * uv) //flip
    //     * uvSize/atlasSize //make it the proper size
    //     * textureSize/uvSize; //make it fit in the larger entity
    
    uv.x = 1.0 - uv.x;
    // uv.y = 1.0 - uv.y;

    float scale = 16.0;

    float dist = distance(uv, float2(0.5,0.5));
    float intensity = (1.0/(dist + 1.0)) * (1.0 - dist * 2.0);

    float4 noise = SAMPLE_TEXTURE(text, uv * 12598.0 + time);

    float4 crystal = SAMPLE_TEXTURE(buh, uv * scale - 0.5 * scale + 0.5 + float2(0,5)/64.0 + (noise.xy * 2.0 - 1.0) * float2(0.03, 0.02));

    return 2.0 * crystal + (float4(0, 0.96 + sin(time * 30.0) * 0.04, 1, 1) - noise.r * 0.1) * intensity;
}

technique Grongle
{
	pass { PixelShader = compile ps_3_0 Buh(); }
}