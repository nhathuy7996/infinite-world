using UnityEngine;

namespace InfiniteWorld
{
    public class ProceduralTerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Mesh Settings")]
        [SerializeField] private int terrainResolution = 20;
        [SerializeField] private float heightMultiplier = 10f;
        [SerializeField] private float noiseScale = 0.1f;

        [Header("Material")]
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private bool generateCollider = true;

        [Header("Terrain Colors (Gradient by Height)")]
        [SerializeField] private Color deepColor = new Color(0.2f, 0.3f, 0.1f);
        [SerializeField] private Color midColor = new Color(0.3f, 0.5f, 0.2f);
        [SerializeField] private Color highColor = new Color(0.6f, 0.6f, 0.5f);

        private int worldSeed;

        public void Initialize(int seed)
        {
            worldSeed = seed;
        }

        public GameObject GenerateTerrainChunk(Vector2Int chunkIndex, float chunkSize, Transform parent)
        {
            GameObject terrainObj = new GameObject($"Terrain_{chunkIndex.x}_{chunkIndex.y}");
            terrainObj.transform.parent = parent;
            terrainObj.transform.localPosition = Vector3.zero;

            Mesh terrainMesh = GenerateTerrainMesh(chunkIndex, chunkSize);

            MeshFilter meshFilter = terrainObj.AddComponent<MeshFilter>();
            meshFilter.mesh = terrainMesh;

            MeshRenderer meshRenderer = terrainObj.AddComponent<MeshRenderer>();
            if (terrainMaterial != null)
            {
                meshRenderer.material = terrainMaterial;
            }
            else
            {
                meshRenderer.material = new Material(Shader.Find("Standard"));
                meshRenderer.material.color = midColor;
            }

            if (generateCollider)
            {
                MeshCollider meshCollider = terrainObj.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = terrainMesh;
            }

            return terrainObj;
        }

        private Mesh GenerateTerrainMesh(Vector2Int chunkIndex, float chunkSize)
        {
            Mesh mesh = new Mesh();
            mesh.name = $"TerrainMesh_{chunkIndex.x}_{chunkIndex.y}";

            int vertexCount = (terrainResolution + 1) * (terrainResolution + 1);
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];

            float stepSize = chunkSize / terrainResolution;
            float worldOffsetX = chunkIndex.x * chunkSize;
            float worldOffsetZ = chunkIndex.y * chunkSize;

            float seedOffsetX = GetSeededOffset(worldSeed, 0);
            float seedOffsetZ = GetSeededOffset(worldSeed, 1);

            int index = 0;
            for (int z = 0; z <= terrainResolution; z++)
            {
                for (int x = 0; x <= terrainResolution; x++)
                {
                    float localX = x * stepSize;
                    float localZ = z * stepSize;

                    float worldX = worldOffsetX + localX;
                    float worldZ = worldOffsetZ + localZ;

                    float height = GetTerrainHeight(worldX, worldZ, seedOffsetX, seedOffsetZ);

                    vertices[index] = new Vector3(localX, height, localZ);
                    uvs[index] = new Vector2((float)x / terrainResolution, (float)z / terrainResolution);

                    colors[index] = GetColorByHeight(height);

                    index++;
                }
            }

            int triangleCount = terrainResolution * terrainResolution * 6;
            int[] triangles = new int[triangleCount];
            int triIndex = 0;

            for (int z = 0; z < terrainResolution; z++)
            {
                for (int x = 0; x < terrainResolution; x++)
                {
                    int topLeft = z * (terrainResolution + 1) + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (z + 1) * (terrainResolution + 1) + x;
                    int bottomRight = bottomLeft + 1;

                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = topRight;

                    triangles[triIndex++] = topRight;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = bottomRight;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.colors = colors;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private float GetTerrainHeight(float worldX, float worldZ, float seedOffsetX, float seedOffsetZ)
        {
            float height = 0f;

            float freq1 = noiseScale;
            float amp1 = heightMultiplier;
            height += Mathf.PerlinNoise(
                (worldX + seedOffsetX) * freq1,
                (worldZ + seedOffsetZ) * freq1
            ) * amp1;

            float freq2 = noiseScale * 2f;
            float amp2 = heightMultiplier * 0.5f;
            height += Mathf.PerlinNoise(
                (worldX + seedOffsetX + 1000) * freq2,
                (worldZ + seedOffsetZ + 1000) * freq2
            ) * amp2;

            float freq3 = noiseScale * 4f;
            float amp3 = heightMultiplier * 0.25f;
            height += Mathf.PerlinNoise(
                (worldX + seedOffsetX + 2000) * freq3,
                (worldZ + seedOffsetZ + 2000) * freq3
            ) * amp3;

            return height;
        }

        private Color GetColorByHeight(float height)
        {
            float normalizedHeight = Mathf.Clamp01(height / (heightMultiplier * 1.5f));

            if (normalizedHeight < 0.33f)
            {
                return Color.Lerp(deepColor, midColor, normalizedHeight * 3f);
            }
            else if (normalizedHeight < 0.66f)
            {
                return Color.Lerp(midColor, highColor, (normalizedHeight - 0.33f) * 3f);
            }
            else
            {
                return highColor;
            }
        }

        private float GetSeededOffset(int seed, int axis)
        {
            System.Random rng = new System.Random(seed + axis * 1000);
            return (float)rng.NextDouble() * 10000f;
        }

        public GameObject GenerateFlatTerrain(Vector2Int chunkIndex, float chunkSize, Transform parent)
        {
            GameObject terrainObj = new GameObject($"FlatTerrain_{chunkIndex.x}_{chunkIndex.y}");
            terrainObj.transform.parent = parent;
            terrainObj.transform.localPosition = Vector3.zero;

            Mesh mesh = new Mesh();
            mesh.name = "FlatTerrainMesh";

            mesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(chunkSize, 0, 0),
                new Vector3(0, 0, chunkSize),
                new Vector3(chunkSize, 0, chunkSize)
            };

            mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            mesh.RecalculateNormals();

            MeshFilter meshFilter = terrainObj.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = terrainObj.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.color = Color.green;

            if (generateCollider)
            {
                MeshCollider meshCollider = terrainObj.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
            }

            return terrainObj;
        }
    }
}
