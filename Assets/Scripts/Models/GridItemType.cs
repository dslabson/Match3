using UnityEngine;
using System;

[Serializable]
public class ItemType
{
    private static int usedId = 0;

    [HideInInspector]
    [SerializeField]
    private int id;
    public int Id { get { return id; } }

    [SerializeField]
    private string name;
    public string Name { get { return name; } }

    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite { get { return sprite; } }

    [SerializeField]
    private Color color;
    public Color Color { get { return color; } }


    public ItemType(string name, Sprite sprite, Color color = default(Color))
    {
        id = usedId++;
        this.name = name;
        this.sprite = sprite;
        this.color = color;
    }


    public override string ToString()
    {
        return string.Format("[ID: {0}][Name: {1}][Color: {2}]", Id, Name, Color.ToString());
    }
}