#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomStructure))]
public class CustomStructureInspector : Editor
{
	CustomStructureWindow window;
	bool defaultVisible;
	public override void OnInspectorGUI()
	{
		defaultVisible = EditorGUILayout.Foldout(defaultVisible, "Show Default");

		if (defaultVisible)
		{
			EditorGUI.indentLevel++;
			base.OnInspectorGUI();
			EditorGUI.indentLevel--;
		}

		else if (GUILayout.Button("Open Editor"))
		{
			window = CustomStructureWindow.Open((CustomStructure)target);
		}
	}
	private void OnSceneGUI()
	{
		if (window == null) return;
		Tools.current = 0;
		CustomStructure cs = ((CustomStructure)target);

		cs.targetBlock = Handles.PositionHandle(cs.targetBlock + Vector3.one * 0.5f, Quaternion.identity) - Vector3.one * 0.5f;
		cs.pivot = Handles.PositionHandle(cs.pivot + Vector3.one * 0.5f, Quaternion.identity) - Vector3.one * 0.5f;
		cs.FixValues();
	}
}
#endif