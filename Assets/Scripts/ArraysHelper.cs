using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class ArraysHelper
{
	public static short[] Convert3DTo1D(short[,,] threeDArray)
	{
		int length = threeDArray.Length;
		short[] oneDArray = new short[length];

		int index = 0;
		foreach (var value in threeDArray)
		{
			oneDArray[index++] = value;
		}

		return oneDArray;
	}
	public static NativeArray<short> ConvertIntArrayToNativeArray(short[] shortArray)
	{
		NativeArray<short> nativeArray = new NativeArray<short>(shortArray.Length, Allocator.Temp);

		// Copy data from intArray to nativeArray
		nativeArray.CopyFrom(shortArray);

		return nativeArray;
	}
	public static NativeArray<short> ConvertShort3DArrayToNativeArray(short[,,] short3DArray)
	{
		int length = short3DArray.Length;
		NativeArray<short> shortNativeArray = new NativeArray<short>(length, Allocator.Persistent);

		// Flatten the 3D array into a 1D array for NativeArray
		for (int i = 0; i < short3DArray.GetLength(0); i++)
		{
			for (int j = 0; j < short3DArray.GetLength(1); j++)
			{
				for (int k = 0; k < short3DArray.GetLength(2); k++)
				{
					int index = i * short3DArray.GetLength(1) * short3DArray.GetLength(2) + j * short3DArray.GetLength(2) + k;
					shortNativeArray[index] = short3DArray[i, j, k];
				}
			}
		}

		return shortNativeArray;
	}
	public static short[,,] Convert1DTo3D(short[] oneDArray, int sizeX, int sizeY, int sizeZ)
	{
		short[,,] threeDArray = new short[sizeX, sizeY, sizeZ];

		for (int i = 0; i < oneDArray.Length; i++)
		{
			int z = i / (sizeX * sizeY);
			int remainder = i % (sizeX * sizeY);
			int y = remainder / sizeX;
			int x = remainder % sizeX;

			threeDArray[x, y, z] = oneDArray[i];
		}

		return threeDArray;
	}
	public static ChunkBlock[,,] Convert1DTo3D_CB(short[] blocks, byte[] lighting, int sizeX, int sizeY, int sizeZ)
	{
		ChunkBlock[,,] threeDArray = new ChunkBlock[sizeX, sizeY, sizeZ];

		Parallel.For(0, blocks.Length, (int i) => {
			int z = i / (sizeX * sizeY);
			int remainder = i % (sizeX * sizeY);
			int y = remainder / sizeX;
			int x = remainder % sizeX;

			threeDArray[x, y, z] = new ChunkBlock(blocks[i], lighting[i]);
		});

		return threeDArray;
	}
	public static ChunkBlock[,,] Convert1DTo3D_CB(short[] oneDArray, int sizeX, int sizeY, int sizeZ)
	{
		ChunkBlock[,,] threeDArray = new ChunkBlock[sizeX, sizeY, sizeZ];

		Parallel.For(0, oneDArray.Length, (int i) =>
		{
			int z = i / (sizeX * sizeY);
			int remainder = i % (sizeX * sizeY);
			int y = remainder / sizeX;
			int x = remainder % sizeX;

			threeDArray[x, y, z] = new ChunkBlock(oneDArray[i]);
		});

		return threeDArray;
	}

	public static Vector3[] ConvertFloat3ArrayToVector3Array(NativeArray<float3> float3Array)
	{
		int length = float3Array.Length;
		Vector3[] vector3Array = new Vector3[length];

		for (int i = 0; i < length; i++)
		{
			vector3Array[i] = new Vector3(float3Array[i].x, float3Array[i].y, float3Array[i].z);
		}

		return vector3Array;
	}
	public static Vector2[] ConvertFloat2ArrayToVector2Array(NativeArray<float2> float3Array)
	{
		int length = float3Array.Length;
		Vector2[] vector2Array = new Vector2[length];

		for (int i = 0; i < length; i++)
		{
			vector2Array[i] = new Vector2(float3Array[i].x, float3Array[i].y);
		}

		return vector2Array;
	}
}
