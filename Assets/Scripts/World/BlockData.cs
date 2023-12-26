[System.Serializable]
public struct BlockData
{
	public string BlockName;
	public int BlockModel;
	public bool isSolid;
	public bool isTransparent;
	public byte lightStrength;
	public byte lightBlockage;
	public BlockBehaviour blockBehaviour;
	public InventorySlotContent ItemToGive;
	public int[] Textures;

	public bool isLightSource {
		get
		{
			return lightStrength > 0;
		}
	}

}
public enum DefaultBlocks
{
	AIR = 0,
	BEDROCK = 1,
	STONE = 2,
	DIRT = 3,
	GRASS = 4,
	DEEPSLATE = 5,
	SHORTGRASS = 11,
}
