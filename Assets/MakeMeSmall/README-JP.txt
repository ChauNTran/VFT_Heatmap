//--------------------------------------------------
// MakeMeSmall
// @ 2017 furaga
// Version 1.0.0
//--------------------------------------------------

Texture2Dをダウンスケールするためのライブラリです。

以下の7種類のダウンスケール手法が利用できます。

  1. Subsampling
  2. Box (= Average filter)
  3. Bilinear
  4. Bicubic
  5. Gaussian (5x5)
  6. Perceptualy based donwscaling [A. C. Oztireli et al., SIGGRAPH 2015]
  7. Content-Adaptive Image Downscaling [J. Kopf et al., SIGGRAPH Asia 2013].

Perceptualy based donwscaling は遅いですが、写真などを高品質にダウンスケールすることができます。
Content-Adaptive Image Downscaling は非常に遅いですが、アニメ画像を高品質にダウンスケールすることができます。

これらはすべてC#で実装されています。


# 使い方

1) MakeMeSmall.unitypackage をインポートします。

2) 以下の3つのデモを起動して遊んでみてください。

  * ./MakeMeSmall/Demo/Scenes/1_Simple
    * 画面左側のボタンを押すことで各ダウンスケールを実行できます。

  * ./MakeMeSmall/Demo/Scenes/2_FileConverter: 
    * PNG画像を読み込んで、ダウンスケールして、結果画像をファイル保存できます。
	
  * ./MakeMeSmall/Demo/Scenes/3_FolderConverter:
    * ディレクトリ内のすべてのPNG画像を一括でダウンスケールできます。

3) あなたのC#スクリプトで各ダウンスケールを実行するには以下のようなコードを書いてください。

```   
    Texture2D outputTexture2D = MakeMeSmall.Downscale.Subsample(inputTexture2D, new Vector3(w, h, 0));
```   

		  
# Version History
1.0.0
- First release


# Contact
furaga.fukahori@gmail.com
