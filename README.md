Hardware accelerated graphics on RGB LED display with OpenGL and the Raspberry Pi GPIO
=====
A GLSL ES 1.2 shader rendering engine based on the excellent rpi-rgb-led-matrix library. 

[![Rings Demo](http://i.imgur.com/bT9bxltm.png)](https://youtu.be/KcxGaBQGMv4 "Rings Demo")
[![Fire Demo](https://i.imgur.com/S7eQXKKm.png)](https://youtu.be/B6gl0P_Xw4Y "Fire Demo")
[![Lightning2 Demo](http://i.imgur.com/8GKdwHJm.png)](https://youtu.be/Bzn-x0ncxcU "Lightning2 Demo")
[![Lightning Demo](http://i.imgur.com/CjdX3RPm.png)](https://youtu.be/ZzeQzPtST1I "Lightning Demo")

## Setup
This project is based on mono/C# and has been tested on Raspbian Stretch. 
You will want to make sure you install the full raspbian image with x-server not the lite version.
After installing the raspbian strech image you need to enable OpenGL in raspi-config (Advanced Menu).
```
$ sudo raspi-config
```
Next, the rpi-rgb-led-matrix library uses the PWM chip on the pi for timing which is also used for sound on the raspberry PI, since having sound enabled may interfere with the PWM you should disable sound for the best results. You can disable sound in /boot/config.txt. comment or remove the line dtparam=audio=on to disable the audio
```
$ sudo nano /boot.config.txt
```
Finally, to build the project you will need to install the mono complete package which include the mcs compiler we will use to build the project and the .NET environment we will need to run it. 
```
$ sudo apt-get install mono-complete
```
To build the project run the build script
```
$ ./build.sh
```
after building you should see some DLL files and rpi-rgbledmatrix-glsl.exe in the root folder of the project.
You can now run the shader engine via:
```
sudo mono rpi-rgbledmatrix-glsl.exe
```

### Notes
If you are running your raspberry pi headless without an hdmi display and using ssh to login, you may need to set the default display manually on the terminal first before running your shaders.
```
export DISPLAY=:0
export XAUTHORITY=~/.Xauthority
```

### Hardware
The details of connecting a RGB LED matrix to the raspberry PI GPIO are covered in the [rpi-rgb-led-matrix github page.](https://github.com/hzeller/rpi-rgb-led-matrix/blob/master/wiring.md) 

### Changing parameters via command-line flags

The main option is the fragment-shader flag. Use this to pass in your glsl code to see it run on your RGB LED panel.
```
--fragment-shader: path of a fragment shader to compile and run
```
We also supply a option to specify the vertex shader if needed.
```
--vertex-shader: path of a vertex shader to compile and run (not typically used)
```
A display mode option is also provided in case you want to run test your shader in a regular application window instead of on the matrix display. options currently are "ledmatrix" and "window"
```
--display-mode: type of display mode to use. Default "ledmatrix"
```
since some displays are wide, depending on your particular shader it may be desirable to swap the x and y coordinates to make your shader fit the display better.
```
--swap-xy: a quick flag to swap the x and y coordinates of the shader
```
These flags allow you to define the total width and height of your display in pixels/LEDS
```
--display-width: Default 32
--display-height: Default 32
```

### LED Matrix display parameters and command-line flags

For the most part, all of the command line flags supported in the rpi-rgb-led-matrix
are also supported the the GLSL shader renderer so that you don't have to re-compile 
if you want to use a different panel layout or configuration.
Some flags might need to be changed for your particular kind of panel.

Here is a little run-down of what these command-line flags do and when you'd
like to change them.

First things first: if you have a different wiring than described in
[wiring](./wiring.md), for instance if you have an Adafruit HAT, you can
choose these here:

```
--led-gpio-mapping=<gpio-mapping>: Name of GPIO mapping used. Default "regular"
```

This can have values such as
  - `--led-gpio-mapping=regular` The standard mapping of this library, described in the [wiring](https://github.com/hzeller/rpi-rgb-led-matrix/blob/master/wiring.md) page.
  - `--led-gpio-mapping=adafruit-hat` standard Adafruit HAT or
  - `--led-gpio-mapping=adafruit-hat-pwm` Adafruit HAT with the anti-flicker hardware mod [described here](https://github.com/hzeller/rpi-rgb-led-matrix/blob/master/README.md#improving-flicker).

The next most important flags describe the type and number of displays connected

```
--led-rows=<rows>         : Panel rows. 8, 16, 32 or 64. (Default: 32).
--led-chain=<chained>     : Number of daisy-chained panels. (Default: 1).
--led-parallel=<parallel> : For A/B+ models or RPi2,3b: parallel chains. range=1..3 (Default: 1).
```

These are the most important ones: here you choose how many panels you have
connected and how many rows are in each panel. Panels can be chained (each panel
has an input and output connector, see the
[wiring documentation](wiring.md#chains)) -- the `--led-chain` flag tells the
library how many panels are chained together. The newer Raspberry Pi's allow
to connect multiple chains in parallel, the `--led-parallel` flag tells it how
many there are.

This illustrates what each of these parameters mean:

<a href="https://github.com/hzeller/rpi-rgb-led-matrix/blob/master/wiring.md"><img src="https://github.com/hzeller/rpi-rgb-led-matrix/raw/master/img/coordinates.png"></a>

```
--led-brightness=<percent>: Brightness in percent (Default: 100).
```

Self explanatory.


```
--led-pwm-bits=<1..11>    : PWM bits (Default: 11).
```

The LEDs can only be switched on or off, so the shaded brightness perception
is achieved via PWM (Pulse Width Modulation). In order to get a good 8 Bit
per color resolution (24Bit RGB), the 11 bits default per color are good
(why ? Because our eyes are actually perceiving brightness logarithmically, so
we need a lot more physical resolution to get 24Bit sRGB).

With this flag, you can change how many bits it should use for this; lowering it
means the lower bits (=more subtle color nuances) are omitted.
Typically you might be mostly interested in the extremes: 1 Bit for situations
that only require 8 colors (e.g. for high contrast text displays) or 11 Bit
for everything else (e.g. showing images or videos). Why would you bother at all ?
Lower number of bits use slightly less CPU and result in a higher refresh rate.

```
--led-show-refresh        : Show refresh rate.
```

This shows the current refresh rate of the LED panel, the time to refresh
a full picture. Typically, you want this number to be pretty high, because the
human eye is pretty sensitive to flicker. Depending on the settings, the
refresh rate with this library are typically in the hundreds of Hertz but
can drop low with very long chains. Humans have different levels of perceiving
flicker - some are fine with 100Hz refresh, others need 250Hz.
So if you are curious, this gives you the number (shown on the terminal).

The refresh rate depends on a lot of factors, from `--led-rows` and `--led-chain`
to `--led-pwm-bits` and `--led-pwm-lsb-nanoseconds`. If you are tweaking these
parameters, showing the refresh rate can be a useful tool.

```
--led-scan-mode=<0..1>    : 0 = progressive; 1 = interlaced (Default: 0).
```

This switches from progressive scan and interlaced scan. The latter might
look be a little nicer when you have a very low refresh rate.

```
--led-pwm-lsb-nanoseconds : PWM Nanoseconds for LSB (Default: 130)
```

This allows to change the base time-unit for the on-time in the lowest
significant bit in nanoseconds.
Lower values will allow higher frame-rate, but will also negatively impact
qualty in some panels (less accurate color or more ghosting).

Good values for full-color display (PWM=11) are somewhere between 100 and 300.

If you you use reduced bit color (e.g. PWM=1) and have sharp contrast
applications, then higher values might be good to minimize ghosting.

How to decide ? Just leave the default if things are fine. But some panels have
trouble with sharp contrasts and short pulses that results
in ghosting. It is particularly apparent in situations such as bright text
on black background. In these cases increase the value until you don't see
this ghosting anymore.

The following example shows how this might look like:

Ghosting with low --led-pwm-lsb-nanoseconds  | No ghosting after tweaking
---------------------------------------------|------------------------------
![](https://github.com/hzeller/rpi-rgb-led-matrix/raw/master/img/text-ghosting.jpg)                   |![](https://github.com/hzeller/rpi-rgb-led-matrix/raw/master/img/text-no-ghosting.jpg)

If you tweak this value, watch the framerate while playing with this number.

```
--led-slowdown-gpio=<0..2>: Slowdown GPIO. Needed for faster Pis and/or slower panels (Default: 1).
```

The Raspberry Pi 2 and 3 are putting out data too fast for almost all LED panels
I have seen. In this case, you want to slow down writing to GPIO. Zero for this
parameter means 'no slowdown'.

The default 1 (one) typically works fine, but often you have to even go further
by setting it to 2 (two). If you have a Raspberry Pi with a slower processor
(Model A, A+, B+, Zero), then a value of 0 (zero) might work and is desirable.

```
--led-no-hardware-pulse   : Don't use hardware pin-pulse generation.
```

This library uses a hardware subsystem that also is used by the sound. You can't
use them together. If your panel does not work, this might be a good start
to debug if it has something to do with the sound subsystem (see Troubleshooting
section). This is really only recommended for debugging; typically you actually
want the hardware pulses as it results in a much more stable picture.

```
--led-inverse             : Switch if your matrix has inverse colors on.
--led-rgb-sequence        : Switch if your matrix has led colors swapped (Default: "RGB")
```

These are if you have a different kind of LED panel in which the logic of the
color bits is reversed (`--led-inverse`) or where the Red, Green and Blue LEDs
are mixed up (`--led-rgb-sequence`). You know it when you see it.
