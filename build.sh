#!/bin/bash

DIR="./rpi-rgb-led-matrix"
if [ ! -d $DIR ]; then
	git clone https://github.com/digital-synapse/rpi-rgb-led-matrix.git
	cd $DIR 
	git pull 
	make build-csharp
	cp ./bindings/c#/RGBLedMatrix.dll ../RGBLedMatrix.dll
	cp ./lib/librgbmatrix.so.1 ../librgbmatrix.so
	cd ..
fi

DIR="./opentk"
if [ ! -f OpenTK.dll ]; then
	mkdir $DIR
	cd $DIR
	wget -O opentk.zip https://www.nuget.org/api/v2/package/OpenTK/2.0.0
	unzip opentk.zip
	mv ./content/OpenTK.dll.config ../
	mv ./lib/net20/* ../	
	cd ..
	#cleanup
	rm -rf $DIR
fi

cd ./src
make
cd ..