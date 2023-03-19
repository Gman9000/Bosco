using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Button {Up, Down, Left, Right, A, B, Start, Select}
public class PlayerInput
{
    private static Dictionary<Button, KeyCode[]> keyboardInputMap = new Dictionary<Button, KeyCode[]>(){
        {
            Button.Up, new[]{
            KeyCode.UpArrow, KeyCode.W
        }},
        {
            Button.Down, new[]{
            KeyCode.DownArrow, KeyCode.S
        }},
        {
            Button.Left, new[]{
            KeyCode.LeftArrow, KeyCode.A
        }},
        {
            Button.Right, new[]{
            KeyCode.RightArrow, KeyCode.D
        }},
        {
            Button.A, new[]{
            KeyCode.C, KeyCode.K, KeyCode.P
        }},
        {
            Button.B, new[]{
            KeyCode.Z, KeyCode.J, KeyCode.I
        }},
        {
            Button.Start, new[]{
            KeyCode.Return, KeyCode.Escape
        }},
        {
            Button.Select, new[]{
            KeyCode.R, KeyCode.LeftShift, KeyCode.RightShift
        }},
    };

    public static bool Pressed(Button input)
    {
        foreach (KeyCode key in keyboardInputMap[input])
            if (Input.GetKeyDown(key))  return true;
        return false;
    }
    public static bool Held(Button input)
    {
        foreach (KeyCode key in keyboardInputMap[input])
            if (Input.GetKey(key))  return true;
        return false;
    }

    public static bool Released(Button input)
    {
        foreach (KeyCode key in keyboardInputMap[input])
            if (Input.GetKeyUp(key))  return true;
        return false;
    }
}
