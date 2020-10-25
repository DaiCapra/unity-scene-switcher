using UnityEditor;
using UnityEngine;

namespace SceneManagement.Code.Runtime
{
    [CreateAssetMenu(menuName = "Scenes/Info", fileName = "Scene_")]
    public class SceneInfo : ScriptableObject, ISerializationCallbackReceiver
    {
        [HideInInspector] public string sceneName;
#if UNITY_EDITOR
#pragma warning disable 0649
        [SerializeField] private SceneAsset scene;
#pragma warning restore 0649
#endif

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (scene == null)
            {
                return;
            }

            sceneName = scene.name;
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }
}