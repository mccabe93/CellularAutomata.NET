using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Silk.NET.OpenGL;

namespace Conway3D.Rendering
{
    internal class Wolfram3DCube : IDisposable
    {
        public Vector<int> Position;

        public BufferObject<float> Vbo;
        public BufferObject<uint> Ebo;
        public VertexArrayObject<float, uint> Vao;

        public float Size { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public readonly float[] Vertices;

        // csharpier-ignore
        public readonly uint[] Indices =
        {
            0, 1, 2, 3, 4, 5,
            6, 7, 8, 9,10,11,
           12,13,14,15,16,17,
           18,19,20,21,22,23,
           24,25,26,27,28,29,
           30,31,32,33,34,35
        };

        public Wolfram3DCube(Vector<int> position, GL gl, float size, float x, float y, float z)
        {
            Position = position;
            Size = size;
            X = x;
            Y = y;
            Z = z;

            Vertices = GetVertices(size, x, y, z);

            Vbo = new BufferObject<float>(gl, Vertices, BufferTargetARB.ArrayBuffer);
            Ebo = new BufferObject<uint>(gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Vao = new VertexArrayObject<float, uint>(gl, Vbo, Ebo);

            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);
        }

        private float[] GetVertices(float size, float x, float y, float z)
        {
            // csharpier-ignore
            float[] baseVerts = new float[]
            {
                //X    Y      Z     U   V
               -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
                0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
                0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
               -0.5f,  0.5f, -0.5f,  0.0f, 0.0f,
               -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

               -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
                0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
                0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
               -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
               -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,

               -0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
               -0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
               -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
               -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
               -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
               -0.5f,  0.5f,  0.5f,  1.0f, 1.0f,

                0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
                0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
                0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
                0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
                0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
                0.5f,  0.5f,  0.5f,  1.0f, 1.0f,

               -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
                0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
                0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
                0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
               -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
               -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

               -0.5f,  0.5f, -0.5f,  0.0f, 0.0f,
                0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
                0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
                0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
               -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
               -0.5f,  0.5f, -0.5f,  0.0f, 0.0f
            };

            // Transform positions: scale (Size) and translate (X,Y,Z).
            var outVerts = new float[baseVerts.Length];
            for (int i = 0; i < baseVerts.Length; i += 5)
            {
                // position
                outVerts[i + 0] = baseVerts[i + 0] * Size + X;
                outVerts[i + 1] = baseVerts[i + 1] * Size + Y;
                outVerts[i + 2] = baseVerts[i + 2] * Size + Z;

                // uv copied unchanged
                outVerts[i + 3] = baseVerts[i + 3];
                outVerts[i + 4] = baseVerts[i + 4];
            }

            return outVerts;
        }

        ~Wolfram3DCube()
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
            if (disposing)
            {
                Vbo.Dispose();
                Ebo.Dispose();
                Vao.Dispose();
            }
        }
    }
}
