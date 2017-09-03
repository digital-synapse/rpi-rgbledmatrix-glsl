using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Diagnostics;
using System.Collections.Generic;
using OpenTK.Graphics;
using SDPixelFormat = System.Drawing.Imaging.PixelFormat;
using glshader_test.renderer;
using System.Runtime.InteropServices;

namespace glshader_test
{
    public class ShaderRenderer : GameWindow
    {
        int _shaderId;
        int _vao;
        int _glbuf;
        int _fragObj;
        int _vertexObj;
        Vector3[] _triangleVert;

        private int BufferSize;
        private ShaderRendererConfig config;

        public ShaderRenderer(ShaderRendererConfig config) : base(config.Width, config.Height, 
            new GraphicsMode(), "ShaderRenderer",
            GameWindowFlags.Default,DisplayDevice.Default,2,1,GraphicsContextFlags.ForwardCompatible)
        {
            BufferSize = config.Width * config.Height * 4;
            this.config = config;

            _triangleVert = new Vector3[] {
                new Vector3(-1f, -1f, 0.0f),
                new Vector3(1f, -1f, 0.0f),
                new Vector3(-1f, 1f, 0.0f),
                new Vector3(1f, 1f,0.0f)
            };

        }
        
        DateTime start = DateTime.Now;
        public void RenderFrame()
        {
            // update uniforms
            var time = (DateTime.Now - start).TotalSeconds;
            GL.Uniform1(GL.GetUniformLocation(_shaderId, "time"), (float)time);

            GL.Uniform2(GL.GetUniformLocation(_shaderId, "resolution"),
                (float)ClientSize.Width, (float)ClientSize.Height);

            // render
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.BindVertexArray(0);

            GL.Flush();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            RenderFrame();
            SwapBuffers();            
        }

        public IEnumerable<Pixel> IterateBitmap()
        {
            //var pixels = new List<Pixel>();

            using (Bitmap bmp = GrabScreenshot())
            {

                // Iterate over bitmap pixels. When a non-black and non-white pixel is found, update the initial height array
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color c = bmp.GetPixel(x, y);  // Slow! But can refine the area to iterate at a later date
                        

                        //if (c.R + c.G + c.B > 0)
                        {
                            yield return new Pixel() { x = x, y=y, r = c.R, g = c.G, b = c.B};                            
                            //pixels.Add(new Pixel() { x = x, y = y, r = c.R, g = c.G, b = c.B });
                        }
                    }
                }
            }
            //return pixels;
        }

        public IEnumerable<Pixel> IterateBufferData()
        {
            //var pixels = new List<Pixel>();

            var data = GrabBackBuffer();
            int i = 0;
            for (int y = ClientSize.Height; y > 0; --y)                
            {
                for (int x = 0; x < ClientSize.Width; x++)
                {
                    byte b = data[i++];
                    byte g = data[i++];
                    byte r = data[i++];
                    i++;
                    yield return new Pixel() { x = x, y = y, r = r, g = g, b = b };                                        
                }
            }
        }

        public Bitmap GrabScreenshot()
        {
              
            var bmp = new Bitmap(ClientSize.Width, ClientSize.Height, SDPixelFormat.Format32bppArgb);
            var mem = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, SDPixelFormat.Format32bppArgb);
            GL.PixelStore(PixelStoreParameter.PackRowLength, mem.Stride / 4);
            GL.ReadPixels(0, 0, bmp.Width, bmp.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, mem.Scan0);
            bmp.UnlockBits(mem);
            return bmp;
                        
        }     
        public byte[] GrabBackBuffer()
        {
            var bytes = new byte[BufferSize];
            var buff = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.PixelPackBuffer, buff);
            GL.BufferData(BufferTarget.PixelPackBuffer, BufferSize, IntPtr.Zero, BufferUsageHint.StaticRead);
            GL.BindBuffer(BufferTarget.PixelPackBuffer, buff);
            GL.ReadPixels(0, 0, ClientSize.Width, ClientSize.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            IntPtr pixels = GL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly);
            Marshal.Copy(pixels, bytes, 0, BufferSize);
            GL.UnmapBuffer(BufferTarget.PixelPackBuffer);
            return bytes;
        }

        public override void Dispose()
        {
            GL.DeleteProgram(_shaderId);
            GL.DeleteShader(_vertexObj);
            GL.DeleteShader(_fragObj);
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_glbuf);
            base.Dispose();
        }

        public void Init()
        {

            Console.WriteLine(string.Format("OpenGL version: {0}", GL.GetString(StringName.Version)));
            GL.ClearColor(Color.Purple);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, ClientSize.Width, 0, ClientSize.Height, -1, 1);
            GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
            GL.PointSize(3f);

            // speed optimization
            GL.Disable(EnableCap.AlphaTest | EnableCap.Fog | EnableCap.Dither | EnableCap.Blend | EnableCap.ScissorTest | EnableCap.StencilTest );

            _vertexObj = GL.CreateShader(ShaderType.VertexShader);
            _fragObj = GL.CreateShader(ShaderType.FragmentShader);
            int statusCode;
            string info;

            GL.ShaderSource(_vertexObj, File.ReadAllText(config.VertexShader));            
            GL.CompileShader(_vertexObj);
            info = GL.GetShaderInfoLog(_vertexObj);
            Console.WriteLine(string.Format("{1} compile: {0}", string.IsNullOrEmpty(info) ? "ok" : info, config.VertexShader));
            GL.GetShader(_vertexObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            GL.ShaderSource(_fragObj, File.ReadAllText(config.FragmentShader));
            GL.CompileShader(_fragObj);
            info = GL.GetShaderInfoLog(_fragObj);
            Console.WriteLine(string.Format("{1} compile: {0}", string.IsNullOrEmpty(info) ? "ok" : info, config.FragmentShader));
            GL.GetShader(_fragObj, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1) throw new ApplicationException(info);

            _shaderId = GL.CreateProgram();
            GL.AttachShader(_shaderId, _fragObj);
            GL.AttachShader(_shaderId, _vertexObj);
            GL.LinkProgram(_shaderId);
            info = GL.GetProgramInfoLog(_shaderId);
            Console.WriteLine(string.Format("link program: {0}", string.IsNullOrEmpty(info) ? "ok" : info));
            GL.UseProgram(_shaderId);
            info = GL.GetProgramInfoLog(_shaderId);
            Console.WriteLine(string.Format("use program: {0}", string.IsNullOrEmpty(info) ? "ok" : info));

            _vao = GL.GenVertexArray();
            _glbuf = GL.GenBuffer();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _glbuf);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, 
                new IntPtr(Vector3.SizeInBytes * _triangleVert.Length), 
                _triangleVert, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Init();
            
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            /*
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            */
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();
        }
    }
}
