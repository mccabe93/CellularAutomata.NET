using System;
using System.Collections.Generic;
using System.Text;

namespace Conway3D.Rendering
{
    public static class MathHelper
    {
        public static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }
    }
}
