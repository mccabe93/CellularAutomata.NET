using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading.Channels;
using ImageMagick;

namespace CellularAutomata.NET
{
    public enum CellularBitmapType
    {
        None = 0,
        NeighborhoodMap = 1,
        StateMap = 2,
    }

    public enum CellularBitmapNeighborhoodState
    {
        Transparent = -2,
        Origin = -1,
    }

    public record CellularBitmapColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public CellularBitmapColor(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public CellularBitmapColor(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public Color ToColor()
        {
            return Color.FromArgb(A, R, G, B);
        }
    }

    public class CellularBitmapConfiguration<T>
    {
        public CellularBitmapType Type { get; set; } = CellularBitmapType.None;
        public required string File { get; set; }
        public Dictionary<CellularBitmapColor, T> ColorToStateBitmap { get; set; } =
            new Dictionary<CellularBitmapColor, T>();

        /// <summary>
        /// The default map uses White to indicate dead and Black to indicate alive.
        /// NOTE: Red indicates the origin cell, and Transparent pixels are ignored. These are hardcoded values and can only be modified by editing the source code.
        /// </summary>
        public static readonly Dictionary<CellularBitmapColor, int> DefaultNeighborhoodMap =
            new Dictionary<CellularBitmapColor, int>()
            {
                {
                    new CellularBitmapColor(Color.Transparent),
                    (int)CellularBitmapNeighborhoodState.Transparent
                },
                { new CellularBitmapColor(Color.Red), (int)CellularBitmapNeighborhoodState.Origin },
                { new CellularBitmapColor(Color.White), 0 },
                { new CellularBitmapColor(Color.Black), 1 },
            };
    }

    public class CellularBitmap<T>(CellularBitmapConfiguration<T> configuration)
    {
        private readonly CellularBitmapConfiguration<T> _configuration = configuration;

        public Dictionary<Vector<int>, T> LoadState()
        {
            var stateMap = new Dictionary<Vector<int>, T>();
            var mapping = _configuration.ColorToStateBitmap;
            if (mapping == null || mapping.Count == 0)
            {
                throw new InvalidDataException(
                    "No color to state mapping provided in configuration."
                );
            }
            using var img = new MagickImage(_configuration.File);
            byte[] pixels = img.ToByteArray(MagickFormat.Rgba);
            int width = (int)img.Width;
            int height = (int)img.Height;
            const int channels = 4; // R, G, B, A
            CellularBitmapColor px = new CellularBitmapColor(0, 0, 0, 0);
            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * width * channels;
                for (int x = 0; x < width; x++)
                {
                    int i = rowOffset + x * channels;
                    byte r = pixels[i + 0];
                    byte g = pixels[i + 1];
                    byte b = pixels[i + 2];
                    byte a = pixels[i + 3];
                    px.R = r;
                    px.G = g;
                    px.B = b;
                    px.A = a;
                    if (!mapping.TryGetValue(px, out var state))
                        continue;
                    if (
                        typeof(T) == typeof(int)
                        && state is int stint
                        && stint == (int)CellularBitmapNeighborhoodState.Transparent
                    )
                        continue;
                    var position = AutomataVector.Create(x, y);
                    stateMap[position] = (T)(object)state!;
                }
            }
            return stateMap;
        }

        public List<Vector<int>> LoadNeighborhood()
        {
            // Use provided mapping or fall back to the built-in default neighborhood map
            var mapping = _configuration.ColorToStateBitmap;
            if (mapping == null || mapping.Count == 0)
            {
                throw new InvalidDataException(
                    "No color to state mapping provided in configuration."
                );
            }

            using var img = new MagickImage(_configuration.File);
            byte[] pixels = img.ToByteArray(MagickFormat.Rgba);
            int width = (int)img.Width;
            int height = (int)img.Height;

            const int channels = 4; // R, G, B, A

            (int originX, int originY) = FindOrigin(width, height, pixels, channels);
            if (originX < 0 || originY < 0)
                throw new InvalidOperationException(
                    "No origin pixel (mapped to Color.Red / Origin) found in bitmap."
                );

            var neighbors = new List<Vector<int>>();

            // Iterate in row-major order so the returned list order is deterministic
            CellularBitmapColor px = new CellularBitmapColor(0, 0, 0, 0);
            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * width * channels;
                for (int x = 0; x < width; x++)
                {
                    int i = rowOffset + x * channels;
                    px.R = pixels[i + 0];
                    px.G = pixels[i + 1];
                    px.B = pixels[i + 2];
                    px.A = pixels[i + 3];

                    if (!mapping.TryGetValue(px, out var state))
                        continue;

                    if (state is int stint)
                    {
                        if (stint == (int)CellularBitmapNeighborhoodState.Transparent)
                            continue;

                        if (stint == (int)CellularBitmapNeighborhoodState.Origin)
                            continue; // skip origin itself
                    }

                    // For neighborhood we include any mapped state (e.g. 0/1) as a neighbor
                    int dx = x - originX;
                    int dy = y - originY;
                    neighbors.Add(AutomataVector.Create(dx, dy));
                }
            }

            return neighbors;
        }

        private (int x, int y) FindOrigin(int width, int height, byte[] pixels, int channels)
        {
            bool foundOrigin = false;
            CellularBitmapColor px = new CellularBitmapColor(0, 0, 0, 0);
            for (int y = 0; y < height && !foundOrigin; y++)
            {
                int rowOffset = y * width * channels;
                for (int x = 0; x < width; x++)
                {
                    int i = rowOffset + x * channels;
                    px.R = pixels[i + 0];
                    px.G = pixels[i + 1];
                    px.B = pixels[i + 2];
                    px.A = pixels[i + 3];
                    if (
                        _configuration.ColorToStateBitmap.TryGetValue(px, out var state)
                        && state is int pixelState
                        && pixelState == (int)CellularBitmapNeighborhoodState.Origin
                    )
                    {
                        return (x, y);
                    }
                }
            }
            return (-1, -1);
        }
    }
}
