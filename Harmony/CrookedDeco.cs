using OCB;
using System;
using System.IO;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

public class CrookedDeco : IModApi
{

    public static string PathSettings = null;

    static public Dictionary<string, CrookedConfig>
        Config = new Dictionary<string, CrookedConfig>();

    public void InitMod(Mod mod)
    {
        Debug.Log("Loading OCB Crooked Deco/Tree Patch: " + GetType().ToString());
        new Harmony(GetType().ToString()).PatchAll(mod.MainAssembly);
        ModEvents.GameStartDone.RegisterHandler(ReloadMapping);
        PathSettings = mod.Path + "/Settings/";
        ReloadConfig();
    }

    static public void ReloadConfig()
    {
        if (PathSettings == null) return;
        if(!Directory.Exists(PathSettings))
        {
            throw new Exception("Settings directory invalid: " + PathSettings);
        }
        foreach (string file in Directory.GetFiles(PathSettings))
        {
            if (!file.EndsWith(".cfg")) continue;
            if (!File.Exists(file)) continue;
            CrookedParser.ParseConfig(file, ref Config);
        }
    }

    static public void ReloadMapping()
    {
        if (PathSettings == null) return;
        if (!Block.BlocksLoaded) return;
        if (!Directory.Exists(PathSettings))
        {
            throw new Exception("Settings directory invalid: " + PathSettings);
        }
        foreach (string file in Directory.GetFiles(PathSettings))
        {
            if (!file.EndsWith(".cfg")) continue;
            if (!File.Exists(file)) continue;
            CrookedParser.ParseMapping(file);
        }
    }

    static readonly ulong Seed = StaticRandom.RandomSeed();

    static void ApplyTransform(
        Vector3i _blockPos,
        BlockValue _blockValue,
        BlockEntityData _ebcd,
        CrookedConfig range,
        bool toppled = false)
    {
        ulong seed = Seed;
        StaticRandom.HashSeed(ref seed, _blockPos.x);
        StaticRandom.HashSeed(ref seed, _blockPos.y);
        StaticRandom.HashSeed(ref seed, _blockPos.z);
        range.AddRotation(_ebcd.transform, seed, toppled);
        range.ApplyScale(_ebcd.transform, seed);
    }

    static void Transform(
        Vector3i _blockPos,
        BlockValue _blockValue,
        BlockEntityData _ebcd,
        int Pass)
    {
        if (_ebcd.transform == null) return;
        if (_blockValue.Block.Properties.Values.TryGetString(
            "DynamicTransform", out string dynamicTransform))
        {
            if (dynamicTransform.ToLower() == "none") return;
            if (dynamicTransform.ToLower() == "report")
                Log.Warning("Reporting {0}", _blockValue.Block);
            else if (Config.TryGetValue(dynamicTransform, out CrookedConfig config))
            {
                if (config.Pass != -1 && config.Pass != Pass) return;
                bool toppled = config.IsToppled(_blockValue.rotation);
                ApplyTransform(_blockPos, _blockValue, _ebcd, config, toppled);
            }
            else
            {
                Log.Out("Crooked Type not found: {0}", dynamicTransform);
            }
        }
        // else if (_blockValue.Block.Properties.Values.TryGetString(
        //     "IsTerrainDecoration", out string deco))
        // {
        //     // We probably want at least to support these?
        //     if (!deco.EqualsCaseInsensitive("false"))
        //     {
        //         Log.Out("Unsupported Deco " + deco
        //             + " => " + _blockValue.Block
        //             + " at pass " + Pass);
        //     }
        // }
    }

    [HarmonyPatch(typeof(BlockShape))]
    [HarmonyPatch("OnBlockEntityTransformAfterActivated")]
    public class BlockShape_OnBlockEntityTransformAfterActivated
    {
        static void Postfix(
            Vector3i _blockPos,
            BlockValue _blockValue,
            BlockEntityData _ebcd)
        {
            Transform(_blockPos, _blockValue, _ebcd, 0);
        }
    }

    [HarmonyPatch(typeof(BlockShapeModelEntity))]
    [HarmonyPatch("OnBlockEntityTransformBeforeActivated")]
    public class BlockShapeModelEntity_OnBlockEntityTransformBeforeActivated
    {
        static void Postfix(
            Vector3i _blockPos,
            BlockValue _blockValue,
            BlockEntityData _ebcd)
        {
            Transform(_blockPos, _blockValue, _ebcd, 1);
        }
    }

    [HarmonyPatch(typeof(BlockShapeDistantDeco))]
    [HarmonyPatch("OnBlockEntityTransformAfterActivated")]
    public class BlockShapeDistantDeco_OnBlockEntityTransformAfterActivated
{
        static void Postfix(
            Vector3i _blockPos,
            BlockValue _blockValue,
            BlockEntityData _ebcd)
        {
            Transform(_blockPos, _blockValue, _ebcd, 2);
        }
    }

    [HarmonyPatch(typeof(BlockShapeDistantDecoTree))]
    [HarmonyPatch("OnBlockEntityTransformAfterActivated")]
    public class BlockShapeModelEntity_OnBlockEntityTransformAfterActivated
    {
        static void Postfix(
            Vector3i _blockPos,
            BlockValue _blockValue,
            BlockEntityData _ebcd)
        {
            Transform(_blockPos, _blockValue, _ebcd, 3);
        }
    }

    [HarmonyPatch(typeof(BlockShapeBillboardPlant))]
    [HarmonyPatch("RenderSpinMesh")]
    public class BlockShapeBillboardPlant_RenderSpinMesh
    {

        static readonly ulong Seed01 = StaticRandom.RandomSeed();
        static readonly ulong Seed02 = StaticRandom.RandomSeed();
        static readonly ulong Seed03 = StaticRandom.RandomSeed();
        static readonly ulong Seed04 = StaticRandom.RandomSeed();
        static readonly ulong Seed05 = StaticRandom.RandomSeed();

        static void Prefix(Vector3 _drawPos,
            ref BlockShapeBillboardPlant.RenderData _data)
        {
            ulong seed = Seed01;
            StaticRandom.HashSeed(ref seed, _drawPos.x);
            StaticRandom.HashSeed(ref seed, _drawPos.y);
            StaticRandom.HashSeed(ref seed, _drawPos.z);
            _data.count += StaticRandom.RangeSquare(0,
                MeshDescription.GrassQualityPlanes, seed);
            StaticRandom.HashSeed(ref seed, Seed02);
            _data.scale *= StaticRandom.Range(0.8f, 1.2f, seed);
            StaticRandom.HashSeed(ref seed, Seed03);
            _data.rotation += StaticRandom.Range(-22.5f, 22.5f, seed);
            StaticRandom.HashSeed(ref seed, Seed04);
            _data.height *= StaticRandom.RangeSquare(0.9f, 1.3f, seed);
            StaticRandom.HashSeed(ref seed, Seed05);
            _data.sideShift += StaticRandom.RangeSquare(-.2f, .2f, seed);
        }
    }

    [HarmonyPatch(typeof(BlockShapeBillboardPlant))]
    [HarmonyPatch("RenderGridMesh")]
    public class BlockShapeBillboardPlant_RenderGridMesh
    {

        static readonly ulong Seed01 = StaticRandom.RandomSeed();
        static readonly ulong Seed02 = StaticRandom.RandomSeed();
        static readonly ulong Seed03 = StaticRandom.RandomSeed();
        static readonly ulong Seed04 = StaticRandom.RandomSeed();
        static readonly ulong Seed05 = StaticRandom.RandomSeed();


        static void Prefix(Vector3 _drawPos,
            ref BlockShapeBillboardPlant.RenderData _data)
        {
            ulong seed = Seed01;
            StaticRandom.HashSeed(ref seed, _drawPos.z);
            StaticRandom.HashSeed(ref seed, _drawPos.y);
            StaticRandom.HashSeed(ref seed, _drawPos.x);
            _data.count += StaticRandom.RangeSquare(0,
                MeshDescription.GrassQualityPlanes, seed);
            StaticRandom.HashSeed(ref seed, Seed02);
            _data.scale *= StaticRandom.Range(0.8f, 1.2f, seed);
            StaticRandom.HashSeed(ref seed, Seed03);
            _data.rotation += StaticRandom.Range(-22.5f, 22.5f, seed);
            StaticRandom.HashSeed(ref seed, Seed04);
            _data.height *= StaticRandom.RangeSquare(0.9f, 1.3f, seed);
            StaticRandom.HashSeed(ref seed, Seed05);
            _data.sideShift += StaticRandom.RangeSquare(-.2f, .2f, seed);
        }
    }

}
