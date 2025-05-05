using FishNet.Managing.Scened;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace FishNet.Insthync.AddressableAsset
{
    public class AddressableSceneProcessor : DefaultSceneProcessor
    {
        private AsyncOperationHandle<SceneInstance> _currentAddressableAsyncOp;
        private readonly List<AsyncOperationHandle<SceneInstance>> _loadingAsyncOps = new List<AsyncOperationHandle<SceneInstance>>();
        private readonly Dictionary<int, AsyncOperationHandle<SceneInstance>> _loadedAddressableScenesByHandle = new Dictionary<int, AsyncOperationHandle<SceneInstance>>();

        private static bool IsSceneInBuild(string sceneName)
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

            for (int i = 0; i < sceneCount; ++i)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public override void LoadStart(LoadQueueData queueData)
        {
            base.LoadStart(queueData);
            ResetAddressableValues();
        }

        public override void LoadEnd(LoadQueueData queueData)
        {
            base.LoadEnd(queueData);
            ResetAddressableValues();
        }

        private void ResetAddressableValues()
        {
            _currentAddressableAsyncOp = default;
            _loadingAsyncOps.Clear();
        }

        public override void ActivateLoadedScenes()
        {
            base.ActivateLoadedScenes();
            foreach (var loadingAsyncOp in _loadingAsyncOps)
            {
                loadingAsyncOp.Result.ActivateAsync();
            }
        }

        public override void BeginLoadAsync(string sceneName, LoadSceneParameters parameters)
        {
            if (IsSceneInBuild(sceneName))
            {
                base.BeginLoadAsync(sceneName, parameters);
                return;
            }
            // Determine that the `sceneName` is adressable key
            var newOp = Addressables.LoadSceneAsync(sceneName, parameters, false);
            _loadingAsyncOps.Add(newOp);
            _currentAddressableAsyncOp = newOp;
        }

        public override void BeginUnloadAsync(Scene scene)
        {
            if (!_loadedAddressableScenesByHandle.TryGetValue(scene.handle, out var loadHandle))
            {
                // Scene is not loaded by addressable asset system?
                base.BeginUnloadAsync(scene);
                return;
            }
            // Scene is loaded by addressable asset system
            var unloadHandle = Addressables.UnloadSceneAsync(loadHandle, false);
            _currentAddressableAsyncOp = unloadHandle;
            _loadedAddressableScenesByHandle.Remove(scene.handle);
            Scenes.Remove(scene);
        }

        public override bool IsPercentComplete()
        {
            if (CurrentAsyncOperation != null)
            {
                return CurrentAsyncOperation.progress >= 0.9f;
            }
            else if (_currentAddressableAsyncOp.IsValid())
            {
                bool isDone = _currentAddressableAsyncOp.IsDone;
                if (isDone)
                {
                    Scene scene = _currentAddressableAsyncOp.Result.Scene;
                    if (_loadedAddressableScenesByHandle.TryAdd(scene.handle, _currentAddressableAsyncOp))
                        Scenes.Add(scene);
                }
                return isDone;
            }
            return false;
        }

        public override float GetPercentComplete()
        {
            if (CurrentAsyncOperation != null)
            {
                return CurrentAsyncOperation.progress;
            }
            else if (_currentAddressableAsyncOp.IsValid())
            {
                return _currentAddressableAsyncOp.PercentComplete;
            }
            return 1f;
        }

        public override IEnumerator AsyncsIsDone()
        {
            bool notDone;

            do
            {
                notDone = false;
                foreach (AsyncOperation ao in LoadingAsyncOperations)
                {

                    if (!ao.isDone)
                    {
                        notDone = true;
                        break;
                    }
                }
                yield return null;
            } while (notDone);

            do
            {
                notDone = false;
                foreach (var ao in _loadingAsyncOps)
                {

                    if (!ao.IsDone)
                    {
                        notDone = true;
                        break;
                    }
                }
                yield return null;
            } while (notDone);

            yield break;
        }
    }
}
