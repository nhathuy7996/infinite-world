using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace InfiniteWorld
{
    public class ChunkManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private WorldGenerator worldGenerator;

        [Header("Chunk Settings")]
        [SerializeField] private float chunkSize = 50f;
        [SerializeField] private int viewDistance = 2;
        [SerializeField] private float updateInterval = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color loadedChunkColor = Color.green;
        [SerializeField] private Color activeRangeColor = Color.yellow;

        private Dictionary<Vector2Int, ChunkData> loadedChunks = new Dictionary<Vector2Int, ChunkData>();

        private Vector2Int currentPlayerChunk;
        private float updateTimer;

        private void Start()
        {
            if (player == null)
            {
                Debug.LogError("Player reference is not set in ChunkManager!");
                enabled = false;
                return;
            }

            if (worldGenerator == null)
            {
                worldGenerator = GetComponent<WorldGenerator>();
                if (worldGenerator == null)
                {
                    Debug.LogError("WorldGenerator not found!");
                    enabled = false;
                    return;
                }
            }

            UpdatePlayerChunk();
            LoadChunksAroundPlayer();
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;

            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateChunks();
            }
        }

        private void UpdateChunks()
        {
            Vector2Int newPlayerChunk = GetChunkIndexFromPosition(player.position);

            if (newPlayerChunk != currentPlayerChunk)
            {
                currentPlayerChunk = newPlayerChunk;
                LoadChunksAroundPlayer();
                UnloadDistantChunks();
            }
        }

        private void UpdatePlayerChunk()
        {
            currentPlayerChunk = GetChunkIndexFromPosition(player.position);
        }

        private Vector2Int GetChunkIndexFromPosition(Vector3 worldPosition)
        {
            int chunkX = Mathf.FloorToInt(worldPosition.x / chunkSize);
            int chunkZ = Mathf.FloorToInt(worldPosition.z / chunkSize);
            return new Vector2Int(chunkX, chunkZ);
        }

        private void LoadChunksAroundPlayer()
        {
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    Vector2Int chunkIndex = currentPlayerChunk + new Vector2Int(x, z);

                    if (!loadedChunks.ContainsKey(chunkIndex))
                    {
                        LoadChunk(chunkIndex);
                    }
                }
            }
        }

        private void LoadChunk(Vector2Int chunkIndex)
        {
            if (loadedChunks.ContainsKey(chunkIndex))
                return;

            ChunkData chunkData = worldGenerator.GenerateChunk(chunkIndex, chunkSize, transform);
            loadedChunks.Add(chunkIndex, chunkData);

            Debug.Log($"Loaded chunk {chunkIndex}");
        }

        private void UnloadDistantChunks()
        {
            List<Vector2Int> chunksToUnload = new List<Vector2Int>();

            foreach (var kvp in loadedChunks)
            {
                Vector2Int chunkIndex = kvp.Key;

                int distance = WorldCoordinate.ChunkDistance(chunkIndex, currentPlayerChunk);

                if (distance > viewDistance)
                {
                    chunksToUnload.Add(chunkIndex);
                }
            }

            foreach (var chunkIndex in chunksToUnload)
            {
                UnloadChunk(chunkIndex);
            }
        }

        private void UnloadChunk(Vector2Int chunkIndex)
        {
            if (!loadedChunks.ContainsKey(chunkIndex))
                return;

            ChunkData chunkData = loadedChunks[chunkIndex];
            chunkData.Unload();
            loadedChunks.Remove(chunkIndex);

            Debug.Log($"Unloaded chunk {chunkIndex}");
        }

        public ChunkData GetChunkAt(Vector2Int chunkIndex)
        {
            return loadedChunks.ContainsKey(chunkIndex) ? loadedChunks[chunkIndex] : null;
        }

        public Dictionary<Vector2Int, ChunkData> GetLoadedChunks()
        {
            return loadedChunks;
        }

        public Vector2Int GetPlayerChunkIndex()
        {
            return currentPlayerChunk;
        }

        public void ClearAllChunks()
        {
            foreach (var kvp in loadedChunks)
            {
                kvp.Value.Unload();
            }
            loadedChunks.Clear();
        }

        private void OnDestroy()
        {
            ClearAllChunks();
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || !Application.isPlaying)
                return;

            Gizmos.color = loadedChunkColor;
            foreach (var kvp in loadedChunks)
            {
                Vector3 center = new Vector3(
                    kvp.Key.x * chunkSize + chunkSize * 0.5f,
                    0,
                    kvp.Key.y * chunkSize + chunkSize * 0.5f
                );
                Gizmos.DrawWireCube(center, new Vector3(chunkSize, 1, chunkSize));
            }

            if (player != null)
            {
                Gizmos.color = activeRangeColor;
                Vector3 playerChunkCenter = new Vector3(
                    currentPlayerChunk.x * chunkSize + chunkSize * 0.5f,
                    0,
                    currentPlayerChunk.y * chunkSize + chunkSize * 0.5f
                );
                float rangeSize = (viewDistance * 2 + 1) * chunkSize;
                Gizmos.DrawWireCube(playerChunkCenter, new Vector3(rangeSize, 2, rangeSize));
            }
        }
    }
}
