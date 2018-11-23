using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Alien : NetworkBehaviour {
    public int alienDamage = 40;

    public float moveSpeed = 8;
    public float sneakSpeed = 4;
    private float currentSpeed;
    private const float ROTATE_SPEED = 180;
    private const float SOUND_COOLDOWN = 12;

    public Material alienMat;
    public float sneakAlpha = 0.3f;

    [SyncVar]
    private bool isWalking;

    [SyncVar(hook ="OnAttackChange")]
    private bool isAttacking;

    [SyncVar(hook = "OnSneakChange")]
    private bool isSneaking;

    private float soundTimer, timer;
    private Rigidbody rb;
    private Animator animator;

    public AudioSource walkingSound, roarSound, hitSound;

    private bool prevIsWalking, prevIsAttacking;

    // Use this for initialization
    void Start ()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (isLocalPlayer)
        {
            GameObject.FindGameObjectWithTag("AmmoText").SetActive(false);
            currentSpeed = moveSpeed;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (isLocalPlayer)
        {
            MoveAlien();
            CheckForAttack();

            if (prevIsWalking != isWalking || prevIsAttacking != isAttacking)
                CmdSetAnimationServer(isWalking, isAttacking);
        }

        PlaySounds();
        AnimateAlien(isWalking, isAttacking);

        prevIsWalking = isWalking;
        prevIsAttacking = isAttacking;
    }

    private void MoveAlien()
    {
        isWalking = false;

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentSpeed = sneakSpeed;
            isSneaking = true;
            CmdSetSneaking(true);
            SetSneakAlpha(isSneaking);
        }
        
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            currentSpeed = moveSpeed;
            isSneaking = false;
            CmdSetSneaking(false);
            SetSneakAlpha(isSneaking);
        }

        if (Input.GetKey(KeyCode.W))
        {
            rb.velocity = transform.forward * currentSpeed;
            isWalking = true;
        }

        else if (Input.GetKey(KeyCode.S))
        {
            rb.velocity = -transform.forward * currentSpeed;
            isWalking = true;
        }
        else
        {
            rb.velocity = Vector3.zero;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up, ROTATE_SPEED * Time.deltaTime);
            isWalking = true;
        }

        else if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.up, -ROTATE_SPEED * Time.deltaTime);
            isWalking = true;
        }
    }

    [Command]
    private void CmdSetSneaking(bool sneak)
    {
        isSneaking = sneak;
    }

    private void OnSneakChange(bool currentlySneaking)
    {
        if (!isLocalPlayer)
        {
            isSneaking = currentlySneaking;
            SetSneakAlpha(currentlySneaking);
        }
    }

    private void SetSneakAlpha(bool currentlySneaking)
    {
        if (currentlySneaking)
        {
            alienMat.SetColor("_Color", new Color(1, 1, 1, sneakAlpha));
        }
        else
        {
            alienMat.SetColor("_Color", new Color(1, 1, 1, 1));
        }
    }

    [Command]
    private void CmdSetAnimationServer(bool walkingOrNot, bool currentlyAttacking)
    {
        isWalking = walkingOrNot;
        isAttacking = currentlyAttacking;
    }

    private void AnimateAlien(bool walkingOrNot, bool currentlyAttacking)
    {
        animator.SetBool("isWalking", walkingOrNot);
    }

    private void CheckForAttack()
    {
        //When the alien presses space, start attack.
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking && !isSneaking)
        {
            isAttacking = true;
            AttackEffect(isAttacking);
        }
    }

    private void OnAttackChange(bool currentlyAttacking)
    {
        if (isLocalPlayer)
        {
            return;
        }

        AttackEffect(currentlyAttacking);
    }

    private void AttackEffect(bool currentlyAttacking)
    {
        if (currentlyAttacking && !prevIsAttacking)
            hitSound.PlayOneShot(hitSound.clip, Random.Range(0.5f, 1));

        animator.SetBool("isAttacking", currentlyAttacking);
    }

    private void PlaySounds()
    {
        if (isSneaking)
            return;

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
        animator.SetBool("isAttacking", isAttacking);
    }

    public void DoAttack()
    {
        if (isLocalPlayer)
            LocalAttack();
    }

    public void LocalAttack()
    {
        Vector3 centerPos = transform.position + transform.forward * 0.8f + Vector3.up;
        Vector3 extends = Vector3.one * 1.4f + Vector3.up;

        Collider[] cols = Physics.OverlapBox(centerPos, extends);

        for (int i = 0; i < cols.Length; i++)
        {
            Transform theRoot = cols[i].transform.root;
            if (theRoot.CompareTag("Player"))
            {
                PlayerHealth pHealth = theRoot.GetComponent<PlayerHealth>();
                CmdHitEnemy(pHealth.gameObject);
            }
        }
    }

    [Command]
    private void CmdHitEnemy(GameObject enemy)
    {
        enemy.GetComponent<PlayerHealth>().TakeDamage(alienDamage);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.DrawWireCube(transform.position + transform.forward * 0.8f + Vector3.up, Vector3.one * 1.4f + Vector3.up);
#endif
    }
}
