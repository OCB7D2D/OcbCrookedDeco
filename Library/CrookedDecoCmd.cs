using System.Collections.Generic;

public class CrookedDecoCmd : ConsoleCmdAbstract
{

    private static string info = "CrookedDeco";
    public override string[] GetCommands()
    {
        return new string[3] { info, "crd", "cdr" };
    }

    public override bool IsExecuteOnClient => true;
    public override bool AllowedInMainMenu => true;

    public override string GetDescription() => "Crooked Deco Render Settings";

    public override string GetHelp() => "Fine tune how crooked deco is rendered\n";

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {

        if (_params.Count == 1)
        {
            switch (_params[0])
            {
                case "count":
                    int counter = 0;
                    if (Block.list == null) break;
                    if (!Block.BlocksLoaded) break;
                    foreach (var block in Block.list)
                    {
                        if (block == null) continue;
                        if (block.Properties == null) continue;
                        if (block.Properties.Values == null) continue;
                        if (block.Properties.Values.ContainsKey("DynamicTransform")) ++counter;
                    }
                    Log.Out("Counted {0} blocks with dynamic transform", counter);
                    break;
                case "list":
                    Log.Out("Listing all crooked types:");
                    foreach (var pair in CrookedDeco.Config)
                        Log.Out("  {0} (Pass {1})",
                            pair.Key, pair.Value.Pass);
                    break;
                case "reload":
                    CrookedDeco.ReloadConfig();
                    CrookedDeco.ReloadMapping();
                    break;
                default:
                    Log.Warning("Unknown command " + _params[0]);
                    break;
            }
        }

        else if (_params.Count == 2)
        {
            switch (_params[0])
            {

                case "get":
                case "set":
                    if (CrookedDeco.Config.TryGetValue(_params[1], out var config))
                    {
                        Log.Out("Show config for crooked type {0} (Pass {1})", _params[1], config.Pass);
                        Log.Out("  Scale: {0}", config.Scale);
                        Log.Out("  Rotation: {0}", config.Rotation);
                        Log.Out("  AltRotation: {0}", config.AltRotation);
                    }
                    else Log.Out(" Could not find config for " + _params[1]);
                    break;
                case "find":
                    Log.Out("Finding all blocks with DynamicTransform='{0}'", _params[1]);
                    if (Block.list == null || !Block.BlocksLoaded)
                    {
                        Log.Out("Blocks have not been loaded yet!");
                        break;
                    }
                    foreach (var block in Block.list)
                    {
                        if (block.Properties == null || block.Properties.Values == null) continue;
                        if (block.Properties.Values.TryGetValue("DynamicTransform", out string type))
                        {
                            if (type != _params[1]) continue;
                            Log.Out("  - {0}", block);
                        }
                    }
                    break;

                default:
                    Log.Warning("Unknown command " + _params[0]);
                    break;

            }
        }

        else if (_params.Count == 3)
        {
            switch (_params[0])
            {

                case "block":
                    if (Block.GetBlockByName(_params[1]) is Block block)
                    {
                        Block.GetBlockByName(_params[1]).Properties.
                            Values["DynamicTransform"] = _params[2];
                    }
                    else
                    {
                        Log.Warning("Block not known: " + _params[1]);
                    }
                    break;

                default:
                    Log.Warning("Unknown command " + _params[0]);
                    break;

            }
        }
        else if (_params.Count == 4)
        {
            switch (_params[0])
            {

                case "set":
                    if (CrookedDeco.Config.TryGetValue(_params[1], out var config))
                    {
                        switch (_params[2])
                        {

                            case "Pass":
                                config.Pass = int.Parse(_params[3]);
                                break;

                            case "ScaleMin":
                                config.Scale.AxisX.Min = config.Scale.AxisY.Min =
                                config.Scale.AxisZ.Min = float.Parse(_params[3]);
                                break;
                            case "ScaleMax":
                                config.Scale.AxisX.Max = config.Scale.AxisY.Max = 
                                config.Scale.AxisZ.Max = float.Parse(_params[3]);
                                break;

                            case "ScaleMinX":
                                config.Scale.AxisX.Min = float.Parse(_params[3]);
                                break;
                            case "ScaleMaxX":
                                config.Scale.AxisX.Max = float.Parse(_params[3]);
                                break;
                            case "ScaleMinY":
                                config.Scale.AxisY.Min = float.Parse(_params[3]);
                                break;
                            case "ScaleMaxY":
                                config.Scale.AxisY.Max = float.Parse(_params[3]);
                                break;
                            case "ScaleMinZ":
                                config.Scale.AxisZ.Min = float.Parse(_params[3]);
                                break;
                            case "ScaleMaxZ":
                                config.Scale.AxisZ.Max = float.Parse(_params[3]);
                                break;

                            case "RotX":
                                config.Rotation.AxisX.Min = -float.Parse(_params[3]) / 2;
                                config.Rotation.AxisX.Max = +float.Parse(_params[3]) / 2;
                                break;
                            case "RotY":
                                config.Rotation.AxisY.Min = -float.Parse(_params[3]) / 2;
                                config.Rotation.AxisY.Max = +float.Parse(_params[3]) / 2;
                                break;
                            case "RotZ":
                                config.Rotation.AxisZ.Min = -float.Parse(_params[3]) / 2;
                                config.Rotation.AxisZ.Max = +float.Parse(_params[3]) / 2;
                                break;

                            case "RotMinX":
                                config.Rotation.AxisX.Min = float.Parse(_params[3]);
                                break;
                            case "RotMaxX":
                                config.Rotation.AxisX.Max = float.Parse(_params[3]);
                                break;
                            case "RotMinY":
                                config.Rotation.AxisY.Min = float.Parse(_params[3]);
                                break;
                            case "RotMaxY":
                                config.Rotation.AxisY.Max = float.Parse(_params[3]);
                                break;
                            case "RotMinZ":
                                config.Rotation.AxisZ.Min = float.Parse(_params[3]);
                                break;
                            case "RotMaxZ":
                                config.Rotation.AxisZ.Max = float.Parse(_params[3]);
                                break;

                            case "AltRotX":
                                config.AltRotation.AxisX.Min = -float.Parse(_params[3]) / 2;
                                config.AltRotation.AxisX.Max = +float.Parse(_params[3]) / 2;
                                break;
                            case "AltRotY":
                                config.AltRotation.AxisY.Min = -float.Parse(_params[3]) / 2;
                                config.AltRotation.AxisY.Max = +float.Parse(_params[3]) / 2;
                                break;
                            case "AltRotZ":
                                config.AltRotation.AxisZ.Min = -float.Parse(_params[3]) / 2;
                                config.AltRotation.AxisZ.Max = +float.Parse(_params[3]) / 2;
                                break;

                            case "AltRotMinX":
                                config.AltRotation.AxisX.Min = float.Parse(_params[3]);
                                break;
                            case "AltRotMaxX":
                                config.AltRotation.AxisX.Max = float.Parse(_params[3]);
                                break;
                            case "AltRotMinY":
                                config.AltRotation.AxisY.Min = float.Parse(_params[3]);
                                break;
                            case "AltRotMaxY":
                                config.AltRotation.AxisY.Max = float.Parse(_params[3]);
                                break;
                            case "AltRotMinZ":
                                config.AltRotation.AxisZ.Min = float.Parse(_params[3]);
                                break;
                            case "AltRotMaxZ":
                                config.AltRotation.AxisZ.Max = float.Parse(_params[3]);
                                break;

                            default:
                                Log.Warning("Unknown set option " + _params[1]);
                                break;

                        }
                        Log.Out("Updated {0} (Pass {1})", _params[1], config.Pass);
                        Log.Out("  Scale: {0}", config.Scale);
                        Log.Out("  Rotation: {0}", config.Rotation);
                        Log.Out("  AltRotation: {0}", config.AltRotation);

                    }
                    else Log.Out("Could not find config for " + _params[1]);
                    break;

                default:
                    Log.Warning("Unknown command " + _params[0]);
                    break;

            }
        }
        else
        {
            Log.Warning("Invalid `crd` command");
        }

    }

    public override string[] getCommands()
    {
        return new string[]
        {
            "reload",
            "list",
            "count",
            "find",
            "get",
            "set",
            "block",
        };
    }

    public override string getDescription()
    {
        return "Mess with crooked deco settings";
    }

}
