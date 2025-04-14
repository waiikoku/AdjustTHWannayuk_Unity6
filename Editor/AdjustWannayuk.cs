using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel; // Added for Unity 6 TextCore types

public class AdjustWannayuk : EditorWindow
{
    static bool overrideAll = true;
    static float wannayukHeight = 30f;
    static float aumXPlacementAfter = -35f;
    static float aumXPlacementBefore = 10f;

    Object _fontAsset;
    void OnGUI()
    {
        _fontAsset = EditorGUILayout.ObjectField("FontAsset", _fontAsset, typeof(TMP_FontAsset), false);
        overrideAll = EditorGUILayout.Toggle("Override", overrideAll);
        wannayukHeight = EditorGUILayout.FloatField("Height offset", wannayukHeight);
        aumXPlacementAfter = EditorGUILayout.FloatField("สระอำ x offset after", aumXPlacementAfter);
        aumXPlacementBefore = EditorGUILayout.FloatField("สระอำ x offset before", aumXPlacementBefore);

        if (GUILayout.Button("Adjust"))
        {
            TMP_FontAsset fontAsset = _fontAsset as TMP_FontAsset;
            _Adjust(fontAsset);
        }
        if (GUILayout.Button("Clear"))
        {
            TMP_FontAsset fontAsset = _fontAsset as TMP_FontAsset;
            _Clear(fontAsset);
        }
    }

    [MenuItem("Window/TextMeshPro/AdjustWannayuk")]
    public static void AdjustThaiSara()
    {
        EditorWindow.GetWindow(typeof(AdjustWannayuk));
    }

    /// <summary>
    /// Quick adjust by ContextMenu
    /// </summary>
    [MenuItem("Assets/AdjustWannayuk")]
    static void Adjust()
    {
        TMP_FontAsset fontAsset = Selection.activeObject as TMP_FontAsset;
        _Adjust(fontAsset);
    }

    [MenuItem("Assets/AdjustWannayuk", true)]
    static bool ValidateLogSelection()
    {
        return Selection.activeObject is TMP_FontAsset;
    }

    static void _Adjust(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null)
        {
            Debug.Log("No FontAsset selected");
            return;
        }

        // Get the list of TMP_GlyphPairAdjustmentRecords and convert them to UnityEngine.TextCore.LowLevel.GlyphPairAdjustmentRecord
        var tmpGlyphPairAdjustmentRecords = GetGlyphPairAdjustmentRecords(fontAsset);
        var lookupTable = fontAsset.characterLookupTable;

        // Get the lookup dictionary via reflection
        var dictionaryField = fontAsset.fontFeatureTable.GetType().GetField("m_GlyphPairAdjustmentRecordLookupDictionary",
                                                                          BindingFlags.NonPublic | BindingFlags.Instance);
        var glyphPairAdjustmentRecordLookupDictionary = dictionaryField != null ?
            (Dictionary<uint, TMP_GlyphPairAdjustmentRecord>)dictionaryField.GetValue(fontAsset.fontFeatureTable) : null;

        int[] saras = new int[7];
        int[] wannayuks = new int[4];

        //get sara 
        saras[0] = (int)lookupTable[GetUnicodeCharacter("ิ")].glyphIndex;  // อิ
        saras[1] = (int)lookupTable[GetUnicodeCharacter("ี")].glyphIndex;  // อี
        saras[2] = (int)lookupTable[GetUnicodeCharacter("ึ")].glyphIndex;  // อึ
        saras[3] = (int)lookupTable[GetUnicodeCharacter("ื")].glyphIndex;  // อื
        saras[4] = (int)lookupTable[GetUnicodeCharacter("ำ")].glyphIndex; // ำ
        saras[5] = (int)lookupTable[GetUnicodeCharacter("ั")].glyphIndex;  // ั
        saras[6] = (int)lookupTable[GetUnicodeCharacter("ํ")].glyphIndex;  // ํ
        //get wanna yuk
        wannayuks[0] = (int)lookupTable[GetUnicodeCharacter("่")].glyphIndex; //เอก
        wannayuks[1] = (int)lookupTable[GetUnicodeCharacter("้")].glyphIndex; //โท
        wannayuks[2] = (int)lookupTable[GetUnicodeCharacter("๊")].glyphIndex; //ตรี
        wannayuks[3] = (int)lookupTable[GetUnicodeCharacter("๋")].glyphIndex; //จัตวา
        int recordAdd = 0;
        foreach (var sara in saras)
        {
            foreach (var wannayuk in wannayuks)
            {
                float xPlacement = sara == saras[4] || sara == saras[6] ? aumXPlacementAfter : 0;

                TMP_GlyphValueRecord saraPosition = new TMP_GlyphValueRecord(0, 0, 0, 0);
                TMP_GlyphAdjustmentRecord saraGlyph = new TMP_GlyphAdjustmentRecord((uint)sara, saraPosition);

                TMP_GlyphValueRecord wannayukPosition = new TMP_GlyphValueRecord(xPlacement, wannayukHeight, 0, 0);
                TMP_GlyphAdjustmentRecord wannayukGlyph = new TMP_GlyphAdjustmentRecord((uint)wannayuk, wannayukPosition);

                var saraThenWannayukGlyphPair = new TMP_GlyphPairAdjustmentRecord(saraGlyph, wannayukGlyph);

                if (sara == saras[4] || sara == saras[6])
                {
                    xPlacement = aumXPlacementBefore;
                    wannayukPosition = new TMP_GlyphValueRecord(xPlacement, wannayukHeight, 0, 0);
                    wannayukGlyph = new TMP_GlyphAdjustmentRecord((uint)wannayuk, wannayukPosition);
                }

                var wannayukThenSaraGlyphPair = new TMP_GlyphPairAdjustmentRecord(wannayukGlyph, saraGlyph);

                uint firstPairKey = saraThenWannayukGlyphPair.firstAdjustmentRecord.glyphIndex << 16 | saraThenWannayukGlyphPair.secondAdjustmentRecord.glyphIndex;
                uint secondPairKey = wannayukThenSaraGlyphPair.firstAdjustmentRecord.glyphIndex << 16 | wannayukThenSaraGlyphPair.secondAdjustmentRecord.glyphIndex;

                if (overrideAll)
                {
                    tmpGlyphPairAdjustmentRecords.RemoveAll(record => IsGlyphPairEqual(record, saraThenWannayukGlyphPair) ||
                                                                   IsGlyphPairEqual(record, wannayukThenSaraGlyphPair));

                    tmpGlyphPairAdjustmentRecords.Add(saraThenWannayukGlyphPair);
                    tmpGlyphPairAdjustmentRecords.Add(wannayukThenSaraGlyphPair);

                    if (glyphPairAdjustmentRecordLookupDictionary != null)
                    {
                        if (glyphPairAdjustmentRecordLookupDictionary.ContainsKey(firstPairKey))
                            glyphPairAdjustmentRecordLookupDictionary[firstPairKey] = saraThenWannayukGlyphPair;
                        else
                            glyphPairAdjustmentRecordLookupDictionary.Add(firstPairKey, saraThenWannayukGlyphPair);

                        if (glyphPairAdjustmentRecordLookupDictionary.ContainsKey(secondPairKey))
                            glyphPairAdjustmentRecordLookupDictionary[secondPairKey] = wannayukThenSaraGlyphPair;
                        else
                            glyphPairAdjustmentRecordLookupDictionary.Add(secondPairKey, wannayukThenSaraGlyphPair);
                    }

                    recordAdd += 2;
                }
                else if (glyphPairAdjustmentRecordLookupDictionary != null)
                {
                    if (!glyphPairAdjustmentRecordLookupDictionary.ContainsKey(firstPairKey))
                    {
                        tmpGlyphPairAdjustmentRecords.Add(saraThenWannayukGlyphPair);
                        glyphPairAdjustmentRecordLookupDictionary.Add(firstPairKey, saraThenWannayukGlyphPair);
                        recordAdd++;
                    }

                    if (!glyphPairAdjustmentRecordLookupDictionary.ContainsKey(secondPairKey))
                    {
                        tmpGlyphPairAdjustmentRecords.Add(wannayukThenSaraGlyphPair);
                        glyphPairAdjustmentRecordLookupDictionary.Add(secondPairKey, wannayukThenSaraGlyphPair);
                        recordAdd++;
                    }
                }
            }
        }

        if (recordAdd > 0)
        {
            // Convert TMP_GlyphPairAdjustmentRecord list to UnityEngine.TextCore.LowLevel.GlyphPairAdjustmentRecord list
            SetGlyphPairAdjustmentRecords(fontAsset, tmpGlyphPairAdjustmentRecords);

            // Call SortGlyphPairAdjustmentRecords through reflection to avoid type issues
            var sortMethod = fontAsset.fontFeatureTable.GetType().GetMethod("SortGlyphPairAdjustmentRecords");
            if (sortMethod != null)
            {
                sortMethod.Invoke(fontAsset.fontFeatureTable, null);
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            Canvas.ForceUpdateCanvases();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        Debug.Log("Adjust font : <color=#2bcaff>" + fontAsset.name + "</color>" +
                  " Height offset : <color=#d8ff2b>" + wannayukHeight + "</color>" +
                  " Number of adjustment add : <color=#5dfa41>" + recordAdd + "</color>");
    }

    static void _Clear(TMP_FontAsset fontAsset)
    {
        // Use reflection to set an empty list
        var setListMethod = fontAsset.fontFeatureTable.GetType().GetMethod("SetGlyphPairAdjustmentRecords");
        if (setListMethod != null)
        {
            // Create an empty list of the correct type
            var emptyListType = typeof(List<>).MakeGenericType(
                System.Type.GetType("UnityEngine.TextCore.LowLevel.GlyphPairAdjustmentRecord") ??
                System.Type.GetType("TMPro.TMP_GlyphPairAdjustmentRecord"));

            var emptyList = System.Activator.CreateInstance(emptyListType);
            setListMethod.Invoke(fontAsset.fontFeatureTable, new[] { emptyList });
        }
        else
        {
            // Fallback approach for older Unity versions
            try
            {
                var field = fontAsset.fontFeatureTable.GetType().GetField("m_GlyphPairAdjustmentRecords",
                                                                        BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var listType = field.FieldType;
                    var emptyList = System.Activator.CreateInstance(listType);
                    field.SetValue(fontAsset.fontFeatureTable, emptyList);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to clear adjustment records: " + ex.Message);
            }
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        Canvas.ForceUpdateCanvases();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    static uint GetUnicodeCharacter(string source)
    {
        uint unicode;

        if (source.Length == 1)
            unicode = source[0];
        else if (source.Length == 6)
            unicode = (uint)TMP_TextUtilities.StringHexToInt(source.Replace("\\u", ""));
        else
            unicode = (uint)TMP_TextUtilities.StringHexToInt(source.Replace("\\U", ""));

        return unicode;
    }

    static bool IsGlyphPairEqual(TMP_GlyphPairAdjustmentRecord a, TMP_GlyphPairAdjustmentRecord b)
    {
        return a.firstAdjustmentRecord.glyphIndex == b.firstAdjustmentRecord.glyphIndex &&
               a.secondAdjustmentRecord.glyphIndex == b.secondAdjustmentRecord.glyphIndex;
    }

    // Helper method to get TMP_GlyphPairAdjustmentRecords safely
    private static List<TMP_GlyphPairAdjustmentRecord> GetGlyphPairAdjustmentRecords(TMP_FontAsset fontAsset)
    {
        List<TMP_GlyphPairAdjustmentRecord> result = new List<TMP_GlyphPairAdjustmentRecord>();

        try
        {
            // First try to get via property
            var propertyInfo = fontAsset.fontFeatureTable.GetType().GetProperty("glyphPairAdjustmentRecords");
            if (propertyInfo != null)
            {
                // Handle the possible type difference using reflection
                var records = propertyInfo.GetValue(fontAsset.fontFeatureTable);

                // If it's already the right type, just cast it
                if (records is List<TMP_GlyphPairAdjustmentRecord> tmpRecords)
                {
                    return new List<TMP_GlyphPairAdjustmentRecord>(tmpRecords);
                }

                // Otherwise, try to convert each item
                if (records is IEnumerable<object> enumerable)
                {
                    foreach (var record in enumerable)
                    {
                        // Convert from Unity TextCore type to TMP type
                        if (record != null)
                        {
                            var firstGlyphIndex = (uint)record.GetType().GetProperty("firstGlyphIndex").GetValue(record);
                            var secondGlyphIndex = (uint)record.GetType().GetProperty("secondGlyphIndex").GetValue(record);

                            var firstAdjustmentRecord = new TMP_GlyphAdjustmentRecord(
                                firstGlyphIndex,
                                new TMP_GlyphValueRecord(0, 0, 0, 0)
                            );

                            var secondAdjustmentRecord = new TMP_GlyphAdjustmentRecord(
                                secondGlyphIndex,
                                new TMP_GlyphValueRecord(0, 0, 0, 0)
                            );

                            result.Add(new TMP_GlyphPairAdjustmentRecord(firstAdjustmentRecord, secondAdjustmentRecord));
                        }
                    }
                }
            }

            // Alternatively, try to access the private field
            if (result.Count == 0)
            {
                var fieldInfo = fontAsset.fontFeatureTable.GetType().GetField("m_GlyphPairAdjustmentRecords",
                                                                           BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    var records = fieldInfo.GetValue(fontAsset.fontFeatureTable);

                    if (records is List<TMP_GlyphPairAdjustmentRecord> tmpRecords)
                    {
                        return new List<TMP_GlyphPairAdjustmentRecord>(tmpRecords);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error getting glyph pair adjustment records: " + ex.Message);
        }

        return result;
    }

    // Helper method to set TMP_GlyphPairAdjustmentRecords safely
    private static void SetGlyphPairAdjustmentRecords(TMP_FontAsset fontAsset, List<TMP_GlyphPairAdjustmentRecord> records)
    {
        try
        {
            // First try to use a method that might be present in newer TMPro
            var setMethod = fontAsset.fontFeatureTable.GetType().GetMethod("SetGlyphPairAdjustmentRecords");
            if (setMethod != null)
            {
                // Check the parameter type of the method
                var parameters = setMethod.GetParameters();
                if (parameters.Length > 0)
                {
                    var paramType = parameters[0].ParameterType;

                    // Create a list of the expected type and convert our TMP records
                    var targetListType = typeof(List<>).MakeGenericType(paramType.GetGenericArguments()[0]);
                    var targetList = System.Activator.CreateInstance(targetListType);

                    // Get the Add method of the target list
                    var addMethod = targetListType.GetMethod("Add");

                    // Get the expected element type
                    var elementType = paramType.GetGenericArguments()[0];

                    // For each of our TMP records, create a record of the expected type
                    foreach (var record in records)
                    {
                        // Create a new instance of the expected type
                        var newRecord = System.Activator.CreateInstance(elementType);

                        // Set the properties based on our TMP record
                        elementType.GetProperty("firstGlyphIndex").SetValue(
                            newRecord, record.firstAdjustmentRecord.glyphIndex);

                        elementType.GetProperty("secondGlyphIndex").SetValue(
                            newRecord, record.secondAdjustmentRecord.glyphIndex);

                        // Get the adjustment record types and properties
                        var glyphAdjustmentRecordType = elementType.GetProperty("firstAdjustmentRecord").PropertyType;
                        var firstAdjRecord = System.Activator.CreateInstance(glyphAdjustmentRecordType);
                        var secondAdjRecord = System.Activator.CreateInstance(glyphAdjustmentRecordType);

                        // Set values
                        glyphAdjustmentRecordType.GetProperty("glyphIndex").SetValue(
                            firstAdjRecord, record.firstAdjustmentRecord.glyphIndex);
                        glyphAdjustmentRecordType.GetProperty("glyphIndex").SetValue(
                            secondAdjRecord, record.secondAdjustmentRecord.glyphIndex);

                        // Set adjustment records
                        elementType.GetProperty("firstAdjustmentRecord").SetValue(newRecord, firstAdjRecord);
                        elementType.GetProperty("secondAdjustmentRecord").SetValue(newRecord, secondAdjRecord);

                        // Add to our target list
                        addMethod.Invoke(targetList, new[] { newRecord });
                    }

                    // Invoke the set method with our converted list
                    setMethod.Invoke(fontAsset.fontFeatureTable, new[] { targetList });
                    return;
                }
            }

            // Alternatively, try to set the property directly
            var propertyInfo = fontAsset.fontFeatureTable.GetType().GetProperty("glyphPairAdjustmentRecords");
            if (propertyInfo != null)
            {
                // Try to set it directly if types match
                try
                {
                    propertyInfo.SetValue(fontAsset.fontFeatureTable, records);
                    return;
                }
                catch
                {
                    // If direct set fails, it's likely due to type mismatch
                    Debug.LogWarning("Direct property set failed, trying alternative approach");
                }
            }

            // Last resort: try to set the underlying field
            var fieldInfo = fontAsset.fontFeatureTable.GetType().GetField("m_GlyphPairAdjustmentRecords",
                                                                       BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                try
                {
                    fieldInfo.SetValue(fontAsset.fontFeatureTable, records);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Failed to set glyph pair adjustment records field: " + ex.Message);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error setting glyph pair adjustment records: " + ex.Message);
        }
    }
}