using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hitmarker : MonoBehaviour {

    public float fadeSpeed;

    private float alphaFade;
    private RawImage hitmarkerUI;

    private void Start()
    {
        hitmarkerUI = GetComponent<RawImage>();
    }

    void Update () {
		if(alphaFade >= 0)
        {
            alphaFade -= fadeSpeed * Time.deltaTime;
            hitmarkerUI.color = new Color(1, 1, 1, alphaFade);
        }
        else
        {
            alphaFade = 0;
            hitmarkerUI.color = new Color(1, 1, 1, alphaFade);
        }
	}

    public void RefreshHitmarker()
    {
        alphaFade = 1;
    }
}
