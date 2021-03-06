﻿using UnityEngine;
using System.Collections;
using Bolt.Compiler;
using UnityEditor;

public class PropertyEditorArray : PropertyEditor<PropertyTypeArray> {
  protected override void Edit(bool array) {
    BoltEditorGUI.WithLabel("Element Type", () => {
      PropertyType.ElementType = BoltEditorGUI.PropertyTypePopup(PropertyType.AllowedElementTypes, PropertyType.ElementType);
    });

    BoltEditorGUI.WithLabel("Element Count", () => {
      PropertyType.ElementCount = Mathf.Max(2, EditorGUILayout.IntField(PropertyType.ElementCount));
    });

    if (PropertyType.ElementType.HasSettings) {
      EditorGUILayout.BeginVertical();
      PropertyEditorRegistry.GetEditor(PropertyType.ElementType).EditArrayElement(Asset, Definition, PropertyType.ElementType);
      EditorGUILayout.EndVertical();
    }
  }
}
