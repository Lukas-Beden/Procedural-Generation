using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourGenerator
{
    [SerializeField] Material _material;

    public ColourGenerator(Material material)
    {
        _material = material;
    }

    public void UpdateElevation(MinMax elevationMinMax)
    {
        if (_material == null)
            Debug.Log("pasla");
        else
            Debug.Log("la");
            _material.SetVector("_ElevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));
    }
}