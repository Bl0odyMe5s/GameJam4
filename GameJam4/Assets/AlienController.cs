using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AlienController : NetworkBehaviour {
    public GameObject theCamera;
    public GameObject alienObject;
    public GameObject alienLight;
    GameObject mainCamera;
    public float speed = 4;

	// Use this for initialization
	void Start () {
        if (!isLocalPlayer)
        {
            theCamera.SetActive(false);
            alienLight.SetActive(false);
        }
        else
            Camera.main.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer)
            return;

        if(Input.GetKey(KeyCode.A))
        {
            alienObject.transform.Translate(-speed * Time.deltaTime, 0, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            alienObject.transform.Translate(speed * Time.deltaTime, 0, 0);
        }

        if (Input.GetKey(KeyCode.W))
        {
            alienObject.transform.Translate(0, 0, speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            alienObject.transform.Translate(0, 0, -speed * Time.deltaTime);
        }
    }
}
