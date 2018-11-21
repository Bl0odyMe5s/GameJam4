using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ShowAlien : NetworkBehaviour {

    GameObject theAlien;
    Text tekst;

	void Start () {
        tekst = GetComponent<Text>();
        tekst.enabled = false;

        if (isServer)
        {
            theAlien = GameObject.FindGameObjectWithTag("Alien");
            StartCoroutine(TryShowAlienText());
        }
	}

    public IEnumerator TryShowAlienText()
    {
        while(theAlien == null)
        {
            theAlien = GameObject.FindGameObjectWithTag("Alien");
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        RpcShowAlien(theAlien.GetComponent<Player>().playerName);
    }

    [ClientRpc]
    public void RpcShowAlien(string theName)
    {
        StartCoroutine(ShowAlienText(theName));
    }

    public IEnumerator ShowAlienText(string theName)
    {
        tekst.enabled = true;
        tekst.text = theName + " is the\nalien!";

        yield return new WaitForSeconds(3);

        tekst.enabled = false;
    }
}
