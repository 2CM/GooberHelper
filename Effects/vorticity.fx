#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 textureSize;
uniform float curl;
uniform float timestep;

DECLARE_TEXTURE(velocity, 0);
DECLARE_TEXTURE(divergenceCurl, 1);


float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
    float L = tex2D(divergenceCurlSampler, uv - float2(1, 0) / textureSize.x).y;
    float R = tex2D(divergenceCurlSampler, uv + float2(1, 0) / textureSize.x).y;
    float T = tex2D(divergenceCurlSampler, uv - float2(0, 1) / textureSize.y).y;
    float B = tex2D(divergenceCurlSampler, uv + float2(0, 1) / textureSize.y).y;
    float C = tex2D(divergenceCurlSampler, uv).y;

    float2 force = 0.5 * float2(abs(T) - abs(B), abs(R) - abs(L));
    force /= length(force) + 0.0001;
    force *= curl * C;
    force.y *= -1.0;

    float2 velocity = tex2D(velocitySampler, uv).xy;
    velocity += force * timestep;
    // velocity = min(max(velocity, -1.0), 1.0);
    return float4(velocity, 0.0, 1.0);
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