using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class UIElementList
{
    public List<UIElement> elements;
}

[System.Serializable]
public class UIElement
{
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public Color color;
    public List<string> componentNames = new List<string>();
    public List<UIElement> components = new List<UIElement>();

    public static UIElement CreateNew(string name, Vector3 position, Vector3 rotation, Vector3 scale, Color color)
    {
        UIElement newElement = new UIElement
        {
            name = name,
            position = position,
            rotation = rotation,
            scale = scale,
            color = color
        };
        return newElement;
    }
}

public class JsonCreator : MonoBehaviour
{
    public string jsonFilePath = "Assets/UIHierarchy.json";
    private UIElement rootElement;

    void Start()
    {
        rootElement = new UIElement();
        GenerateJSONData(transform, rootElement);
        string jsonData = JsonUtility.ToJson(rootElement, true);
        SaveJSON(jsonData);
    }

    void GenerateJSONData(Transform currentTransform, UIElement currentElement)
    {
        currentElement.name = currentTransform.name;
        currentElement.position = currentTransform.localPosition;
        currentElement.rotation = currentTransform.localEulerAngles;
        currentElement.scale = currentTransform.localScale;
        currentElement.color = Color.white;

        Component[] gameComponents = currentTransform.GetComponents<Component>();
        foreach (Component comp in gameComponents)
        {
            currentElement.componentNames.Add(comp.GetType().Name);
        }

        foreach (Transform child in currentTransform)
        {
            UIElement childElement = new UIElement();
            GenerateJSONData(child, childElement);
            currentElement.components.Add(childElement);
        }
    }

    void SaveJSON(string jsonData)
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



}
