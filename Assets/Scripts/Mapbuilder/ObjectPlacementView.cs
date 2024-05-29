using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPlacementView : MonoBehaviour
{
    [SerializeField] private ObjectPlacement objectPlacement;
    [SerializeField] private Text heightLabel;
    [SerializeField] private Toggle floorHitToggle;

    private void Start()
    {
    }

    void Update()
    {
        heightLabel.text = $"{objectPlacement.Height:F1}";
        floorHitToggle.isOn = objectPlacement.OnlyHitFloor;
    }
}
