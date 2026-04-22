using UnityEngine;
using System.Collections.Generic;

public class SceneTransfer : MonoBehaviour
{
    public static Dictionary<string, Vector2> doorPositions = new Dictionary<string, Vector2>();

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}