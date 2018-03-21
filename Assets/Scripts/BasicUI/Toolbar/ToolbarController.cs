﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarController : MonoBehaviour
{
    public static ToolbarController instance = null;
    [HideInInspector] public bool hoverDetect = false; // Once a selectable is selected, hover actions over other selectables should be detected
    public RectTransform dropdownTransform;
    public ToolbarDropdownItem dropdownItemPrefab;
    private ObjectPool<ToolbarDropdownItem> dropdownItemPool;
    private List<ToolbarDropdownItem> dropdownItems = new List<ToolbarDropdownItem>();
    [HideInInspector] public ToolbarSelectable currentSelected = null;
    public UIParameters uiParameters;
    [HideInInspector] public int onObjectCount = 0;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instance of ToolbarController");
        }
        dropdownItemPool = new ObjectPool<ToolbarDropdownItem>(dropdownItemPrefab, 10, dropdownTransform);
    }
    public void InitializeDropdownItems(List<ToolbarOperation> infos, float offset)
    {
        Vector2 offsetMin = dropdownTransform.offsetMin;
        offsetMin.x = offset;
        dropdownTransform.offsetMin = offsetMin;
        float maxTextWidth = 0.0f;
        for (int i = 0; i < infos.Count; i++)
        {
            ToolbarDropdownItem item = dropdownItemPool.GetObject();
            string[] strings = infos[i].strings.Clone() as string[];
            if (infos[i].operation.shortcut != null)
                for (int j = 0; j < strings.Length; j++)
                    strings[j] += "(" + infos[i].operation.shortcut.ToString() + ")";
            item.description.Strings = strings;
            item.shortcut.text = infos[i].globalShortcut?.ToString();
            item.callback = infos[i].operation.callback;
            item.gameObject.SetActive(true);
            float textWidth = LayoutUtility.GetPreferredWidth(item.descriptionTransform) + LayoutUtility.GetPreferredWidth(item.shortcutTransform);
            if (textWidth > maxTextWidth) maxTextWidth = textWidth;
            dropdownItems.Add(item);
        }
        for (int i = 0; i < infos.Count; i++)
        {
            Vector2 sizeDelta = dropdownItems[i].buttonTransform.sizeDelta;
            sizeDelta.x = maxTextWidth + 2 * uiParameters.toolbarMainButtonSideSpace + uiParameters.toolbarItemMiddleSpace;
            dropdownItems[i].buttonTransform.sizeDelta = sizeDelta;
        }
    }
    public void ReturnAllDropdownItems()
    {
        while (dropdownItems.Count > 0)
        {
            dropdownItems[0].gameObject.SetActive(false);
            dropdownItemPool.ReturnObject(dropdownItems[0]);
            dropdownItems.RemoveAt(0);
        }
    }
    public void CloseDropdown()
    {
        ReturnAllDropdownItems();
        hoverDetect = false;
    }
    public void DeselectAll()
    {
        CloseDropdown();
        currentSelected.SetDefaultColor();
        currentSelected = null;
        onObjectCount = 0;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && onObjectCount == 0)
        {
            CloseDropdown();
            currentSelected?.SetDefaultColor();
            currentSelected = null;
        }
    }
}
