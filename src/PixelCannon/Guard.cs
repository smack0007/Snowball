using System;

namespace PixelCannon
{
    public static class Guard
    {
        public static void NotNull<T>(T value, string name)
            where T : class
        {
#if DEBUG
            if (value is null)
                throw new ArgumentNullException(name);
#endif
        }
    }
}
