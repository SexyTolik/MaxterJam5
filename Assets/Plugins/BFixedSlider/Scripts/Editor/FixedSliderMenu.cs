using UnityEditor;
using UnityEngine;

namespace Plugins.BFixedSlider.Scripts.Editor
{
    public static class FixedSliderMenu
    {
        private static void SpawnSlider(string prefabName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Plugins/BFixedSlider/Prefabs/{prefabName}.prefab");

            if (prefab == null)
            {
                Debug.LogError($"❌ Не найден префаб по пути Assets/Plugins/BFixedSlider/Prefabs/{prefabName}.prefab");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            Undo.RegisterCreatedObjectUndo(instance, "Spawn BFixedSlider");

            if (Selection.activeTransform != null)
            {
                instance.transform.SetParent(Selection.activeTransform, false);
            }
            
            Selection.activeObject = instance;
        }

        [MenuItem("GameObject/UI/BFixed Slider/DefaultSlider", false, 10)]
        private static void SpawnDefaultSlider()
        {
            SpawnSlider("DefaultSlider");
        }
        [MenuItem("GameObject/UI/BFixed Slider/NameSlider", false, 10)]
        private static void SpawnNameSlider()
        {
            SpawnSlider("NameSlider");
        }
        [MenuItem("GameObject/UI/BFixed Slider/IconSlider", false, 10)]
        private static void SpawnImageSlider()
        {
            SpawnSlider("ImageSlider");
        }
    }
}