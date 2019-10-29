﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditorVars
{
    public static class ToolTips
    {
        public static GUIContent toggleBlend = new GUIContent("Blend", "Blends out the active texture to create a smooth transition the surrounding textures.");
        public static GUIContent rangeLow = new GUIContent("From:", "The lowest value to paint the active texture.");
        public static GUIContent rangeHigh = new GUIContent("To:", "The highest value to paint the active texture.");
        public static GUIContent blendLow = new GUIContent("Blend Low:", "The lowest value to blend out to.");
        public static GUIContent blendHigh= new GUIContent("Blend High:", "The highest value to blend out to.");
    }
}
