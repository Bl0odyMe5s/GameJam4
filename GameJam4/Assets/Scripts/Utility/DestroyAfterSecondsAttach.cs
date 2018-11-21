using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys the GameObject this script is attached to after a set delay.
/// </summary>
public class DestroyAfterSecondsAttach : MonoBehaviour
{
	public void StartDestroying(float destroyDelay)
	{
		StartCoroutine(Die(destroyDelay));
	}

	private IEnumerator Die(float destroyDelay)
	{
		yield return new WaitForSeconds(destroyDelay);
		Destroy(gameObject);
		yield break;
	}
}
