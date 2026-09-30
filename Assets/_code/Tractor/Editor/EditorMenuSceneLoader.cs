using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tractor.Editor
{
    public class EditorMenuSceneLoader : EditorWindow
    {
        static void LoadScene(string sceneName)
        {
            string path = "Assets/Scenes/";

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(path + sceneName + ".unity", OpenSceneMode.Single);
        }

        [MenuItem("Tractor/Открыть сцену/Main")]
        static void LoadSceneInit()
        {
            LoadScene("Main");
        }

        // Тестовые сцены

        [MenuItem("Tractor/Открыть сцену/Тест/Test Inputs")]
        static void LoadSceneTestInputs()
        {
            LoadScene("Test/Test Inputs");
        }

        [MenuItem("Tractor/Открыть сцену/Тест/Test PC Android Connection")]
        static void LoadSceneTestPCAndroidConnection()
        {
            LoadScene("Test/Test PC Android Connection");
        }

        [MenuItem("Tractor/Открыть сцену/Тест/Test UI")]
        static void LoadSceneTestUI()
        {
            LoadScene("Test/Test UI");
        }
    }
}
