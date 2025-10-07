using UnityEngine;

public class ConvertTerrainObjects : MonoBehaviour
{
    public Terrain terrain;
    public int minPerCluster = 2;
    public int maxPerCluster = 5;
    public float clusterRadius = 1.5f;
    public float spacingFactor = 20f;

    // Seuil pour ne pas placer de roches sur des pentes trop raides (0.0 = vertical, 1.0 = plat)
    [Range(0f, 1f)]
    public float minSlopeDot = 0.6f;

    // Supprime les anciens clusters à chaque génération
    public bool clearOldClusters = true;

    [ContextMenu("Convert Paint Details to GameObjects")]
    public void Start()
    {
        ConvertDetails();
    }

    void ConvertDetails()
    {
        if (terrain == null) terrain = GetComponent<Terrain>();
        TerrainData data = terrain.terrainData;
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("Aucune caméra trouvée. Assure-toi qu'il y a une caméra principale dans la scène.");
            return;
        }

        // Supprimer les anciens clusters si demandé
        if (clearOldClusters)
        {
            var existing = GameObject.Find("ConvertedStoneClusters");
            if (existing) DestroyImmediate(existing);
        }

        GameObject parent = new GameObject("ConvertedStoneClusters");
        parent.transform.position = terrain.transform.position;

        int detailRes = data.detailResolution;
        Vector3 terrainSize = data.size;

        float maxDistance = terrain.detailObjectDistance; // distance d'affichage identique à Paint Details

        for (int layerIndex = 0; layerIndex < data.detailPrototypes.Length; layerIndex++)
        {
            var detailProto = data.detailPrototypes[layerIndex];
            if (detailProto.prototype == null) continue;

            int[,] layer = data.GetDetailLayer(0, 0, detailRes, detailRes, layerIndex);

            for (int y = 0; y < detailRes; y++)
            {
                for (int x = 0; x < detailRes; x++)
                {
                    int count = layer[y, x];
                    if (count <= 0) continue;

                    // Échantillonnage pour éviter trop d'objets
                    if (Random.value > (1f / spacingFactor)) continue;

                    float normX = (float)x / detailRes;
                    float normZ = (float)y / detailRes;

                    Vector3 cellCenter = new Vector3(normX * terrainSize.x, 0, normZ * terrainSize.z) + terrain.transform.position;
                    cellCenter.y = terrain.SampleHeight(cellCenter);

                    int clusterCount = Random.Range(minPerCluster, maxPerCluster + 1);

                    for (int i = 0; i < clusterCount; i++)
                    {
                        Vector2 offset = Random.insideUnitCircle * clusterRadius;
                        Vector3 pos = cellCenter + new Vector3(offset.x, 0, offset.y);
                        pos.y = terrain.SampleHeight(pos);

                        float distance = Vector3.Distance(cam.transform.position, pos);

                        // 1. Ne pas placer si au-delà de la distance du terrain
                        if (distance > maxDistance) continue;

                        // 2. Réduire progressivement la densité avec la distance
                        float distanceFactor = Mathf.InverseLerp(0, maxDistance, distance);
                        float densityFactor = Mathf.Lerp(1f, 0.2f, distanceFactor);
                        if (Random.value > densityFactor) continue;

                        // 3. Aligner sur la pente
                        Vector3 normal = terrain.terrainData.GetInterpolatedNormal(normX, normZ);
                        if (normal.y < minSlopeDot) continue; // pente trop raide

                        GameObject go = Instantiate(detailProto.prototype, pos, Quaternion.identity, parent.transform);
                        go.transform.up = normal;

                        // Échelle uniforme
                        go.transform.localScale = detailProto.prototype.transform.localScale;

                        // Rotation aléatoire autour de l'axe Y
                        go.transform.Rotate(Vector3.up, Random.Range(0f, 360f));

                        go.name = detailProto.prototype.name + "_Clustered";
                    }
                }
            }
        }

        Debug.Log("Clusters générés avec comportement similaire à Paint Details dans " + parent.name);
    }
}
