using UnityEngine;

namespace InfiniteWorld.Examples
{

    public class BiomeTerrainGenerator : ProceduralTerrainGenerator
    {
        [Header("Biome Settings")]
        [SerializeField] private BiomeData[] biomes;

        [System.Serializable]
        public class BiomeData
        {
            public string biomeName;
            public Color terrainColor;
            public float heightScale = 1f;
            public float noiseScale = 0.1f;
            [Range(0f, 1f)] public float threshold; // Ngưỡng noise để chọn biome
        }


        public GameObject GenerateBiomeTerrainChunk(Vector2Int chunkIndex, float chunkSize, Transform parent, int seed)
        {

            BiomeData biome = GetBiomeForChunk(chunkIndex, seed);

            if (biome == null)
            {

                return GenerateTerrainChunk(chunkIndex, chunkSize, parent);
            }


            GameObject terrainObj = new GameObject($"Terrain_{biome.biomeName}_{chunkIndex.x}_{chunkIndex.y}");
            terrainObj.transform.parent = parent;
            terrainObj.transform.localPosition = Vector3.zero;

            Debug.Log($"Chunk {chunkIndex} generated as {biome.biomeName}");

            return terrainObj;
        }


        private BiomeData GetBiomeForChunk(Vector2Int chunkIndex, int seed)
        {
            if (biomes == null || biomes.Length == 0)
                return null;


            System.Random rng = new System.Random(seed + chunkIndex.x * 1000 + chunkIndex.y);
            float biomeNoise = (float)rng.NextDouble();


            foreach (var biome in biomes)
            {
                if (biomeNoise <= biome.threshold)
                    return biome;
            }

            return biomes[biomes.Length - 1];
        }
    }


    public class CaveTerrainGenerator : ProceduralTerrainGenerator
    {
        [Header("Cave Settings")]
        [SerializeField] private float caveThreshold = 0.5f;
        [SerializeField] private float caveDepth = 5f;

        public GameObject GenerateCaveTerrain(Vector2Int chunkIndex, float chunkSize, Transform parent)
        {

            Debug.Log($"Generating terrain with caves at chunk {chunkIndex}");


            return GenerateTerrainChunk(chunkIndex, chunkSize, parent);
        }
    }


    public class WaterTerrainGenerator : ProceduralTerrainGenerator
    {
        [Header("Water Settings")]
        [SerializeField] private float waterLevel = 0f;
        [SerializeField] private Material waterMaterial;
        [SerializeField] private Color underwaterColor = new Color(0.2f, 0.4f, 0.6f);


        public GameObject GenerateWaterTerrain(Vector2Int chunkIndex, float chunkSize, Transform parent)
        {

            GameObject terrainObj = GenerateTerrainChunk(chunkIndex, chunkSize, parent);


            CreateWaterPlane(terrainObj.transform, chunkSize);

            return terrainObj;
        }

        private void CreateWaterPlane(Transform parent, float size)
        {
            GameObject waterObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterObj.name = "WaterPlane";
            waterObj.transform.parent = parent;
            waterObj.transform.localPosition = new Vector3(size * 0.5f, waterLevel, size * 0.5f);
            waterObj.transform.localScale = new Vector3(size * 0.1f, 1, size * 0.1f);

            if (waterMaterial != null)
            {
                waterObj.GetComponent<MeshRenderer>().material = waterMaterial;
            }
            else
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.5f, 0.8f, 0.7f);
                mat.SetFloat("_Mode", 3); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                waterObj.GetComponent<MeshRenderer>().material = mat;
            }
        }
    }
}
