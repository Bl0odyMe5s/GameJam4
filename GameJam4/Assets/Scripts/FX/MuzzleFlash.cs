using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fades out the given light over the course of the set timer.
/// </summary>
public class MuzzleFlash : MonoBehaviour
{
    public Light myLight;
    private float fadeOutDelay = 1;
    private float startingIntensity;
    private float timeSinceSpawn = 0;

    private void Start()
    {
        startingIntensity = myLight.intensity;
    }

    public void InitializeLight(Color targetColor, float fadeOutDelay)
    {
        this.fadeOutDelay = fadeOutDelay;
        myLight.color = targetColor;
		var destroyScript = gameObject.AddComponent<DestroyAfterSecondsAttach>();
        destroyScript.StartDestroying(fadeOutDelay);
    }

    private void Update()
    {
        timeSinceSpawn += Time.deltaTime;
        myLight.intensity = Mathf.Lerp(startingIntensity, 0, Mathf.Clamp01(timeSinceSpawn / fadeOutDelay));
    }
}
