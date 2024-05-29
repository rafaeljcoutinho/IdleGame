using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectPlacement : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private GameObject parent;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private bool randomRotation;

    [SerializeField] private bool performCollisionChecks;

    private GameObject instance;
    private GameObject Instance {
        get
        {
            if (instance == null)
            {
                instance = Instantiate(prefabs[selectedPrefab]);
                instance.layer = 1;
            }
            return instance;
        }
    }

    private float height;
    private float rotation;
    private Vector3 scale = Vector3.one;
    private int selectedPrefab = 0;
    private bool onlyHitFloor = true;
    private bool snapToGrid = true;
    
    public float Height => height;
    public int SelectedPrefab => selectedPrefab;
    public bool OnlyHitFloor => onlyHitFloor;

    public void ChangePrefabs(int delta)
    {
        var selected = selectedPrefab + delta;
        if (selected != selectedPrefab && instance != null)
        {
            Destroy(instance);
            instance = null;
        }
        selectedPrefab = selected % prefabs.Count;
        if (selectedPrefab < 0)
        {
            selectedPrefab = prefabs.Count - 1;
        }
    }

    public void ChangeHeight(float delta)
    {
        height += delta;
        height = Mathf.Clamp(height, 0, 5);
    }

    public void ToggleHitFloor()
    {
        onlyHitFloor = !onlyHitFloor;
        height = 0;
    }

    [Obsolete("Obsolete")]
    void Update()
    {
        var colliding = IsColliding();

        if (Input.GetKeyDown(KeyCode.DownArrow))
            ChangeHeight(-.5f);
        if (Input.GetKeyDown(KeyCode.UpArrow))
            ChangeHeight(.5f);
        
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleHitFloor();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            ChangePrefabs(-1);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ChangePrefabs(+1);

        if (Input.GetKeyDown(KeyCode.S))
            snapToGrid = !snapToGrid;

        if (Input.GetKeyDown(KeyCode.H))
        {
            scale.x += 1f;
            scale.z += 1f;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            scale.x = Mathf.Clamp(scale.x - 1f, 1f, scale.x);
            scale.z = Mathf.Clamp(scale.z - 1f, 1f, scale.z);
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            scale.y += 1f;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            scale.y = Mathf.Clamp(scale.y - 1f, 1f, scale.y);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            scale = Vector3.one;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            rotation += 90;
            rotation %= 360;
        }

        UpdatePreviewPosition(colliding);

        var canPlaceOnFloor = !colliding && onlyHitFloor && Input.GetMouseButton(0);
        var canPlaceAttachment = !colliding && !onlyHitFloor && Input.GetMouseButtonDown(0);

        #if UNITY_EDITOR
        if (canPlaceOnFloor || canPlaceAttachment)
        {
            var instanced =  UnityEditor.PrefabUtility.InstantiatePrefab(prefabs[selectedPrefab], parent.transform) as GameObject;
            instanced.transform.position = Instance.transform.position;
            instanced.transform.rotation = Instance.transform.rotation;
            if (randomRotation)
            {
                instanced.transform.RotateAround(Vector3.up, Random.Range(0,360));
            }
            instanced.transform.localScale = scale;
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(parent);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(instanced.transform);
//            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        #endif
    }

    private void UpdatePreviewPosition(bool colliding)
    {
        var cam = Camera.main;
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        
        var layer = onlyHitFloor ? (int)layerMask : Int32.MaxValue ^ ( 1 << Instance.layer);
        Instance.transform.localScale = scale;
        Instance.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));

        if (Physics.Raycast(ray, out var hitInfo, Int32.MaxValue, layer))
        {
            var p = hitInfo.point;
            var h = onlyHitFloor ? 0 : Mathf.RoundToInt(p.y);
            if (snapToGrid)
            {
                var gridCenter = new Vector3(Mathf.RoundToInt(p.x), h + height, Mathf.RoundToInt(p.z));
                Instance.transform.position = gridCenter;
            }
            else
            {
                Instance.transform.position = new Vector3(p.x, p.y + height, p.z);
            }
//            PaintMaterial(Instance, colliding);
        }
    }

    void PaintMaterial(GameObject go, bool colliding)
    {
        void Paint(MeshRenderer meshRenderer)
        {
            var materialColor = meshRenderer.material.color;
            materialColor = colliding ? Color.red : Color.white;
            meshRenderer.material.color = materialColor;
        }

        var mr = Instance.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Paint(mr);
        }
        else
        {
            var mrs = Instance.GetComponentsInChildren<MeshRenderer>();
            if (mrs == null) return;
            foreach (var mr2 in mrs)
            {
                Paint(mr2);
            }
        }
    }
        
    bool IsColliding()
    {
        if (!performCollisionChecks)
            return false;
        var col = Instance.GetComponent<Collider>();
        var layer = Int32.MaxValue ^ ( 1 << Instance.layer);
        return Physics.CheckBox(col.bounds.center, col.bounds.extents/2, Instance.transform.rotation, layer);
    }
}
