using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using CellularAutomata.NET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Wolfram3D.Rendering
{
    // Reference: https://github.com/dotnet/Silk.NET/blob/main/examples/CSharp/OpenGL%20Tutorials/Tutorial%202.2%20-%20Camera/Program.cs
    internal class Wolfram3DWindow(
        int width,
        int height,
        CellularAutomataGrid<int> cells,
        float cubeSize = 1f
    )
    {
        private GL _gl;
        private IKeyboard? primaryKeyboard;

        private readonly CellularAutomataGrid<int> _cells = cells;
        private readonly float _cubeSize = cubeSize;
        private readonly HashSet<Wolfram3DCube> _wolframCubes = new HashSet<Wolfram3DCube>();

        //Setup the camera's location, directions, and movement speed
        private Vector3 _cameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
        private Vector3 _cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
        private Vector3 _cameraUp = Vector3.UnitY;
        private Vector3 _cameraDirection = Vector3.Zero;
        private Texture[]? _textures;
        private Shader? _shader;
        private float _cameraYaw = -90f;
        private float _cameraPitch = 0f;
        private float _cameraZoom = 45f;

        private bool _rotateX = false;
        private bool _rotateY = false;
        private bool _rotateZ = false;

        private float _difference = 45f;

        //Used to track change in mouse movement to allow for moving of the Camera
        private Vector2 LastMousePosition;

        private readonly IWindow _window = Window.Create(
            WindowOptions.Default with
            {
                Size = new Silk.NET.Maths.Vector2D<int>(width, height),
                Title = "Wolfram 3D",
            }
        );

        public void Start()
        {
            _window.Render += OnRender;
            _window.FramebufferResize += OnFramebufferResize;
            _window.Closing += OnClose;
            _window.Update += OnUpdate;
            _window.Load += OnLoad;
            _window.Run();
        }

        public void LoadWolframCubes()
        {
            foreach (var cell in _cells.State)
            {
                if (cell.Value.State == 1)
                {
                    var x = cell.Key[0] * _cubeSize;
                    var y = cell.Key[1] * _cubeSize;
                    var z = cell.Key[2] * _cubeSize;
                    _wolframCubes.Add(new Wolfram3DCube(_gl, _cubeSize, x, y, z));
                }
            }
        }

        private void OnUpdate(double deltaTime)
        {
            if (primaryKeyboard == null)
            {
                throw new InvalidProgramException("A keyboard is required for this application.");
            }

            var moveSpeed = 2.5f * (float)deltaTime;

            if (primaryKeyboard.IsKeyPressed(Key.W))
            {
                //Move forwards
                _cameraPosition += moveSpeed * _cameraFront;
            }
            if (primaryKeyboard.IsKeyPressed(Key.S))
            {
                //Move backwards
                _cameraPosition -= moveSpeed * _cameraFront;
            }
            if (primaryKeyboard.IsKeyPressed(Key.A))
            {
                //Move left
                _cameraPosition -=
                    Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * moveSpeed;
            }
            if (primaryKeyboard.IsKeyPressed(Key.D))
            {
                //Move right
                _cameraPosition +=
                    Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * moveSpeed;
            }
        }

        private void OnLoad()
        {
            IInputContext input = _window.CreateInput();
            primaryKeyboard =
                input.Keyboards[0]
                ?? throw new InvalidProgramException(
                    "A keyboard is required for this application."
                );
            if (primaryKeyboard != null)
            {
                primaryKeyboard.KeyDown += KeyDown;
            }
            for (int i = 0; i < input.Mice.Count; i++)
            {
                input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
                input.Mice[i].MouseMove += OnMouseMove;
                input.Mice[i].Scroll += OnMouseWheel;
            }

            _gl = GL.GetApi(_window);

            LoadWolframCubes();

            _shader = new Shader(_gl, "shader.vert", "shader.frag");

            _textures = new Texture[4];
            for (int i = 1; i <= 4; i++)
            {
                _textures[i - 1] = new Texture(_gl, $"Textures/Ground_0{i}.png");
            }
        }

        private void OnRender(double deltaTime)
        {
            if (_textures == null)
            {
                throw new InvalidProgramException("Texture not initialized.");
            }

            if (_shader == null)
            {
                throw new InvalidProgramException("Shader not initialized.");
            }

            _gl.Enable(EnableCap.DepthTest);
            _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            _shader.Use();
            _shader.SetUniform("uTexture0", 0);

            if (_rotateX || _rotateZ)
            {
                //Use elapsed time to convert to radians to allow our cube to rotate over time
                _difference = (float)(_window.Time * 100);
            }

            Matrix4x4 model = Matrix4x4.Identity;

            if (_rotateX)
            {
                model *= Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(_difference));
            }

            if (_rotateY)
            {
                model *= Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(_difference));
            }

            if (_rotateZ)
            {
                model *= Matrix4x4.CreateRotationZ(MathHelper.DegreesToRadians(_difference));
            }

            foreach (var cube in _wolframCubes)
            {
                _textures[cube.GetHashCode() % _textures.Length].Bind();
                //_texture.Bind();
                cube.Vao.Bind();
                var size = _window.FramebufferSize;

                _shader.SetUniform("uModel", model);

                var view = Matrix4x4.CreateLookAt(
                    _cameraPosition,
                    _cameraPosition + _cameraFront,
                    _cameraUp
                );
                var projection = Matrix4x4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(_cameraZoom),
                    (float)size.X / size.Y,
                    0.1f,
                    100.0f
                );
                _shader.SetUniform("uView", view);
                _shader.SetUniform("uProjection", projection);

                //We're drawing with just vertices and no indices, and it takes 36 vertices to have a six-sided textured cube
                _gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
        }

        private void OnFramebufferResize(Vector2D<int> newSize)
        {
            _gl.Viewport(newSize);
        }

        private unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            var lookSensitivity = 0.1f;
            if (LastMousePosition == default)
            {
                LastMousePosition = position;
            }
            else
            {
                var xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
                var yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
                LastMousePosition = position;

                _cameraYaw += xOffset;
                _cameraPitch -= yOffset;

                //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
                _cameraPitch = Math.Clamp(_cameraPitch, -89.0f, 89.0f);

                _cameraDirection.X =
                    MathF.Cos(MathHelper.DegreesToRadians(_cameraYaw))
                    * MathF.Cos(MathHelper.DegreesToRadians(_cameraPitch));
                _cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(_cameraPitch));
                _cameraDirection.Z =
                    MathF.Sin(MathHelper.DegreesToRadians(_cameraYaw))
                    * MathF.Cos(MathHelper.DegreesToRadians(_cameraPitch));
                _cameraFront = Vector3.Normalize(_cameraDirection);
            }
        }

        private unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            _cameraZoom = Math.Clamp(_cameraZoom - scrollWheel.Y, 1.0f, 45f);
        }

        private void KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Escape:
                    _window.Close();
                    break;
                case Key.X:
                    _rotateX = !_rotateX;
                    break;
                case Key.Y:
                    _rotateY = !_rotateY;
                    break;
                case Key.Z:
                    _rotateZ = !_rotateZ;
                    break;
            }
        }

        private void OnClose()
        {
            foreach (var cube in _wolframCubes)
            {
                cube.Vbo.Dispose();
                cube.Ebo.Dispose();
                cube.Vao.Dispose();
            }
            _shader?.Dispose();
            foreach (var texture in _textures ?? Array.Empty<Texture>())
            {
                texture.Dispose();
            }
        }
    }
}
