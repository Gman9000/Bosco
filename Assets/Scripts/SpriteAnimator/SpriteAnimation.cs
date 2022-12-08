using UnityEngine;

/// <summary>
/// Data class that stores an individual animation to use in the SpriteAnim component.
/// </summary>
[System.Serializable]
public class SpriteAnimation 
{

    /// <summary>
    /// The name identifier used to play the animation.
    /// </summary>
    public string name;
    /// <summary>
    /// The spritesheet used for this animation.
    /// </summary>
    public Texture2D sheet;
    /// <summary>
    /// The delay between each frame.
    /// </summary>
    public float delay = .08F;
    /// <summary>
    /// The starting frame.
    /// </summary>
    public int startFrame = 0;
    /// <summary>
    /// The final frame. Make this below zero as shorthand for the entire sheet's length.
    /// </summary>
    public int endFrame = -1;
    /// <summary>
    /// The size of an individual frame. Leave this empty to use the default setting.
    /// </summary>
    public Vector2Int offset = Vector2Int.zero;
    /// <summary>
    /// The size of an individual frame. Leave this empty to use the default setting.
    /// </summary>
    public Vector2Int celSize = Vector2Int.zero;
}