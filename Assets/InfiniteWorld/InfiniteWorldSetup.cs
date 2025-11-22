using UnityEngine;

namespace InfiniteWorld
{
    public class InfiniteWorldSetup : MonoBehaviour
    {
        [Header("Setup Configuration")]
        [SerializeField] private int worldSeed = 12345;
        [SerializeField] private float chunkSize = 50f;
        [SerializeField] private int viewDistance = 2;
        [SerializeField] private float floatingOriginThreshold = 1000f;

        [Header("Sample Prefabs (Optional)")]
        [SerializeField] private GameObject[] sampleTerrainPrefabs;
        [SerializeField] private GameObject[] sampleObjectPrefabs;

        [ContextMenu("Setup Infinite World")]
        public void SetupWorld()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = CreateDefaultPlayer();
            }

            GameObject worldSystem = new GameObject("WorldSystem");

            WorldGenerator worldGen = worldSystem.AddComponent<WorldGenerator>();

            ChunkManager chunkMgr = worldSystem.AddComponent<ChunkManager>();
            typeof(ChunkManager).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(chunkMgr, player.transform);
            typeof(ChunkManager).GetField("worldGenerator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(chunkMgr, worldGen);

            FloatingOrigin floatingOrigin = worldSystem.AddComponent<FloatingOrigin>();
            typeof(FloatingOrigin).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(floatingOrigin, player.transform);
            typeof(FloatingOrigin).GetField("chunkManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(floatingOrigin, chunkMgr);

            if (player.GetComponent<PlayerController>() == null)
            {
                PlayerController controller = player.AddComponent<PlayerController>();
                typeof(PlayerController).GetField("floatingOrigin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(controller, floatingOrigin);
                typeof(PlayerController).GetField("chunkManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(controller, chunkMgr);
            }

            Debug.Log("Infinite World System Setup Complete!");
            Debug.Log("Player: " + player.name);
            Debug.Log("World System: " + worldSystem.name);
        }

        private GameObject CreateDefaultPlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0, 2, 0);

            // Add CharacterController
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.radius = 0.5f;
            cc.height = 2f;
            cc.center = new Vector3(0, 1, 0);

            GameObject cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.parent = player.transform;
            cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);

            Camera cam = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";

            Debug.Log("Created default player with camera");
            return player;
        }
    }
}
