using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static bool LMB => Input.GetMouseButtonDown(0);
    public static bool RMB => Input.GetMouseButtonDown(1);
    public static bool E => Input.GetKeyDown(KeyCode.E);
}
