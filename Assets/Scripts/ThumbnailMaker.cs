#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class ThumbnailMaker : MonoBehaviour
{
	[SerializeField] RenderTexture texture;
	[SerializeField] string[] path;

	[ContextMenu("Take Screenshot")]
	public void Screenshot()
	{
		Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
		// ReadPixels looks at the active RenderTexture.
		RenderTexture.active = texture;
		tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
		tex.Apply();
		byte[] bytes = tex.EncodeToPNG();

		File.WriteAllBytes(Path.Combine(Application.dataPath,Path.Combine(path))+".png", bytes);
		AssetDatabase.Refresh();
	}
}
#endif