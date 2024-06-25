using HarmonyLib;
using OCB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

public class CrookedDeco : IModApi
{

    public static string PathSettings = null;

    static public Dictionary<string, CrookedConfig>
        Config = new Dictionary<string, CrookedConfig>();

    public void InitMod(Mod mod)
    {
        Log.Out("Loading OCB Crooked Deco/Tree Patch: " + GetType().ToString());
        new Harmony(GetType().ToString()).PatchAll(mod.AllAssemblies[0]);
        // ModEvents.GameStartDone.RegisterHandler(ReloadMapping);
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

    // Hook into specific event (whatever works)
    // Note `ModEvents.GameStartDone` is too late!
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("createWorld")]
    public class CraftingManager_InitForNewGame
    {
        static void Prefix()
        {
            ReloadMapping();
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
        if (_blockValue.Block.Properties.Values.TryGetValue(
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

    // BlockShapeGrass/BlockShapeBillboardPlant
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
            StaticRandom.HashSeed(ref seed, Mathf.Floor(_drawPos.x));
            StaticRandom.HashSeed(ref seed, Mathf.Floor(_drawPos.y));
            StaticRandom.HashSeed(ref seed, Mathf.Floor(_drawPos.z));
            _data.count += StaticRandom.RangeSquare(0,
                MeshDescription.GrassQualityPlanes, seed);
            StaticRandom.HashSeed(ref seed, Seed02);
            _data.scale *= StaticRandom.Range(0.8f, 1.2f, seed);
            StaticRandom.HashSeed(ref seed, Seed03);
            _data.rotation += StaticRandom.Range(-22.5f, 22.5f, seed);
            StaticRandom.HashSeed(ref seed, Seed04);
            _data.height *= StaticRandom.RangeSquare(0.9f, 1.3f, seed);
            StaticRandom.HashSeed(ref seed, Seed05);
            _data.sideShift *= StaticRandom.RangeSquare(0.8f, 1.2f, seed);
        }
    }

    // BlockShapeGrassShort
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
            StaticRandom.HashSeed(ref seed, Mathf.Floor(_drawPos.z));
            StaticRandom.HashSeed(ref seed, Mathf.Floor(_drawPos.y));
            StaticRandom.HashSeed(ref seed, Mathf.Floor(_drawPos.x));
            _data.count += StaticRandom.RangeSquare(0,
                MeshDescription.GrassQualityPlanes, seed);
            StaticRandom.HashSeed(ref seed, Seed02);
            _data.scale *= StaticRandom.Range(0.8f, 1.2f, seed);
            StaticRandom.HashSeed(ref seed, Seed03);
            _data.rotation += StaticRandom.Range(-22.5f, 22.5f, seed);
            StaticRandom.HashSeed(ref seed, Seed04);
            _data.height *= StaticRandom.RangeSquare(0.9f, 1.3f, seed);
            StaticRandom.HashSeed(ref seed, Seed05);
            // Input for `sideShift` varies from 0.18 to 0.3
            _data.sideShift *= StaticRandom.RangeSquare(.8f, 1.2f, seed);
        }
    }

    // Code below is for BlockShapeNew

    // Poor man's fix to save IL headache
    static Vector3 CurrentScale = Vector3.one;

    [HarmonyPatch(typeof(BlockShapeNew))]
    [HarmonyPatch("renderFace")]
    public static class BlockShapeNew_renderFace
    {
        static readonly ulong Seed00 = StaticRandom.RandomSeed();
        static Quaternion DynamicRotation(Vector3 _blockPos, BlockValue _blockValue, Quaternion quat)
        {
            // if (Enabled == false) return;
            if (_blockValue.Block.Properties.Values.TryGetValue(
                "DynamicTransform", out string dynamicTransform))
            {
                ulong seed = Seed00;
                if (dynamicTransform.ToLower() == "none") return quat;
                if (dynamicTransform.ToLower() == "report")
                    Log.Warning("Reporting {0}", _blockValue.Block);
                else if (Config.TryGetValue(dynamicTransform, out CrookedConfig config))
                {
                    bool toppled = config.IsToppled(_blockValue.rotation);
                    StaticRandom.HashSeed(ref seed, _blockPos.x);
                    StaticRandom.HashSeed(ref seed, _blockPos.y);
                    StaticRandom.HashSeed(ref seed, _blockPos.z);
                    CurrentScale = config.GetScale(seed); // Used later
                    StaticRandom.HashSeed(ref seed, seed);
                    if (config.GetRotation(toppled) is ICrookedVector rot)
                        return quat * rot.GetRotation(seed);
                }
                else
                {
                    Log.Out("Crooked Type not found: {0}", dynamicTransform);
                }
            }
            CurrentScale = Vector3.one;
            return quat;
        }
        static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ldsfld) continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Ldarga_S) continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Call) continue;
                if (!(codes[i].operand is MethodInfo met)) continue;
                if (met.Name != "get_rotation") continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Ldelem) continue;
                if (!(codes[i].operand is Type trf)) continue;
                if (trf.FullName != "UnityEngine.Quaternion") continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Stloc_S) continue;
                if (!(codes[i].operand is LocalVariableInfo vdf)) continue;
                // if (vdf.LocalIndex != 28) continue;
                if (++i >= codes.Count) break;
                // Since we insert always at this position, you need to read the code from bottom to top ;)
                MethodInfo method = AccessTools.Method(typeof(BlockShapeNew_renderFace), "DynamicRotation");
                codes.Insert(i, new CodeInstruction(OpCodes.Stloc_S, vdf.LocalIndex)); // pop and store
                codes.Insert(i, new CodeInstruction(OpCodes.Call, method)); // call function
                codes.Insert(i, new CodeInstruction(OpCodes.Ldloc_S, vdf.LocalIndex)); // put quaternion
                codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_2)); // put blockValue
                codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_3)); // put drawPos
                Log.Out(" Patched BlockShapeNew.renderFace rotation");
                break;
            }
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ldloc_S) continue;
                if (!(codes[i].operand is LocalVariableInfo vdf)) continue;
                if (vdf.LocalIndex != 29) continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Call) continue;
                if (!(codes[i].operand is MethodInfo met1)) continue;
                if (met1.Name != "op_Addition") continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Call) continue;
                if (!(codes[i].operand is MethodInfo met2)) continue;
                if (met2.Name != "op_Multiply") continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Ldloc_S) continue;
                if (!(codes[i].operand is LocalVariableInfo rdf)) continue;
                if (rdf.LocalIndex != 30) continue;
                // We don't remove the item from the stack, we re-use it
                MethodInfo method = AccessTools.Method(typeof(Vector3), "Scale",
                    new Type[] { typeof(Vector3), typeof(Vector3) });
                codes.Insert(i-1, new CodeInstruction(OpCodes.Call, method)); // call function
                FieldInfo field = AccessTools.Field(typeof(CrookedDeco), "CurrentScale");
                codes.Insert(i - 1, new CodeInstruction(OpCodes.Ldsfld, field)); // put scale
                Log.Out(" Patched BlockShapeNew.renderFace scaling");
            }
            return codes;
        }
    }

    [HarmonyPatch(typeof(NGuiWdwDebugPanels), "showDebugPanel_FocusedBlock")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(bool) })]
    public static class FocusedBlockDebugPatch
    {
        static void Postfix(NGuiWdwDebugPanels __instance, int x, int y, bool forceFocusedBlock,
            EntityPlayerLocal ___playerEntity, GUIStyle ___guiStyleDebug, ref int __result)
        {
            // if (!___bDebugFocusedBlockEnabled && !forceDisplay) return;
            WorldRayHitInfo hitInfo = ___playerEntity.inventory.holdingItemData.hitInfo;
            Vector3i _blockPos = UnityEngine.Input.GetKey(KeyCode.LeftShift) | forceFocusedBlock ? hitInfo.hit.blockPos : hitInfo.lastBlockPos;
            var world = GameManager.Instance.World;
            if (world == null) return;
            ChunkCluster chunkCluster = world.ChunkClusters[hitInfo.hit.clrIdx];
            if (chunkCluster == null) return;
            Chunk chunkFromWorldPos = (Chunk)chunkCluster.GetChunkFromWorldPos(_blockPos);
            if (chunkFromWorldPos == null) return;
            int blockXz1 = World.toBlockXZ(_blockPos.x);
            int blockY = World.toBlockY(_blockPos.y);
            int blockXz2 = World.toBlockXZ(_blockPos.z);
            BlockValue bv = chunkFromWorldPos.GetBlock(blockXz1, blockY, blockXz2);
            GUI.Box(new Rect(x, __result, 260, 42), "");
            GUI.color = Color.yellow;
            GUI.Label(new Rect(x + 5, __result, 200f, 25f), "Crooked Deco");
            __result += 21;
            GUI.color = Color.white;
            // Check if we have a dynamic transform config
            if (bv.Block.Properties.Values.TryGetValue(
                "DynamicTransform", out string dynamicTransform))
            {
                Utils.DrawOutline(new Rect(x + 5, __result, 200f, 25f),
                    "Config: " + dynamicTransform.ToString(),
                    ___guiStyleDebug, Color.black, Color.white);
                __result += 16;
            }
            else
            {
                Utils.DrawOutline(new Rect(x + 5, __result, 200f, 25f),
                    "No crooked deco config", ___guiStyleDebug,
                    Color.black, Color.white);
                __result += 16;
            }
            __result += 10;

        }
    }

}
