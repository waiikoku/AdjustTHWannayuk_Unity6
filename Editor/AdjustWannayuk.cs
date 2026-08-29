using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using GlyphAdjustmentRecord = UnityEngine.TextCore.LowLevel.GlyphAdjustmentRecord;
using GlyphPairAdjustmentRecord = UnityEngine.TextCore.LowLevel.GlyphPairAdjustmentRecord;
using GlyphValueRecord = UnityEngine.TextCore.LowLevel.GlyphValueRecord;

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

        var glyphPairAdjustmentRecords = fontAsset.fontFeatureTable.glyphPairAdjustmentRecords;
        var lookupTable = fontAsset.characterLookupTable;

        int[] saras = new int[7];
        int[] wannayuks = new int[4];

        saras[0] = (int)lookupTable[GetUnicodeCharacter("ิ")].glyphIndex;
        saras[1] = (int)lookupTable[GetUnicodeCharacter("ี")].glyphIndex;
        saras[2] = (int)lookupTable[GetUnicodeCharacter("ึ")].glyphIndex;
        saras[3] = (int)lookupTable[GetUnicodeCharacter("ื")].glyphIndex;
        saras[4] = (int)lookupTable[GetUnicodeCharacter("ำ")].glyphIndex;
        saras[5] = (int)lookupTable[GetUnicodeCharacter("ั")].glyphIndex;
        saras[6] = (int)lookupTable[GetUnicodeCharacter("ํ")].glyphIndex;

        wannayuks[0] = (int)lookupTable[GetUnicodeCharacter("่")].glyphIndex;
        wannayuks[1] = (int)lookupTable[GetUnicodeCharacter("้")].glyphIndex;
        wannayuks[2] = (int)lookupTable[GetUnicodeCharacter("๊")].glyphIndex;
        wannayuks[3] = (int)lookupTable[GetUnicodeCharacter("๋")].glyphIndex;

        int recordAdd = 0;

        foreach (var sara in saras)
        {
            foreach (var wannayuk in wannayuks)
            {
                float xPlacement = sara == saras[4] || sara == saras[6] ? aumXPlacementAfter : 0;

                var saraPosition = new GlyphValueRecord(0, 0, 0, 0);
                var saraGlyph = new GlyphAdjustmentRecord((uint)sara, saraPosition);

                var wannayukPosition = new GlyphValueRecord(xPlacement, wannayukHeight, 0, 0);
                var wannayukGlyph = new GlyphAdjustmentRecord((uint)wannayuk, wannayukPosition);

                var saraThenWannayukGlyphPair = new GlyphPairAdjustmentRecord(saraGlyph, wannayukGlyph);

                if (sara == saras[4] || sara == saras[6])
                {
                    xPlacement = aumXPlacementBefore;
                    wannayukPosition = new GlyphValueRecord(xPlacement, wannayukHeight, 0, 0);
                    wannayukGlyph = new GlyphAdjustmentRecord((uint)wannayuk, wannayukPosition);
                }

                var wannayukThenSaraGlyphPair = new GlyphPairAdjustmentRecord(wannayukGlyph, saraGlyph);

                if (overrideAll)
                {
                    glyphPairAdjustmentRecords.RemoveAll(record =>
                        IsGlyphPairEqual(record, saraThenWannayukGlyphPair) ||
                        IsGlyphPairEqual(record, wannayukThenSaraGlyphPair));

                    glyphPairAdjustmentRecords.Add(saraThenWannayukGlyphPair);
                    glyphPairAdjustmentRecords.Add(wannayukThenSaraGlyphPair);
                    recordAdd += 2;
                }
                else
                {
                    if (!ContainsGlyphPair(glyphPairAdjustmentRecords,
                            saraThenWannayukGlyphPair.firstAdjustmentRecord.glyphIndex,
                            saraThenWannayukGlyphPair.secondAdjustmentRecord.glyphIndex))
                    {
                        glyphPairAdjustmentRecords.Add(saraThenWannayukGlyphPair);
                        recordAdd++;
                    }

                    if (!ContainsGlyphPair(glyphPairAdjustmentRecords,
                            wannayukThenSaraGlyphPair.firstAdjustmentRecord.glyphIndex,
                            wannayukThenSaraGlyphPair.secondAdjustmentRecord.glyphIndex))
                    {
                        glyphPairAdjustmentRecords.Add(wannayukThenSaraGlyphPair);
                        recordAdd++;
                    }
                }
            }
        }

        if (recordAdd > 0)
            ApplyChangesToFontAsset(fontAsset);

        Debug.Log("Adjust font : <color=#2bcaff>" + fontAsset.name + "</color>" +
                  " Height offset : <color=#d8ff2b>" + wannayukHeight + "</color>" +
                  " Number of adjustment add : <color=#5dfa41>" + recordAdd + "</color>");
    }

    static void _Clear(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null)
        {
            Debug.Log("No FontAsset selected");
            return;
        }

        fontAsset.fontFeatureTable.glyphPairAdjustmentRecords.Clear();
        ApplyChangesToFontAsset(fontAsset);
    }

    static void ApplyChangesToFontAsset(TMP_FontAsset fontAsset)
    {
        fontAsset.fontFeatureTable.SortGlyphPairAdjustmentRecords();
        fontAsset.ReadFontAssetDefinition();
        TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
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

    static bool IsGlyphPairEqual(GlyphPairAdjustmentRecord a, GlyphPairAdjustmentRecord b)
    {
        return a.firstAdjustmentRecord.glyphIndex == b.firstAdjustmentRecord.glyphIndex &&
               a.secondAdjustmentRecord.glyphIndex == b.secondAdjustmentRecord.glyphIndex;
    }

    static bool ContainsGlyphPair(List<GlyphPairAdjustmentRecord> records, uint firstGlyphIndex, uint secondGlyphIndex)
    {
        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            if (record.firstAdjustmentRecord.glyphIndex == firstGlyphIndex &&
                record.secondAdjustmentRecord.glyphIndex == secondGlyphIndex)
                return true;
        }

        return false;
    }
}
