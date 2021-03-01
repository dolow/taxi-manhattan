using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIField : MonoBehaviour
{
    public delegate void RequestRotateRight();
    public delegate void RequestRotateLeft();

    public RequestRotateRight OnRequestRotateRight = null;
    public RequestRotateLeft OnRequestRotateLeft = null;

    public void RotateRight()
    {
        this.OnRequestRotateRight?.Invoke();
    }

    public void RotateLeft()
    {
        this.OnRequestRotateLeft?.Invoke();
    }
}
