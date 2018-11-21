using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Alien : NetworkBehaviour {
    public int alienDamage = 40;

    private const float MOVE_SPEED = 6;
    private const float ROTATE_SPEED = 3;
    private const float SOUND_COOLDOWN = 12;
    private bool isWalking, isAttacking;
    private float soundTimer, timer;
    private Rigidbody rb;
    private Animator animator;

    public AudioSource walkingSound, roarSound, hitSound;

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
        PlaySounds();
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
            hitSound.PlayOneShot(hitSound.clip, Random.Range(0.5f, 1));
        }

        animator.SetBool("isAttacking", isAttacking);
    }

    private void PlaySounds()
    {
        soundTimer += Time.deltaTime;

        int randomDecision = Random.Range(0, 1);
        if (soundTimer >= SOUND_COOLDOWN && randomDecision == 0)
        {
            walkingSound.PlayOneShot(walkingSound.clip, Random.Range(0.5f, 1));
            soundTimer = 0;
        }

        else if (soundTimer >= SOUND_COOLDOWN && randomDecision == 1)
        {
            roarSound.PlayOneShot(roarSound.clip, Random.Range(0.5f, 1));
            soundTimer = 0;
        }
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
