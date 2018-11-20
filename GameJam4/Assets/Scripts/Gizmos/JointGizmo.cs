#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class JointGizmo : MonoBehaviour
{
	public float radius = 0.1f;
	public Vector3 offset = Vector3.zero;
	public Color color = Color.red;

	public void OnDrawGizmos()
	{
		Gizmos.color = color;
		Gizmos.DrawWireSphere(transform.position + offset, radius);
	}
}
#endif
