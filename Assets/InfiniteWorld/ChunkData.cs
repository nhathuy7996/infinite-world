using UnityEngine;
using System.Collections.Generic;

namespace InfiniteWorld
{
    public class ChunkData
    {
        public Vector2Int chunkIndex;
        public GameObject chunkObject;
        public bool isLoaded;
        public List<GameObject> objects = new List<GameObject>();

        public ChunkData(Vector2Int index)
        {
            this.chunkIndex = index;
            this.isLoaded = false;
        }

        public void Unload()
        {
            if (chunkObject != null)
            {
                Object.Destroy(chunkObject);
                chunkObject = null;
            }

            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }
            objects.Clear();
            isLoaded = false;
        }
    }
}
