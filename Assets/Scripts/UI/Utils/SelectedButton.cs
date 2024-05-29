using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedButton : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransformButton;
    [SerializeField] private float bigSize;
    [SerializeField] private float smallSize;


    public void IncrementHeight(){
        rectTransformButton.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, bigSize);
    }

    public void DecrementHeight(){
        rectTransformButton.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, smallSize);
    }
}
