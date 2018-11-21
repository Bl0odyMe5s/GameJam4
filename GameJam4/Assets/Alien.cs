using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Alien : NetworkBehaviour {
    public int alienDamage = 40;

    private const float MOVE_SPEED = 6;
    private const float ROTATE_SPEED = 3;
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
        PlayAttackAnimation();
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
            rb.velocity = Vector3.zero;
        }

    }

    private void AnimateAlien(bool walkingOrNot)
    {
       animator.SetBool("isWalking", walkingOrNot);
    }

    private void PlayAttackAnimation()
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

    public void DoAttack()
    {
        if (isLocalPlayer)
            CmdAttack();
    }

    [Command]
    public void CmdAttack()
    {
        Collider[] cols = Physics.OverlapBox(transform.position + transform.forward * 0.5f, Vector3.one);
        for (int i = 0; i < cols.Length; i++)
        {
            Transform theRoot = cols[i].transform.root;
            if (theRoot.CompareTag("Player"))
            {
                Debug.Log(cols[i].gameObject.name);
                PlayerHealth pHealth = theRoot.GetComponent<PlayerHealth>();
                pHealth.TakeDamage(alienDamage);
            }
        }
    }
}
