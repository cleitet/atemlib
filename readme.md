# Blackmagic ATEM C# Library

## Introduction

This tool was based on the MIT-licenced project at https://github.com/mintopia/atemlib

This library is a collection of tools for working with Blackmagic ATEM video switchers. It is intended to be used as part of automation solutions.

MediaUpload.exe allows you to upload images to specific slots in a BlackMagic ATEM switcher's media pool.

MediaUpload.exe also allows you to upload images as clips to the clip pool for models that support clips. A clip is multiple files, each it's own frame that will be played back by the switcher. The tool will look for image files in the directory if a directory is given as an argument rather than a filename.

MediaPool.exe lists all the media in the switcher's media pool.

## Usage

### Media Upload

```
Usage: mediaupload.exe [options] <hostname> <slot> <filename>
Uploads an image or clip to a BlackMagic ATEM switcher

Arguments:

 hostname        - The hostname or IP of the ATEM switcher
 slot            - The number of the media slot to upload to
 file/directory  - The filename of the image to upload as still, or directory with images to upload as a clip

Options:

 -h, --help      - This help message
 -d, --debug     - Debug output
 -v, --version   - Version information
 -n, --name      - The name for the item in the media pool

 Image Format:

The image(s) must be the same resolution as the switcher. Accepted formats are BMP, JPEG, GIF, PNG and TIFF. Alpha channels are supported.
```

Example:

To upload myfile.png to Slot 1 on a switcher at 192.168.0.254:

    MediaUpload.exe 192.168.0.254 1 myfile.png

To upload the image files in c:\images to media pool 1 at 192.168.0.254:
    MediaUpload.exe 192.168.0.254 1 c:\images

### Media Pool

```
MediaPool.exe [options] <hostname>

Arguments:

 hostname        - The hostname or IP of the ATEM switcher

Options:

 -h, --help      - This help message
 -d, --debug     - Debug output
 -v, --version   - Version information
 -f, --format    - The output format. Either xml, csv, json or text
```

Example:

To see what's in the media pool for a switcher at 192.168.0.254:

    MediaPool.exe 192.168.0.254

To view the output in JSON format:

    MediaPool.exe -f json 192.168.0.254

## Requirements

 - [Microsoft .NET Framework 4.5](http://www.microsoft.com/en-gb/download/details.aspx?id=30653) 
 - [Blackmagic ATEM Switchers Update 8.5](https://www.blackmagicdesign.com/uk/support/family/atem-live-production-switchers) or later

## Supported Image Formats

The Windows GD+ library is used for image manipulation. This currently supports:

  - PNG
  - BMP
  - JPEG
  - GIF
  - TIFF

Alpha channels are supported and will be included in the images sent to the switcher.

Images will need to be the same resolution as the switcher. Running in debug mode you can see the detected resolution on the switcher.

## Notes

This has been tested with a Blackmagic Design ATEM Production Studio 4K. I do not have access to any other switchers to test with, but if they use version 6.2 or greater of the SDK, then they should work.

## ChangeLog

### Version 2.1.0 - 2020-10-06:
 - Add support for media clips
 - Built against Blackmagic Switcher SDK 8.5

### Version 2.0.2 - 2018-02-02:
 - Add support for NTSC SD

### Version 2.0.1 - 2018-02-01:
 - Built against Blackmagic Switcher SDK 7.3

### Version 2.0.0 - 2014-12-24:
 - Rebuilt from decompiled source of original binary
 - Added enumerating of the media pool

### Version 1.0.1 - 2014-09-22:
 - Moved switcher functions into a separate library to allow development of more tools
 - Slight change to arguments
 - Add support for specifying the name of the image when uploading it

### Version 1.0.0 - 2014-09-21:
 - Initial version

## MIT License

Copyright (C) 2016 by Jessica Smith 
Copyright (C) 2020 Chris Leitet

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
