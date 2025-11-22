using UnityEngine;

namespace InfiniteWorld
{
    [System.Serializable]
    public struct WorldCoordinate
    {
        public Vector2Int chunkIndex;
        public Vector2 localPosition;

        public WorldCoordinate(Vector2Int chunkIndex, Vector2 localPosition)
        {
            this.chunkIndex = chunkIndex;
            this.localPosition = localPosition;
        }

        public static WorldCoordinate FromWorldPosition(Vector3 worldPos, float chunkSize)
        {
            int chunkX = Mathf.FloorToInt(worldPos.x / chunkSize);
            int chunkZ = Mathf.FloorToInt(worldPos.z / chunkSize);

            float localX = worldPos.x - (chunkX * chunkSize);
            float localZ = worldPos.z - (chunkZ * chunkSize);

            return new WorldCoordinate(
                new Vector2Int(chunkX, chunkZ),
                new Vector2(localX, localZ)
            );
        }

        public Vector3 ToWorldPosition(float chunkSize)
        {
            return new Vector3(
                chunkIndex.x * chunkSize + localPosition.x,
                0,
                chunkIndex.y * chunkSize + localPosition.y
            );
        }

        public static int ChunkDistance(Vector2Int chunk1, Vector2Int chunk2)
        {
            return Mathf.Abs(chunk1.x - chunk2.x) + Mathf.Abs(chunk1.y - chunk2.y);
        }

        public override string ToString()
        {
            return $"Chunk({chunkIndex.x}, {chunkIndex.y}) Local({localPosition.x:F2}, {localPosition.y:F2})";
        }
    }
}
