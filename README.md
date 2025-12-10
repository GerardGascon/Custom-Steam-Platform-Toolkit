# Custom Platform Toolkit for Steam

> [!WARNING]
> I'm still developing the package, it probably doesn't work as I'm currently under a research phase to see how Platform
> Toolkit works.
>
> Once the package is tested and working I'll remove this message.
>
> To see the status, you can take a look at the [tests folder](Packages/com.geri.platformtoolkit.steam/Tests), all
> passing tests are implemented features.

An open-source alternative to Unity's Steam Platform Toolkit extension currently in progress.

## Disclaimer

I just do this because I'd like to use platform toolkit on my projects but as an average indie developer I don't have
€2000 on my pockets.

I'll probably implement all the features needed for my games, for now it should cover the entire platform toolkit
features. But, some potential future features may not be developed by me (i.e. online features).

If you want to add support to any feature, feel free to make a PR.

## Important details

The assemblies use the following names and **cannot** be changed because unity restricts internals access to some
specific assemblies:

- Unity.PlatformToolkit.Steam
- Unity.PlatformToolkit.Steam.Editor
- Unity.PlatformToolkit.Steam.Tests

That also means that you can't use the open-source and Unity's implementations simultaneously. If you have Unity Pro,
just use the official version, as it probably works better than this.

## Documentation

For development and usage documentation I'll be using
the [Documentation~ folder](Packages/com.geri.platformtoolkit.steam/Documentation~), so please refer to that when
extending the functionality and/or using this package in your project.

## Installation

If you want to add it to your project, you may refer to
this [file](Packages/com.geri.platformtoolkit.steam/Documentation~/installation.md).
