using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PAWS
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames,GameScenes.FLIGHT)]
    class PAWSScenario : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            foreach (var v in PAWSGlobalSettings.instance.enabledEvents)
            {
                ConfigNode cn = new ConfigNode("EVENT");
                cn.SetValue("Name", v.Key, true);
                cn.SetValue("Enabled", v.Value, true);
                node.AddNode(cn);
            }
            foreach (var v in PAWSGlobalSettings.instance.enabledFields)
            {
                ConfigNode cn = new ConfigNode("FIELD");
                cn.SetValue("Name", v.Key, true);
                cn.SetValue("Enabled", v.Value, true);
                node.AddNode(cn);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            ConfigNode[] loaded = node.GetNodes("EVENT");
            if(loaded.Count() >0)
            {
                for (int i = 0; i < loaded.Count(); i++)
                {
                    string name = node.GetValue("Name");
                    if (name == null) continue;
                    if (bool.TryParse(node.GetValue("Enabled"), out bool enabled)) PAWSGlobalSettings.instance.enabledEvents.Add(name, enabled);
                }
            }
            loaded = node.GetNodes("FIELD");
            if (loaded.Count() > 0)
            {
                for (int i = 0; i < loaded.Count(); i++)
                {
                    string name = node.GetValue("Name");
                    if (name == null) continue;
                    if (bool.TryParse(node.GetValue("Enabled"), out bool enabled)) PAWSGlobalSettings.instance.enabledFields.Add(name, enabled);
                }
            }
            PAWSGlobalSettings.instance.UpdateAllEvents();
        }
    }
}
