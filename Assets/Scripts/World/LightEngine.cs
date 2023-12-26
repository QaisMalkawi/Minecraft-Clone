using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class LightEngine
{

	static Queue<Vector3Int> pointsToPropegate = new Queue<Vector3Int>();

	static Vector2Int chunkSize;

	public static bool isBusy;
	public static ChunkBlock[,,] CalculateLights(ChunkBlock[,,] blocks)
	{
		isBusy = true;
		chunkSize = new Vector2Int(blocks.GetLength(0), blocks.GetLength(1));
		blocks = ShineRay(blocks);
		if(pointsToPropegate.Count > 0)
		{
			Debug.Log($"there are {pointsToPropegate.Count} points to be propegated");
			blocks = PropegateLighting(blocks);
		}

		isBusy = false;
		return blocks;
	}

	static ChunkBlock[,,] ShineRay(ChunkBlock[,,] blocks)
	{
		for (int x = 0; x < chunkSize.x; x++)
		{
			for (int z = 0; z < chunkSize.x; z++)
			{
				bool blocked = false;
				for (int y = chunkSize.y - 1; y >= 0; y--)
				{
					BlockData bd = WorldManager.Instance.blocks[blocks[x, y, z].BlockID];
					if (bd.isSolid && !blocked)
					{
						blocked = true;
						blocks[x, y, z].LightLevel = 0;
					}
					else if (blocked)
					{
						blocks[x, y, z].LightLevel = 0;
						continue;
					}
					else
					{
						blocks[x, y, z].LightLevel = WorldTable.LightLevels;
					}

					if (bd.isLightSource)
					{
						blocks[x, y, z].LightLevel = bd.lightStrength;
					}
					pointsToPropegate.Enqueue(new Vector3Int(x, y, z));

				}
			}
		}
		return blocks;
	}
	
	static ChunkBlock[,,] PropegateLighting(ChunkBlock[,,] blocks)
	{
		while(pointsToPropegate.Count > 0)
		{
			Vector3Int position = pointsToPropegate.Dequeue();
			ChunkBlock block = blocks[position.x, position.y, position.z];
			if (block.LightLevel > 1)
			{
				for (int i = 0; i < WorldTable.LightDirections.Length; i++)
				{
					Vector3Int sidePos = position + WorldTable.LightDirections[i];
					bool isOut = sidePos.x >= chunkSize.x || sidePos.x < 0 ||
								 sidePos.z >= chunkSize.x || sidePos.z < 0||
								 sidePos.y >= chunkSize.y || sidePos.y < 0;


					if (isOut) continue;

					ChunkBlock sideBlock = blocks[sidePos.x, sidePos.y, sidePos.z];
					BlockData bd = WorldManager.Instance.blocks[sideBlock.BlockID];
					if (bd.isSolid && !bd.isTransparent)
					{
						continue;
					}
					if (sideBlock.LightLevel < block.LightLevel)
					{
						int newLightLevel = (block.LightLevel - (bd.lightBlockage + 1));
						newLightLevel = math.clamp(newLightLevel, 0, 255);
						blocks[sidePos.x, sidePos.y, sidePos.z].LightLevel = (byte)newLightLevel;
						if(newLightLevel > 1)
							pointsToPropegate.Enqueue(sidePos);
					}

				}
			}

		}
		return blocks;
	}
}