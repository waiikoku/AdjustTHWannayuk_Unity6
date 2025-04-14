# AdjustWannayuk for Unity 6+

## Description
AdjustWannayuk is a Unity Editor tool specifically designed for Thai font adjustments in TextMeshPro. This fork addresses compatibility issues with Unity 6, which introduced changes to the underlying text handling system.

The tool primarily aims to solve a common problem in Thai typography where tone marks (wannayuk) and vowels (sara) need precise positioning adjustments to display correctly. Without these adjustments, Thai text often appears with incorrectly positioned diacritical marks, leading to poor readability and unprofessional appearance.

**Note: This fork is specifically designed for Unity 6+ and is NOT compatible with earlier versions of Unity due to fundamental changes in TextCore's implementation.**

## Features
- Adjust the vertical height of tone marks (wannayuk) for better positioning
- Configure horizontal positioning of special vowels like สระอำ (sara am)
- Easily apply adjustments to any TextMeshPro font asset
- Option to override existing adjustments or only add missing ones
- One-click clear function to remove all adjustments

## Changes from Original
This is a fork of [Nolkeg/AdjustTHWannayuk](https://github.com/Nolkeg/AdjustTHWannayuk) that addresses critical compatibility issues with Unity 6. The main changes include:

- Updated to handle the type conversion between `TMPro.TMP_GlyphPairAdjustmentRecord` and `UnityEngine.TextCore.LowLevel.GlyphPairAdjustmentRecord`
- Added reflection-based approach to safely handle different TextCore implementations
- Improved error handling and diagnostics
- Added Unity 6 namespace requirements

## Installation

### Manual Installation
PackageManager > Add package from git URL... > https://github.com/waiikoku/AdjustTHWannayuk_Unity6.git

## Usage
1. Open the tool via `Window > TextMeshPro > AdjustWannayuk`
2. Drag and drop your TMP_FontAsset into the "FontAsset" field
3. Configure the height offset and x-offset values as needed
4. Click "Adjust" to apply the adjustments
5. Click "Clear" to remove all adjustments if needed

Alternatively, you can right-click on a font asset in the Project window and select "AdjustWannayuk" to quickly apply adjustments with the current settings.

## Compatibility Warning ⚠️
**This tool is ONLY compatible with Unity 6 and above.**

Due to significant changes in Unity's TextCore system, this version will NOT work with Unity 5.x or earlier. If you need adjustments for earlier Unity versions, please use the original repository instead.

The core issue addressed in this fork relates to Unity 6's change from `TMPro.TMP_GlyphPairAdjustmentRecord` to `UnityEngine.TextCore.LowLevel.GlyphPairAdjustmentRecord` as the underlying type for glyph pair adjustments.

## How It Works
The tool creates glyph pair adjustment records for combinations of Thai vowels and tone marks. These adjustments modify the positioning of characters when they appear together in text. 

For each combination of Thai vowel and tone mark, the tool:
1. Creates adjustments for when the vowel comes before the tone mark
2. Creates adjustments for when the tone mark comes before the vowel 
3. Makes special adjustments for specific vowels like สระอำ

All adjustments are stored in the font asset and will be applied automatically whenever text is rendered using that font.

## Contributing
Contributions to improve the tool are welcome. Please feel free to submit issues or pull requests.

## License
This project is licensed under the same terms as the original repository by Nolkeg.

## Acknowledgments
- Original tool by [Nolkeg](https://github.com/Nolkeg/AdjustTHWannayuk)
- Adapted for Unity 6+ by [Your Name]