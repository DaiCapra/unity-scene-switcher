using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement.Code.Runtime
{
    public class SceneController
    {
        public bool IsWorking => _isWorking;
        private readonly List<SceneData> _queue;
        private bool _isWorking;

        private SceneInfo _loadingScreen;

        public SceneController()
        {
            _queue = new List<SceneData>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }


        public void Load(SceneInfo info, SceneOptions options = null)
        {
            if (info == null)
            {
                return;
            }

            var data = GetData(info, options);
            data.Load = true;


            var index = _queue.Count;
            if (options != null && options.UseLoadingScreen && _loadingScreen != null)
            {
                var loading = new SceneData()
                {
                    Info = _loadingScreen,
                    Load = true,
                    IsLoadingScreen = true,
                };
            }

            _queue.Add(data);
        }

        public void RegisterLoadingScene(SceneInfo sceneInfo)
        {
            if (sceneInfo == null)
            {
                return;
            }

            _loadingScreen = sceneInfo;
        }

        public void Tick()
        {
            if (_isWorking)
            {
                return;
            }

            if (IsEmpty())
            {
                return;
            }

            Work();
        }

        private void Work()
        {
            var data = _queue[0];
            var async = data.Options?.Async ?? true;
            var loadMode = LoadSceneMode.Additive;
            if (data.Options != null)
            {
                loadMode = data.Options.Additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            }

            try
            {
                if (data.Load)
                {
                    if (async)
                    {
                        SceneManager.LoadSceneAsync(data.Info.sceneName, loadMode);
                    }
                    else
                    {
                        SceneManager.LoadScene(data.Info.sceneName, loadMode);
                    }
                }
                else
                {
                    SceneManager.UnloadSceneAsync(data.Info.sceneName);
                }

                _isWorking = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                _queue.RemoveAt(0);
            }
        }


        public void Unload(SceneInfo info, SceneOptions options = null)
        {
            if (info == null)
            {
                return;
            }

            var data = GetData(info, options);
            data.Load = false;

            _queue.Add(data);
        }

        private void CompleteWork(Scene scene)
        {
            if (IsEmpty())
            {
                return;
            }

            var data = _queue[0];
            if (data.Info.sceneName.Equals(scene.name, StringComparison.InvariantCultureIgnoreCase))
            {
                _queue.RemoveAt(0);
                _isWorking = false;
                data.Options?.Callback?.Invoke();
            }
        }

        private SceneData GetData(SceneInfo info, SceneOptions options)
        {
            var data = new SceneData()
            {
                Info = info,
                Options = options,
            };
            return data;
        }

        private bool IsEmpty()
        {
            return _queue.Count <= 0;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (!_isWorking)
            {
                return;
            }

            CompleteWork(scene);
        }


        private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            if (!_isWorking)
            {
                return;
            }

            CompleteWork(scene);
        }
    }
}