using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpi_rgb_led_matrix_sharp;

namespace glshader_test.renderer
{
    public interface IShaderRenderer
    {        
        void Run(ShaderRendererConfig config);
    }

    public struct ShaderRendererConfig
    {
        public string FragmentShader;
        public string VertexShader;
        public ShaderRendererType ShaderRendererType;
        public bool SwapXY;
        public int Width;
        public int Height;        
        public RGBLedMatrixOptions RGBLedMatrixOptions;
    }

    public enum ShaderRendererType
    {
        LedMatrix,
        Window
    }
}
