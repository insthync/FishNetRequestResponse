using FishNet.Managing;
using FishNet.Managing.Object;
using FishNet.Object;
using GameKit.Dependencies.Utilities;
using Insthync.AddressableAssetTools;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FishNet.Insthync.AddressableAsset
{
    public class AddressablePrefabManager : MonoBehaviour
    {
        [SerializeField]
        private NetworkManager _networkManager;
        public NetworkManager NetworkManager => _networkManager;

        [SerializeField]
        private List<AssetReferenceNetworkObject> _assetReferences = new List<AssetReferenceNetworkObject>();
        public List<AssetReferenceNetworkObject> AssetReferences => _assetReferences;

        [SerializeField]
        private string _spawnableCollectionId = "AddressablePrefabs";

        private List<AssetReferenceNetworkObject> _loadedAssetReferences = new List<AssetReferenceNetworkObject>();

        private void Awake()
        {
            if (_networkManager == null)
                _networkManager = GetComponentInParent<NetworkManager>();
        }

        /// <summary>
        /// Load all prefabs by asset references list, may call this function before online
        /// </summary>
        /// <returns></returns>
        public async Task LoadAllPrefabs()
        {
            ushort id = _spawnableCollectionId.GetStableHashU16();
            SinglePrefabObjects spawnablePrefabs = (SinglePrefabObjects)_networkManager.GetPrefabObjects<SinglePrefabObjects>(id, true);
            List<Task<NetworkObject>> ops = new List<Task<NetworkObject>>();
            for (int i = 0; i < _assetReferences.Count; ++i)
            {
                if (_assetReferences[i].IsDataValid())
                {
                    ops.Add(LoadPrefab(spawnablePrefabs, _assetReferences[i]));
                }
            }
            await Task.WhenAll(ops);
        }

        private async Task<NetworkObject> LoadPrefab(SinglePrefabObjects spawnablePrefabs, AssetReferenceNetworkObject assetRef)
        {
            List<NetworkObject> prefabs = CollectionCaches<NetworkObject>.RetrieveList();
            NetworkObject prefab = await assetRef.GetOrLoadAssetAsync<NetworkObject>();
            prefabs.Add(prefab);
            spawnablePrefabs.AddObjects(prefabs);
            CollectionCaches<NetworkObject>.Store(prefabs);
            _loadedAssetReferences.Add(assetRef);
            return prefab;
        }

        /// <summary>
        /// Clear spawnable prefabs and release all prefabs by loaded asset references, may call this function after offline
        /// </summary>
        public void UnloadAllPrefabs()
        {
            ushort id = _spawnableCollectionId.GetStableHashU16();
            SinglePrefabObjects spawnablePrefabs = (SinglePrefabObjects)_networkManager.GetPrefabObjects<SinglePrefabObjects>(id, true);
            spawnablePrefabs.Clear();
            for (int i = 0; i < _loadedAssetReferences.Count; ++i)
            {
                if (_loadedAssetReferences[i].IsDataValid())
                    AddressableAssetsManager.Release(_loadedAssetReferences[i]);
            }
            _loadedAssetReferences.Clear();
        }
    }
}
