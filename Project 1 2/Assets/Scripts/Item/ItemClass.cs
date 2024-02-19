using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemClass
{
    public string itemName;
    public Sprite sprite;

    public TileClass tile;
    public ToolClass tool;
    public WeaponClass weapon;

    public ItemType itemType;
    public ToolType toolType;
    public WeaponType weaponType;

    public bool stackable;
    public int stackSize;

    public enum ItemType
    {
        Block,
        Tool,
        Weapon
    }

    public enum WeaponType
    {
        Melee, 
        Ranged,
        Magic
    }

    public enum ToolType
    {
        Axe,
        Pickaxe,
        Hammer,
        Shovel
    }

    public ItemClass (TileClass _tile)
    {
        itemName = _tile.tileName;
        sprite = _tile.dropSprite;
        stackable = _tile.stackable;
        itemType = ItemType.Block;
        stackSize = _tile.stackSize;
    }

    public ItemClass (ToolClass _tool)
    {
        itemName = _tool.toolName;
        sprite = _tool.sprite;
        stackable = false;
        itemType = ItemType.Tool;
        toolType = _tool.toolType;
    }

    public ItemClass(WeaponClass _weapon)
    {
        itemName = _weapon.weaponName;
        sprite = _weapon.sprite;
        stackable = false;
        itemType = ItemType.Weapon;
        weaponType = _weapon.weaponType;
    }
}
