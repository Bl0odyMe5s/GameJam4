using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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

    public float bloodDeviationAngle = 10;

	public float muzzleFlashDuration = 0.05f;
    public float maxMuzzleFlashOffset = 0.15f;

    public float fireLineWidth = 0.05f;
    public float fireLineFadeTime = 0.05f;

    public Transform weaponNozzle;
    public Transform shootingRaycastPosition;

	public Color shotColor = Color.yellow;

	public AudioClip[] shootingSounds;
	public AudioClip reloadStartSound;
	public AudioClip reloadEndSound;

    public GameObject humanBloodFX;
    public GameObject alienBloodFX;
    public GameObject wallImpactFX;

    public GameObject humanBloodDecal;
    public GameObject alienBloodDecal;

    public GameObject muzzleFlashFX;
	public GameObject fireLineFX;

    public FadeRawImage hitmarker;

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

    private Text ammoText;

	public void Start()
	{
        currentBulletAmount = bulletsPerMagazine;

        if (isLocalPlayer)
        {
            ammoText = GameObject.FindGameObjectWithTag("AmmoText").GetComponent<Text>();
        }
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
					ReloadLocal();
				}
			}
		}

		if (Input.GetKeyDown(reloadButton) && currentBulletAmount < bulletsPerMagazine && !reloading)
		{
            ReloadLocal();
		}

        if (ammoText != null)
            ammoText.text = "Ammo: " + currentBulletAmount;
    }

    public void ReloadLocal()
    {
        CmdReloadOnServer();
        reloading = true;
    }

    private IEnumerator ReloadRoutine()
    {
        MyAudioSource.PlayOneShot(reloadStartSound);
        yield return new WaitForSeconds(reloadTime);
        MyAudioSource.PlayOneShot(reloadEndSound);
        currentBulletAmount = bulletsPerMagazine;
        reloading = false;
        yield break;
    }

    [Command]
    private void CmdReloadOnServer()
    {
        RpcReload();
    }

    [ClientRpc]
    private void RpcReload()
    {
        StartCoroutine(ReloadRoutine());
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
					ShootLocal(shootingRaycastPosition.position, deviation * shootingRaycastPosition.forward, damage, weaponNozzle.position, weaponNozzle.forward, weaponNozzle.position + Random.insideUnitSphere * maxMuzzleFlashOffset);
                    currentBulletAmount--;
				}
				else
				{
					shooting = false;
					yield break;
				}

                if (delayBetweenShots > 0)
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

    private void ShootLocal(Vector3 position, Vector3 direction, int projectileDamage, Vector3 nozzlePos, Vector3 nozzleForward, Vector3 muzzleFlashPosition)
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
                CmdHitEnemy(healthScript.gameObject, projectileDamage);

                // Play shot and human body impact FX.
                ShootEffect(HitType.Human, hit.point, hit.normal, nozzlePos, muzzleFlashPosition);
                CmdNotifyServerShot(HitType.Human, hit.point, hit.normal, nozzlePos, muzzleFlashPosition);
            }
            else if (hit.transform.root.CompareTag("Alien"))
            {
                if (hitmarker != null)
                    hitmarker.RefreshAlpha();

                var healthScript = hit.transform.root.GetComponent<PlayerHealth>();
                CmdHitEnemy(healthScript.gameObject, projectileDamage);

                // Play shot and alien body impact FX.
                ShootEffect(HitType.Alien, hit.point, hit.normal, nozzlePos, muzzleFlashPosition);
                CmdNotifyServerShot(HitType.Alien, hit.point, hit.normal, nozzlePos, muzzleFlashPosition);
            }
            else
            {
                // Play shot and object impact FX.
                ShootEffect(HitType.Wall, hit.point, hit.normal, nozzlePos, muzzleFlashPosition);
                CmdNotifyServerShot(HitType.Wall, hit.point, hit.normal, nozzlePos, muzzleFlashPosition);
            }

#if UNITY_EDITOR
            vectorTowardsHit = hit.point - shootingRaycastPosition.position;
#endif
        }
        else
        {
            // Didn't hit anything.
            // Play shot FX.
            ShootEffect(HitType.None, nozzlePos + nozzleForward * maxRange, hit.normal, nozzlePos, muzzleFlashPosition);
            CmdNotifyServerShot(HitType.None, nozzlePos + nozzleForward * maxRange, hit.normal, nozzlePos, muzzleFlashPosition);

#if UNITY_EDITOR
            vectorTowardsHit = shootingRaycastPosition.position + shootingRaycastPosition.forward * maxRange;
#endif
        }

#if UNITY_EDITOR
        Debug.DrawRay(shootingRaycastPosition.position, vectorTowardsHit, shotColor, 0.25f, true);
#endif
    }

    [Command]
    private void CmdHitEnemy(GameObject enemy, int damage)
    {
        enemy.GetComponent<PlayerHealth>().TakeDamage(damage);
    }

    [Command]
    private void CmdNotifyServerShot(HitType hitType, Vector3 hitPoint, Vector3 hitAngle, Vector3 nozzlePosition, Vector3 muzzleFlashPosition)
    {
        RpcShot(hitType, hitPoint, hitAngle, nozzlePosition, muzzleFlashPosition);
    }

	[ClientRpc]
	private void RpcShot(HitType hitType, Vector3 hitPoint, Vector3 hitAngle, Vector3 nozzlePosition, Vector3 muzzleFlashPosition)
	{
        if (!isLocalPlayer)
            ShootEffect(hitType, hitPoint, hitAngle, nozzlePosition, muzzleFlashPosition);
	}

    private void ShootEffect(HitType hitType, Vector3 hitPoint, Vector3 hitAngle, Vector3 nozzlePosition, Vector3 muzzleFlashPosition)
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

        float groundSpread = 0.5f;
        int randomKek = 0;

        switch (hitType)
        {
            case HitType.Human:
                // Instantiate human blood FX.
				Instantiate(humanBloodFX, hitPoint, Quaternion.LookRotation(hitAngle));

                randomKek = Random.Range(1, 4);
                for (int i = 0; i < randomKek; i++)
                {
                    GameObject groundBlood = Instantiate(humanBloodDecal, new Vector3(hitPoint.x + Random.Range(-groundSpread, groundSpread), Random.Range(0, 1000) / 100000f, hitPoint.z + Random.Range(-groundSpread, groundSpread)), Quaternion.Euler(90, Random.Range(0, 360), 0));
                    groundBlood.transform.localScale *= Random.Range(0.6f, 1.1f);
                }

                randomKek = Random.Range(1, 4);
                for (int i = 0; i < randomKek; i++)
                {
                    RaycastHit hit;
                    Quaternion deviation = Quaternion.AngleAxis(Random.Range(-bloodDeviationAngle, bloodDeviationAngle), Vector3.up);
                    deviation *= Quaternion.AngleAxis(Random.Range(-bloodDeviationAngle, bloodDeviationAngle), Vector3.right);

                    Vector3 bloodDir = deviation * hitAngle;
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

                break;
            case HitType.Alien:
                // Instantiate alien blood FX.
                Instantiate(alienBloodFX, hitPoint, Quaternion.LookRotation(hitAngle));

                randomKek = Random.Range(1, 4);
                for (int i = 0; i < randomKek; i++)
                {
                    GameObject groundBlood = Instantiate(alienBloodDecal, new Vector3(hitPoint.x + Random.Range(-groundSpread, groundSpread), Random.Range(0, 1000) / 100000f, hitPoint.z + Random.Range(-groundSpread, groundSpread)), Quaternion.Euler(90, Random.Range(0, 360), 0));
                    groundBlood.transform.localScale *= Random.Range(0.6f, 1.1f);
                }

                randomKek = Random.Range(1, 4);
                for (int i = 0; i < randomKek; i++)
                {
                    RaycastHit hit;
                    Quaternion deviation = Quaternion.AngleAxis(Random.Range(-bloodDeviationAngle, bloodDeviationAngle), Vector3.up);
                    deviation *= Quaternion.AngleAxis(Random.Range(-bloodDeviationAngle, bloodDeviationAngle), Vector3.right);

                    Vector3 bloodDir = deviation * hitAngle;
                    Ray bloodRay = new Ray(hitPoint, hitAngle);
                    int layer_mask = LayerMask.GetMask("BloodRaycast");

                    if (Physics.Raycast(bloodRay, out hit, 1.5f, layer_mask))
                    {
                        Debug.Log(hit.collider.gameObject.tag);
                        Debug.DrawLine(hitPoint, hit.point, Color.red, 5f);
                        if (hit.collider.gameObject.CompareTag("Wall"))
                        {
                            GameObject blood = Instantiate(alienBloodDecal, hit.point, Quaternion.LookRotation(-hit.normal));
                            blood.transform.position += hit.normal * (Random.Range(0, 1000) / 100000f);
                            blood.transform.Rotate(blood.transform.forward, Random.Range(0, 360), Space.World);
                            blood.transform.localScale *= Random.Range(0.6f, 1.1f);
                        }
                    }
                }

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
