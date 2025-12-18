using UnityEngine;
using System.Collections.Generic;

public class TransparentMaker : MonoBehaviour
{
    public float radius = 5f;  // Radius for checking nearby objects
    public Material newMaterial = null; // Transparent material to apply to objects within the radius
    public string[] excludeTags;  // Array of tags to exclude from transparency

    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>(); // To store original materials

    void Update()
    {
        // Call to make nearby objects transparent
        MakeNearbyObjectsTransparent();
    }

    // Function to make all GameObjects within the radius transparent, excluding those with the specified tags
    void MakeNearbyObjectsTransparent()
    {
        // Find all colliders within the radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider collider in colliders)
        {
            // Check if the object has a tag that should be excluded
            if (IsExcludedTag(collider.gameObject))
                continue;

            Renderer renderer = collider.GetComponent<Renderer>();
            if (renderer != null)
            {
                // If this renderer doesn't have an original material stored, save it
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.material;
                }

                // Apply the new transparent material
                renderer.material = newMaterial;
            }
        }
    }

    // Function to check if the object has an excluded tag
    bool IsExcludedTag(GameObject obj)
    {
        foreach (string tag in excludeTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;  // Return true if the object has one of the excluded tags
            }
        }
        return false;  // Return false if no excluded tag is found
    }

    // This function will be called when the object is deactivated
    void OnDeactivate()
    {
        // Reset materials of all affected objects
        foreach (var entry in originalMaterials)
        {
            if (entry.Key != null)
            {
                entry.Key.material = entry.Value; // Reset the material to its original state
            }
        }
        originalMaterials.Clear();  // Clear the dictionary
        Debug.Log("All materials have been reset.");
    }

    // Unity's OnDisable method automatically calls OnDeactivate when the object is disabled
    void OnDisable()
    {
        OnDeactivate();  // Ensure materials are reset when the object is disabled
    }
}
