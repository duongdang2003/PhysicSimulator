using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SpringExperimentSceneBuilder
{
    [MenuItem("Tools/Physics/Build Spring Experiment Scene")]
    public static void Build()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Main Camera").AddComponent<Camera>().tag="MainCamera";
        var experiment = new GameObject("SpringExperiment").AddComponent<SpringExperiment>();
        experiment.SetupObjectsInScene();
        EditorUtility.SetDirty(experiment);
        AssetDatabase.CreateFolder("Assets/Scenes", "Generated");
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/SpringExperiment.unity");
        Debug.Log("Created Assets/Scenes/SpringExperiment.unity");
    }
}
