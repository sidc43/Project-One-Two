using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponClass", menuName = "Weapon Class")]
public class WeaponClass : ScriptableObject
{
    public string weaponName;
    public Sprite sprite;
    public ItemClass.WeaponType weaponType;
}
