shader_type canvas_item;

const vec2 SCREEN_SIZE = vec2(416.0, 240.0);

void fragment()
{		
	vec3 diffuse = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
	vec2 pixelPos = SCREEN_UV * SCREEN_SIZE;

	COLOR = vec4(diffuse, 1.0);

	if (mod(pixelPos.y, 4.0) > 2.0) 
	{
		COLOR *= 0.8;
	}
}