using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class Utility
{
    public static bool LMB => Input.GetMouseButtonDown(0);
    public static bool RMB => Input.GetMouseButtonDown(1);
    public static bool MouseWheelUp => Input.GetAxis("Mouse ScrollWheel") > 0f;
    public static bool MouseWheelDown => Input.GetAxis("Mouse ScrollWheel") < 0f;
    public static bool E => Input.GetKeyDown(KeyCode.E);
    public static bool InRange(Vector2 v1, Vector2 v2, float range) => Vector2.Distance(v1, v2) <= range;
}
