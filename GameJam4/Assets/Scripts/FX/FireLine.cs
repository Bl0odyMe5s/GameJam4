using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FireLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
	private float timeSinceSpawn = 0;

	public void InitializeLine(Color color, Vector3 start, Vector3 end, float width, float fadeTime)
	{
		var destroyScript = gameObject.AddComponent<DestroyAfterSecondsAttach>();
		destroyScript.StartDestroying(fadeTime);

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

		lineRenderer.SetPositions(new Vector3[] { start, end });
    }
}
