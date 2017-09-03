using glshader_test.renderer;
using rpi_rgb_led_matrix_sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace glshader_test
{
    public class LEDMatrixRenderer : IShaderRenderer
    {
        RGBLedMatrix matrix = null;
        RGBLedCanvas canvas = null;
        ShaderRendererConfig config;

        public async void Run(ShaderRendererConfig config)
        {
            this.config = config;
            try
            {
                if (config.Width==0 && config.Height == 0)
                {
                    config.Width = config.RGBLedMatrixOptions.ChainLength *32;
                    config.Height = config.RGBLedMatrixOptions.Rows * config.RGBLedMatrixOptions.Parallel ;
                }

                //if (config.RGBLedMatrixOptions.ChainLength==0 && config.Width % 32 == 0 && config.RGBLedMatrixOptions.Rows==0)
                //    matrix = new RGBLedMatrix(config.RGBLedMatrixOptions.Rows, config.RGBLedMatrixOptions.ChainLength, config.RGBLedMatrixOptions.Parallel); 
                //else
                    matrix = new RGBLedMatrix(config.RGBLedMatrixOptions);

                canvas = matrix.CreateOffscreenCanvas();//matrix.GetCanvas();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Problem initializing RGBLedMatrix");
                return;
            }

            if (config.SwapXY)
            {
                var tmp = config.Width;
                config.Width = config.Height;
                config.Height = tmp;
            }
            using (var shaderRenderer = new ShaderRenderer(config))
            {
                try
                {
                    Console.WriteLine("");
                    shaderRenderer.Init();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }


                var sw = new Stopwatch();
                sw.Start();
                long frameTime = 0;
                int frames = 0;
                float avgFps = 0;

                Console.WriteLine();
                var cursor = Console.CursorTop;
                while (!Console.KeyAvailable)
                {
                    // render frame                                  
                    shaderRenderer.RenderFrame();                    
                    updateLedMatrix(shaderRenderer.IterateBufferData());
                    canvas = matrix.SwapOnVsync(canvas);

                    // calculate FPS
                    frameTime += sw.ElapsedMilliseconds;
                    sw.Restart();
                    frames++;
                    if (frameTime >= 1000)
                    {
                        Console.SetCursorPosition(0, cursor);
                        Console.Write("FPS: " + frames);
                        avgFps = (avgFps + frames) / 2f;
                        frames = 0;
                        frameTime = 0;
                    }

                }
                Console.WriteLine();
                Console.WriteLine("AVERAGE FPS: " + avgFps);

            } // using shaderRenderer
            Console.Read();
            return;
        }

        private void updateLedMatrix(IEnumerable<Pixel> pixels)
        {
            //canvas.Clear();
            if (config.SwapXY)
            {
                foreach (var p in pixels)
                    canvas.SetPixel(p.y, p.x, new Color(p.r, p.g, p.b));
            }
            else
            {
                foreach (var p in pixels)
                    canvas.SetPixel(p.x, p.y, new Color(p.r, p.g, p.b));
            }
        }
    }
}
