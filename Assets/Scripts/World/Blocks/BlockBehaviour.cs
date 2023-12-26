using UnityEngine;

public abstract class BlockBehaviour : ScriptableObject, IBlockBehaviour
{
	public static float t;

	[SerializeField] float breakTime;
	[SerializeField] float explosionResistance;
	[SerializeField] bool hasInteraction;
	public float BreakTime => breakTime;
	public float ExplosionResistance => explosionResistance;
	public bool HasInteraction => hasInteraction;


	public abstract bool Break();

	public abstract bool Interact();
}
