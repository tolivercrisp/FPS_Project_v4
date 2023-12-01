using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]


// For those of you wanting to use this for an FPS, configure the player camera to be the raycast start point,
// from what I've heard making the camera the start point fixes a lot of inconsistencies with aiming
// YT comment



// Things strictly related to the guns themselves
public class GunScriptableObject : ScriptableObject
{
    /* https://www.youtube.com/watch?v=kT2ZxjMuT_4
       "Impact Effects with Scriptable Objects | Unity Tutorial"
     public ImpactType ImpactType;
     */ 
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public ShootConfigurationScriptableObject ShootConfig;
    public TrailConfigurationScriptableObject TrailConfig;

    private MonoBehaviour ActiveMonoBehavior;
    private GameObject Model;
    private float LastShootTime;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;


    public void Update()
    {
        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
    }
    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehavior)
    {
        this.ActiveMonoBehavior = ActiveMonoBehavior;
        LastShootTime = 0; // "in editor this will not be properly reset, in build it's fine" - Youtube
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();

    }

    public void Shoot()
    {
        if (Time.time > ShootConfig.FireRate + LastShootTime)
        { 
            LastShootTime = Time.time;
            ShootSystem.Play();
            Vector3 shootDirection = Camera.main.transform.forward
                + new Vector3(
                    Random.Range(
                        -ShootConfig.Spread.x,
                        ShootConfig.Spread.x
                    ),
                    Random.Range(
                        -ShootConfig.Spread.y,
                        ShootConfig.Spread.y
                    ),
                    Random.Range(
                        -ShootConfig.Spread.z,
                        ShootConfig.Spread.z
                    )
                 );

            shootDirection.Normalize();

            if (Physics.Raycast(
                ShootSystem.transform.position,
                shootDirection,
                out RaycastHit hit,
                float.MaxValue,
                ShootConfig.Hitmask
                ))
            {
                ActiveMonoBehavior.StartCoroutine(
                    PlayTrail(
                        ShootSystem.transform.position,
                        hit.point,
                        hit
                    )
                );
            }
            else
            {
                ActiveMonoBehavior.StartCoroutine(
                    PlayTrail(
                        ShootSystem.transform.position,
                        ShootSystem.transform.position + (shootDirection * TrailConfig.MissDistance),
                        new RaycastHit()
                    )
                );

            }

        }
    }

    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null; // "avoids position carry-over from last frame if reused" - YT (LiamAcademy)

        instance.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0) 
        {
            instance.transform.position = Vector3.Lerp(
                StartPoint,
                EndPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
             );
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = EndPoint;

        /* https://www.youtube.com/watch?v=kT2ZxjMuT_4
         * "Impact Effects with Scriptable Objects | Unity Tutorial"
         
        if (Hit.collider != null)
        {
            SurfaceManager.Instance.HandleImpact(
                Hit.transform.gameObject,
                EndPoint,
                Hit.normal,
                ImpactType,
                0

            );
        }
        */

    yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);

    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }

}
