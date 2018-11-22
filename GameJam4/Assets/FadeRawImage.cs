using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeRawImage : MonoBehaviour {

    public float maxAlpha = 1;
    public float fadeSpeed;

    private float alphaFade;
    private RawImage hitmarkerUI;

    private Color startCol;

    private void Start()
    {
        hitmarkerUI = GetComponent<RawImage>();
        startCol = hitmarkerUI.color;
    }

    void Update () {
        

		if(alphaFade > 0)
        {
            alphaFade -= fadeSpeed * Time.deltaTime;
            startCol.a = alphaFade;
            hitmarkerUI.color = startCol;
        }
        else
        {
            alphaFade = 0;
            startCol.a = alphaFade;
            hitmarkerUI.color = startCol;
        }
	}

    public void RefreshAlpha()
    {
        alphaFade = maxAlpha;
    }
}
