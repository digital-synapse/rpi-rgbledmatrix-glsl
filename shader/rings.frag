#version 120 

//precision mediump float;
uniform vec2  resolution;     
uniform vec2  mouse;          
uniform float time;           
uniform sampler2D backbuffer; 

void main(){
    vec2 p = (gl_FragCoord.xy * 2.0 - resolution) / min(resolution.x, resolution.y);
    p = vec2(p.x+cos(time*2.)*cos(time), p.y+cos(time*2.)*sin(time));
    p = mat2(cos(1.57*time), -sin(1.57*time), sin(1.57*time), cos(1.57*time))*p;
    float f = exp(sin(sqrt(p.x*p.x+p.y*p.y + sin(time)*p.x*p.x*(p.x*sin(2.*time))*p.y) * 20.0+time));

	mat3 m = mat3(exp(cos(time)), -exp(sin(time)), 0., exp(sin(time)), exp(cos(time)),0. ,0., 0., 1.);

    gl_FragColor = vec4(
    	vec3(
    	f*abs(cos(2.*time)*cos(time)),
    	f*abs(cos(2.*time)*sin(time)),
    	f*abs(sin(2.*time))*m),
    	1.0);
	
}
