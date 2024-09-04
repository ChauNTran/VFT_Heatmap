//--------------------------------------------------
// MakeMeSmall
// @ 2017 furaga
// Version 1.0.0
//--------------------------------------------------

Simple image downscaling methods for Unity textures2d. 

# Includes

  * Subsampling
  * Box downscaling (= Average filter)
  * Bilinear
  * Bicubic
  * Gaussian (5x5)
  * Perceptualy based donwscaling [A. C. Oztireli et al., SIGGRAPH 2015]
  * Content-Adaptive Image Downscaling [J. Kopf et al., SIGGRAPH Asia 2013].

Perceptualy-based donwscaling is slow, but would produce best quality when you downscale real-world images.
Content-aware downscaling is extremely slow, but would produce best quality when you generate a pixel art by donwscaling a cartoon image.

All of them are implemented in C#.
It does not use shaders nor materials.


# How to use

1) Import MakeMeSmall.unitypackage

2) If you want, just open the following demo scenes to see demonstrations.

  * ./MakeMeSmall/Demo/Scenes/1_Simple
    * You can run downscaling methods by clicking buttons.

  * ./MakeMeSmall/Demo/Scenes/2_FileConverter: 
    * You can load a PNG image on your disc, downscale it, and save the downscaled image on your disc.
	
  * ./MakeMeSmall/Demo/Scenes/3_FolderConverter:
    * You can downscale all PNG images in a folder.

3) To downscale a texture in your C# script, just do like:

```   
    Texture2D outputTexture2D = MakeMeSmall.Downscale.Subsample(inputTexture2D, new Vector3(w, h, 0));
```   
		  
# Version History
1.0.0
- First release


# Contact
furaga.fukahori@gmail.com
