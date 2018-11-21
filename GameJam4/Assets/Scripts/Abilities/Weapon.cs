using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class Weapon : NetworkBehaviour
{
    public KeyCode fireButton = KeyCode.Mouse0;
    public KeyCode reloadButton = KeyCode.R;
	public bool automaticFire = false;
    public int numberOfShotsPerVolley = 1;
	public int numberOfVolleys = 1;

    public float delayBetweenShots = 1.0f;
    public float delayBetweenVolleys = 1.0f;

	public float reloadTime = 3.0f;
    public int bulletsPerMagazine = 60;
	private int currentBulletAmount = 0;

	public float maxRange = 1000;

	public int damage = 1;

	public Transform weaponNozzle;

	public Color shotColor = Color.yellow;

	public AudioClip[] shootingSounds;
	public AudioClip reloadingSound;

    public GameObject humanBloodFX;
    public GameObject alienBloodFX;
    public GameObject wallImpactFX;

    public GameObject muzzleFlashFX;

	public enum HitType { Human, Alien, Wall, None };

	private bool shooting = false;
	private bool reloading = false;

    private Ray shootingRay = new Ray();

	private AudioSource myAudioSource = null;
	private AudioSource MyAudioSource
	{
		get
		{
			if (myAudioSource == null)
			{
                myAudioSource = GetComponent<AudioSource>();
			}
			return myAudioSource;
		}
	}

	public void Start()
	{
        currentBulletAmount = bulletsPerMagazine;
	}

	public void Update()
	{
		if (!isLocalPlayer)
			return;
		
		if ((Input.GetKey(fireButton) && automaticFire) || Input.GetKeyDown(fireButton))
		{
			if (!shooting && !reloading)
            {
				if (currentBulletAmount > 0)
                {
                    Shoot();
				}
				else
				{
					Reload();
				}
			}
		}

		if (Input.GetKeyDown(reloadButton) && currentBulletAmount < bulletsPerMagazine)
		{
            Reload();
		}
    }

    public void Reload()
    {
        StartCoroutine(ReloadRoutine());
        reloading = true;
    }

    private IEnumerator ReloadRoutine()
    {
        CmdReloadOnServer();
		yield return new WaitForSeconds(reloadTime);
		currentBulletAmount = bulletsPerMagazine;
        reloading = false;
        yield break;
    }

	public void Shoot()
	{
		StartCoroutine(ShootRoutine());
		shooting = true;
    }

    private IEnumerator ShootRoutine()
    {
        for (int volley = numberOfVolleys; volley > 0; volley--)
        {
            for (int shot = numberOfShotsPerVolley; shot > 0; shot--)
            {
				if (currentBulletAmount > 0)
                {
                    // Shoot a single projectile.
					CmdShootOnServer(weaponNozzle.position, weaponNozzle.forward);
                    currentBulletAmount--;
				}
				else
				{
					shooting = false;
					yield break;
				}

#if UNITY_EDITOR
                shootingRay.origin = weaponNozzle.position;
				shootingRay.direction = weaponNozzle.forward;
				RaycastHit hit;
				Vector3 vectorTowardsHit;
				if (Physics.Raycast(shootingRay, out hit, maxRange))
				{
					vectorTowardsHit = hit.point - weaponNozzle.position;
				}
				else
				{
                    vectorTowardsHit = weaponNozzle.position + weaponNozzle.forward * maxRange;
				}
                Debug.DrawRay(weaponNozzle.position, vectorTowardsHit, shotColor, 0.25f, true);
#endif

                yield return new WaitForSeconds(delayBetweenShots);
            }

            // Skip delay on final volley.
            if (volley != 1)
            {
                yield return new WaitForSeconds(delayBetweenVolleys);
            }
        }

        shooting = false;
        yield break;
    }

    // Network functions.
	[Command]
    private void CmdShootOnServer(Vector3 position, Vector3 direction)
    {
		RaycastHit hit;
        shootingRay.origin = position;
		shootingRay.direction = direction;
		if (Physics.Raycast(shootingRay, out hit, maxRange))
		{
			// Hit something.
			// Check what we hit.
			if (hit.transform.root.CompareTag("Player"))
			{
				// hit.transform.GetComponent<PlayerHealth>();
				// Reduce health.

				// Play shot and human body impact FX.
				RpcShot(HitType.Human);
			}
			else if (hit.transform.root.CompareTag("Alien"))
			{
                // hit.transform.GetComponent<PlayerHealth>();
                // Reduce health.

                // Play shot and alien body impact FX.
                RpcShot(HitType.Alien);
            }
			else
			{
                // Play shot and object impact FX.
                RpcShot(HitType.Wall);
			}
		}
		else
		{
            // Didn't hit anything.
            // Play shot FX.
            RpcShot(HitType.None);
		}
    }

	[Command]
	private void CmdReloadOnServer()
	{
        RpcReload();
	}

	[ClientRpc]
	private void RpcReload()
    {
        MyAudioSource.PlayOneShot(reloadingSound);
		// Play reloading animation.
	}

	[ClientRpc]
	private void RpcShot(HitType hitType)
	{
        MyAudioSource.PlayOneShot(shootingSounds[Random.Range(0, shootingSounds.Length - 1)]);
		// Instantiate muzzle flash.

		switch (hitType)
        {
            case HitType.Human:
                // Instantiate human blood FX.
                break;
            case HitType.Alien:
                // Instantiate alien blood FX.
                break;
            case HitType.Wall:
                // Instantiate wall impact FX.
                break;
            case HitType.None:
				// Hot nothing, so no FX.
                break;
		}
	}
}
