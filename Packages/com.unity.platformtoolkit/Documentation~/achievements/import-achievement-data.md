# Import achievement data

Use the import feature of the Achievement Editor to bulk import achievement data from a CSV file. This is useful if you have many achievements to add, or if you want to update existing achievements in bulk. You can also export achievement data to a CSV file to update externally and re-import if required.

## CSV format

The CSV file must follow a specific format in order for achievement data to import successfully.

### Required information

| **Column name**       | **Description**                                     |
|-------------------|-------------------------------------------------|
| **ID**         | The unique identifier for the achievement.      |
| **Progress_Target**   | Use **Progress_Target** to define the type of achievement: <br><ul><li>Enter a value of `1` for single achievements.</li><li>Enter a value greater than `1` for progressive achievements, representing the total progress required to unlock the achievement.</li></ul> For more information on Achievement types, refer to [Introduction to Achievements](achievements-introduction.md).|
| **Unity.\<Platform\>**          | Set an achievement ID for each platform service you want to use. The allowed platform service column headings are as follows: <ul><li>`Unity.Steam`</li><li>`Unity.PlayGamesServices`</li><li>`Unity.GameKit`</li><li>`Unity.GDK`</li><li>`Unity.Playstation5`</li><li>`Unity.Switch`</li></ul> |

> [!Note]
> When entering achievement IDs, the following restrictions apply:
> * ID values can't exceed 64 characters in length.
> * ID values can only contain the following alphanumeric characters (`A-Z`, `a-z`, `0-9`), and the following special characters: (`_`, `-`).

### Example

The following example shows a CSV file with achievement data for multiple platforms:

```csv
ID,Progress_Target,Unity.Steam,Unity.Playstation5,Unity.PlayGamesServices,Unity.GDK,Unity.GameKit
AE1,1,AE1,0,4t0OEAIQAQ,1,AE1
AE2,1,AE2,2,C4t0OEAIQAg,2,AE2
AE3,1,AE3,3,4t0OEAIQDg,10,AE3
AP1,100,AP1,1,4t0OEAIQBA,3,AP1
AP2,10000,AP2,4,4t0OEAIQBQ,4,AP2
AP3,150,AP3,7,4t0OEAIQBg,5,AP3
AP4,30,AP4,6,4t0OEAIQCQ,6,AP4
AP5,100,AP5,8,4t0OEAIQCg,7,AP5
AP6,100,AP6,9,4t0OEAIQCw,8,AP6
AP7,2147483647,AP7,10,C4t0OEAIQDQ,9,AP7
AP8,5,AP8,5,4t0OEAIQDw,11,AP8
```

## Import a CSV file

To import achievement data from a CSV file, use the following steps:

> [!NOTE]
> Importing achievement data from a CSV file overrides all existing achievement data in the Achievement Editor. It's recommended to make a backup of your existing Achievement Editor data before proceeding with an import. Refer to [Export a CSV file](#export-a-csv-file) for instructions on exporting achievement data.

1. In the Unity Editor, open the Achievement Editor window by selecting **Window** > **Platform Toolkit** > **Achievement Editor**.
2. In the Achievement Editor window, select the **Import** button.
3. In the file dialog, select the CSV file you want to import.
4. Click **Open** to start the import process.

## Import rules

The following rules apply when importing achievement data from a CSV file:

* The `ID` value serves as the primary key. It must not repeat in the ID column, and must not be empty.
* If the CSV contains columns for platforms that aren't installed, the import process ignores those columns.
* If the Achievement Editor contains achievements that aren't in the CSV, the import process removes those achievements.
* If the CSV contains achievements not in the Achievement Editor, the import process creates those achievements.
* The import process overrides all fields in the Achievement Editor with their CSV equivalents.
* Unity displays a warning if the CSV doesn't contain a column for a known platform.
* If the CSV doesn't contain a column for a known platform, the import process doesn't change the data for that platform.
* If the CSV doesn't contain one of the common achievement headers, the import process aborts, doesn't change any data, and displays an error.
* If the CSV contains any errors, for example a row length mismatch or data type mismatch, the import process fails and doesn't update any data.

## Export a CSV file

To export achievement data to a CSV file, use the following steps:

1. In the Unity Editor, open the Achievement Editor window by selecting **Window** > **Platform Toolkit** > **Achievement Editor**.
2. In the Achievement Editor window, select the **Export** button.
3. In the file dialog, choose the location and file name for the CSV file.

You can then manually edit the exported CSV file and re-import it if required.

## Additional resources

* [Configure achievements](configure-achievements.md)
* [Unlock achievements](unlock-achievements.md)