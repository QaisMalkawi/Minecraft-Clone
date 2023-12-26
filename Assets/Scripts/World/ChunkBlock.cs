using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkBlock
{
	public short BlockID;
	public byte LightLevel;

	public ChunkBlock(short id)
	{
		BlockID = id;
		LightLevel = 0;
	}
	public ChunkBlock(short id, byte lightLevel)
	{
		BlockID = id;
		LightLevel = lightLevel;
	}
}
