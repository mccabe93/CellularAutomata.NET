using System;
using System.Collections.Generic;
using System.Text;
using ImageMagick;
using Silk.NET.OpenGL;

namespace Wolfram3D.Rendering
{
    public class Texture : IDisposable
    {
        private readonly uint _handle;
        private readonly GL _gl;

        public unsafe Texture(GL gl, string path)
        {
            _gl = gl;

            _handle = _gl.GenTexture();
            Bind();

            // https://github.com/dlemstra/Magick.NET/blob/main/docs/ReadingImages.md
            using MagickImage texture = new MagickImage(path);
            byte[] pixels = texture.ToByteArray(MagickFormat.Rgba);

            uint width = texture.Width;
            uint height = texture.Height;

            // Avoid row-alignment issues (default is 4). Set to 1 for tightly packed RGBA.
            _gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            fixed (byte* ptr = pixels)
            {
                _gl.TexImage2D(
                    (GLEnum)TextureTarget.Texture2D,
                    0,
                    (int)InternalFormat.Rgba8,
                    width,
                    height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    ptr
                );
            }

            SetParameters();
        }

        public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
        {
            _gl = gl;

            _handle = _gl.GenTexture();
            Bind();

            fixed (void* d = &data[0])
            {
                _gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    (int)InternalFormat.Rgba,
                    width,
                    height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    d
                );
                SetParameters();
            }
        }

        private void SetParameters()
        {
            _gl.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,
                (int)GLEnum.ClampToEdge
            );
            _gl.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT,
                (int)GLEnum.ClampToEdge
            );
            _gl.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)GLEnum.LinearMipmapLinear
            );
            _gl.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)GLEnum.Linear
            );
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            _gl.ActiveTexture(textureSlot);
            _gl.BindTexture(TextureTarget.Texture2D, _handle);
        }

        ~Texture()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _gl.DeleteTexture(_handle);
        }
    }
}
