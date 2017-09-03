// Lightning
// By: Brandon Fogerty
// bfogerty at gmail dot com 
// xdpixel.com


// EVEN MORE MODS BY 27


#ifdef GL_ES
precision lowp float;
#endif

uniform float time;
uniform vec2 mouse;
uniform vec2 resolution;


float Hash( vec2 p)
{
     vec3 p2 = vec3(p.xy,1.0);
    return fract(sin(dot(p2,vec3(37.1,61.7, 12.4)))*3758.5453123);
}

float noise(in vec2 p)
{
    vec2 i = floor(p);
     vec2 f = fract(p);
     f *= f * (3.0-2.0*f);

    return mix(mix(Hash(i + vec2(0.,0.)), Hash(i + vec2(1.,0.)),f.x),
               mix(Hash(i + vec2(0.,1.)), Hash(i + vec2(1.,1.)),f.x),
               f.y);
}

float fbm(vec2 p)
{
     float v = -0.27;
     v += noise(p*1.0)*.5;
     v += noise(p*2.)*.25;
     v += noise(p*4.)*.125;
     return v * 1.0;
}

void main( void ) 
{

	vec2 uv = ( gl_FragCoord.xy / resolution.xy ) * 2.0 - 1.0;
	uv.x *= resolution.x/resolution.y;
	
	float count = 27.0;
	float lim = abs(count * sin(time)) + 2.0;
		
	vec3 finalColor = vec3( 0.0 );
	for( int i=1; i > 0; ++i )
	{
		if(i > int(lim)) break;
		float val = float(i);
		
		float pct = val / lim;
		float t = abs(pct*pct / ((uv.x + fbm( uv + (time + val + 1000.0)/val)) * (lim * 10.0)));
		finalColor +=  t * vec3( pct+0.1, 0.5, 2.0 );

	}
	
	gl_FragColor = vec4( finalColor, 1.0 );

}