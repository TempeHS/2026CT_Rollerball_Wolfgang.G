using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Pickup_Script : MonoBehaviour
{
    // Prefabs to spawn.
    public GameObject pickUpPrefab;
    public GameObject staminaPickupPrefab;

    // Tags used by PlayerController.OnTriggerEnter.
    public string pickUpTag = "PickUp";
    public string staminaPickUpTag = "staminaPickup";

    // Spawn counts.
    public int pickUpCount = 81;
    public int staminaPickupCount = 20;

    // Spawn area.
    public float spawnY = 0.5f;
    public Vector2 spawnXRange = new Vector2(-20f, 20f);
    public Vector2 spawnZRange = new Vector2(-20f, 20f);

    // NavMesh settings.
    public float navMeshSampleRadius = 2.5f;
    public float spawnHeightPadding = 0.05f;
    public float minimumPickupSpacing = 2.0f;

    NavMeshTriangulation navMeshTriangulation;
    bool hasNavMeshTriangulation;

    void Start()
    {
        CacheNavMeshTriangulation();
        SpawnPrefabEvenly(pickUpPrefab, pickUpCount, pickUpTag);
        SpawnPrefabEvenly(staminaPickupPrefab, staminaPickupCount, staminaPickUpTag);
    }

    void SpawnPrefabEvenly(GameObject prefabToSpawn, int amount, string spawnTag)
    {
        if (prefabToSpawn == null || amount <= 0)
        {
            return;
        }

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = amount * 200;
        List<Vector3> spawnedPositions = new List<Vector3>(amount);

        while (spawned < amount && attempts < maxAttempts)
        {
            attempts = attempts + 1;

            if (!TryGetRandomNavMeshPoint(out Vector3 point))
            {
                continue;
            }

            if (!IsWithinSpawnBounds(point))
            {
                continue;
            }

            if (!IsFarEnoughFromOthers(point, spawnedPositions, minimumPickupSpacing))
            {
                continue;
            }

            GameObject spawnedObject = Instantiate(prefabToSpawn, point, Quaternion.identity);
            SetupSpawnedPickup(spawnedObject, spawnTag);
            spawnedPositions.Add(point);
            spawned = spawned + 1;
        }

        Debug.Log(prefabToSpawn.name + " Spawned: " + spawned);
    }

    void CacheNavMeshTriangulation()
    {
        navMeshTriangulation = NavMesh.CalculateTriangulation();
        hasNavMeshTriangulation =
            navMeshTriangulation.vertices != null &&
            navMeshTriangulation.indices != null &&
            navMeshTriangulation.vertices.Length >= 3 &&
            navMeshTriangulation.indices.Length >= 3;

        if (!hasNavMeshTriangulation)
        {
            Debug.LogWarning("No baked NavMesh triangulation found. Pickups may not spawn.");
        }
    }

    bool TryGetRandomNavMeshPoint(out Vector3 navPoint)
    {
        navPoint = Vector3.zero;

        if (!hasNavMeshTriangulation)
        {
            return false;
        }

        int triangleCount = navMeshTriangulation.indices.Length / 3;
        if (triangleCount <= 0)
        {
            return false;
        }

        int baseIndex = Random.Range(0, triangleCount) * 3;
        int i0 = navMeshTriangulation.indices[baseIndex];
        int i1 = navMeshTriangulation.indices[baseIndex + 1];
        int i2 = navMeshTriangulation.indices[baseIndex + 2];

        Vector3 a = navMeshTriangulation.vertices[i0];
        Vector3 b = navMeshTriangulation.vertices[i1];
        Vector3 c = navMeshTriangulation.vertices[i2];

        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;
        Vector3 randomPoint = ((1f - r1) * a) + (r1 * (1f - r2) * b) + (r1 * r2 * c);

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            navPoint = hit.position;
            return true;
        }

        return false;
    }

    bool IsWithinSpawnBounds(Vector3 point)
    {
        return point.x >= spawnXRange.x &&
               point.x <= spawnXRange.y &&
               point.z >= spawnZRange.x &&
               point.z <= spawnZRange.y;
    }

    bool IsFarEnoughFromOthers(Vector3 candidate, List<Vector3> existing, float minDistance)
    {
        float minDistanceSqr = minDistance * minDistance;
        for (int i = 0; i < existing.Count; i++)
        {
            Vector3 delta = existing[i] - candidate;
            delta.y = 0f;

            if (delta.sqrMagnitude < minDistanceSqr)
            {
                return false;
            }
        }

        return true;
    }

    void SetupSpawnedPickup(GameObject spawnedObject, string spawnTag)
    {
        if (spawnedObject == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(spawnTag))
        {
            try
            {
                spawnedObject.tag = spawnTag;
            }
            catch (UnityException)
            {
                Debug.LogWarning("Tag not found in project: " + spawnTag);
            }
        }

        Collider pickupCollider = spawnedObject.GetComponent<Collider>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;

            Vector3 position = spawnedObject.transform.position;
            float yLift = pickupCollider.bounds.extents.y + spawnHeightPadding;
            spawnedObject.transform.position = new Vector3(position.x, position.y + yLift, position.z);
            return;
        }

        Renderer pickupRenderer = spawnedObject.GetComponentInChildren<Renderer>();
        if (pickupRenderer != null)
        {
            Vector3 position = spawnedObject.transform.position;
            float yLift = pickupRenderer.bounds.extents.y + spawnHeightPadding;
            spawnedObject.transform.position = new Vector3(position.x, position.y + yLift, position.z);
        }
    }
}
