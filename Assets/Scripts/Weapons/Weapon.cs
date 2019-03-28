using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(SphereCollider))]

// Weapon IS A MonoBehaviour IMPLEMENTING IInteractable
public class Weapon : MonoBehaviour, IInteractable
{
    public int damage = 10;
    public int maxAmmo = 500;
    public int maxClip = 30;
    public float range = 10f;
    public float shootRate = .2f;
    public float lineDelay = .1f;
    public Transform shotOrigin;

    private int ammo = 0;
    private int clip = 0;
    private float shootTimer = 0f;
    private bool canShoot = false;
    
    private Rigidbody rigid;
    private BoxCollider boxCollider;
    private LineRenderer lineRenderer;
    private SphereCollider sphereCollider;

    void GetReferences()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        lineRenderer = GetComponent<LineRenderer>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    void Reset()
    {
        GetReferences();

        // Get all transforms inside of children
        Renderer[] children = GetComponentsInChildren<MeshRenderer>();
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer rend in children)
        {
            bounds.Encapsulate(rend.bounds);
        }

        // Apply bounds to box collider
        boxCollider.center = bounds.center - transform.position;
        boxCollider.size = bounds.size;

        // Turn off line renderer
        lineRenderer.enabled = false;

        // Turn off rigidbody
        rigid.isKinematic = false;

        // Enable trigger
        sphereCollider.isTrigger = true;
        sphereCollider.center = boxCollider.center;
        sphereCollider.radius = boxCollider.size.magnitude * 0.5f;
    }

    void Awake()
    {
        GetReferences();
    }

    // Update is called once per frame
    void Update()
    {
        // Increase shoot timer
        shootTimer += Time.deltaTime;
        // If time reaches rate
        if (shootTimer >= shootRate)
        {
            // We can shoot!
            canShoot = true;
        }
    }

    public void Pickup()
    {
        // Disable rigidbody
        rigid.isKinematic = true;
    }
    public void Drop()
    {
        // Enable rigidbody
        rigid.isKinematic = false;
    }
    public string GetTitle()
    {
        return "Weapon";
    }

    IEnumerator ShowLine(Ray bulletRay, float lineDelay)
    {
        // Enable and Set Line
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, bulletRay.origin);
        lineRenderer.SetPosition(1, bulletRay.origin + bulletRay.direction * range);

        yield return new WaitForSeconds(lineDelay);

        // Disable Line
        lineRenderer.enabled = false;
    }

    public virtual void Reload()
    {
        // THIS IS CRAP, DON'T USE IT
        clip += ammo;
        ammo -= maxClip;
    }

    public virtual void Shoot()
    {
        // Can Shoot?
        if (canShoot)
        {
            // Create a bullet ray from shot origin to forward
            Ray bulletRay = new Ray(shotOrigin.position, shotOrigin.forward);
            RaycastHit hit;
            // Perform Raycast (Hit Scan)
            if (Physics.Raycast(bulletRay, out hit, range))
            {
                // Try getting enemy from hit
                IKillable killable = hit.collider.GetComponent<IKillable>();
                if (killable != null)
                {
                    // Deal tamage to enemy
                    killable.TakeDamage(damage);
                }
            }
            // Show Line
            StartCoroutine(ShowLine(bulletRay, lineDelay));
            // Reset timer
            shootTimer = 0;
            // Can't shoot anymore
            canShoot = false;
        }
    }
}
