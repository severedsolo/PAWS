using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PAWS
{
    public class ModulePAWS : PartModule
    {
        public Dictionary<BaseEvent, bool> enabledEvents = new Dictionary<BaseEvent, bool>();
        public Dictionary<BaseField, bool> enabledFields = new Dictionary<BaseField, bool>();
        bool showGUI = false;
        Rect Window = new Rect(20, 100, 240, 50);
        Vector2 scrollPosition1;
        Vector2 scrollPosition2;
        bool showFields = false;
        bool showEvents = false;
        bool advanced = false;
        bool globalSave = true;
        List<BaseField> sortedFields;
        List<BaseEvent> sortedEvents;
        List<BaseField> sortedAdvancedFields;
        List<BaseEvent> sortedAdvancedEvents;
        List<BaseField> sortedEditorFields;
        List<BaseEvent> sortedEditorEvents;

        [KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, guiActiveEditor = true, externalToEVAOnly = false, guiName = "Customise PAW")]
        void CustomisePAW()
        {
            showGUI = !showGUI;
        }

        private void Start()
        {
            List<BaseField> myFieldList = new List<BaseField>();
            List<BaseEvent> myEventList = new List<BaseEvent>();
            List<BaseField> myEditorFieldList = new List<BaseField>();
            List<BaseEvent> myEditorEventList = new List<BaseEvent>();
            List<BaseField> myAdvancedFieldList = new List<BaseField>();
            List<BaseEvent> myAdvancedEventList = new List<BaseEvent>();
            if (part.Events.Count > 0)
            {
                foreach (BaseEvent e in part.Events)
                {
                    if (!HighLogic.LoadedSceneIsEditor)
                    {
                        if (e.guiActive || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(e.name, out bool b)) myEventList.Add(e);
                    }
                    else if(e.guiActiveEditor || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(e.name, out bool b)) myEditorEventList.Add(e);
                    myAdvancedEventList.Add(e);
                }
            }
            if (part.Fields.Count > 0)
            {
                foreach (BaseField bf in part.Fields)
                {
                    if (!HighLogic.LoadedSceneIsEditor)
                    {
                        if (bf.guiActive || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(bf.name, out bool b)) myFieldList.Add(bf);
                    }
                    else if (bf.guiActiveEditor || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(bf.name, out bool b)) myEditorFieldList.Add(bf);
                    myAdvancedFieldList.Add(bf);
                }
            }
            if (part.Modules.Count > 0)
            {
                foreach (PartModule pm in part.Modules)
                {
                    if (pm.Events.Count > 0)
                    {
                        foreach (BaseEvent e in pm.Events)
                        {
                            int id = e.id;
                            if (!HighLogic.LoadedSceneIsEditor)
                            {
                                if (e.guiActive || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(e.name, out bool b)) myEventList.Add(pm.Events[id]);
                            }
                            else if (e.guiActiveEditor || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(e.name, out bool b)) myEditorEventList.Add(pm.Events[id]);
                            myAdvancedEventList.Add(e);
                        }
                    }
                    if (pm.Fields.Count > 0)
                    {
                        foreach (BaseField bf in pm.Fields)
                        {
                            if (!HighLogic.LoadedSceneIsEditor)
                            {
                                if (bf.guiActive || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(bf.name, out bool b)) myFieldList.Add(bf);
                            }
                            else if (bf.guiActiveEditor || PAWSGlobalSettings.instance.enabledEvents.TryGetValue(bf.name, out bool b)) myEditorFieldList.Add(bf);
                            myAdvancedFieldList.Add(bf);
                        }
                    }
                }
            }
            sortedFields = myFieldList.OrderBy(baseField => baseField.guiName).ToList();
            sortedEvents = myEventList.OrderBy(baseEvent => baseEvent.guiName).ToList();
            sortedEditorFields = myEditorFieldList.OrderBy(baseField => baseField.guiName).ToList();
            sortedEditorEvents = myEditorEventList.OrderBy(baseEvent => baseEvent.guiName).ToList();
            sortedAdvancedFields = myAdvancedFieldList.OrderBy(baseField => baseField.guiName).ToList();
            sortedAdvancedEvents = myAdvancedEventList.OrderBy(baseEvent => baseEvent.guiName).ToList();
        }

        public void OnGUI()
        {
            if (showGUI)
            {
                Window = GUILayout.Window(93938174, Window, GUIDisplay, "PAWS", GUILayout.Width(200), GUILayout.Height(200));
            }
        }

        void GUIDisplay(int windowID)
        {
            string label;
            advanced = GUILayout.Toggle(advanced, "Advanced Mode");
            if (GUILayout.Button("Show Fields")) showFields = !showFields;
            if (showFields)
            {
                scrollPosition1 = GUILayout.BeginScrollView(scrollPosition1, GUILayout.Width(200), GUILayout.Height(200));
                if (sortedAdvancedFields.Count() > 0)
                {
                    for (int i = 0; i < sortedAdvancedFields.Count(); i++)
                    {
                        if (!advanced && i >= sortedFields.Count() && !HighLogic.LoadedSceneIsEditor) break;
                        if (!advanced && i >= sortedEditorFields.Count() && HighLogic.LoadedSceneIsEditor) break;
                        BaseField bf;
                        if (!advanced && !HighLogic.LoadedSceneIsEditor) bf = sortedFields.ElementAt(i);
                        else if (!advanced && HighLogic.LoadedSceneIsEditor) bf = sortedEditorFields.ElementAt(i);
                        else bf = sortedAdvancedFields.ElementAt(i);
                        if (bf.guiActive && !HighLogic.LoadedSceneIsEditor) label = "Toggle Off";
                        else if (bf.guiActiveEditor && HighLogic.LoadedSceneIsEditor) label = "Toggle Off";
                        else label = "Toggle On";
                        GUILayout.Label(bf.guiName);
                        if (GUILayout.Button(label))
                        {
                            if (!HighLogic.LoadedSceneIsEditor) bf.guiActive = !bf.guiActive;
                            else bf.guiActiveEditor = !bf.guiActiveEditor;
                            if (globalSave)
                            {
                                if (!HighLogic.LoadedSceneIsEditor)
                                {
                                    PAWSGlobalSettings.instance.enabledFields.Remove(bf.name);
                                    PAWSGlobalSettings.instance.enabledFields.Add(bf.name, bf.guiActive);
                                }
                                else
                                {
                                    PAWSGlobalSettings.instance.enabledEditorFields.Remove(bf.name);
                                    PAWSGlobalSettings.instance.enabledEditorFields.Add(bf.name, bf.guiActive);
                                }
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            if (GUILayout.Button("Show Events")) showEvents = !showEvents;
            if (showEvents)
            {
                scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, GUILayout.Width(200), GUILayout.Height(200));
                if (sortedAdvancedEvents.Count() > 0)
                {
                    for (int i = 0; i < sortedAdvancedEvents.Count(); i++)
                    {
                        if (!advanced && i >= sortedEvents.Count() && !HighLogic.LoadedSceneIsEditor) break;
                        if (!advanced && i >= sortedEditorEvents.Count() && HighLogic.LoadedSceneIsEditor) break;
                        BaseEvent be;
                        if (!advanced && !HighLogic.LoadedSceneIsEditor) be = sortedEvents.ElementAt(i);
                        else if (!advanced && HighLogic.LoadedSceneIsEditor) be = sortedEditorEvents.ElementAt(i);
                        else be = sortedAdvancedEvents.ElementAt(i);
                        if (be.guiActive && !HighLogic.LoadedSceneIsEditor) label = "Toggle Off";
                        else if (be.guiActiveEditor && HighLogic.LoadedSceneIsEditor) label = "Toggle Off";
                        else label = "Toggle On";
                        if (be == Events["CustomisePAW"]) continue;
                        GUILayout.Label(be.guiName);
                        if (GUILayout.Button(label))
                        {
                            if (!HighLogic.LoadedSceneIsEditor) be.guiActive = !be.guiActive;
                            else if (HighLogic.LoadedSceneIsEditor) be.guiActiveEditor = !be.guiActiveEditor;
                            if (globalSave)
                            {
                                if (!HighLogic.LoadedSceneIsEditor)
                                {
                                    PAWSGlobalSettings.instance.enabledEvents.Remove(be.name);
                                    PAWSGlobalSettings.instance.enabledEvents.Add(be.name, be.guiActive);
                                }
                                else
                                {
                                    PAWSGlobalSettings.instance.enabledEditorEvents.Remove(be.name);
                                    PAWSGlobalSettings.instance.enabledEditorEvents.Add(be.name, be.guiActive);
                                }
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            globalSave = GUILayout.Toggle(globalSave, "Save Settings Globally");
            if (GUILayout.Button("Close")) showGUI = false;
            GUI.DragWindow();
        }
    }
}
