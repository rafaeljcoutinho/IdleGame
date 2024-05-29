using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapPrefabSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private float radius;
    [SerializeField] private Transform center;
    [SerializeField] private Transform parent;
    
    [Range(0,500)]
    [SerializeField] private int quantity;
    [SerializeField] private LayerMask placeableOver;

    private void OnValidate()
    {
        Cleanup();
        SpawnObjects();
    }
    
    [ContextMenu("Cleanup")]
    public void Cleanup()
    {
        var amountToSpawn = quantity - parent.childCount;
        var amountToDestroy = -amountToSpawn;
        for (var i = 0; i < amountToDestroy; i++)
        {
            DestroyImmediate(parent.GetChild(parent.childCount-1).gameObject);
        }
    }
    
    public void SpawnObjects()
    {
        var amountToSpawn = quantity - parent.childCount;
        var maxTries = 100;
        for (int i = 0; i < amountToSpawn; i++)
        {
            var hit = new RaycastHit();
            var tries = 0;
            while (true)
            {
                var rr = Random.insideUnitCircle * radius;
                if (Physics.Raycast(center.transform.position + Vector3.up * .1f + new Vector3(rr.x, 0, rr.y),
                    Vector3.down, out hit, int.MaxValue))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    Debug.Log(hit.collider.gameObject.layer.ToString());
                    if ((1 << hit.collider.gameObject.layer & placeableOver) != 0)
                    {
                        break;
                    }
                }
                tries++;
                if (tries > maxTries) return;
            }

            var prefab = prefabs[Random.Range(0, prefabs.Count)];
            Instantiate(prefab, hit.point, prefab.transform.rotation, parent);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (center == null) return;
        Gizmos.DrawWireSphere(center.transform.position, radius);
    }
}
