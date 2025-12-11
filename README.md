# Custom Platform Toolkit for Steam

> [!WARNING]
> Once the package is 100% compatible and tested I'll remove this message.
>
> To see the status, you can take a look at this [section](#status), as well as the [tests folder](Packages/com.geri.platformtoolkit.steam/Tests), all
> passing tests are implemented features.

An open-source alternative to Unity's Steam Platform Toolkit extension currently in progress.

## Disclaimer

I'm just doing this because I'd like to use Platform Toolkit in my projects but as an average indie developer, I don't have
€2000 lying around.

I'll probably implement all the features needed for my games, for now it should cover the entire platform toolkit
features. But, some potential future features may not be developed by me (i.e. online features).

If you want to add support to any feature, feel free to make a PR.

## Status

Custom Steam Platform Toolkit is already in a usable state, but for 100% compatibility, there's still this checklist to
complete.

- [ ] Accounts
    - [x] Get account name
    - [ ] Get account picture (**done, but not tested**)
    - [ ] Attributes
    - [ ] States
- [x] Achievements
    - [x] Single achievements
    - [x] Progressive achievements
- [ ] Save system
    - [x] Save file writing
    - [x] Save file reading
    - [x] Enumerate saves
    - [x] DataStore
    - [ ] Local save system

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
