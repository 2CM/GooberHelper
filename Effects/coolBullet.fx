#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

DECLARE_TEXTURE(text, 0);

float4 SpritePixelShader(float4 inPosition : SV_POSITION, float4 inColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0 {
    float4 tex = SAMPLE_TEXTURE(text, float2(uv.x, uv.y));
    float factor = abs(tex.r - tex.g);

    return lerp(float4(tex.g, tex.g, tex.g, tex.a), inColor, factor) + 0.5 * float4(uv.x, uv.y, 1, 1) * tex.a;
}

void SpriteVertexShader(inout float4 position : SV_Position, inout float4 color : COLOR0, inout float2 uv : TEXCOORD0) {
    position = mul(position, ViewMatrix);
    position = mul(position, TransformMatrix);

    // position.y += sin(position.x * 100.0) * 1.0 + cos(position.y * 100.0) * 1.0;
}

technique Shader {
	pass Buh {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}