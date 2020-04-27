shader_type canvas_item;

const vec2 SCREEN_SIZE = vec2(416.0, 240.0);
uniform float progress = 0.0;
uniform float ffwd_separation = 0.0;

void fragment()
{	
	vec2 screen_uv = SCREEN_UV;
	vec2 pixelPos = screen_uv * SCREEN_SIZE;
	
	if (mod(pixelPos.y, 4.0) > 2.0) 
	{
		pixelPos.x -= ffwd_separation;
	}
	else
	{
		pixelPos.x += ffwd_separation;
	}

	vec3 diffuse = texelFetch(SCREEN_TEXTURE, ivec2(pixelPos), 0).rgb;
	//vec3 diffuse = texture(SCREEN_TEXTURE, screen_uv).rgb;

	if (mod(pixelPos.y, 4.0) > 2.0) 
	{
		if (screen_uv.x > (1.0 - progress))
			COLOR = vec4(0.0, 0.0, 0.0, 1.0);
		else
			COLOR = vec4(diffuse * 0.8, 1.0);
	} 
	else
	{
		if (screen_uv.x < progress)
			COLOR = vec4(0.0, 0.0, 0.0, 1.0);
		else
			COLOR = vec4(diffuse, 1.0);
	} 
}