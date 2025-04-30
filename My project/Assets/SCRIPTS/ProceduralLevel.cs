using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ProceduralLevel : MonoBehaviour
{
    /* ─────────────  Inspector fields  ───────────── */
    [Header("Arena size (tiles)")]
    public int width = 60;
    public int height = 34;

    
    public Sprite floorSprite;                      
    public Sprite wallSprite;                       
    public PhysicsMaterial2D wallPhysicsMaterial;   

    
    public GameObject basePrefab;                   

    
    [Range(0f, 1f)] public float wallChance = 0.33f; // initial noise
    public int smoothIterations = 3;     // CA passes
    public int minBaseDistance = 12;    // tiles apart
    public int seed = 0;                              // 0 = random each run

    /* ─────────────  internal  ───────────── */
    int[,] map;                                      // 0 floor / 1 wall
    readonly Color[] palette = { Color.red, Color.blue };

    
    void Awake() { Generate(); }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    
    void Generate()
    {
        int runtimeSeed = (seed == 0) ? Random.Range(0, int.MaxValue) : seed;
        Random.InitState(runtimeSeed);

        Debug.Log("Procedural Seed: " + runtimeSeed);



        GenerateTileArray();   // cave layout
        InstantiateTiles();    // create floors & walls from sprites
        PlaceFourBases();      // Red, Blue, Red, Blue
    }

    
    void GenerateTileArray()
    {
        map = new int[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                bool border = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                map[x, y] = border ? 1 : (Random.value < wallChance ? 1 : 0);
            }

        for (int i = 0; i < smoothIterations; i++) Smooth();
        EnsureConnectivity();
    }

    void Smooth()
    {
        int[,] copy = (int[,])map.Clone();
        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                int walls = CountNeighbours(x, y);
                copy[x, y] = walls >= 5 ? 1 : 0;
            }
        map = copy;
    }

    int CountNeighbours(int cx, int cy)
    {
        int c = 0;
        for (int x = cx - 1; x <= cx + 1; x++)
            for (int y = cy - 1; y <= cy + 1; y++)
                if (!(x == cx && y == cy) && map[x, y] == 1) c++;
        return c;
    }

    void EnsureConnectivity()
    {
        bool[,] seen = new bool[width, height];
        Flood(1, 1, seen);

        for (int x = 1; x < width - 1; x++)
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] == 0 && !seen[x, y])
                {
                    int tx = x; while (tx > 1) { map[tx, y] = 0; tx--; }
                    int ty = y; while (ty > 1) { map[1, ty] = 0; ty--; }
                }
            }
    }

    void Flood(int sx, int sy, bool[,] seen)
    {
        Stack<Vector2Int> st = new();
        st.Push(new Vector2Int(sx, sy));
        while (st.Count > 0)
        {
            var p = st.Pop();
            if (p.x <= 0 || p.x >= width - 1 || p.y <= 0 || p.y >= height - 1) continue;
            if (seen[p.x, p.y] || map[p.x, p.y] == 1) continue;
            seen[p.x, p.y] = true;
            st.Push(new(p.x + 1, p.y)); st.Push(new(p.x - 1, p.y));
            st.Push(new(p.x, p.y + 1)); st.Push(new(p.x, p.y - 1));
        }
    }

    
    void InstantiateTiles()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new(x, y, 0);

                CreateFloor(pos).transform.parent = transform;
                if (map[x, y] == 1)
                    CreateWall(pos).transform.parent = transform;
            }
    }

    GameObject CreateFloor(Vector3 pos)
    {
        var go = new GameObject("Floor");
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = floorSprite;
        sr.sortingOrder = 1;

        return go;
    }

    GameObject CreateWall(Vector3 pos)
    {
        var go = new GameObject("Wall");
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = wallSprite;

        var col = go.AddComponent<BoxCollider2D>();
        col.sharedMaterial = wallPhysicsMaterial;
        go.layer = LayerMask.NameToLayer("Walls");
        sr.sortingOrder = 2;

        return go;
    }

    
    void PlaceFourBases()
    {
        


        const int desiredBases = 4;
        List<Vector2Int> chosen = new();
        int tries = 0;

        while (chosen.Count < desiredBases && tries < 4000)
        {
            tries++;
            int x = Random.Range(2, width - 2);
            int y = Random.Range(2, height - 2);
            if (map[x, y] == 1) continue;

            Vector2Int cand = new(x, y);
            bool far = true;
            foreach (var p in chosen)
                if (Vector2Int.Distance(p, cand) < minBaseDistance) { far = false; break; }
            if (far) chosen.Add(cand);
        }

        for (int i = 0; i < chosen.Count; i++)
        {
            Vector2Int cell = chosen[i];
            Vector3 pos = new(cell.x, cell.y, 0);

            GameObject b = Instantiate(basePrefab, pos, Quaternion.identity);
            Color col = palette[i % palette.Length];        // R,B,R,B

            var sr = b.GetComponent<SpriteRenderer>();
            sr.color = col;
            b.GetComponent<BillionaireBase>().baseColor = col;
        }
    }
}




