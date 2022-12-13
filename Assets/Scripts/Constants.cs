using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput
{
    public static bool HasPressedA()
    {
        return (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.C));
    }
    public static bool HasHeldA()
    {
        return (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.P) || Input.GetKey(KeyCode.C));
    }

    public static bool HasReleasedA()
    {
        return (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.C));
    }

    public static bool HasPressedB()
    {
        return (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.I) );
    }

    public static bool IsPressingUp()
    {
        return (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W));
    }
    public static bool IsPressingDown()
    {
        return (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S));
    }
    public static bool IsPressingLeft()
    {
        return (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A));
    }
    public static bool IsPressingRight()
    {
        return (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Semicolon));
    }
    public static bool HasPressedSelect()
    {
        return (Input.GetKeyDown(KeyCode.R));
    }

    public static bool HasPressedStart()
    {
        return (Input.GetKeyDown(KeyCode.Return));
    }
}
