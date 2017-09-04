references= 			-reference:../OpenTK.dll \
						-reference:../RGBLedMatrix.dll \
						-reference:System.Drawing.dll

src=				Program.cs \
					IShaderRenderer.cs \
					LEDMatrixRenderer.cs \
					Pixel.cs \
					ShaderRendererCore.cs \
					WindowRenderer.cs
					
exe=				../rpi-rgbledmatrix-glsl.exe

all: $(exe) 
		
$(exe) : $(src)
	mcs $(references) -out:$@ $(src)
	
.PHONY : all
