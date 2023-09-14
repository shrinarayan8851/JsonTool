using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[CustomEditor(typeof(JsonCreator))]
public class JsonInspector : Editor
{
    private string jsonFilePath = "Assets/UIHierarchy.json";
    private string jsonData = "{\"elements\":[]}";
    private Vector2 scrollPosition;

    private UIElementList templateList;
    private List<UIElementList> template;
    private int selectedTemplateIndex = 0;

    // Fields for creating a new template
    private string templateName = "New Template";
    private Vector3 templatePosition = Vector3.zero;
    private Vector3 templateRotation = Vector3.zero;
    private Vector3 templateScale = Vector3.one;
    private Color templateColor = Color.white;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        JsonCreator myScript = (JsonCreator)target;

        // UI to load JSON data
        if (GUILayout.Button("Load JSON"))
        {
            LoadJSON();
        }

        // Display JSON data
        EditorGUILayout.LabelField("JSON Data", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
        jsonData = EditorGUILayout.TextArea(jsonData, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        // UI to create a new template
        EditorGUILayout.LabelField("Create New Template", EditorStyles.boldLabel);
        templateName = EditorGUILayout.TextField("Name", templateName);
        templatePosition = EditorGUILayout.Vector3Field("Position", templatePosition);
        templateRotation = EditorGUILayout.Vector3Field("Rotation", templateRotation);
        templateScale = EditorGUILayout.Vector3Field("Scale", templateScale);
        templateColor = EditorGUILayout.ColorField("Color", templateColor);


        if (GUILayout.Button("Create New Template"))
        {
            CreateNewTemplate();
        }

        // UI to edit and save JSON data
        if (GUILayout.Button("Save JSON"))
        {
            SaveJSON();
        }

        // UI to select and instantiate a template
        if (templateList != null && templateList.elements != null && templateList.elements.Count > 0)
        {
            EditorGUILayout.LabelField("Instantiate Template", EditorStyles.boldLabel);
            string[] templateNames = templateList.elements.ConvertAll(element => element.name).ToArray();
            selectedTemplateIndex = EditorGUILayout.Popup("Select Template", selectedTemplateIndex, templateNames);
            if (GUILayout.Button("Instantiate Template"))
            {
                InstantiateTemplate(templateList.elements[selectedTemplateIndex]);
            }
        }


        if(GUILayout.Button("Instantiate Preload Json"))
        {
            UIElement rootElement = JsonUtility.FromJson<UIElement>(jsonData);
            LoadPreLoadJsonData(rootElement, null);
        }
    }

    private void LoadPreLoadJsonData(UIElement element, Transform parentTransform)
    {
        GameObject newGameObject;

        // If the parentTransform is null, create a new Canvas
        if (parentTransform == null)
        {
            newGameObject = new GameObject(element.name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = newGameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
        }
        else
        {
            // Create a new UI GameObject (e.g., an Image)
            newGameObject = new GameObject(element.name, typeof(Image));
            Image image = newGameObject.GetComponent<Image>();

            // Set the color of the Image component
            if (element.color != null)
            {
                image.color = new Color(element.color.r, element.color.g, element.color.b, element.color.a);
            }
        }

        // Set the parent of the new GameObject
        newGameObject.transform.SetParent(parentTransform);

        // Set the properties of the new GameObject
        newGameObject.transform.localPosition = new Vector3(element.position.x, element.position.y, element.position.z);
        newGameObject.transform.localEulerAngles = new Vector3(element.rotation.x, element.rotation.y, element.rotation.z);
        newGameObject.transform.localScale = new Vector3(element.scale.x, element.scale.y, element.scale.z);

        // Add the components specified in the JSON data
        foreach (string componentName in element.componentNames)
        {
            Type componentType = Type.GetType(componentName);
            if (componentType != null)
            {
                newGameObject.AddComponent(componentType);
            }
        }

        // Recursively instantiate the child GameObjects
        foreach (UIElement childElement in element.components)
        {
            LoadPreLoadJsonData(childElement, newGameObject.transform);
        }
    }

    private void LoadPreLoadJsonDataNonUIElement(UIElement element, Transform parentTransform   )
    {
        // Create a new GameObject with the name from the JSON data
        GameObject newGameObject = new GameObject(element.name);

        // Set the parent of the new GameObject
        newGameObject.transform.SetParent(parentTransform);

        // Set the properties of the new GameObject
        newGameObject.transform.localPosition = new Vector3(element.position.x, element.position.y, element.position.z);
        newGameObject.transform.localEulerAngles = new Vector3(element.rotation.x, element.rotation.y, element.rotation.z);
        newGameObject.transform.localScale = new Vector3(element.scale.x, element.scale.y, element.scale.z);

        // If the element has a color property, add a Renderer component and set the color
        if (element.color != null)
        {
            MeshRenderer renderer = newGameObject.AddComponent<MeshRenderer>();
            if (renderer.sharedMaterial == null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            }
            renderer.sharedMaterial.color = new Color(element.color.r, element.color.g, element.color.b, element.color.a);
        }


        // Add the components specified in the JSON data
        foreach (string componentName in element.componentNames)
        {
            Type componentType = Type.GetType(componentName);
            if (componentType != null)
            {
                newGameObject.AddComponent(componentType);
            }
        }

        // Recursively instantiate the child GameObjects
        foreach (UIElement childElement in element.components)
        {
            LoadPreLoadJsonData(childElement, newGameObject.transform);
        }

    }

    private void LoadJSON()
    {
        try
        {
            if (File.Exists(jsonFilePath))
            {
                jsonData = File.ReadAllText(jsonFilePath);
                templateList = JsonUtility.FromJson<UIElementList>(jsonData);
            }
            else
            {
                Debug.LogError("JSON file not found at " + jsonFilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while loading JSON data: " + e.Message);
        }
    }

    private void SaveJSON()
    {
        try
        {
            File.WriteAllText(jsonFilePath, jsonData);
            Debug.Log("JSON data saved to " + jsonFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred while saving JSON data: " + e.Message);
        }
    }

    private void CreateNewTemplate()
    {
        UIElement newTemplate = UIElement.CreateNew(templateName, templatePosition, templateRotation, templateScale, templateColor);

        // Parse the existing JSON data into a UIElementList object
        if (templateList == null)
        {
            templateList = new UIElementList { elements = new List<UIElement>() };
        }

        // Add the new template to the list
        templateList.elements.Add(newTemplate);

        // Convert the list back to JSON
        jsonData = JsonUtility.ToJson(templateList, true);
    }

    private void InstantiateTemplate(UIElement template)
    {
        GameObject canvas = new GameObject("Canvas");
        canvas.AddComponent<Canvas>();
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;

        CreateUIHierarchy(template, canvas.transform);
    }

    private void CreateUIHierarchy(UIElement element, Transform parentTransform)
    {
        GameObject newGameObject = new GameObject(element.name);
        newGameObject.transform.SetParent(parentTransform);
        newGameObject.transform.localPosition = element.position;
        newGameObject.transform.localEulerAngles = element.rotation;
        newGameObject.transform.localScale = element.scale;

        // Set color if a renderer is available
        Renderer renderer = newGameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = element.color;
        }

        // Create child elements recursively
        foreach (UIElement childElement in element.components)
        {
            CreateUIHierarchy(childElement, newGameObject.transform);
        }
    }


   

}
