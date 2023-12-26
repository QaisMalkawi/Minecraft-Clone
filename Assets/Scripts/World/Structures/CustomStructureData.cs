using UnityEngine;

[System.Serializable]
public struct CustomStructureData
{
	public string name;
	public ChunkBlock[,,] data;
	public Vector3Int pivot;
	public bool forceAir;

	public CustomStructureData(string name, ChunkBlock[,,] data, Vector3Int pivot, bool forceAir)
	{
		this.name = name;
		this.data = data;
		this.pivot = pivot;
		this.forceAir = forceAir;
	}
}
