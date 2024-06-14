#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 textureSize;

DECLARE_TEXTURE(field, 0);


float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
    float  L = tex2D(fieldSampler, uv - float2(1, 0) / textureSize.x).x;   
    float  R = tex2D(fieldSampler, uv + float2(1, 0) / textureSize.x).x;   
    float  B = tex2D(fieldSampler, uv - float2(0, 1) / textureSize.y).y;   
    float  T = tex2D(fieldSampler, uv + float2(0, 1) / textureSize.y).y; 
    float4 C = tex2D(fieldSampler, uv);

    if(uv.x - 1.0 / textureSize.x < 0.0) { L = -C.x; }
    if(uv.x + 1.0 / textureSize.x > 1.0) { R = -C.x; }
    if(uv.y - 1.0 / textureSize.y < 0.0) { B = -C.y; }
    if(uv.y + 1.0 / textureSize.y > 1.0) { T = -C.y; }

    return float4(0.5 * (R - L + T - B), 0, 0, 0);
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