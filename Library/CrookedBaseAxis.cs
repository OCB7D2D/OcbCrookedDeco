using System;

namespace OCB
{
    public class CrookedAxis
    {

        // Axis Range
        public float Min;
        public float Max;

        // Function reference to create random range
        // Supports e.g. normal and square distribution
        public Func<float, float, ulong, float> Range;

        // Generic constructor taking any range function
        public CrookedAxis(float Min, float Max,
            Func<float, float, ulong, float> Range)
        {
            if (Range == null) Range = StaticRandom.RangeSquare;
            this.Min = Min; this.Max = Max; this.Range = Range;

        }

        // Convenient constructor satisfying for most cases
        public CrookedAxis(float Min, float Max, bool squared = true)
        {
            if (!squared) Range = StaticRandom.Range;
            else Range = StaticRandom.RangeSquare;
            this.Min = Min; this.Max = Max;
        }

        // Return float within range from seed
        public float GetValue(ulong seed)
        {
            return Range(Min, Max, seed);
        }

        public override string ToString()
        {
            return String.Format("{{ " +
                "min: {0}, max: {1} }}",
                Min, Max);
        }

    }

}
