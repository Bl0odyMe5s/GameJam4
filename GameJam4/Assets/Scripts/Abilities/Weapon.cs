using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour
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

	public Transform weaponNozzle;

	public Color shotColor = Color.yellow;

	public AudioClip shootSound;
	public AudioClip reloadSound;

	public enum PlaySound { EveryShot, EveryVolley, EverySeconds };
	public PlaySound playSoundAt = PlaySound.EveryShot;

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
                    ShootOnServer(weaponNozzle.position, weaponNozzle.forward);
                    currentBulletAmount--;
				}
				else
				{
					shooting = false;
					yield break;
				}

#if UNITY_EDITOR
                // shootingRay.origin = weaponNozzle.position;
				// shootingRay.direction = weaponNozzle.forward;
				// RaycastHit hit;
				// if (Physics.Raycast(shootingRay, out hit, maxRange))
				// {
				// 	hit.
				// }
                Vector3 directionVector = weaponNozzle.forward;
                directionVector.Scale(new Vector3(100, 100, 100));
                Debug.DrawRay(weaponNozzle.position, directionVector, shotColor, 0.25f, true);
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
    public void ShootOnServer(Vector3 position, Vector3 direction)
    {
        // Inform server a shot was fired on this line.
    }
}
