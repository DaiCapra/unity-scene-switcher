using SceneManagement.Code.Runtime;
using UnityEngine;

namespace SceneManagement.Code.Example
{
    public class SceneExample : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private SceneInfo loadingScreen;
        [SerializeField] private SceneInfo scene;
#pragma warning restore 0649

        void Start()
        {
            var sm = new SceneController();
            sm.Load(scene, new SceneOptions()
            {
                Additive = true,
                Async = true,
                UseLoadingScreen = true
            });


            sm.Tick();
        }
    }
}