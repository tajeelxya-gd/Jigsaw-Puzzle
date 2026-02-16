#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class EnumEditorUtility
{
    public static void AddEnumValue(string enumFilePath, string enumName, string newValue, string comment = "")
    {
        if (!File.Exists(enumFilePath))
        {
            Debug.LogError($"Enum file not found at path: {enumFilePath}");
            return;
        }

        // Sanitize enum name for C# validity
        newValue = SanitizeEnumName(newValue);

        string fileText = File.ReadAllText(enumFilePath);

        // Already exists?
        if (Regex.IsMatch(fileText, $@"\b{newValue}\b"))
        {
            Debug.LogWarning($"{newValue} already exists in {enumName}!");
            return;
        }

        // Find enum body
        var match = Regex.Match(fileText, $@"(public\s+enum\s+{enumName}\s*\{{)([^}}]*)\}}");
        if (!match.Success)
        {
            Debug.LogError($"Enum {enumName} not found in file!");
            return;
        }

        string enumBody = match.Groups[2].Value.TrimEnd();

        // Format new line
        string commentSuffix = string.IsNullOrEmpty(comment) ? "" : $" // {comment}";
        string newEntry = $"\n    {newValue},{commentSuffix}";

        // Add comma if needed
        if (!string.IsNullOrEmpty(enumBody) && !enumBody.TrimEnd().EndsWith(","))
            enumBody += ",";

        string updatedBody = enumBody + newEntry;
        string updatedText = fileText.Replace(match.Value, $"{match.Groups[1].Value}{updatedBody}\n}}");

        File.WriteAllText(enumFilePath, updatedText);
        AssetDatabase.Refresh();

        Debug.Log($"✅ Added {newValue} to {enumName} enum ({Path.GetFileName(enumFilePath)})!");
    }

    public static void RemoveEnumValue(string enumFilePath, string enumName, string valueToRemove)
    {
        if (!File.Exists(enumFilePath))
        {
            Debug.LogError($"Enum file not found at path: {enumFilePath}");
            return;
        }

        string fileText = File.ReadAllText(enumFilePath);

        // Remove matching value with optional comma and comment
        string updatedText = Regex.Replace(
            fileText,
            $@"\s*{valueToRemove}\s*,?\s*(//[^\n]*)?",
            "",
            RegexOptions.Multiline
        );

        File.WriteAllText(enumFilePath, updatedText);
        AssetDatabase.Refresh();

        Debug.Log($"❌ Removed {valueToRemove} from {enumName} enum!");
    }

    public static string SanitizeEnumName(string rawName)
    {
        // Remove invalid chars and make PascalCase
        string valid = Regex.Replace(rawName, @"[^a-zA-Z0-9_]", "");
        if (string.IsNullOrEmpty(valid))
            valid = "Unnamed";

        // Uppercase first letter
        return char.ToUpper(valid[0]) + valid.Substring(1);
    }
}
#endif