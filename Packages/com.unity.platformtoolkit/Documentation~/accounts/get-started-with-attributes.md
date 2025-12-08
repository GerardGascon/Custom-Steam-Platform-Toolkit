# Get started with attributes

Attributes simplify cross-platform development by letting you assign a single, consistent name to a piece of account data, which you then map to the unique API for each platform. This allows you to write platform-agnostic code to retrieve account information.

## Attribute types

There are two types of attributes, default, and user created. Default attributes are predefined and include common platform account APIs such as display name and profile picture. User created attributes are custom attribute names you can define to access specific platform account APIs.

## Access attribute settings

Use the following steps to access attribute values:

1. Open the Project Settings window (menu: **Edit** > **Project Settings**).
2. In the category list, under **Platform Toolkit**, select your target platform to display the Attribute settings panel.

The attribute settings panel has two sections: **Default Attributes** and **User Created Attributes**.

## Assign attribute values

From the **User Created Attributes** section, you can assign a custom attribute value to a specific platform account API. Select the platform account API you want to use from the dropdown, and then enter the desired attribute value.

It's recommended to use the same attribute names across different target platforms when accessing similar account information.

## Use attributes in your project

To access attributes in your project, you can use the [GetAttribute](xref:Unity.PlatformToolkit.IAccount.GetAttribute``1(System.String)) method when interacting with an account. The following example demonstrates how to retrieve a user-created attribute named `MAIN_MENU_NAME` and display it in a UI label:

```csharp
try
{
    if (account.HasAttribute<string>("MAIN_MENU_NAME"))
    {
        string name = await account.GetAttribute<string>("MAIN_MENU_NAME");

        nameLabel.style.display = DisplayStyle.Flex;
        nameLabel.text = name;
    }
    else
    {
        nameLabel.style.display = DisplayStyle.None;
    }
}
catch (InvalidAccountException e)
{
    // Handle signed out account
}

```

## Additional resources

* [IAccount Scripting API](xref:Unity.PlatformToolkit.IAccount)
* [GetAttribute](xref:Unity.PlatformToolkit.IAccount.GetAttribute``1(System.String))
