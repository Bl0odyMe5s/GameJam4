using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreMeFloor : MonoBehaviour {


	void Start () {
        Physics.IgnoreCollision(GetComponent<Collider>(), GameObject.FindGameObjectWithTag("Floor").GetComponent<Collider>());
	}

}
