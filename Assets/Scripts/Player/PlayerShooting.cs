using UnityEngine;
using UnityEngine.UI;


public class PlayerShooting : MonoBehaviour
{
    public int damagePerShot = 20;
    public float basicFireRate = 10;
    public float maxFireRate = 30;
    private float fireRate;
    public Slider fireRateSlider;
    public float range = 100f;


    float timer;
    Ray shootRay = new Ray();
    RaycastHit shootHit;
    int shootableMask;
    ParticleSystem gunParticles;
    LineRenderer gunLine;
    AudioSource gunAudio;
    Light gunLight;
    float effectsDisplayTime = 0.04f;


    void Awake ()
    {
        shootableMask = LayerMask.GetMask ("Shootable");
        gunParticles = GetComponent<ParticleSystem> ();
        gunLine = GetComponent <LineRenderer> ();
        gunAudio = GetComponent<AudioSource> ();
        gunLight = GetComponent<Light> ();

        // fire rate & its UI
        fireRate = basicFireRate;
        fireRateSlider.value = fireRate;
        fireRateSlider.minValue = basicFireRate;
        fireRateSlider.maxValue = maxFireRate;
    }


    void Update ()
    {
        timer += Time.deltaTime;

		if(Input.GetButton ("Fire1") && timer >= (1f / fireRate) && Time.timeScale != 0)
        {
            Shoot ();
            fireRateSlider.value = fireRate;
        }

        if(timer >= effectsDisplayTime)
        {
            DisableEffects ();
        }
    }


    public void DisableEffects ()
    {
        gunLine.enabled = false;
        gunLight.enabled = false;
    }


    void Shoot ()
    {
        timer = 0f;

        gunAudio.Play ();

        gunLight.enabled = true;

        gunParticles.Stop ();
        gunParticles.Play ();

        gunLine.enabled = true;
        gunLine.SetPosition (0, transform.position);

        shootRay.origin = transform.position;
        shootRay.direction = transform.forward;

        if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
        {
            EnemyHealth enemyHealth = shootHit.collider.GetComponent <EnemyHealth> ();
            if(enemyHealth != null)
            {   // This is an alive object - Hit the target!

                // take some health
                enemyHealth.TakeDamage (damagePerShot, shootHit.point);

                // push the enemy away
                Vector3 EnemyPosition = shootHit.collider.transform.position;
                Vector3 directionToEnemy = (EnemyPosition - this.transform.position).normalized;
                Vector3 force = directionToEnemy * 200;
                Rigidbody EnemyRB = shootHit.collider.GetComponent<Rigidbody>();
                EnemyRB.AddForceAtPosition(force, shootHit.point);

                // fire rate bonus
                fireRate *= 1.075f;
                if (fireRate > maxFireRate)
                    fireRate = maxFireRate;

            } else
            { // missed enemy - restoring fire rate
                fireRate *= 0.9f;
                if (fireRate < basicFireRate)
                    fireRate = basicFireRate;
            }

            gunLine.SetPosition (1, shootHit.point);
        }
        else
        {
            gunLine.SetPosition (1, shootRay.origin + shootRay.direction * range);
        }
    }
}
