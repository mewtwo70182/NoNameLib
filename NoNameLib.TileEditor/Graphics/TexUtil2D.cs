﻿using System.Drawing;
using System.IO;

using OpenTK.Graphics.OpenGL;

namespace NoNameLib.TileEditor.Graphics
{
    public static class TexUtil2D
    {
        #region Public

        /// <summary>
        /// Initialize OpenGL state to enable alpha-blended texturing.
        /// Disable again with GL.Disable(EnableCap.Texture2D).
        /// Call this before drawing any texture, when you boot your
        /// application, eg. in OnLoad() of GameWindow or Form_Load()
        /// if you're building a WinForm app.
        /// </summary>
        public static void InitTexturing()
        {
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        }

        /// <summary>
        /// Create an opaque OpenGL texture object from a given byte-array of r,g,b-triplets.
        /// Make sure width and height is 1, 2, .., 32, 64, 128, 256 and so on in size since all
        /// 3d graphics cards support those dimensions. Not necessarily square. Don't forget
        /// to call GL.DeleteTexture(int) when you don't need the texture anymore (eg. when switching
        /// levels in your game).
        /// </summary>
        public static int CreateRGBTexture(int width, int height, byte[] rgb)
        {
            return CreateTexture(width, height, false, rgb);
        }

        /// <summary>
        /// Create a translucent OpenGL texture object from given byte-array of r,g,b,a-triplets.
        /// See CreateRGBTexture for more info.
        /// </summary>
        public static int CreateRGBATexture(int width, int height, byte[] rgba)
        {
            return CreateTexture(width, height, true, rgba);
        }

        /// <summary>
        /// Create an OpenGL texture (translucent or opaque) from a given Bitmap.
        /// 24- and 32-bit bitmaps supported.
        /// </summary>
        public static int CreateTextureFromBitmap(Bitmap bitmap)
        {
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var tex = GiveMeATexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexImage2D(
              TextureTarget.Texture2D,
              0,
              PixelInternalFormat.Rgba,
              data.Width, data.Height,
              0,
              PixelFormat.Bgra,
              PixelType.UnsignedByte,
              data.Scan0);
            bitmap.UnlockBits(data);
            SetParameters();
            return tex;
        }

        /// <summary>
        /// Create an OpenGL texture (translucent or opaque) by loading a bitmap
        /// from file. 24- and 32-bit bitmaps supported.
        /// </summary>
        public static int CreateTextureFromFile(string path)
        {
            string extension = Path.GetExtension(path);
            var newText = new Bitmap(Image.FromFile(path));
            if (extension == "bmp")
            {
                newText.MakeTransparent(Color.FromArgb(0, 136, 255));
            }

            return CreateTextureFromBitmap(newText);
        }

        #endregion

        private static int CreateTexture(int width, int height, bool alpha, byte[] bytes)
        {
            int expectedBytes = width * height * (alpha ? 4 : 3);
            System.Diagnostics.Debug.Assert(expectedBytes == bytes.Length);
            int tex = GiveMeATexture();
            Upload(width, height, alpha, bytes);
            SetParameters();
            return tex;
        }

        private static int GiveMeATexture()
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);
            return tex;
        }

        private static void SetParameters()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private static void Upload(int width, int height, bool alpha, byte[] bytes)
        {
            var internalFormat = alpha ? PixelInternalFormat.Rgba : PixelInternalFormat.Rgb;
            var format = alpha ? PixelFormat.Rgba : PixelFormat.Rgb;
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                internalFormat,
                width, height,
                0,
                format,
                PixelType.UnsignedByte,
                bytes);
        }
    }
}
