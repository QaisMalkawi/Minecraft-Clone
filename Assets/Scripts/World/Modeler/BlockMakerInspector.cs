#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BlocksMaker))]
public class BlockMakerInspector : Editor
{
	private void OnSceneGUI()
	{
		BlocksMaker bm = (BlocksMaker)target;
		if (bm.blocks.Length == 0) return;

		for (int i = 0; i < bm.verts.Count; i++)
		{
			bm.verts[i] = Handles.PositionHandle(bm.verts[i], Quaternion.identity);
		}
		bm.PreviewEdited();
	}
}
#endif
