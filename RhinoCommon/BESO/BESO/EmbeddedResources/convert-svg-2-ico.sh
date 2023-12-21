#!/bin/sh

echo "*** One way: use Inkscape ***"
inkscape -w 16 -h 16 -o 16.png icon.svg
inkscape -w 24 -h 24 -o 24.png icon.svg
inkscape -w 32 -h 32 -o 32.png icon.svg
inkscape -w 48 -h 48 -o 48.png icon.svg
inkscape -w 256 -h 256 -o 256.png icon.svg
magick convert 16.png 24.png 32.png 48.png 256.png icon.ico
magick identify icon.ico

echo "*** Other way ***"
magick convert -background none icon.svg -resize 16x16   -depth 32 16-32.png
magick convert -background none icon.svg -resize 24x24   -depth 32 24-32.png
magick convert -background none icon.svg -resize 32x32   -depth 32 32-32.png
magick convert -background none icon.svg -resize 48x48   -depth 32 48-32.png
magick convert -background none icon.svg -resize 256x256 -depth 32 256-32.png
magick convert -background none 16-32.png 24-32.png 32-32.png 48-32.png 256-32.png -depth 32 icon.ico
magick identify icon.ico

echo "*** Other way: simpler ***"
magick convert -background none icon.svg -define icon:auto-resize="256,48,32,24,16" -depth 32 icon.ico
magick identify icon.ico

