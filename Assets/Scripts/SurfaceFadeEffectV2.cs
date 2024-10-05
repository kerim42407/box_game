using UnityEngine;
using System.Collections.Generic;

public class SurfaceFadeEffectV2 : MonoBehaviour
{
    public Transform surfaceObject;
    public float fadeDuration = 1f;
    public float fadeDistance = 1f;
    public float minAlpha = 0f;
    public bool showMeshColliderBounds = true;

    private List<Renderer> trackedObjects = new List<Renderer>();
    private Dictionary<Renderer, float> fadeValues = new Dictionary<Renderer, float>();
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    private void Update()
    {
        MeshCollider meshCollider = surfaceObject.GetComponent<MeshCollider>();

        foreach (Renderer rend in trackedObjects)
        {
            if (rend == null || meshCollider == null) continue;

            // Calculate the distance to the surface of the mesh
            float distance = DistanceToMeshSurface(meshCollider, rend.bounds.center);
            float targetAlpha = Mathf.Clamp01(1 - (distance / fadeDistance));
            targetAlpha = Mathf.Max(targetAlpha, minAlpha);

            float currentAlpha = fadeValues[rend];
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime / fadeDuration);
            fadeValues[rend] = currentAlpha;

            SetObjectFade(rend, currentAlpha);
        }
    }

    private void OnDrawGizmos()
    {
        // Eğer sınırları göstermek aktifse
        if (showMeshColliderBounds)
        {
            // MeshCollider bileşenini al
            MeshCollider meshCollider = GetComponent<MeshCollider>();

            if (meshCollider != null)
            {
                // Collider'ın bağlı olduğu Mesh'in Bound'larını çiz
                Gizmos.color = Color.green;
                Gizmos.DrawWireMesh(meshCollider.sharedMesh, transform.position, transform.rotation, transform.localScale);
            }
        }
    }
    
    private float DistanceToMeshSurface(MeshCollider meshCollider, Vector3 point)
    {
        // Get the closest point on the mesh surface
        Vector3 closestPoint = meshCollider.ClosestPoint(point);
        return Vector3.Distance(closestPoint, point);
    }

    private void SetObjectFade(Renderer rend, float fadeAmount)
    {
        var materials = rend.materials;
        bool fade = fadeAmount <1;
        foreach (var mat in materials)
        {
            mat.SetFloat("_FadeAmount", fadeAmount);
            if(fade) mat.ToFadeMode();
            else mat.ToOpaqueMode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Renderer rend = other.GetComponent<Renderer>();
        if (rend != null && !trackedObjects.Contains(rend))
        {
            trackedObjects.Add(rend);
            fadeValues[rend] = 1f;
            originalMaterials[rend] = rend.sharedMaterials;
            SetupObjectForFading(rend);
        }
    }

    private void SetupObjectForFading(Renderer rend)
    {
        Material[] newMaterials = new Material[rend.sharedMaterials.Length];
        for (int i = 0; i < rend.sharedMaterials.Length; i++)
        {
            Material originalMat = rend.sharedMaterials[i];
            Material newMat = new Material(Shader.Find("Custom/URPLitFadeV2"));
            
            newMat.CopyPropertiesFromMaterial(originalMat);
            newMat.SetFloat("_FadeAmount", 1);
            
            newMaterials[i] = newMat;
        }
        rend.materials = newMaterials;
    }

    private void OnDisable()
    {
        foreach (var rend in trackedObjects)
        {
            if (rend != null && originalMaterials.ContainsKey(rend))
            {
                rend.materials = originalMaterials[rend];
            }
        }
        trackedObjects.Clear();
        fadeValues.Clear();
        originalMaterials.Clear();
    }
}
