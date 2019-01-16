using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bluegrams.Application
{
    /// <summary>
    /// Provides portable, persistent application settings in JSON format.
    /// </summary>
    public class PortableJsonSettingsProvider : SettingsProvider, IApplicationSettingsProvider
    {
        /// <summary>
        /// Specifies the name of the settings file to be used.
        /// </summary>
        public static string SettingsFileName { get; set; } = "settings.json";

        /// <summary>
        /// Specifies the directory of the settings file.
        /// </summary>
        public static string SettingsDirectory { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Specifies if all settings should be roaming.
        /// </summary>
        public static bool AllRoaming { get; set; } = false;

        public override string Name => "PortableJsonSettingsProvider";

        /// <summary>
        /// Applies this settings provider to each property of the given settings.
        /// </summary>
        /// <param name="settingsList">An array of settings.</param>
        public static void ApplyProvider(params ApplicationSettingsBase[] settingsList)
        {
            foreach (var settings in settingsList)
            {
                var provider = new PortableJsonSettingsProvider();
                settings.Providers.Clear();
                settings.Providers.Add(provider);
                foreach (SettingsProperty prop in settings.Properties)
                    prop.Provider = provider;
                settings.Reload();
            }
        }

        private string ApplicationSettingsFile => Path.Combine(SettingsDirectory, SettingsFileName);

        public override string ApplicationName { get { return Assembly.GetExecutingAssembly().GetName().Name; } set { } }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (String.IsNullOrEmpty(name)) name = Name;
            base.Initialize(name, config);
        }

        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
        {
            throw new NotImplementedException();
        }

        public void Reset(SettingsContext context)
        {
            if (File.Exists(ApplicationSettingsFile))
                File.Delete(ApplicationSettingsFile);
        }

        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
        { /* don't do anything here*/ }
        
        private JObject GetJObject()
        {
            // to deal with multiple settings providers accessing the same file, reload on every set or get request.
            JObject jObject = null;
            bool initnew = false;
            if (File.Exists(this.ApplicationSettingsFile))
            {
                try
                {
                    jObject = JObject.Parse(File.ReadAllText(ApplicationSettingsFile));
                }
                catch { initnew = true; }
            }
            else
                initnew = true;
            if (initnew)
            {
                jObject = new JObject(
                            new JProperty("userSettings",
                                 new JObject(
                                    new JProperty("roaming",
                                        new JObject()))));
            }
            return jObject;
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            JObject jObject = GetJObject();
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
            // iterate through settings to be retrieved
            foreach(SettingsProperty setting in collection)
            {
                SettingsPropertyValue value = new SettingsPropertyValue(setting);
                value.IsDirty = false;
                //Set serialized value to element from file. This will be deserialized by SettingsPropertyValue when needed.
                value.SerializedValue = getSettingsValue(jObject, (string)context["GroupName"], setting);
                values.Add(value);
            }
            return values;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            JObject jObject = GetJObject();
            foreach (SettingsPropertyValue value in collection)
            {
                setSettingsValue(jObject, (string)context["GroupName"], value);
            }
            try
            {
                File.WriteAllText(ApplicationSettingsFile, jObject.ToString());
            } catch { /* We don't want the app to crash if the settings file is not available */ }
        }

        private object getSettingsValue(JObject jObject, string scope, SettingsProperty prop)
        {
            object result = null;
            if (!IsUserScoped(prop))
                return result;
            //determine the location of the settings property
            JObject settings = (JObject)jObject.SelectToken("userSettings");
            if (IsRoaming(prop))
                settings = (JObject)settings["roaming"];
            else settings = (JObject)settings["PC_" + Environment.MachineName];
            // retrieve the value or set to default if available
            if (settings != null && settings[scope] != null)
            {
                JToken propVal = settings[scope][prop.Name];
                if (propVal != null)
                {
                    switch (prop.SerializeAs)
                    {
                        case SettingsSerializeAs.Xml:
                            // Convert json back to xml as this is expected for an xml-serialized element.
                            result =  JsonConvert.DeserializeXNode(propVal.ToString())?.ToString();
                            break;
                        case SettingsSerializeAs.Binary:
                            result = Convert.FromBase64String(propVal.ToString());
                            break;
                        default:
                            result = propVal.ToString();
                            break;
                    }
                }
                else result = prop.DefaultValue;
            }
            else
                result = prop.DefaultValue;
            return result;
        }

        private void setSettingsValue(JObject jObject, string scope, SettingsPropertyValue value)
        { 
            if (!IsUserScoped(value.Property)) return;
            //determine the location of the settings property
            JObject settings = (JObject)jObject.SelectToken("userSettings");
            JObject settingsLoc;
            if (IsRoaming(value.Property))
                settingsLoc = (JObject)settings["roaming"];
            else settingsLoc = (JObject)settings["PC_" + Environment.MachineName];
            // the serialized value to be saved
            JToken serialized;
            if (value.SerializedValue == null) serialized = new JValue("");
            else if (value.Property.SerializeAs == SettingsSerializeAs.Xml)
            {
                // Convert serialized XML to JSON
                serialized = JObject.Parse(JsonConvert.SerializeXNode(XElement.Parse(value.SerializedValue.ToString())));
            }
            else if (value.Property.SerializeAs == SettingsSerializeAs.Binary)
                serialized = new JValue(Convert.ToBase64String((byte[])value.SerializedValue));
            else serialized = new JValue((string)value.SerializedValue);
            // check if setting already exists, otherwise create new
            if (settingsLoc == null)
            {
                string settingsSection;
                if (IsRoaming(value.Property)) settingsSection = "roaming";
                else settingsSection = "PC_" + Environment.MachineName;
                settingsLoc = new JObject(new JProperty(scope,
                    new JObject(new JProperty(value.Name, serialized))));
                settings.Add(settingsSection, settingsLoc);
            }
            else
            {
                JObject scopeProp = (JObject)settingsLoc[scope];
                if (scopeProp != null)
                {
                    scopeProp[value.Name] = serialized;
                }
                else
                {
                    settingsLoc.Add(scope, new JObject(new JProperty(value.Name, serialized)));
                }
            }
        }

        // Iterates through the properties' attributes to determine whether it's user-scoped or application-scoped.
        private bool IsUserScoped(SettingsProperty prop)
        {
            foreach (DictionaryEntry d in prop.Attributes)
            {
                Attribute a = (Attribute)d.Value;
                if (a.GetType() == typeof(UserScopedSettingAttribute))
                    return true;
            }
            return false;
        }

        // Iterates through the properties' attributes to determine whether it's set to roam.
        private bool IsRoaming(SettingsProperty prop)
        {
            if (AllRoaming)
                return true;
            foreach (DictionaryEntry d in prop.Attributes)
            {
                Attribute a = (Attribute)d.Value;
                if (a.GetType() == typeof(SettingsManageabilityAttribute))
                    return true;
            }
            return false;
        }
    }
}
