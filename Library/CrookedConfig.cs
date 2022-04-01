using System;
using UnityEngine;

namespace OCB
{

    public class CrookedConfig
    {

        public int Pass;
        public ICrookedVector Rotation;
        public ICrookedVector AltRotation;
        public ICrookedVector Scale;
        public Func<int, bool> FnToppled;

        public CrookedConfig(int Pass,
            ICrookedVector Scale,
            ICrookedVector Rotation)
        {
            this.Pass = Pass;
            this.Scale = Scale;
            this.Rotation = Rotation;
            this.AltRotation = null;
            this.FnToppled = null;
        }

        public CrookedConfig(int Pass,
            ICrookedVector Scale,
            ICrookedVector Rotation,
            ICrookedVector InvRotation,
            Func<int, bool> IsToppled = null)
        {
            this.Pass = Pass;
            this.Scale = Scale;
            this.Rotation = Rotation;
            this.AltRotation = InvRotation;
            this.FnToppled = IsToppled;
        }

        public bool IsToppled(int rotation)
        {
            if (FnToppled == null) return false;
            return FnToppled(rotation);
        }

        public Vector3 GetScale(ulong seed)
        {
            if (Scale == null) return Vector3.one;
            return Scale.GetVector(seed);
        }

        public ICrookedVector GetRotation(bool toppled)
        {
            if (toppled && AltRotation != null)
            {
                return AltRotation;
            }
            return Rotation;
        }

        public Quaternion GetRotation(ulong seed, bool toppled = false)
        {
            if (toppled && AltRotation != null)
            {
                return AltRotation.GetRotation(seed);
            }
            else if (Rotation == null)
            {
                return Quaternion.identity;
            }
            else
            {
                return Rotation.GetRotation(seed);
            }
        }

        public void ApplyScale(Transform t, ulong seed)
        {
            if (Scale == null || t == null) return;
            // Scale does not trigger the assignment operator!
            // Can't use `*=` since `Vector3*Vector3` isn't a thing.
            // Bug: we can't apply the scale, it will accumulate on the transform.
            // Luckily it seems we always get uniform scales to begin with.
            // Therefore nothing lost if we just reset the scale to what we want.
            // if (Math.Abs(t.localScale.x - 1f) > 0.0001) Log.Error("Have non uniform x scale: {0}", t.localScale.x);
            // if (Math.Abs(t.localScale.y - 1f) > 0.0001) Log.Error("Have non uniform y scale: {0}", t.localScale.y);
            // if (Math.Abs(t.localScale.z - 1f) > 0.0001) Log.Error("Have non uniform z scale: {0}", t.localScale.z);
            // t.localScale = Vector3.Scale(t.localScale, Scale.GetVector(seed));
            t.localScale = Scale.GetVector(seed);
        }

        public void AddRotation(Transform t, ulong seed, bool toppled = false)
        {
            if (GetRotation(toppled) is ICrookedVector rot)
            {
                // Seems to give better results than multiplying!?
                // Quaternions and Euler always hurts my brain...
                // var euler = t.localRotation.eulerAngles;
                // if (Math.Abs(euler.x % 45) > 0.0001) Log.Error("Have non uniform x rotation: {0}", euler.x);
                // if (Math.Abs(euler.y % 45) > 0.0001) Log.Error("Have non uniform x rotation: {0}", euler.y);
                // if (Math.Abs(euler.z % 45) > 0.0001) Log.Error("Have non uniform x rotation: {0}", euler.z);
                t.localRotation = Quaternion.Euler(t.localRotation
                    .eulerAngles + rot.GetVector(seed));
            }
        }

        public void ApplyRotation(Transform t, ulong seed, bool toppled = false)
        {
            if (GetRotation(toppled) is ICrookedVector rot)
            {
                t.localRotation *= rot.GetRotation(seed);
            }
        }

        public void PreApplyRotation(Transform t, ulong seed, bool toppled = false)
        {
            if (GetRotation(toppled) is ICrookedVector rot)
            {
                t.localRotation = rot.GetRotation(seed) * t.localRotation;
            }
        }

        public override string ToString()
        {
            return String.Format("Crooked(" +
                "pass: {0}, rot: {1}, scale: {2})",
                Pass, Rotation, Scale);
        }

    }

}
