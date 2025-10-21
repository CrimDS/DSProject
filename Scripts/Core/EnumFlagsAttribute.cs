using UnityEngine;

/// <summary>
/// A custom attribute that allows a System.Flags enum to be displayed
/// as a multi-select dropdown in the Unity Inspector.
/// </summary>
public class EnumFlagsAttribute : PropertyAttribute
{
    public EnumFlagsAttribute() { }
}
