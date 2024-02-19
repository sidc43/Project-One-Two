using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    //test
    public ToolClass tool;

    public Vector2 offset;
    public Vector2 multiplier;
    public GameObject inventoryUI;
    public GameObject inventorySlotPF;
    public GameObject[,] uiSlots;
    public Slot[,] slots;
    public int inventoryWidth, inventoryHeight;

    private void Start()
    {
        slots = new Slot[inventoryWidth, inventoryHeight];
        uiSlots = new GameObject[inventoryWidth, inventoryHeight];
        SetupUI();
        UpdateInventoryUI();
        Add(new ItemClass(tool));
        Add(new ItemClass(tool));
        Add(new ItemClass(tool));
    }

    private void Update()
    {
        if (Utility.E)
            inventoryUI.SetActive(!inventoryUI.activeSelf);
    }

    private void SetupUI()
    {
        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                GameObject inventorySlot = Instantiate(inventorySlotPF, inventoryUI.transform.GetChild(0).transform);
                inventorySlot.GetComponent<RectTransform>().localPosition = new Vector2((x * multiplier.x) + offset.x, (y * multiplier.y) + offset.y);
                uiSlots[x, y] = inventorySlot;
                slots[x, y] = null;
            }
        }
    }

    private void UpdateInventoryUI()
    {
        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                if (slots[x, y] == null)
                {
                    uiSlots[x, y].transform.GetChild(0).GetComponent<Image>().sprite = null;
                    uiSlots[x, y].transform.GetChild(0).GetComponent<Image>().enabled = false;

                    uiSlots[x, y].transform.GetChild(1).GetComponent<Text>().text = "0";
                    uiSlots[x, y].transform.GetChild(1).GetComponent<Text>().enabled = false;
                }
                else
                {
                    uiSlots[x, y].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    uiSlots[x, y].transform.GetChild(0).GetComponent<Image>().sprite = slots[x, y].item.sprite;

                    uiSlots[x, y].transform.GetChild(1).GetComponent<Text>().text = slots[x, y].count.ToString();
                    uiSlots[x, y].transform.GetChild(1).GetComponent<Text>().enabled = true;
                }
            }
        }
    }

    public bool Add(ItemClass item)
    {
        Vector2Int itemPos = Contains(item);
        bool added = false;
        if (itemPos != Vector2Int.one * -1)
        {
            if (slots[itemPos.x, itemPos.y].count < item.stackSize)
            {
                slots[itemPos.x, itemPos.y].count++;
                added = true;
            }
        }
        if (!added)
        {
            for (int y = inventoryHeight - 1; y >= 0; y--)
            {
                if (added)
                    break;
                for (int x = 0; x < inventoryWidth; x++)
                {
                    if (slots[x, y] == null)
                    {
                        // slot empty
                        slots[x, y] = new Slot { item = item, position = new Vector2Int(x, y), count = 1 };
                        added = true;
                        break;
                    }
                }
            }
        }

        UpdateInventoryUI();
        return added;
    }

    public Vector2Int Contains(ItemClass item)
    {
        for (int y = inventoryHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                if (slots[x, y] != null)
                {
                    if (slots[x, y].item.itemName == item.itemName)
                    {
                        Debug.Log(slots[x, y].item.itemName);
                        return new Vector2Int(x, y);
                    }
                }
            }
        }

         return Vector2Int.one * -1;
    }

    public void Remove(ItemClass item)
    {

    }
}
