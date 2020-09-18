using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Runtime;
using NUnit.Framework;
using Plugins.AsyncAwaitUtil.Source;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Code.Tests.Runtime
{
    public class TestScene
    {
        private string _sceneTransition = "Test_SceneTransition";
        private string _sceneA = "Test_SceneA";
        private string _sceneB = "Test_SceneB";
        private string _sceneC = "Test_SceneC";
        private string _sceneD = "Test_SceneD";
        private Task _task;
        private SceneService _service;

        [SetUp]
        public void Setup()
        {
            _service = new SceneService();
            _service.RegisterTransitionScene(_sceneTransition);
        }

        [UnityTest]
        public IEnumerator LoadUnload()
        {
            _service.Enqueue(_sceneA);
            while (!_service.IsComplete)
            {
                yield return new WaitForEndOfFrame();
            }

            Assert.True(_service.IsSceneLoaded(_sceneA));
            Assert.False(_service.IsSceneLoaded(_sceneB));
            Assert.False(_service.IsSceneLoaded(_sceneTransition));

            _service.Enqueue(_sceneB);
            while (!_service.IsComplete)
            {
                yield return new WaitForEndOfFrame();
            }

            Assert.True(_service.IsSceneLoaded(_sceneA));
            Assert.True(_service.IsSceneLoaded(_sceneB));

            _service.Enqueue(_sceneA, WorkType.Unload);
            while (!_service.IsComplete)
            {
                yield return new WaitForEndOfFrame();
            }

            Assert.False(_service.IsSceneLoaded(_sceneA));
            Assert.True(_service.IsSceneLoaded(_sceneB));
            _service.Enqueue(_sceneB, WorkType.Unload);
            while (!_service.IsComplete)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        [UnityTest]
        public IEnumerator Invalid()
        {
            _service.Enqueue("");
            while (!_service.IsComplete)
            {
                yield return new WaitForEndOfFrame();
            }

            _service.Enqueue(_sceneA);
            while (!_service.IsComplete)
            {
                yield return new WaitForEndOfFrame();
            }

            Assert.True(_service.IsSceneLoaded(_sceneA));
            _service.Enqueue(_sceneA, WorkType.Unload);
            while (!_service.IsComplete)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        [UnityTest]
        public IEnumerator LoadSingle()
        {
            _service.Enqueue(_sceneA);
            _service.Enqueue(_sceneB);
            _service.Enqueue(_sceneC);
            while (!_service.IsComplete)
            {
                yield return new WaitForEndOfFrame();
            }

            Assert.True(_service.IsSceneLoaded(_sceneA));
            Assert.True(_service.IsSceneLoaded(_sceneB));
            Assert.True(_service.IsSceneLoaded(_sceneC));
            Assert.False(_service.IsSceneLoaded(_sceneD));
            
            _service.Enqueue(_sceneD, WorkType.Load, LoadSceneMode.Single);
            while (!_service.IsComplete)
            {
                yield return new WaitForEndOfFrame();
            }
            Assert.False(_service.IsSceneLoaded(_sceneA));
            Assert.False(_service.IsSceneLoaded(_sceneB));
            Assert.False(_service.IsSceneLoaded(_sceneC));
            Assert.True(_service.IsSceneLoaded(_sceneD));
            
        }

        [UnityTest]
        public IEnumerator Stress()
        {
            int testSize = 100;
            Random.InitState(42);
            var list = new List<string>()
            {
                _sceneA,
                _sceneB,
                _sceneC,
                _sceneD,
            };

            var prev = "";
            for (int i = 0; i < testSize; i++)
            {
                var r = Random.Range(0, list.Count - 1);
                var e = list[r];

                _service.Enqueue(prev, WorkType.Unload);
                prev = e;

                while (!_service.IsComplete)
                {
                    yield return new WaitForEndOfFrame();
                }

                Assert.False(_service.IsSceneLoaded(prev));

                _service.Enqueue(e);
                while (!_service.IsComplete)
                {
                    yield return new WaitForEndOfFrame();
                }

                Assert.True(_service.IsSceneLoaded(e));
            }

            yield return new WaitForEndOfFrame();
        }

        private async Task WaitForTask()
        {
            while (!_task.IsCompleted)
            {
                await new WaitForEndOfFrame();
            }
        }
    }
}