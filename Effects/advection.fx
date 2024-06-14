#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

// #define DECLARE_TEXTURE(Name, index) \
//     texture Name: register(t##index); \
//     sampler Name##Sampler: register(s##index)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 pixelSize;
uniform float timestep;

DECLARE_TEXTURE(velocity, 0);
DECLARE_TEXTURE(source, 1);

// float4 bilerp(sampler2D tex, float2 uv, float2 size) {
//     float2 a = floor(uv / size - 0.5) + 0.5;
//     float2 b = frac (uv / size - 0.5);

//     float4 bl = tex2D(tex, size * (a + float2(0,0)));
//     float4 tl = tex2D(tex, size * (a + float2(0,1)));
//     float4 br = tex2D(tex, size * (a + float2(1,0)));
//     float4 tr = tex2D(tex, size * (a + float2(1,1)));

//     return 100.0 * (tl - bl);

//     return lerp(lerp(tl, tr, b.x), lerp(bl, br, b.x), 1.0-b.y);
// }

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
    // float2 pos = uv - pixelSize * 10 * tex2D(velocitySampler, uv).xy;
    // float2 pos = uv - float2(100.0/200.0, 100.0/200.0) * tex2D(velocitySampler, uv).xy;
    float2 pos = uv - timestep * pixelSize * tex2D(velocitySampler, uv).xy;

    return tex2D(sourceSampler, pos);
}

void SpriteVertexShader(inout float4 position : SV_Position,
                        inout float2 texCoord : TEXCOORD0)
{
    position = mul(position, ViewMatrix);
    position = mul(position, TransformMatrix);
}

technique Shader
{
    pass pass0
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}