
using glshader_test.renderer;
using OpenTK;
using rpi_rgb_led_matrix_sharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glshader_test
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            // parse command line options
            var a = new List<string>(args);
            var config = new ShaderRendererConfig()
            {
                FragmentShader = findArgValue(a, "fragment-shader", "f")
                    ?? ("shader" + Path.DirectorySeparatorChar + "lightning.frag"),
                VertexShader = findArgValue(a, "vertex-shader", "v")
                    ?? ("shader" + Path.DirectorySeparatorChar + "standard.vert"),
                ShaderRendererType = ((findArgValue(a, "display-mode", "disp") ?? "ledmatrix") == "ledmatrix")
                    ? ShaderRendererType.LedMatrix : ShaderRendererType.Window,
                SwapXY = findArg(a, "swap-xy", "s"),                
                Width = int.Parse((findArgValue(a, "display-width", "w") ?? "0")),
                Height = int.Parse((findArgValue(a, "display-height", "h") ?? "0")),
                RGBLedMatrixOptions = new RGBLedMatrixOptions()
                {
                    ChainLength = int.Parse((findArgValue(a, "led-chain-length", "c") ?? "1")),
                    Brightness = int.Parse((findArgValue(a, "led-brightness", "b") ?? "100")),                    
                    DisableHardwarePulsing = findArg(a, "led-no-hardware-pulse", "pulse"),
                    InverseColors = findArg(a, "led-inverse", "i"),                   
                    HardwareMapping = findArgValue(a, "led-gpio-mapping", "mapping") ?? "regular",                    
                    LedRgbSequence = findArgValue(a, "led-rgb-sequence", "rgbs") ?? "rgb",
                    Parallel = int.Parse((findArgValue(a, "parallel", "par")) ?? "1"),
                    PwmBits = int.Parse((findArgValue(a,"pwm-bits","pwm")) ?? "11"),
                    PwmLsbNanoseconds = int.Parse((findArgValue(a, "led-pwm-lsb-nanoseconds", "lsb")) ?? "130"),
                    Rows = int.Parse((findArgValue(a, "rows", "r")) ?? "32"),
                    ScanMode = int.Parse((findArgValue(a, "led-scan-mode", "scan")) ?? "1")
                    //ShowRefreshRate =findArg(a,"led-show-refresh","fps")
                }

            };

            // run shader
            IShaderRenderer renderer=null;
            switch (config.ShaderRendererType)
            {
                case ShaderRendererType.LedMatrix:
                    renderer = new LEDMatrixRenderer();
                    break;
                case ShaderRendererType.Window:
                    renderer = new WindowRenderer();
                    break;
            }                           
            renderer.Run(config);            
            return 0;
        }


        // helper method to parse command line args
        private static string findArgValue(List<string> args, string longName, string shortName)
        {
            string value = null;
            var i = args.FindIndex(x => x.StartsWith("--" + longName) || x.StartsWith("-" + shortName));
            if (i != -1)
            {
                if (args[i].Contains("="))
                {
                    value = args[i].Split('=')[1].Replace("\"", string.Empty);
                }
                else if (i + 1 < args.Count)
                {
                    value = args[i + 1].Replace("\"", string.Empty);
                }
            }
            return value;
        }
        private static bool findArg(List<string> args, string longName, string shortName)
        {
            string value = null;
            var i = args.FindIndex(x => x.StartsWith("--" + longName) || x.StartsWith("-" + shortName));
            return (i != -1);            
        }
    }
}
