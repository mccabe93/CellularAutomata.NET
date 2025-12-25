using System;
using System.Collections.Generic;
using System.Text;
using Silk.NET.OpenGL;

namespace Conway3D.Rendering
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private readonly uint _handle;
        private readonly BufferTargetARB _bufferType;
        private readonly GL _gl;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            _gl = gl;
            _bufferType = bufferType;

            _handle = _gl.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                _gl.BufferData(
                    bufferType,
                    (nuint)(data.Length * sizeof(TDataType)),
                    d,
                    BufferUsageARB.StaticDraw
                );
            }
        }

        public void Bind()
        {
            _gl.BindBuffer(_bufferType, _handle);
        }

        ~BufferObject()
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
            _gl.DeleteBuffer(_handle);
        }
    }
}
