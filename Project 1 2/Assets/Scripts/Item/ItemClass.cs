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
        None,
        Block,
        Tool,
        Weapon
    }

    public enum WeaponType
    {
        None,
        Melee, 
        Ranged,
        Magic
    }

    public enum ToolType
    {
        None,
        Axe,
        Pickaxe,
        Hammer,
        Shovel
    }

    public ItemClass (TileClass _tile)
    {
        itemName = _tile.tileName;
        sprite = _tile.tileDrop.sprites[0];
        stackable = _tile.stackable;
        itemType = ItemType.Block;
        stackSize = _tile.stackSize;
        tile = _tile;
    }

    public ItemClass (ToolClass _tool)
    {
        itemName = _tool.toolName;
        sprite = _tool.sprite;
        stackable = false;
        itemType = ItemType.Tool;
        toolType = _tool.toolType;
        tool = _tool;
    }

    public ItemClass(WeaponClass _weapon)
    {
        itemName = _weapon.weaponName;
        sprite = _weapon.sprite;
        stackable = false;
        itemType = ItemType.Weapon;
        weaponType = _weapon.weaponType;
        weapon = _weapon;
    }
}
