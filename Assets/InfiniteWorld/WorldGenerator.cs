using UnityEngine;

namespace InfiniteWorld
{
    public class WorldGenerator : MonoBehaviour
    {
        [Header("World Settings")]
        [SerializeField] private int worldSeed = 12345;
        [SerializeField] private float noiseScale = 0.1f;
        [SerializeField] private float heightMultiplier = 10f;

        [Header("Terrain Generation")]
        [SerializeField] private bool useProceduralTerrain = true;
        [SerializeField] private GameObject[] terrainPrefabs;

        [Header("Chunk Content")]
        [SerializeField] private GameObject[] objectPrefabs;
        [SerializeField] private int objectsPerChunk = 5;
        [SerializeField] private float objectSpawnThreshold = 0.5f;

        private System.Random randomGenerator;
        private ProceduralTerrainGenerator terrainGenerator;

        private void Awake()
        {
            InitializeSeed(worldSeed);
            SetupTerrainGenerator();
        }

        private void SetupTerrainGenerator()
        {
            terrainGenerator = gameObject.GetComponent<ProceduralTerrainGenerator>();
            if (terrainGenerator == null && useProceduralTerrain)
            {
                terrainGenerator = gameObject.AddComponent<ProceduralTerrainGenerator>();
            }

            if (terrainGenerator != null)
            {
                terrainGenerator.Initialize(worldSeed);
            }
        }

        public void InitializeSeed(int seed)
        {
            worldSeed = seed;
            randomGenerator = new System.Random(seed);

            if (terrainGenerator != null)
            {
                terrainGenerator.Initialize(seed);
            }
        }

        public ChunkData GenerateChunk(Vector2Int chunkIndex, float chunkSize, Transform parent)
        {
            ChunkData chunkData = new ChunkData(chunkIndex);

            GameObject chunkObj = new GameObject($"Chunk_{chunkIndex.x}_{chunkIndex.y}");
            chunkObj.transform.parent = parent;
            chunkObj.transform.position = new Vector3(
                chunkIndex.x * chunkSize,
                0,
                chunkIndex.y * chunkSize
            );
            chunkData.chunkObject = chunkObj;

            GenerateTerrain(chunkData, chunkSize);

            GenerateObjects(chunkData, chunkSize);

            chunkData.isLoaded = true;
            return chunkData;
        }

        private void GenerateTerrain(ChunkData chunkData, float chunkSize)
        {
            Vector2Int chunkIndex = chunkData.chunkIndex;
            GameObject terrainObj = null;

            if (useProceduralTerrain && terrainGenerator != null)
            {
                terrainObj = terrainGenerator.GenerateTerrainChunk(
                    chunkIndex,
                    chunkSize,
                    chunkData.chunkObject.transform
                );
            }
            else if (terrainPrefabs != null && terrainPrefabs.Length > 0)
            {
                float worldX = chunkIndex.x * chunkSize;
                float worldZ = chunkIndex.y * chunkSize;

                float seedOffsetX = GetSeededOffset(worldSeed, 0);
                float seedOffsetZ = GetSeededOffset(worldSeed, 1);

                float noiseValue = Mathf.PerlinNoise(
                    (worldX + seedOffsetX) * noiseScale,
                    (worldZ + seedOffsetZ) * noiseScale
                );

                float height = noiseValue * heightMultiplier;

                int prefabIndex = GetSeededInt(chunkIndex, 0) % terrainPrefabs.Length;
                terrainObj = Instantiate(
                    terrainPrefabs[prefabIndex],
                    chunkData.chunkObject.transform
                );
                terrainObj.transform.localPosition = new Vector3(chunkSize * 0.5f, height, chunkSize * 0.5f);
            }

            if (terrainObj != null)
            {
                chunkData.objects.Add(terrainObj);
            }
        }

        private void GenerateObjects(ChunkData chunkData, float chunkSize)
        {
            if (objectPrefabs == null || objectPrefabs.Length == 0)
                return;

            Vector2Int chunkIndex = chunkData.chunkIndex;

            int objectCount = GetSeededInt(chunkIndex, 1) % (objectsPerChunk + 1);

            for (int i = 0; i < objectCount; i++)
            {
                float localX = GetSeededFloat(chunkIndex, i * 2) * chunkSize;
                float localZ = GetSeededFloat(chunkIndex, i * 2 + 1) * chunkSize;

                float spawnChance = GetSeededFloat(chunkIndex, i * 2 + 100);
                if (spawnChance < objectSpawnThreshold)
                    continue;

                int prefabIndex = GetSeededInt(chunkIndex, i + 10) % objectPrefabs.Length;

                float worldX = chunkIndex.x * chunkSize + localX;
                float worldZ = chunkIndex.y * chunkSize + localZ;
                float seedOffsetX = GetSeededOffset(worldSeed, 0);
                float seedOffsetZ = GetSeededOffset(worldSeed, 1);

                float noiseValue = Mathf.PerlinNoise(
                    (worldX + seedOffsetX) * noiseScale,
                    (worldZ + seedOffsetZ) * noiseScale
                );
                float height = noiseValue * heightMultiplier;

                GameObject obj = Instantiate(
                    objectPrefabs[prefabIndex],
                    chunkData.chunkObject.transform
                );
                obj.transform.localPosition = new Vector3(localX, height, localZ);

                float rotation = GetSeededFloat(chunkIndex, i + 200) * 360f;
                obj.transform.localRotation = Quaternion.Euler(0, rotation, 0);

                chunkData.objects.Add(obj);
            }
        }

        private int GetSeededInt(Vector2Int chunkIndex, int salt)
        {
            int hash = worldSeed;
            hash = hash * 31 + chunkIndex.x;
            hash = hash * 31 + chunkIndex.y;
            hash = hash * 31 + salt;
            return Mathf.Abs(hash);
        }

        private float GetSeededFloat(Vector2Int chunkIndex, int salt)
        {
            int intValue = GetSeededInt(chunkIndex, salt);
            return (intValue % 10000) / 10000f;
        }

        private float GetSeededOffset(int seed, int axis)
        {
            System.Random rng = new System.Random(seed + axis * 1000);
            return (float)rng.NextDouble() * 10000f;
        }

        public void SetSeed(int newSeed)
        {
            InitializeSeed(newSeed);
        }

        public int GetCurrentSeed()
        {
            return worldSeed;
        }
    }
}
