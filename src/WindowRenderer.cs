using glshader_test.renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glshader_test
{
    public class WindowRenderer : IShaderRenderer
    {
        public void Run(ShaderRendererConfig config)
        {
            if (config.Width==0 && config.Height == 0)
            {
                config.Width = 640;
                config.Height = 320;
            }
            using (var shaderRenderer = new ShaderRenderer(config))
            {
                shaderRenderer.Run(30);
            }
        }

   
    }
}
