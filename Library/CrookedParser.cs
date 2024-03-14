using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OCB
{
    class CrookedParser
    {
        static bool IsToppledUp(int rotation)
        {
            switch (rotation)
            {
                case 8: case 10: case 13: case 15:
                case 16: case 18: case 21: case 23:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsToppledFlat(int rotation)
        {
            switch (rotation)
            {
                case 9: case 11: case 12: case 14:
                case 17: case 19: case 20: case 22:
                    return true;
                default:
                    return false;
            }
        }

        private static readonly Regex StripWS = new Regex(@"\s+");

        public static void ParseMapping(string path)
        {
            bool debug = false;
            // Log.Warning("Reading crooked mappings at {0}", path);
            foreach (string line in System.IO.File.ReadLines(path))
            {
                string parsing = StripWS.Replace(line, "");
                if (string.IsNullOrWhiteSpace(parsing)) continue;
                if (parsing[0] == '#') continue;
                int pos = parsing.IndexOf(':');
                if (pos == 0) throw new Exception("Invalid config");
                if (pos == -1) throw new Exception("Invalid config");
                string name = line.Substring(0, pos);
                if (name == "DEBUG")
                {
                    debug = bool.Parse(parsing.Substring(pos + 1));
                    continue;
                }
                if (name != "MAP") continue;
                string[] ops = parsing.Substring(pos + 1).Split(':');
                if (ops.Length < 2) throw new Exception("Not enough arguments for mapping");
                for (int i = 1; i < ops.Length; i += 1)
                {
                    Regex search = new Regex(ops[i]);
                    foreach (Block block in Block.list)
                    {
                        if (block == null) continue;
                        string blk = block.GetBlockName();
                        blk = blk.Split(':').Last();
                        if (search.Match(blk).Success)
                        {
                            if (debug) Log.Warning("Map {0} to {1}", ops[0], block);
                            block.Properties.Values["DynamicTransform"] = ops[0];
                        }
                    }
                }
            }
        }

        public static void ParseConfig(string path,
            ref Dictionary<string, CrookedConfig> Config)
        {
            bool debug = false;
            // Log.Out("Reading crooked settings at {0}", path);
            foreach (string line in System.IO.File.ReadLines(path))
            {
                string parsing = StripWS.Replace(line, "");
                if (string.IsNullOrWhiteSpace(parsing)) continue;
                if (parsing[0] == '#') continue;
                int pos = parsing.IndexOf(':');
                if (pos == 0) throw new Exception("Invalid config");
                if (pos == -1) throw new Exception("Invalid config");
                string name = line.Substring(0, pos);
                if (name == "DEBUG")
                {
                    debug = bool.Parse(parsing.Substring(pos + 1));
                    continue;
                }
                if (name == "MAP") continue;
                string[] ops = parsing.Substring(pos + 1).Split(';');
                if (ops.Length == 0) throw new Exception("Pass must be defined");
                Config.TryGetValue(name, out CrookedConfig config);
                if (config == null) config = new CrookedConfig(0, null, null);
                config.Pass = int.Parse(ops[0]); // will throw on wrong format
                for (int i = 1; i < ops.Length; i += 1)
                {
                    string cfg = ops[i];
                    if (cfg == "IsToppledUp")
                    {
                        config.FnToppled = IsToppledUp;
                    }
                    else if (cfg == "IsToppledFlat")
                    {
                        config.FnToppled = IsToppledFlat;
                    }
                    else if (cfg == "" || cfg == "NONE") { }
                    else
                    {
                        pos = cfg.IndexOf('(');
                        if (pos == -1) throw new Exception("Opening parentheses not found: " + cfg);
                        if (cfg.Last() != ')') throw new Exception("Closing parentheses not found" + cfg);
                        var numbers = cfg.Substring(pos + 1, cfg.Length - pos - 2)
                            .Split(',').ToList().ConvertAll(float.Parse);
                        switch (cfg.Substring(0, pos))
                        {
                            case "SCL":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Scale = new CrookedVector(
                                            numbers[0], numbers[1]);
                                        break;
                                    case 6:
                                        config.Scale = new CrookedVector3(
                                            numbers[0], numbers[1],
                                            numbers[2], numbers[3],
                                            numbers[4], numbers[5]);
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for SCL");
                                }
                                break;
                            case "SCLXY":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Scale = new CrookedVector3(
                                            numbers[0], numbers[1],
                                            numbers[0], numbers[1],
                                            1f, 1f);
                                        break;
                                    case 4:
                                        config.Scale = new CrookedVector3(
                                            numbers[0], numbers[1],
                                            numbers[2], numbers[3],
                                            1f, 1f);
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for SCLXY");
                                }
                                break;
                            case "SCLXZ":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Scale = new CrookedVector3(
                                            numbers[0], numbers[1],
                                            1f, 1f,
                                            numbers[0], numbers[1]);
                                        break;
                                    case 4:
                                        config.Scale = new CrookedVector3(
                                            numbers[0], numbers[1],
                                            1f, 1f,
                                            numbers[2], numbers[3]);
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for SCLXZ");
                                }
                                break;
                            case "SCLYZ":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Scale = new CrookedVector3(
                                            1f, 1f,
                                            numbers[0], numbers[1],
                                            numbers[0], numbers[1]);
                                        break;
                                    case 4:
                                        config.Scale = new CrookedVector3(
                                            1f, 1f,
                                            numbers[0], numbers[1],
                                            numbers[2], numbers[3]);
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for SCLXZ");
                                }
                                break;
                            case "SCLX":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Scale = new CrookedVector3(
                                            numbers[0], numbers[1],
                                            1f, 1f,
                                            1f, 1f);
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for SCLXZ");
                                }
                                break;
                            case "SCLY":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Scale = new CrookedVector3(
                                            1f, 1f,
                                            numbers[0], numbers[1],
                                            1f, 1f);
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for SCLXZ");
                                }
                                break;
                            case "SCLZ":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Scale = new CrookedVector3(
                                            1f, 1f,
                                            1f, 1f,
                                            numbers[0], numbers[1]);
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for SCLXZ");
                                }
                                break;
                            case "RX1":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(numbers[0], numbers[1], StaticRandom.Range),
                                            new CrookedAxis(0, 0, StaticRandom.Range),
                                            new CrookedAxis(0, 0, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RX1");
                                }
                                break;
                            case "RY1":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(0, 0, StaticRandom.Range),
                                            new CrookedAxis(numbers[0], numbers[1], StaticRandom.Range),
                                            new CrookedAxis(0, 0, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RY1");
                                }
                                break;
                            case "RZ1":
                                switch (numbers.Count)
                                {
                                    case 2:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(0, 0, StaticRandom.Range),
                                            new CrookedAxis(0, 0, StaticRandom.Range),
                                            new CrookedAxis(numbers[0], numbers[1], StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RZ1");
                                }
                                break;
                            case "RX1Y1Z1":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RX1Y1Z1");
                                }
                                break;
                            case "RX1Y2Z2":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.RangeSquare));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RX1Y2Z2");
                                }
                                break;
                            case "RX2Y1Z2":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.RangeSquare));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RX2Y1Z2");
                                }
                                break;
                            case "RX2Y2Z1":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RX2Y2Z1");
                                }
                                break;
                            case "RX2Y1Z1":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RX2Y1Z1");
                                }
                                break;
                            case "RX1Y2Z1":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RX1Y2Z1");
                                }
                                break;
                            case "RX1Y1Z2":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.RangeSquare));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RX1Y1Z2");
                                }
                                break;
                            case "RX2Y2Z2":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.Rotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.RangeSquare));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for RX2Y2Z2");
                                }
                                break;
                            case "ARX1Y1Z1":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.AltRotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for ARX1Y1Z1");
                                }
                                break;
                            case "ARX1Y2Z2":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.AltRotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.RangeSquare));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for ARX1Y2Z2");
                                }
                                break;
                            case "ARX2Y1Z2":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.AltRotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.RangeSquare));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for ARX2Y1Z2");
                                }
                                break;
                            case "ARX2Y2Z1":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.AltRotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for ARX2Y2Z1");
                                }
                                break;
                            case "ARX2Y1Z1":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.AltRotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for ARX2Y1Z1");
                                }
                                break;
                            case "ARX1Y2Z1":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.AltRotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.Range));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for ARX1Y2Z1");
                                }
                                break;
                            case "ARX1Y1Z2":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.AltRotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.Range),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.RangeSquare));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for ARX1Y1Z2");
                                }
                                break;
                            case "ARX2Y2Z2":
                                switch (numbers.Count)
                                {
                                    case 3:
                                        config.AltRotation = new CrookedVector3(
                                            new CrookedAxis(-numbers[0] / 2, numbers[0] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[1] / 2, numbers[1] / 2, StaticRandom.RangeSquare),
                                            new CrookedAxis(-numbers[2] / 2, numbers[2] / 2, StaticRandom.RangeSquare));
                                        break;
                                    default:
                                        throw new Exception("Invalid number of arguments for ARX2Y2Z2");
                                }
                                break;
                            default:
                                Log.Out("Unknown {0}", cfg);
                                break;
                        }
                    }
                }
                // Only for debugging purposes
                if (debug) Log.Out("Parsed: {0} => {1}", name, config);
                // Set or update config
                Config[name] = config;
            }
        }
    }
}