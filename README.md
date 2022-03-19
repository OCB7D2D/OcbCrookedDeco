# OCB Crooked Deco Modlet - 7 Days to Die Addon

This mod adds (small) random variations to decorations and blocks.
It alters scale and rotation for configured blocks within certain
ranges. These need to be setup individually in order to not mess
up visual rendering. It supports nearly all block model types,
except `Ext3dModel`, `ShapeNew` and `BillboardPlants`. For some
of these I'll release other more specialized mods in the future.

## Block Variation Settings

The whole setup can be configured via config files that live
inside the [`Settings`][2] folder (files ending with `.cfg`). The
setup has two phases, one is to configure the *variation types*.
Second is to map *Block names* to *variation types*.

### Variation Config

The format is `${TYPE}:${PASS};${OPT1};${OPT...}`, e.g.

`TrashPile:0;SCLXZ(0.85,1.2);RX2Y1Z2(15,45,15)`

This will configure the variation to be applied on pass 0.
Often pass 0 will work and you only need to increase this
if you find a block that only gets the variations applied on
another pass (trees being a prime example needing pass 3).

#### Repeatable Options

You can repeat as many options as you want, but normally you
at most need two, the scale and the rotation. The example
above will scale the x and z axis with a random factor
between 0.85 and 1.2 (with square distribution).

Additionally it will add a rotation in the range -7.5/+7.5
to the x-axis and z-axis, and -22.5/+22.5 in the y-axis.
The x- and z- axis has a square distribution, while the
y-axis has a normal distribution.

Square distributions will prefer the middle of the range.

### Variation Mapping

The format is `Map:${TYPE}:${REGEX}`, e.g.

`MAP:TrashPile:^cntTrashPile`

## Static Randomness

In order to support these variations I had to come up with
a static pseudo random number generator. We can't use regular
random number generators, since we need to ensure that we get
the same random number at the same map position each time.
To support this I create random numbers by using the block
positions, with some additional random seeds where needed.

## Console Commands

In order to ease testing there are quite a few console [commands
available][3] (bring up the console by hitting F1 in-game). Note
that already rendered blocks will not be updated. Only newly
created blocks will have the latest config applied.

- `crd reload`: Re-read all config files
- `crd list`: Last all crooked config types
- `crd find ${type}`: List blocks with `type`
- `crd get ${type}`: Get options for `type`
- `crd set ${type} ...`: Set options for `type`
- `crd block ${block} ${type}`: Set `block` to `type`

## Changelog

### Version 0.1.0

- Initial version

## Compatibility

I've developed and tested this Mod against version a20.3b3.

[1]: https://github.com/OCB7D2D/A20BepInExPreloader
[3]: Library/CrookedDecoCmd.cs
[2]: Settings
