using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PAWS
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class PAWSGlobalSettings : MonoBehaviour
    {
        public static PAWSGlobalSettings instance;
        public bool globalToggle = false;
        public Dictionary<string, bool> enabledEvents = new Dictionary<string, bool>();
        public Dictionary<string, bool> enabledFields = new Dictionary<string, bool>();
        public Dictionary<string, bool> enabledEditorEvents = new Dictionary<string, bool>();
        public Dictionary<string, bool> enabledEditorFields = new Dictionary<string, bool>();
        bool ready;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        public void UpdateAllEvents()
        {
            ready = false;
            if (FlightGlobals.ActiveVessel == null) return;
            List<Part> parts = FlightGlobals.ActiveVessel.parts;
            if (parts.Count == 0) return;
            for (int i = 0; i < parts.Count; i++)
            {
                Part p = parts.ElementAt(i);
                BaseEventList events = p.Events;
                if (events.Count > 0)
                {
                    for (int e = 0; e < events.Count; e++)
                    {
                        BaseEvent ev = events.ElementAt(e);
                        if (enabledEvents.TryGetValue(ev.name, out bool b)) ev.guiActive = b;
                        if (enabledEditorEvents.TryGetValue(ev.name, out b)) ev.guiActiveEditor = b;
                    }
                }
                BaseFieldList fields = p.Fields;
                if (fields.Count > 0)
                {
                    foreach (BaseField field in fields)
                    {
                        if (enabledFields.TryGetValue(field.name, out bool b)) field.guiActive = b;
                        if (enabledEditorFields.TryGetValue(field.name, out b)) field.guiActiveEditor = b;
                    }
                }
                PartModuleList modules = p.Modules;
                if (modules.Count == 0) continue;
                foreach(PartModule partModule in modules)
                {
                    events = partModule.Events;
                    if (events.Count > 0)
                    {
                        for (int e = 0; e < events.Count; e++)
                        {
                            BaseEvent ev = events.ElementAt(e);
                            if (enabledEvents.TryGetValue(ev.name, out bool b)) ev.guiActive = b;
                            if (enabledEditorEvents.TryGetValue(ev.name, out b)) ev.guiActiveEditor = b;
                        }
                    }
                    fields = partModule.Fields;
                    if (fields.Count > 0)
                    {
                        foreach (BaseField field in fields)
                        {
                            if (enabledFields.TryGetValue(field.name, out bool b)) field.guiActive = b;
                            if (enabledEditorFields.TryGetValue(field.name, out b)) field.guiActiveEditor = b;
                        }
                    }
                }
            }
            ready = true;
        }

        private void Update()
        {
            if (!ready) UpdateAllEvents();
        }
    }
}
