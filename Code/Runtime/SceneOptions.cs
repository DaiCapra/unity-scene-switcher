using System;
using UnityEngine.SceneManagement;

namespace SceneManagement.Code.Runtime
{
    public class SceneOptions
    {
        public bool Additive;
        public bool Async;
        public Action Callback;
        public bool UseLoadingScreen;
    }
}