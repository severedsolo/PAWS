using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PAWS
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ModulePAWS : PartModule
    {
        public Dictionary<BaseEvent, bool> enabledEvents = new Dictionary<BaseEvent, bool>();
        public Dictionary<BaseField, bool> enabledFields = new Dictionary<BaseField, bool>();
        bool showGUI = false;
        private bool showSliderGUI = false;
        Rect Window = new Rect(20, 100, 400, 50);
        Vector2 scrollPosition1;
        Vector2 scrollPosition2;
        bool showFields = false;
        bool showEvents = false;
        bool advanced = false;
        bool globalSave = true;
        private bool handleSymmetry = true;
        List<BaseField> sortedFields;
        List<BaseEvent> sortedEvents;
        List<BaseField> sortedAdvancedFields;
        List<BaseEvent> sortedAdvancedEvents;
        int windowID = 0;
        private List<Part> partCache;

        [KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, guiActiveEditor = false, guiName = "Customise PAW")]
        void CustomisePAW()
        {
            windowID = Guid.NewGuid().GetHashCode();
            showSliderGUI = false;
            showGUI = !showGUI;
        }
        
        [KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, guiActiveEditor = true, externalToEVAOnly = false, guiName = "Customise Sliders")]
        void CustomiseSliders()
        {
            windowID = Guid.NewGuid().GetHashCode();
            showGUI = false;
            showSliderGUI = !showSliderGUI;
        }


        private void Start()
        {
            List<BaseField> myFieldList = new List<BaseField>();
            List<BaseEvent> myEventList = new List<BaseEvent>();
            List<BaseField> myAdvancedFieldList = new List<BaseField>();
            List<BaseEvent> myAdvancedEventList = new List<BaseEvent>();
            if (part.Events.Count > 0)
            {
                foreach (BaseEvent e in part.Events)
                {
                    if (e.guiActive || e.guiActiveEditor || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(e.name, out bool b)) myEventList.Add(e);
                    myAdvancedEventList.Add(e);
                }
            }
            if (part.Fields.Count > 0)
            {
                foreach (BaseField bf in part.Fields)
                {
                    if (bf.guiActive || PAWSGlobalSettings.instance.enabledFields.TryGetValue(bf.name, out bool b)) myFieldList.Add(bf);
                    myAdvancedFieldList.Add(bf);
                }
            }
            if (part.Modules.Count > 0)
            {
                foreach (PartModule pm in part.Modules)
                {
                    if (pm.Fields.Count > 0)
                    {
                        foreach (BaseField bf in pm.Fields)
                        {
                            if (bf.guiActive || PAWSGlobalSettings.instance.enabledFields.TryGetValue(bf.name, out bool b)) myFieldList.Add(bf);
                            myAdvancedFieldList.Add(bf);
                        }
                    }
                    if (pm.Events.Count() > 0)
                    {
                        foreach (BaseEvent e in pm.Events)
                        {
                            int id = e.id;
                            if (e.guiActive || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(e.name, out bool b)) myEventList.Add(pm.Events[id]);
                            myAdvancedEventList.Add(e);
                        }
                    }
                }
            }
            sortedFields = myFieldList.OrderBy(baseField => baseField.guiName).ToList();
            sortedEvents = myEventList.OrderBy(baseEvent => baseEvent.guiName).ToList();
            sortedAdvancedFields = myAdvancedFieldList.OrderBy(baseField => baseField.guiName).ToList();
            sortedAdvancedEvents = myAdvancedEventList.OrderBy(baseEvent => baseEvent.guiName).ToList();
        }

        public void OnGUI()
        {
            if (showSliderGUI)
            {
                Window = GUILayout.Window(windowID, Window, GUIDisplay, "PAWS", GUILayout.Width(400), GUILayout.Height(200));
            }
            else if (showGUI)
            {
                Window = GUILayout.Window(windowID, Window, GUIDisplay, "PAWS", GUILayout.Width(200),GUILayout.Height(200));
            }
        }

        void GUIDisplay(int windowID)
        {
            GUILayout.Label(part.partInfo.title);
            if (showGUI)
            {
                string label;
                advanced = GUILayout.Toggle(advanced, "Advanced Mode");
                if (GUILayout.Button("Show Fields")) showFields = !showFields;
                if (showFields)
                {
                    scrollPosition1 =
                        GUILayout.BeginScrollView(scrollPosition1, GUILayout.Width(200), GUILayout.Height(200));
                    if (sortedAdvancedFields.Count() > 0)
                    {
                        for (int i = 0; i < sortedAdvancedFields.Count(); i++)
                        {
                            if (!advanced && i >= sortedFields.Count()) break;
                            BaseField bf;
                            if (!advanced) bf = sortedFields.ElementAt(i);
                            else bf = sortedAdvancedFields.ElementAt(i);
                            if (bf.guiActive) label = "Toggle Off";
                            else label = "Toggle On";
                            GUILayout.Label(bf.guiName);
                            if (GUILayout.Button(label))
                            {
                                bf.guiActive = !bf.guiActive;
                                if (globalSave)
                                {
                                    PAWSGlobalSettings.instance.enabledFields.Remove(bf.name);
                                    PAWSGlobalSettings.instance.enabledFields.Add(bf.name, bf.guiActive);
                                }
                            }
                        }
                    }

                    GUILayout.EndScrollView();
                }

                if (GUILayout.Button("Show Events")) showEvents = !showEvents;
                if (showEvents)
                {
                    scrollPosition2 =
                        GUILayout.BeginScrollView(scrollPosition2, GUILayout.Width(200), GUILayout.Height(200));
                    if (sortedAdvancedEvents.Count() > 0)
                    {
                        for (int i = 0; i < sortedAdvancedEvents.Count(); i++)
                        {
                            if (!advanced && i >= sortedEvents.Count()) break;
                            BaseEvent be;
                            if (!advanced) be = sortedEvents.ElementAt(i);
                            else be = sortedAdvancedEvents.ElementAt(i);
                            if (be.guiActive) label = "Toggle Off";
                            else label = "Toggle On";
                            if (be == Events["CustomisePAW"]) continue;
                            GUILayout.Label(be.guiName);
                            if (GUILayout.Button(label))
                            {
                                be.guiActive = !be.guiActive;
                                if (globalSave)
                                {
                                    PAWSGlobalSettings.instance.enabledEvents.Remove(be.name);
                                    PAWSGlobalSettings.instance.enabledEvents.Add(be.name, be.guiActive);
                                }
                            }
                        }
                    }

                    GUILayout.EndScrollView();
                }

                globalSave = GUILayout.Toggle(globalSave, "Save Settings Globally");
            }
            else
            {
                for (int i = 0; i < sortedAdvancedFields.Count(); i++)
                {
                    BaseField bf = sortedAdvancedFields.ElementAt(i);
                    UI_FloatRange floatRange;
                    if (HighLogic.LoadedSceneIsEditor)
                    {
                        if (!bf.guiActiveEditor || bf.uiControlEditor.GetType() != typeof(UI_FloatRange)) continue;
                        floatRange = bf.uiControlEditor as UI_FloatRange;
                    }
                    else
                    {
                        if (!bf.guiActive || bf.uiControlFlight.GetType() != typeof(UI_FloatRange)) continue;
                        floatRange = bf.uiControlFlight as UI_FloatRange;
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(bf.guiName);
                    string s = GUILayout.TextField(bf.GetValue(bf.host).ToString());
                    float.TryParse(s, out float output);
                    if (floatRange != null)
                    {
                        if (output > floatRange.maxValue) output = floatRange.maxValue;
                        if (output < floatRange.minValue) output = floatRange.minValue;
                    }
                    bf.SetValue(output, bf.host);
                    if (handleSymmetry)
                    {
                        partCache = part.symmetryCounterparts;
                        for (int p = 0; p < partCache.Count(); p++)
                        {
                            Part symmetry = partCache.ElementAt(p);
                            BaseField symmetryField = findField(bf.name, symmetry);
                            symmetryField.SetValue(output, symmetryField.host);
                        }
                    }

                    if (GUILayout.Button("Apply All"))
                    {
                        if (!HighLogic.LoadedSceneIsEditor) partCache = vessel.parts;
                        else partCache = EditorLogic.fetch.ship.parts;
                        foreach (Part p in partCache)
                        {
                            foreach (PartModule pm in p.Modules)
                            {
                                foreach (BaseField field in pm.Fields)
                                {
                                    if (field.name == bf.name) field.SetValue(output, field.host);
                                }
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            handleSymmetry = GUILayout.Toggle(handleSymmetry, "Enable Symmetry");
            if (GUILayout.Button("Close"))
            {
                showGUI = false;
                showSliderGUI = false;
            }
            GUI.DragWindow();
        }

        private BaseField findField(string fieldName, Part p)
        {
            foreach (PartModule pm in p.Modules)
            {
                foreach (BaseField field in pm.Fields)
                {
                    if (field.name == fieldName) return field;
                }
            }
            return null;
        }
    }
}
