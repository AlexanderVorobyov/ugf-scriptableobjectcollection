# Unity Game Framework

Provides tools for creating games powered by Unity Game Engine.

## UGF.ScriptableObjectCollection

- **Version**: 1.11.0
- **Unity Version**: 2018.2.0
- **Scripting Runtime Version**: .Net 4.x Equivalent
- **Api Compatibility Level**: .Net 4.x

## Dependencies

 - N/A

## About

This package provides **ScriptableObject** collection with nested **ScriptableObjects**.

Features:
- **ScriptableObject** with collection of other **ScriptableObjects**, which behave similar to **GameObject** with **Components**.
- Can edit any element in multiple selection.
- Ability to use the **CreateAsset** or custom **ScriptableObjectCollectionCreate** attribute to listed in add menu.
- **Copy/PasteAsNew/PasteValues** support.
- **Undo/Redo** support.

Limitation:
- Can not reorder elements while in multiple selection.
- Can not add or remove elements while in multiple selection.
- Do not support any of user context menu, using **MenuItem** or **ContextMenu** attributes.
    - But you can add custom menu items through the implementation of **IHasCustomMenu** in custom **Editor** of your type.
- Drag to reorder, not implemented yet.
- Context menu for adding new elements, similar to **AddComponent** menu, not implemented yet.
- Collection not support **Preset**, only items separately support this (**Preset** button still available, but it will work with undefined).

Usage:
- [Define And Work](docs/define_and_work.md)

## How to Install

Copy the `Packages/UGF.ScriptableObjectCollection` folder to the `Packages` folder of your own project.

---
> Unity Game Framework | Alexander Vorobyov | Copyright 2018