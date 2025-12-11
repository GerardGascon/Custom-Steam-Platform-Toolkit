# Custom Platform Toolkit for Steam

An open-source alternative to Unity's Steam Platform Toolkit extension.

## Disclaimer

I'm just doing this because I'd like to use Platform Toolkit in my projects but as an average indie developer, I don't
have €2000 lying around.

I'll probably implement all the features needed for my games, for now it should cover the entire platform toolkit
features. But, some potential future features may not be developed by me (i.e. online features).

If you want to add support to any feature, feel free to make a PR.

## Installation

If you want to add it to your project, you may refer to
this [file](Packages/com.geri.platformtoolkit.steam/Documentation~/installation.md).

## Status

See which features are implemented. If all the checkboxes are checked, it means we have 100% compatibility with Platform
Toolkit.

- [x] Accounts
    - [x] Get account name
    - [x] Get account picture
    - [x] Attributes
    - [x] States
- [x] Achievements
    - [x] Single achievements
    - [x] Progressive achievements
- [x] Save system
    - [x] Save file writing
    - [x] Save file reading
    - [x] Enumerate saves
    - [x] DataStore
    - [x] Local save system
        - Note: Even though PCs support local save systems, I won't add support for now as you can simply use an
          account-based save system, and it will just work.

          Quoting Unity's documentation:
          `On platforms that support both an account saving system and a local saving system, it's recommended to use the account saving system and only fall back on the local saving system if an account saving system is unavailable`
          I won't be adding support for that as Steam can store data for different accounts.

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
