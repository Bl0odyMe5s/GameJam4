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
	[Tooltip("Angle in degrees.")]
    public float maxDeviationAngle = 1;

	public float muzzleFlashDuration = 0.05f;
    public float maxMuzzleFlashOffset = 0.15f;

    public float fireLineWidth = 0.05f;
    public float fireLineFadeTime = 0.05f;

    public Transform weaponNozzle;
    public Transform shootingRaycastPosition;

	public Color shotColor = Color.yellow;

	public AudioClip[] shootingSounds;
	public AudioClip reloadingSound;

    public GameObject humanBloodFX;
    public GameObject alienBloodFX;
    public GameObject wallImpactFX;

    public GameObject muzzleFlashFX;
	public GameObject fireLineFX;

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
                    // Generate random deviation rotation.
                    Quaternion deviation = Quaternion.AngleAxis(Random.Range(-maxDeviationAngle, maxDeviationAngle), Vector3.up);
                    deviation *= Quaternion.AngleAxis(Random.Range(-maxDeviationAngle, maxDeviationAngle), Vector3.right);

                    // Shoot a single projectile.
					CmdShootOnServer(shootingRaycastPosition.position, deviation * shootingRaycastPosition.forward, damage, weaponNozzle.position, weaponNozzle.forward, weaponNozzle.position + Random.insideUnitSphere * maxMuzzleFlashOffset);
                    currentBulletAmount--;
				}
				else
				{
					shooting = false;
					yield break;
				}

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
    private void CmdShootOnServer(Vector3 position, Vector3 direction, int projectileDamage, Vector3 nozzlePos, Vector3 nozzleForward, Vector3 muzzleFlashPosition)
    {
		RaycastHit hit;
        shootingRay.origin = position;
		shootingRay.direction = direction;

#if UNITY_EDITOR
        Vector3 vectorTowardsHit;
#endif

		if (Physics.Raycast(shootingRay, out hit, maxRange))
		{
			// Hit something.
			// Check what we hit.
			if (hit.transform.root.CompareTag("Player"))
			{
				var healthScript = hit.transform.root.GetComponent<PlayerHealth>();
                healthScript.TakeDamage(projectileDamage);

				// Play shot and human body impact FX.
				RpcShot(HitType.Human, hit.point, nozzlePos, muzzleFlashPosition);
			}
			else if (hit.transform.root.CompareTag("Alien"))
            {
                var healthScript = hit.transform.root.GetComponent<PlayerHealth>();
                healthScript.TakeDamage(projectileDamage);

                // Play shot and alien body impact FX.
                RpcShot(HitType.Alien, hit.point, nozzlePos, muzzleFlashPosition);
            }
			else
			{
                // Play shot and object impact FX.
                RpcShot(HitType.Wall, hit.point, nozzlePos, muzzleFlashPosition);
			}

#if UNITY_EDITOR
            vectorTowardsHit = hit.point - shootingRaycastPosition.position;
#endif
        }
		else
		{
            // Didn't hit anything.
            // Play shot FX.
            RpcShot(HitType.None, nozzlePos + nozzleForward * maxRange, nozzlePos, muzzleFlashPosition);
			
#if UNITY_EDITOR
			vectorTowardsHit = shootingRaycastPosition.position + shootingRaycastPosition.forward * maxRange;
#endif
		}

#if UNITY_EDITOR
        Debug.DrawRay(shootingRaycastPosition.position, vectorTowardsHit, shotColor, 0.25f, true);
#endif
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
	private void RpcShot(HitType hitType, Vector3 hitPoint, Vector3 nozzlePosition, Vector3 muzzleFlashPosition)
	{
        MyAudioSource.PlayOneShot(shootingSounds[Random.Range(0, shootingSounds.Length)]);
        var muzzleFlashObject = Instantiate(muzzleFlashFX, muzzleFlashPosition, Quaternion.identity);
        muzzleFlashObject.GetComponent<MuzzleFlash>().InitializeLight(shotColor, muzzleFlashDuration);

		if (hitType == HitType.None)
		{
			// Hit nothing.
		}
		else
		{
            // Hit something.
            var fireLine = Instantiate(fireLineFX, nozzlePosition, Quaternion.identity);
            fireLine.GetComponent<FireLine>().InitializeLine(shotColor, nozzlePosition, hitPoint, fireLineWidth, fireLineFadeTime);
		}

		switch (hitType)
        {
            case HitType.Human:
                // Instantiate human blood FX.
				// Instantiate(humanBloodFX, hitPoint, Quaternion.identity);
                break;
            case HitType.Alien:
                // Instantiate alien blood FX.
                // Instantiate(alienBloodFX, hitPoint, Quaternion.identity);
                break;
            case HitType.Wall:
                // Instantiate wall impact FX.
                // Instantiate(wallImpactFX, hitPoint, Quaternion.identity);
                break;
            case HitType.None:
				// Hit nothing, so no FX.
                break;
		}
	}
}
