using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Alien : NetworkBehaviour {

    private const float MOVE_SPEED = 6;
    private const float ROTATE_SPEED = 1;
    private bool isWalking, isAttacking;
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
        if (!isLocalPlayer)
            return;

        MoveAlien();
        AnimateAlien(isWalking);
        Attack();
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

    private void Attack()
    {
        //When the alien presses space, start attack.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isAttacking = true;
        }

        animator.SetBool("isAttacking", isAttacking);
    }

    public void EndAttack()
    {
        isAttacking = false;
    }
}
