// Lightning
// By: Brandon Fogerty
// bfogerty at gmail dot com 
// xdpixel.com


// MORE MODS BY 27


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
     float v = 0.0;
     v += noise(p*1.0)*.5;
     v -= noise(p*2.)*.25;
     v += noise(p*4.)*.125;
     return v * -1.25;
}

void main( void ) 
{

	vec2 uv = ( gl_FragCoord.xy /3. / resolution.xy ) * 2.0 -0.15 ;
	uv.x *= resolution.x/resolution.y;
	uv.xy = uv.yx;
	float j = 2.5;
	vec3 finalColor = vec3( 0.0 );
	for( int i=2; i < 13; ++i )
	{
		float hh =  0.5 - float(i);
		
		float t = abs(1.0 / ((uv.x + fbm( uv + (time + 40.)/float(i)))*200.));
		finalColor +=  t * vec3( sin(hh-j), fract(t/j), cos(j-t) );
		j = float(i)-hh;
	}
	
	gl_FragColor = vec4( sqrt(finalColor), 1.0 );

}