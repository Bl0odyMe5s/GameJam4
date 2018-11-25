
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
    public Animator outlineAnim;

    public AudioSource walkingSound, roarSound, hitSound, boneSound;

    private bool prevIsWalking, prevIsAttacking;
    public GameObject humanBloodFX;
    public GameObject humanBloodDecal;
    public float humanBloodDeviationAngle = 20;

    private bool mouseLocked;
    public float knockbackForce = 1f;

    private bool hasDoneDamage;

    // Use this for initialization
    void Start ()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (isLocalPlayer)
        {
            GameObject.FindGameObjectWithTag("AmmoText").SetActive(false);
            currentSpeed = moveSpeed;
            mouseLocked = true;
            CheckMouseLock();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (isLocalPlayer)
        {
            MoveAlien();
            CheckForAttack();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                mouseLocked = !mouseLocked;
                CheckMouseLock();
            }

            if (prevIsWalking != isWalking || prevIsAttacking != isAttacking)
                CmdSetAnimationServer(isWalking, isAttacking);
        }

        PlaySounds();
        AnimateAlien(isWalking, isAttacking);

        prevIsWalking = isWalking;
        prevIsAttacking = isAttacking;
    }

    private void CheckMouseLock()
    {
        if(mouseLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void MoveAlien()
    {
        rb.velocity = Vector3.zero;
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
            rb.velocity += transform.forward * currentSpeed;
            isWalking = true;
        }

        else if (Input.GetKey(KeyCode.S))
        {
            rb.velocity += -transform.forward * currentSpeed * 0.75f;
            isWalking = true;
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb.velocity += transform.right * currentSpeed / 2;
            isWalking = true;
        }

        else if (Input.GetKey(KeyCode.A))
        {
            rb.velocity += -transform.right * currentSpeed * 0.5f;
            isWalking = true;
        }

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, currentSpeed);

        if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1)
        {
            transform.Rotate(Vector3.up, ROTATE_SPEED * Time.deltaTime * Input.GetAxis("Mouse X"));
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
        if(outlineAnim.isActiveAndEnabled)
            outlineAnim.SetBool("isWalking", walkingOrNot);
    }

    private void CheckForAttack()
    {
        //When the alien presses space, start attack.
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isSneaking)
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
            hitSound.PlayOneShot(hitSound.clip);

        animator.SetBool("isAttacking", currentlyAttacking);
        if (outlineAnim.isActiveAndEnabled)
            outlineAnim.SetBool("isAttacking", currentlyAttacking);
    }

    private void PlaySounds()
    {
        if (isSneaking)
            return;

        soundTimer += Time.deltaTime;

        int randomDecision = Random.Range(0, 1);
        if (soundTimer >= SOUND_COOLDOWN && randomDecision == 0)
        {
            walkingSound.PlayOneShot(walkingSound.clip);
            soundTimer = 0;
        }

        else if (soundTimer >= SOUND_COOLDOWN && randomDecision == 1)
        {
            roarSound.PlayOneShot(roarSound.clip);
            soundTimer = 0;
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        hasDoneDamage = false;
        animator.SetBool("isAttacking", isAttacking);
        if (outlineAnim.isActiveAndEnabled)
            outlineAnim.SetBool("isAttacking", isAttacking);
    }

    public void DoAttack()
    {
        if (isLocalPlayer && !hasDoneDamage)
        {
            hasDoneDamage = true;
            LocalAttack();
        }
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
                int layer_mask = LayerMask.GetMask("Marines");
                Ray effectRay = new Ray(transform.position, (theRoot.position - transform.position).normalized);
                RaycastHit rayHit;

                if(Physics.Raycast(effectRay, out rayHit, Mathf.Infinity, layer_mask))
                {
                    PlayEnemyHitEffect(theRoot.gameObject, transform.position, rayHit.point, rayHit.normal);
                    CmdServerHitEffect(theRoot.gameObject, transform.position, rayHit.point, rayHit.normal);
                }

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

    [Command]
    private void CmdServerHitEffect(GameObject enemy, Vector3 origin, Vector3 hitPoint, Vector3 hitAngle)
    {
        RpcClientHitEffect(enemy, origin, hitPoint, hitAngle);
    }
    
    [ClientRpc]
    private void RpcClientHitEffect(GameObject enemy, Vector3 origin, Vector3 hitPoint, Vector3 hitAngle)
    {
        if (!isLocalPlayer)
            PlayEnemyHitEffect(enemy, origin, hitPoint, hitAngle);
    }

    private void PlayEnemyHitEffect(GameObject enemy, Vector3 origin, Vector3 hitPoint, Vector3 hitAngle)
    {
        // Instantiate human blood FX.
        Instantiate(humanBloodFX, hitPoint, Quaternion.LookRotation(hitAngle));

        float groundSpread = 0.5f;
        int randomKek = Random.Range(1, 4);

        for (int i = 0; i < randomKek; i++)
        {
            GameObject groundBlood = Instantiate(humanBloodDecal, new Vector3(hitPoint.x + Random.Range(-groundSpread, groundSpread), Random.Range(0, 1000) / 100000f, hitPoint.z + Random.Range(-groundSpread, groundSpread)), Quaternion.Euler(90, Random.Range(0, 360), 0));
            groundBlood.transform.localScale *= Random.Range(0.6f, 1.1f);
        }

        for (int i = 0; i < 6; i++)
        {
            RaycastHit hit;
            Quaternion deviation = Quaternion.AngleAxis(Random.Range(-humanBloodDeviationAngle, humanBloodDeviationAngle), Vector3.up);

            Vector3 randomDir = Random.onUnitSphere;
            if (Mathf.Abs(randomDir.y) == 1)
                continue;

            randomDir.y = 0;
            Vector3 bloodDir = deviation * randomDir.normalized;

            Ray bloodRay = new Ray(hitPoint, bloodDir);
            int layer_mask = LayerMask.GetMask("BloodRaycast");

            if (Physics.Raycast(bloodRay, out hit, 1.5f, layer_mask))
            {
                if (hit.collider.gameObject.CompareTag("Wall"))
                {
                    GameObject blood = Instantiate(humanBloodDecal, hit.point, Quaternion.LookRotation(-hit.normal));
                    blood.transform.position += hit.normal * (Random.Range(0, 1000) / 100000f);
                    blood.transform.Rotate(blood.transform.forward, Random.Range(0, 360), Space.World);
                    blood.transform.localScale *= Random.Range(0.6f, 1.1f);
                }
            }
        }

        boneSound.Play();

        if(enemy.GetComponent<Player>().isLocalPlayer)
        {
            Debug.Log("called");
            Vector3 dir = hitPoint - origin;
            enemy.GetComponent<KnockBack>().AddImpact(dir, knockbackForce);
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.DrawWireCube(transform.position + transform.forward * 0.8f + Vector3.up, Vector3.one * 1.4f + Vector3.up);
#endif
    }
}
