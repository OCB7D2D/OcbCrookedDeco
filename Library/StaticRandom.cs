using System;

namespace OCB
{
    public static class StaticRandom
    {

        public static readonly Random rng = new Random();

        public static ulong DOUBLE2ULONG(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static double ULONG2DOUBLE(ulong value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static ulong RandomSeed()
        {
            var buffer = new byte[sizeof(ulong)];
            rng.NextBytes(buffer); // Fill in bits
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static ulong HashSeed(ulong seed, float value)
        {
            // A mix between boost `hash_combine` and Knuth Hashing
            // Pseudo randomness is really good enough for our use case
            seed ^= ((((3074457345618258791ul + seed) * 2774457345618258799ul) +
                DOUBLE2ULONG(value)) * 3374457345618258793ul) * 1387457109323358256ul;
            seed ^= ((((3074457345618258791ul + seed) * 2774457345618258799ul) +
                DOUBLE2ULONG(value)) * 3374457345618258795ul) * 1387457109323358256ul;
            return seed;
        }

        public static void HashSeed(ref ulong seed, float value)
        {
            seed = HashSeed(seed, value);
        }

        public static float Range(float min, float max, ulong seed)
        {
            float range = Math.Abs(max - min);
            return Math.Min(min, max) +
                range * seed / (ulong.MaxValue);
        }

        public static float RangeSquare(float min, float max, ulong seed)
        {
            float range = Math.Abs(max - min);
            double rnd = (double)seed / (ulong.MaxValue) - 0.5;
            rnd = rnd * rnd * 2 * Math.Sign(rnd) + 0.5;
            return Math.Min(min, max) + range * (float)rnd;
        }

        public static int Range(int min, int max, ulong seed)
        {
            return (int)Math.Floor(Range(Math.Min(min, max),
                Math.Max(min, max) + 0.999999999f, seed));
        }

        public static int RangeSquare(int min, int max, ulong seed)
        {
            return (int)Math.Floor(RangeSquare(Math.Min(min, max),
                Math.Max(min, max) + 0.999999999f, seed));
        }

    }
}
