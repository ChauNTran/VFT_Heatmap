using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HigherLevel.Brewer {
	[InitializeOnLoad]
public class BrewerPaletteWindow : EditorWindow {
	[Range(3, 9)]
	public int numberOfColours;
	public Palette SelectedPalette;

	private Nature selectedNature;
	private ColourBlindSupport selectedColourBlindness;
	private BrewerData PaletteData;

	private Vector2 swatchSize;
	private RectOffset padding;
	private string[] colourBlindLanguage;

	void OnEnable()
	{
		swatchSize = new Vector2(26f, 22f);
		padding = new RectOffset(4, 4, 4, 4);
		colourBlindLanguage = new [] {"Accommodating", "Possibly confusing", "Definitely confusing"};
	}

	void OnGUI()
	{
		// Calculate some flags to determine what kind of layout to use
		var width = EditorGUIUtility.currentViewWidth;
		var wideMode = width > 400f;
		var thinMode = !wideMode && width < 250f;

		DrawSettingsControls(wideMode, thinMode);

		if(PaletteData == null) LoadData();

		DrawPaletteControls(wideMode, thinMode);

		if (GUILayout.Button ("Reload data")) LoadData ();
	}

	void DrawSettingsControls(bool wideMode, bool thinMode)
	{
		EditorGUILayout.BeginVertical(EditorStyles.numberField);

		EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

		if (wideMode) EditorGUILayout.BeginHorizontal();
		else EditorGUILayout.BeginVertical();
		
		selectedNature = GetPaletteNatureFromControl(selectedNature, thinMode);
		numberOfColours = GetNumberOfColorsFromControl(numberOfColours, thinMode);

		if (wideMode) EditorGUILayout.EndHorizontal();
		else EditorGUILayout.EndVertical();

		selectedColourBlindness = GetColourBlindnessSupportFromControl(selectedColourBlindness, thinMode);
		
		EditorGUILayout.HelpBox("Colour blindness categories are as follows:\n\n"
			+ "Accommodating - Colour blind users did not report difficulty distinguishing colours\n\n"
			+ "Possibly Confusing - Difficulty distinguishing some colours for some types of colour blindness\n\n"
			+ "Definitely Confusing - Consistently difficult for colour blind users to distinguish colours", MessageType.Info);

		EditorGUILayout.EndVertical();
	}

	void LoadData()
	{
		var path = Application.dataPath + "/HigherLevel/Brewer/BrewerData.json";
		var dataAsJson = File.ReadAllText(path);
		PaletteData = JsonUtility.FromJson<BrewerData>(dataAsJson);
	}

	Nature GetPaletteNatureFromControl(Nature current, bool thinMode)
	{
		if (thinMode) EditorGUILayout.BeginVertical();
		else EditorGUILayout.BeginHorizontal();
		
		EditorGUIUtility.labelWidth = 90;
		EditorGUILayout.PrefixLabel("Palette Type:");
		var selected = (Nature)EditorGUILayout.EnumPopup("", current, GUILayout.MaxWidth(80));
		
		if (thinMode) EditorGUILayout.EndVertical();
		else EditorGUILayout.EndHorizontal();

		return selected;
	}

	int GetNumberOfColorsFromControl(int current, bool thinMode)
	{
		EditorGUIUtility.labelWidth = 120;

		if (thinMode) EditorGUILayout.BeginVertical();
		else EditorGUILayout.BeginHorizontal();
		
		EditorGUILayout.PrefixLabel("Number of Colours:");
		var selected = EditorGUILayout.IntSlider("", current, 3, 9, GUILayout.MaxWidth(120));
		
		if (thinMode) EditorGUILayout.EndVertical();
		else EditorGUILayout.EndHorizontal();

		return selected;
	}

	ColourBlindSupport GetColourBlindnessSupportFromControl(ColourBlindSupport current, bool thinMode)
	{
		if (thinMode) EditorGUILayout.BeginVertical();
		else EditorGUILayout.BeginHorizontal();
		
		EditorGUIUtility.labelWidth = 120;
		EditorGUILayout.PrefixLabel("Colour Blindness:");
		var selected = (ColourBlindSupport)EditorGUILayout.EnumPopup("", current, GUILayout.MaxWidth(120));
		
		if (thinMode) EditorGUILayout.EndVertical();
		else EditorGUILayout.EndHorizontal();

		return selected;
	}

	void GetPaletteFromControl(Nature nature, int colours, ColourBlindSupport cbSupport, bool thinMode)
	{
		EditorGUILayout.BeginVertical(GUILayout.Width((colours + 1) * swatchSize.x));

		var schemes = PaletteData.GetSchemes(nature, colours, cbSupport);

		if (schemes.Length > 0) {
			foreach(var s in schemes)
			{
				DrawPaletteSelector(s);
			}
		} else {
			EditorGUILayout.HelpBox("There are no palettes available for the chosen settings. Try a smaller number of colours, a less restrictive colour blindness setting, or a different palette type.", MessageType.None);
		}

		EditorGUILayout.EndVertical();
	}

	void DrawPaletteControls(bool wideMode, bool thinMode)
	{
		EditorGUILayout.BeginVertical(EditorStyles.numberField);

		EditorGUILayout.LabelField("Palettes", EditorStyles.boldLabel);

		if (wideMode) EditorGUILayout.BeginHorizontal();
		else EditorGUILayout.BeginVertical();

		GetPaletteFromControl(selectedNature, numberOfColours, selectedColourBlindness, thinMode);
		DisplaySelectedPalette(SelectedPalette);

		if (wideMode) EditorGUILayout.EndHorizontal();
		else EditorGUILayout.EndVertical();

		EditorGUILayout.EndVertical();
	}

	void DrawPaletteSelector(Palette palette)
	{
		var mainRect = EditorGUILayout.GetControlRect(
			GUILayout.Width(swatchSize.x * palette.Colours.Length + padding.horizontal),
			GUILayout.Height(swatchSize.y + padding.vertical));
		var swatchRect = new Rect(mainRect.position + padding.top * Vector2.up + padding.left * Vector2.right, swatchSize);

		var isSelected = SelectedPalette == palette;

		if (GUI.Toggle(mainRect, isSelected, "", GUI.skin.button)) {
			SelectedPalette = palette;
		}

		for (int i = 0; i < palette.Colours.Length; i++)
		{
			EditorGUI.DrawRect(swatchRect, palette.Colours[i]);
			swatchRect.position += swatchSize.x * Vector2.right;
		}
	}

	void DisplaySelectedPalette(Palette palette) {
		if (SelectedPalette == null) {
			EditorGUILayout.HelpBox("Click on a colour scheme!", MessageType.Info);
			return;
		};

		EditorGUILayout.BeginVertical();
		EditorGUILayout.LabelField("Colour scheme:", palette.Scheme, EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Colour blindness:", colourBlindLanguage[palette.ColourBlindSafe], EditorStyles.boldLabel);
		
		for (int i = 0; i < palette.Colours.Length; i++)
		{
			var colour = palette.Colours[i];
			var rect = EditorGUILayout.GetControlRect(GUILayout.Width(swatchSize.x * 8), GUILayout.Height(swatchSize.y * 2));
			EditorGUI.ColorField(rect, new GUIContent(), colour, false, false, false, null);
		}

		var helpRect = EditorGUILayout.GetControlRect(GUILayout.Width(swatchSize.x * 8), GUILayout.Height(60));
		EditorGUI.HelpBox(helpRect, "Left-click a colour for more info; right-click a colour to copy it", MessageType.Info);

		EditorGUILayout.EndVertical();
	}

	[MenuItem("Window/Brewer Palettes")]
	static void ShowWindow ()
	{
		EditorWindow.GetWindow(typeof(BrewerPaletteWindow), false, "Brewer Palettes");
	}
}

}