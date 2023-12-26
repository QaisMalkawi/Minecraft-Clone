using UnityEngine;

[System.Serializable]
public struct BlockModel
{
	public Vector3[,] Faces;
	public Vector2[,] UVS;
	public int[,] triangleTable;

}