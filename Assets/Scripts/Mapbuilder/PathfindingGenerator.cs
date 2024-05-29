using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
public class PathfindingGenerator : MonoBehaviour
{
    [SerializeField] private LayerMask floorLayer;
    private List<Node> paths;
    private Dictionary<Transform, Node> nodeDict;

    public static PathfindingGenerator Instance;
    public PathfindingQuery Query;
    public bool Initialized = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance != null)
        {
            Debug.LogError("Only one path finding generator could be instantiated");
        }
        Instance = this;
    }

    private void Start()
    {
        Generate();
    }

    private void Generate()
    {
        if (!Application.isPlaying) return;
        paths = new List<Node>();
        nodeDict = new Dictionary<Transform, Node>();
        StartCoroutine(GeneratePath(transform.GetChild(0), () =>
        {
            Query = new PathfindingQuery();
            Query.BakeMap(paths[0]);
            Initialized = true;
        }));
    }

    public Node GetNode(Transform t)
    {
        if (nodeDict.ContainsKey(t)) return nodeDict[t];
        return null;
    }
    
    public Node GetPath(Transform o, Transform t)
    {
        var originNode = nodeDict.ContainsKey(o);
        var targetNode = nodeDict.ContainsKey(t);

        if (!originNode || !targetNode) return null;
        if (Query.NavMap.ContainsKey(nodeDict[t]))
        {
            Debug.Log("Nav map has O");
            if (Query.NavMap[nodeDict[t]].ContainsKey(nodeDict[o]))
            {
                Debug.Log("Nav map has T");
                return Query.NavMap[nodeDict[t]][nodeDict[o]];
            }
        }

        return null;
    }
    
    IEnumerator GeneratePath(Transform root, Action onComplete)
    {
        yield return null;
        var worldPos = root.position + Vector3.up * 1.5f;
        Physics.Raycast(worldPos, Vector3.down, out var hitInfo, int.MaxValue, floorLayer);
        var rootNode = NewNodeFromCollider(hitInfo.collider);

        var visited = new HashSet<Vector3Int> {rootNode.Coords};
        var queue = new List<Node> {rootNode};
        nodeDict.Add(rootNode.T, rootNode);

        while (queue.Count > 0)
        {
            var element = nodeDict[queue[0].T];
            queue.RemoveAt(0);
            var reachableNodes = VisitableNodes(element);
            foreach (var node in reachableNodes)
            {
                if (visited.Contains(node.Coords))
                {
                    element.Neighbours.Add(nodeDict[node.T]);
                    continue;
                }
                element.Neighbours.Add(node);
                nodeDict.Add(node.T, node);
                visited.Add(node.Coords);
                queue.Add(node);
            }
        }
        paths.Add(rootNode);
        onComplete?.Invoke();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (true) return;
        /*
        if (paths != null && paths.Count > 0)
        {
            var visited = new HashSet<Node>();
            DrawNode(paths[0], ref visited);
        }
        */
    }
    
    void DrawNode(Node n, ref HashSet<Node> done)
    {
        if (done.Contains(n)) return;
        done.Add(n);
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(n.SurfacePos, .1f);
        foreach (var vizIt in n.Neighbours)
        {
            var viz = nodeDict[vizIt.T];
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(n.SurfacePos, n.SurfacePos + (viz.SurfacePos-n.SurfacePos) * .45f);
            DrawNode(viz, ref done);
        }
    }

    
    List<Node> VisitableNodes(Node node)
    {
        var ans = new List<Node>();
        foreach (var dirRaw in new List<Vector3> {Vector3.forward, Vector3.back, Vector3.right, Vector3.left})
        {
            var p = node.SurfacePos;
            var n = node.SurfaceNormal;
            var dir = Vector3.ProjectOnPlane(dirRaw, n);
            
//            Debug.DrawLine(p + n * .1f, p + n *.1f + dir, Color.red, 5f);
            var colResults = Physics.Raycast(p + n * .1f, dir, out var nHit, 1, floorLayer);
            if (colResults)
            {
                // two valid states -> climbing a scope, exiting a scope
                var normalIsHorizontal = Vector3.Dot(nHit.normal, -dirRaw) > .99f;
                if (normalIsHorizontal) continue;
                ans.Add(NewNodeFromCollider(nHit.collider));
            }
            else
            {
                if (Physics.Raycast(p + Vector3.up * .1f + dir, Vector3.down, out var nHit2, .5f, floorLayer))
                {
                    ans.Add(NewNodeFromCollider(nHit2.collider));
                }
            }
        }
        return ans;
    }

    Node NewNodeFromCollider(Collider col)
    {
        var p =  col.transform.position + Vector3.up * 1.1f;
        Physics.Raycast(p, Vector3.down, out var hitInfo, 1.5f, floorLayer);
        var newNode = new Node(col.transform, hitInfo);
        return newNode;
    }

    public class Node
    {
        public Vector3Int Coords;
        public List<Node> Neighbours;
        public Transform T;
        public Vector3 SurfacePos;
        public Vector3 SurfaceNormal;

        public Node(Transform transform, RaycastHit hit)
        {
            T = transform;
            var worldPos = transform.position;
            Coords = new Vector3Int(
                Mathf.RoundToInt(worldPos.x * 2),
                Mathf.RoundToInt(worldPos.y * 2), 
                Mathf.RoundToInt(worldPos.z * 2));
            SurfacePos = hit.point;
            SurfaceNormal = hit.normal;
            Neighbours = new List<Node>();
        }
    }

    public class PathfindingQuery
    {
        private Dictionary<Node, Dictionary<Node, Node>> navMap;
        public Dictionary<Node, Dictionary<Node, Node>> NavMap => navMap;

        public void BakeMap(Node origin)
        {
            navMap = new Dictionary<Node, Dictionary<Node, Node>>();
            var allReachable = Generate(origin);
            foreach (var n in allReachable)
            {
                Generate(n);
                if (navMap[n].Count == 3)
                {
                    n.T.DOShakeScale(60, 1, 100);
                }
            }
        }

        public void DrawMap(Node origin)
        {
            foreach (var kvPair in navMap[origin])
            {
                var from = kvPair.Key;
                var to = kvPair.Value;
                Debug.DrawLine(from.SurfacePos + Vector3.up * .1f, to.SurfacePos + Vector3.up * .1f, Color.magenta, 60f);
            }
        }

        public List<Node> Path(Node o, Node t)
        {
            if (!navMap.ContainsKey(o))
            {
                Debug.LogError("Nav map has no origin");
                return null;                
            }

            if (!navMap[o].ContainsKey(t))
            {
                Debug.LogError("Nav map has no path to target from origin");
                return null;
            }
            var ans = new List<Node>();

            var curr = t;
            int maxD = 100;
            int i = 0;
            while (curr.Coords != o.Coords && i < maxD)
            {
                ans.Add(curr);
                curr = navMap[o][curr];
                i++;
            }

            if (curr.Coords == o.Coords)
            {
                ans.Add(curr);
                return ans;
            }
            return null;
        }

        List<Node> Generate(Node n)
        {
            var visited = new HashSet<Vector3Int>();
            var toVisit = new Queue<Node>();
            var allNodes = new List<Node>();
            toVisit.Enqueue(n);
            visited.Add(n.Coords);
            if (navMap.ContainsKey(n))
            {
                navMap[n] = new Dictionary<Node, Node> {{n, n}};
            }
            else
            {
                navMap.Add(n, new Dictionary<Node, Node>{{n,n}});                
            }

            while (toVisit.Count > 0)
            {
                var visit = toVisit.Dequeue();
                foreach (var viz in visit.Neighbours)
                {
                    if (!visited.Contains(viz.Coords))
                    {
                        visited.Add(viz.Coords);
                        toVisit.Enqueue(viz);
                        allNodes.Add(viz);
                        navMap[n].Add(viz, visit);
                    }
                }
            }

            return allNodes;
        }
    }
}
