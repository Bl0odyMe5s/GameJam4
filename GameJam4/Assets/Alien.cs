using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour {

    private const float MOVE_SPEED = 6;
    private const float ROTATE_SPEED = 1;
    private bool isWalking;
    private Rigidbody rb;
    private Animator animator;

    // Use this for initialization
    void Start ()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        MoveAlien();
        AnimateAlien(isWalking);
	}

    private void MoveAlien()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
        {
            isWalking = true;

            if (Input.GetKey(KeyCode.W))
            {
                rb.velocity = transform.forward * MOVE_SPEED;
            }

            else if (Input.GetKey(KeyCode.S))
            {
                rb.velocity = -transform.forward * MOVE_SPEED;
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.Rotate(Vector3.up, ROTATE_SPEED);
            }

            else if (Input.GetKey(KeyCode.A))
            {
                transform.Rotate(Vector3.up, -ROTATE_SPEED);
            }
        }

        else
        {
            isWalking = false;
        }

    }

    private void AnimateAlien(bool walkingOrNot)
    {
       animator.SetBool("isWalking", walkingOrNot);
    }
}
