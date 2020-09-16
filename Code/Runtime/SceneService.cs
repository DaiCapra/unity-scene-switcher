using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugins.AsyncAwaitUtil.Source;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Runtime
{
    public class SceneService
    {
        public bool HasTransitionScene { get; set; }
        public bool IsComplete => _task == null || _task.IsCompleted;
        private Task _task;
        private readonly List<SceneInfo> _queue;

        private bool _isWorking;

        private CancellationToken _cancellationToken;
        private bool _isWaitingForSceneEvent;

        private string _sceneTransition;

        public SceneService()
        {
            _queue = new List<SceneInfo>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public void Enqueue(string name, WorkType work = WorkType.Load, LoadSceneMode loadType = LoadSceneMode.Additive,
            bool useTransition = true)
        {
            if (!IsSceneValid(name))
            {
                return;
            }

            EnqueueScene(new SceneInfo()
            {
                Name = name,
                WorkType = work,
                LoadType = loadType,
            }, useTransition);
            Work();
        }

        private void Work()
        {
            if (!_isWorking)
            {
                _task = DoWork();
            }
        }

        private async Task DoWork()
        {
            _isWorking = true;
            bool isDone = false;
            while (!isDone && !_cancellationToken.IsCancellationRequested)
            {
                if (!_isWaitingForSceneEvent)
                {
                    if (_queue.Count == 0)
                    {
                        isDone = true;
                    }
                    else
                    {
                        var first = _queue[0];

                        switch (first.WorkType)
                        {
                            case WorkType.Load:
                                SceneManager.LoadSceneAsync(first.Name, first.LoadType);
                                break;
                            case WorkType.Unload:
                                SceneManager.UnloadSceneAsync(first.Name);
                                break;
                        }

                        _isWaitingForSceneEvent = true;
                    }
                }

                await new WaitForEndOfFrame();
            }

            _isWorking = false;
        }

        private void OnSceneEvent(Scene scene, WorkType workType)
        {
            if (_queue.Count <= 0)
            {
                return;
            }

            var first = _queue[0];
            if (!scene.name.Equals(first.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (first.WorkType == workType)
            {
                _isWaitingForSceneEvent = false;
                _queue.RemoveAt(0);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            OnSceneEvent(scene, WorkType.Load);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            OnSceneEvent(scene, WorkType.Unload);
        }


        private void EnqueueScene(SceneInfo sceneInfo, bool useTransition)
        {
            // Strip filepath if any
            sceneInfo.Name = Path.GetFileName(sceneInfo.Name);
            bool t = useTransition && HasTransitionScene;

            var info = GetTransitionInfo();

            if (t)
            {
                _queue.Add(info);
            }

            _queue.Add(sceneInfo);

            info = GetTransitionInfo();
            info.WorkType = WorkType.Unload;

            if (t)
            {
                _queue.Add(info);
            }
        }

        private SceneInfo GetTransitionInfo()
        {
            var info = new SceneInfo()
            {
                Name = _sceneTransition,
                WorkType = WorkType.Load,
                LoadType = LoadSceneMode.Additive
            };
            return info;
        }


        public void RegisterTransitionScene(string sceneName)
        {
            _sceneTransition = sceneName;
            HasTransitionScene = true;
        }

        public async Task AwaitWork()
        {
            while (_task.IsCompleted)
            {
                await new WaitForEndOfFrame();
            }
        }

        public bool IsSceneLoaded(string name)
        {
            return GetSceneByName(name).isLoaded;
        }

        private static Scene GetSceneByName(string name)
        {
            var scene = SceneManager.GetSceneByName(name);
            return GetScenesInBuildSettings().FirstOrDefault(t => t.Equals(scene));
        }

        private static bool IsSceneValid(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var scene = SceneManager.GetSceneByName(name);
            return GetScenesInBuildSettings().Any(t => t.Equals(scene));
        }

        private static List<Scene> GetScenesInBuildSettings()
        {
            var list = new List<Scene>();
            var l = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < l; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = Path.GetFileNameWithoutExtension(path);
                var scene = SceneManager.GetSceneByName(name);
                list.Add(scene);
            }

            return list;
        }
    }
}