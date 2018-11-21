using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys the GameObject this script is attached to after a set delay.
/// </summary>
public class DestroyAfterSeconds : MonoBehaviour
{
	private ParticleSystem _particleSystem;
	public float destroyDelay = 5f;

	private void Start()
	{
		StartCoroutine(Die());
	}

	private IEnumerator Die()
	{
		yield return new WaitForSeconds(destroyDelay);
		Destroy(gameObject);
		yield break;
	}
}
