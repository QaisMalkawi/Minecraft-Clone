using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBlockBehaviour
{
	float BreakTime { get; }
	float ExplosionResistance { get; }
	bool HasInteraction { get; }

	public bool Interact();
	public bool Break();
}
