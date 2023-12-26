#if UNITY_EDITOR
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CustomStructureWindow : EditorWindow
{
	public static CustomStructure cs;
	[MenuItem("Custom Editors/Structures")]
	public static CustomStructureWindow Open(CustomStructure customStructure)
	{
		CustomStructureWindow window = GetWindow<CustomStructureWindow>("Structures");
		cs = customStructure;
		return window;
	}

	Vector2 structuresscrolviewPos;
	Vector2 blocksscrolviewPos;
	private void OnGUI()
	{

		if (cs == null)
		{
			cs = FindObjectOfType<CustomStructure>();
			return;
		}

		if (cs.blocks == null)
		{
			cs.PrepareData();
			cs.Load();
			return;
		}
		EditorGUILayout.BeginHorizontal();

		EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.Width(150));
		blocksscrolviewPos = EditorGUILayout.BeginScrollView(blocksscrolviewPos);
		DrawBlockSelectionPanel();
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
		DrawMainPanel();
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true), GUILayout.MinWidth(100), GUILayout.MaxWidth(150));
		structuresscrolviewPos = EditorGUILayout.BeginScrollView(structuresscrolviewPos);
		DrawSidePanel();
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();

		EditorGUILayout.EndHorizontal();

	}
	void DrawBlockSelectionPanel()
	{
		for (short i = 0; i < cs.blockModels.Length; i++)
		{
			string[] currentBlockNames = cs.blockModels[i].BlockName.Split(".");
			bool ind = cs.model == i;
			if (ind)
			{
				EditorGUILayout.BeginVertical("box");
			}

			DrawBlockButton(i, currentBlockNames[currentBlockNames.Length - 1].FirstCharacterToUpper());

			if (ind)
			{
				EditorGUILayout.EndVertical();
			}

		}
	}
	void DrawBlockButton(short i, string bName)
	{
		if (GUILayout.Button(bName))
		{
			cs.model = i;
			cs.FixValues();
		}
	}
	void DrawSidePanel()
	{
		for (int i = 0; i < cs.structures.Count; i++)
		{
			if(GUILayout.Button(cs.structures[i].name))
			{
				cs.structure = i;
				cs.FixValues();
				cs.LoadStructure(i);
			}
		}
	}
	void DrawMainPanel()
	{
		cs.currentStructureName = EditorGUILayout.TextField("Structure Name:", cs.currentStructureName);

		DrawScaleField();
		DrawBlockSelector();
		DrawPivotTools();

		if (GUILayout.Button("Reset"))
		{
			cs.PrepareData();
		}
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Load"))
		{
			cs.Load();
		}
		if (GUILayout.Button("Save"))
		{
			cs.Save();
		}
		GUILayout.EndHorizontal();

	}

	bool scaleExpanded;
	void DrawScaleField()
	{
		scaleExpanded = EditorGUILayout.Foldout(scaleExpanded, "Scale:");
		if (scaleExpanded)
		{
			GUILayout.BeginVertical("box");

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Expand X"))
			{
				cs.ExpandSize(0);
			}
			if (GUILayout.Button("Expand Y"))
			{
				cs.ExpandSize(1);
			}
			if (GUILayout.Button("Expand Z"))
			{
				cs.ExpandSize(2);
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.Vector3IntField("", cs.structureScale);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Shrink X"))
			{
				cs.ShrinkSize(0);
			}
			if (GUILayout.Button("Shrink Y"))
			{
				cs.ShrinkSize(1);
			}
			if (GUILayout.Button("Shrink Z"))
			{
				cs.ShrinkSize(2);
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

		}
	}
	
	bool blockSelectorExpanded;
	void DrawBlockSelector()
	{
		blockSelectorExpanded = EditorGUILayout.Foldout(blockSelectorExpanded, $"Structure Editor ({cs.currentStructureName}):");
		if (blockSelectorExpanded)
		{
			EditorGUI.indentLevel++;
			GUILayout.BeginVertical("box");

			GUILayout.BeginHorizontal();
			string[] currentBlockNames = cs.blockModels[cs.model].BlockName.Split(".");
			if (GUILayout.Button("Previous Block"))
			{
				cs.model--;
				cs.FixValues();
			}
			if (GUILayout.Button($"Set Block ({currentBlockNames[currentBlockNames.Length - 1].FirstCharacterToUpper()})"))
			{
				cs.SetBlock();
			}
			if (GUILayout.Button("Next Block"))
			{
				cs.model++;
				cs.FixValues();
			}
			GUILayout.EndHorizontal();

			DrawBucketTools();
			DrawLineTools();

			GUILayout.EndVertical();
			EditorGUI.indentLevel--;

		}
	}
	
	bool lineToolsExpanded;
	void DrawLineTools()
	{
		lineToolsExpanded = EditorGUILayout.Foldout(lineToolsExpanded, "Lines:");
		if (lineToolsExpanded)
		{
			EditorGUI.indentLevel++;
			GUILayout.BeginVertical("box");

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Line x"))
			{
				cs.Line(0);
			}
			if (GUILayout.Button("Line y"))
			{
				cs.Line(1);
			}
			if (GUILayout.Button("Line z"))
			{
				cs.Line(2);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Force Line x"))
			{
				cs.Line(0, true);
			}
			if (GUILayout.Button("Force Line y"))
			{
				cs.Line(1, true);
			}
			if (GUILayout.Button("Force Line z"))
			{
				cs.Line(2, true);
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}
	}
	
	bool bucketToolsExpanded;
	void DrawBucketTools()
	{
		bucketToolsExpanded = EditorGUILayout.Foldout(bucketToolsExpanded, "Bucket:");
		if (bucketToolsExpanded)
		{
			EditorGUI.indentLevel++;
			GUILayout.BeginVertical("box");

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Bucket x"))
			{
				cs.Bucket(0);
			}
			if (GUILayout.Button("Bucket y"))
			{
				cs.Bucket(1);
			}
			if (GUILayout.Button("Bucket z"))
			{
				cs.Bucket(2);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Force Bucket x"))
			{
				cs.Bucket(0, true);
			}
			if (GUILayout.Button("Force Bucket y"))
			{
				cs.Bucket(1, true);
			}
			if (GUILayout.Button("Force Bucket z"))
			{
				cs.Bucket(2, true);
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			EditorGUI.indentLevel--;

		}
	}

	bool pivotControlsExpanded;
	void DrawPivotTools()
	{
		pivotControlsExpanded = EditorGUILayout.Foldout(pivotControlsExpanded, "Pivot:");
		if (pivotControlsExpanded)
		{
			EditorGUI.indentLevel++;
			GUILayout.BeginVertical("box");

			GUILayout.BeginHorizontal("box");
			if (GUILayout.Button("Pivot To Selection"))
			{
				cs.pivot = cs.targetBlock;
			}
			if (GUILayout.Button("Selection To Pivot"))
			{
				cs.targetBlock = cs.pivot;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal("box");
			if (GUILayout.Button("Pivot To Origin"))
			{
				cs.pivot = Vector3.zero;
			}
			if (GUILayout.Button("Selection To Origin"))
			{
				cs.targetBlock = Vector3.zero;
			}
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			EditorGUI.indentLevel--;

		}
	}
}
#endif