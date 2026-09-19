using UnityEngine;

[DisallowMultipleComponent]
public sealed class Targetable : MonoBehaviour
{
    [SerializeField] private string displayName = "Training Unit";

    private GameObject lockMarker;
    private bool isLocked;

    public string DisplayName => displayName;
    public Transform AimPoint => transform;
    public bool IsActive => gameObject.activeInHierarchy;

    private void Awake()
    {
        CreateLockMarker();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        if (lockMarker != null)
        {
            lockMarker.SetActive(locked);
        }
    }

    private void CreateLockMarker()
    {
        lockMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lockMarker.name = "LockOnMarker";
        lockMarker.transform.SetParent(transform, false);
        lockMarker.transform.localPosition = Vector3.up * 1.65f;
        lockMarker.transform.localScale = Vector3.one * 0.18f;

        Collider markerCollider = lockMarker.GetComponent<Collider>();
        if (markerCollider != null)
        {
            Destroy(markerCollider);
        }

        Renderer renderer = lockMarker.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = CreateMaterial(new Color(0.15f, 0.95f, 1f));
            renderer.material = material;
        }

        lockMarker.SetActive(false);
        isLocked = false;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard");

        if (shader == null)
        {
            throw new System.InvalidOperationException("No compatible Unity material shader was found.");
        }

        Material material = new Material(shader)
        {
            color = color
        };

        return material;
    }

    private void OnDestroy()
    {
        if (lockMarker != null)
        {
            Destroy(lockMarker);
        }
    }
}
