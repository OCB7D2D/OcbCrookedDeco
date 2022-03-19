using System;
using UnityEngine;

namespace OCB
{

    public interface ICrookedVector
    {
        Vector3 GetVector(ulong seed);
        Quaternion GetRotation(ulong seed);
        CrookedAxis AxisX { get; set; }
        CrookedAxis AxisY { get; set; }
        CrookedAxis AxisZ { get; set; }
        string ToString();
    }

    public class CrookedVector : ICrookedVector
    {

        public CrookedAxis Axis;

        public CrookedAxis AxisX { get => Axis; set => Axis = value; }
        public CrookedAxis AxisY { get => Axis; set => Axis = value; }
        public CrookedAxis AxisZ { get => Axis; set => Axis = value; }

        // Generic constructor passing crooked axes objects directly
        public CrookedVector(CrookedAxis axis)
        {
            Axis = axis;
        }

        public CrookedVector(float Min, float Max, bool squared = true)
        {
            if (!squared) Axis = new CrookedAxis(Min, Max, StaticRandom.Range);
            else Axis = new CrookedAxis(Min, Max, StaticRandom.RangeSquare);
        }

        static readonly ulong Seed = StaticRandom.RandomSeed();

        public Vector3 GetVector(ulong seed)
        {
            StaticRandom.HashSeed(ref seed, Seed);
            var val = this.Axis.GetValue(seed);
            return new Vector3(val, val, val);
        }

        public Quaternion GetRotation(ulong seed)
        {
            var rot = GetVector(seed);
            return Quaternion.Euler(rot);
        }

        public override string ToString()
        {
            return String.Format("[{0}]", Axis);
        }

    }

    public class CrookedVector3 : ICrookedVector
    {

        public CrookedAxis AxisX { get; set; }
        public CrookedAxis AxisY { get; set; }
        public CrookedAxis AxisZ { get; set; }

        // Generic constructor passing crooked axes objects directly
        public CrookedVector3(CrookedAxis x, CrookedAxis y, CrookedAxis z)
        {
            this.AxisX = x;
            this.AxisY = y;
            this.AxisZ = z;
        }

        public CrookedVector3(float MinX, float MaxX,
            float MinY, float MaxY, float MinZ, float MaxZ)
        {
            AxisX = new CrookedAxis(MinX, MaxX, StaticRandom.RangeSquare);
            AxisY = new CrookedAxis(MinY, MaxY, StaticRandom.RangeSquare);
            AxisZ = new CrookedAxis(MinZ, MaxZ, StaticRandom.RangeSquare);
        }

        static readonly ulong SeedX = StaticRandom.RandomSeed();
        static readonly ulong SeedY = StaticRandom.RandomSeed();
        static readonly ulong SeedZ = StaticRandom.RandomSeed();

        public Vector3 GetVector(ulong seed)
        {
            StaticRandom.HashSeed(ref seed, SeedX);
            var x = this.AxisX.GetValue(seed);
            StaticRandom.HashSeed(ref seed, SeedY);
            var y = this.AxisY.GetValue(seed);
            StaticRandom.HashSeed(ref seed, SeedZ);
            var z = this.AxisZ.GetValue(seed);
            return new Vector3(x, y, z);
        }

        public Quaternion GetRotation(ulong seed)
        {
            var rot = GetVector(seed);
            return Quaternion.Euler(rot);
        }

        public override string ToString()
        {
            return String.Format("[ " +
                "x: {0}, y: {1}, z: {2} ]",
                AxisX, AxisY, AxisZ);
        }

    }

}