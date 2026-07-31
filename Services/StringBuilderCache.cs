using System;
using System.Text;

namespace DemoPulse.Services
{
    /// <summary>
    /// Thread-static cache for StringBuilder instances to reduce GC allocations during high-frequency parsing and string formatting.
    /// </summary>
    public static class StringBuilderCache
    {
        public const int DefaultCapacity = 256;
        public const int MaxCapacity = 1024 * 16;

        [ThreadStatic]
        private static StringBuilder? _cachedInstance;

        public static StringBuilder Acquire(int capacity = DefaultCapacity)
        {
            if (capacity <= MaxCapacity)
            {
                StringBuilder? sb = _cachedInstance;
                if (sb != null && capacity <= sb.Capacity)
                {
                    _cachedInstance = null;
                    sb.Clear();
                    return sb;
                }
            }
            return new StringBuilder(capacity);
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            string result = sb.ToString();
            Release(sb);
            return result;
        }

        public static void Release(StringBuilder sb)
        {
            if (sb != null && sb.Capacity <= MaxCapacity)
            {
                _cachedInstance = sb;
            }
        }
    }
}
