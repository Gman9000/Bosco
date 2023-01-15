using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    protected Rect spawnerBounds;
    public void SetBounds(Rect rect) => spawnerBounds = rect;
    public abstract void Start();
}
