using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public static class LeverExperimentSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/LeverExperiment.unity";

    [MenuItem("Tools/Physics/Create Lever Experiment Scene")]
    public static void CreateScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("LeverExperiment");
        LeverExperiment experiment = root.AddComponent<LeverExperiment>();
        LeverController controller = root.AddComponent<LeverController>();

        Camera camera = new GameObject("Experiment Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 3.6f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.backgroundColor = new Color(.025f, .045f, .08f);

        Transform fulcrum = CreatePrimitive("Fulcrum", PrimitiveType.Cylinder, new Vector3(0f, -0.25f, 0f), new Vector3(.35f, .25f, .35f), new Color(.95f, .75f, .18f));
        Transform beam = CreatePrimitive("Lever Beam", PrimitiveType.Cube, new Vector3(0f, 0f, 0f), new Vector3(5.4f, .16f, .22f), new Color(.20f, .75f, .92f));
        Transform leftWeight = CreatePrimitive("Left Weight", PrimitiveType.Cube, Vector3.zero, new Vector3(.42f, .42f, .42f), new Color(.95f, .32f, .18f));
        Transform rightWeight = CreatePrimitive("Right Weight", PrimitiveType.Cube, Vector3.zero, new Vector3(.42f, .42f, .42f), new Color(.35f, .95f, .45f));

        LeverView view = root.AddComponent<LeverView>();
        SetReference(view, "leverBeam", beam);
        SetReference(view, "fulcrum", fulcrum);
        SetReference(view, "leftWeight", leftWeight);
        SetReference(view, "rightWeight", rightWeight);

        ForceVector leftForce = CreateForce("Left Force", new Color(1f, .3f, .2f));
        ForceVector rightForce = CreateForce("Right Force", new Color(.3f, 1f, .4f));

        Canvas canvas = new GameObject("Lever Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1600f, 900f);
        canvas.gameObject.AddComponent<GraphicRaycaster>();
        GameObject panel = CreatePanel(canvas.transform);
        TMP_Text readout = CreateText(panel.transform, "Readout", "THÍ NGHIỆM ĐÒN BẨY", 22, new Vector2(.06f, .62f), new Vector2(.94f, .98f));
        Slider leftMass = CreateSlider(panel.transform, "Khối lượng trái", .54f, .59f, .05f, 1f, .2f);
        Slider rightMass = CreateSlider(panel.transform, "Khối lượng phải", .45f, .50f, .05f, 1f, .1f);
        Slider leftDistance = CreateSlider(panel.transform, "Khoảng cách trái", .36f, .41f, .2f, 1.3f, .5f);
        Slider rightDistance = CreateSlider(panel.transform, "Khoảng cách phải", .27f, .32f, .2f, 1.3f, 1f);
        Button forcesButton = CreateButton(panel.transform, "Hiện / ẩn lực", .06f, .16f);
        Button rulerButton = CreateButton(panel.transform, "Hiện / ẩn thước", .52f, .16f);
        Button resetButton = CreateButton(panel.transform, "ĐẶT LẠI", .06f, .07f);
        CreateText(panel.transform, "Instructions", "Kéo vật nặng dọc theo thanh để thay đổi khoảng cách.\nMô phỏng dùng F = m × g và M = F × d.", 13, new Vector2(.06f, .01f), new Vector2(.94f, .055f));

        LeverUI ui = root.AddComponent<LeverUI>();
        SetReference(ui, "readoutText", readout);
        SetReference(ui, "leftMassSlider", leftMass);
        SetReference(ui, "rightMassSlider", rightMass);
        SetReference(ui, "leftDistanceSlider", leftDistance);
        SetReference(ui, "rightDistanceSlider", rightDistance);
        SetReference(ui, "toggleForcesButton", forcesButton);
        SetReference(ui, "toggleRulerButton", rulerButton);
        SetReference(ui, "resetButton", resetButton);

        SetReference(experiment, "view", view);
        SetReference(experiment, "ui", ui);
        SetReference(experiment, "leftForceVector", leftForce);
        SetReference(experiment, "rightForceVector", rightForce);
        SetReference(controller, "experiment", experiment);
        SetReference(controller, "experimentCamera", camera);

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject events = new GameObject("EventSystem");
            events.AddComponent<EventSystem>();
            events.AddComponent<InputSystemUIInputModule>();
        }

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        Selection.activeGameObject = root;
        Debug.Log("Created and configured " + ScenePath);
    }

    private static Transform CreatePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name; go.transform.position = position; go.transform.localScale = scale;
        go.GetComponent<Renderer>().material = CreateMaterial(color);
        return go.transform;
    }

    private static ForceVector CreateForce(string name, Color color)
    {
        ForceVector force = new GameObject(name).AddComponent<ForceVector>();
        force.Build(color);
        return force;
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject("Lever Control Panel");
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(.70f, .04f); rect.anchorMax = new Vector2(.98f, .96f); rect.offsetMin = rect.offsetMax = Vector2.zero;
        Image image = panel.AddComponent<Image>(); image.color = new Color(.035f, .07f, .12f, .95f);
        return panel;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float size, Vector2 min, Vector2 max)
    {
        GameObject go = new GameObject(name); go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>(); text.text = value; text.fontSize = size; text.color = Color.white; text.enableWordWrapping = true;
        text.alignment = TextAlignmentOptions.Left;
        return text;
    }

    private static Button CreateButton(Transform parent, string label, float x, float y)
    {
        GameObject go = new GameObject(label); go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>(); rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(x + .42f, y + .065f); rect.offsetMin = rect.offsetMax = Vector2.zero;
        Image image = go.AddComponent<Image>(); image.color = new Color(.08f, .34f, .55f);
        Button button = go.AddComponent<Button>();
        TMP_Text text = CreateText(go.transform, "Label", label, 14, Vector2.zero, Vector2.one); text.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private static Slider CreateSlider(Transform parent, string label, float y, float maxY, float min, float max, float value)
    {
        CreateText(parent, label, label, 13, new Vector2(.06f, y + .015f), new Vector2(.46f, maxY));
        GameObject go = new GameObject(label + " Slider"); go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>(); rect.anchorMin = new Vector2(.48f, y + .01f); rect.anchorMax = new Vector2(.94f, maxY); rect.offsetMin = rect.offsetMax = Vector2.zero;
        Slider slider = go.AddComponent<Slider>(); slider.minValue = min; slider.maxValue = max; slider.value = value;
        Image background = new GameObject("Background").AddComponent<Image>(); background.transform.SetParent(go.transform, false); background.color = new Color(.15f, .2f, .25f); Stretch(background.rectTransform);
        Image fill = new GameObject("Fill").AddComponent<Image>(); fill.transform.SetParent(go.transform, false); fill.color = new Color(.2f, .7f, .9f); Stretch(fill.rectTransform); slider.fillRect = fill.rectTransform;
        Image handle = new GameObject("Handle").AddComponent<Image>(); handle.transform.SetParent(go.transform, false); handle.color = Color.white; handle.rectTransform.anchorMin = new Vector2(0f, .5f); handle.rectTransform.anchorMax = new Vector2(0f, .5f); handle.rectTransform.sizeDelta = new Vector2(18f, 28f); slider.handleRect = handle.rectTransform; slider.targetGraphic = handle;
        return slider;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static Material CreateMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.color = color; return material;
    }

    private static void SetReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) Debug.LogWarning($"Could not find {propertyName} on {target.name}");
        else { property.objectReferenceValue = value; serialized.ApplyModifiedPropertiesWithoutUndo(); }
    }

    private static void AddToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene scene in scenes) if (scene.path == scenePath) return;
        scenes.Add(new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray();
    }
}
