using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;

namespace KronisHue
{
    public class BridgeLocator
    {
        public static async Task<string[]> FindHueBridgeViaMeetHue()
        {
            const string uri = "https://discovery.meethue.com";

            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync(uri);
                JArray arr = JArray.Parse(json);
                string[] ips = arr.Children<JObject>().Select(x => (string)x.Property("internalipaddress").Value).ToArray();
                return ips;
            }
        }

        public static async Task<string[]> FindHueBridgeViaUpnp(Action<string> found, CancellationTokenSource cancelSource)
        {
            var finder = new SsdpRadar.FinderService(1, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), cancelSource?.Token);

            var devices = await finder.FindDevicesAsync(
                device =>
                {
                    if(device.Info.FriendlyName.StartsWith("Philips hue"))
                    {
                        found(device.RemoteEndPoint.ToString());
                    }
                }
                );
            return devices.Select(
                device => device.RemoteEndPoint.ToString()
            ).ToArray();
        }

    }

    #region Api Classes

    public class LightState : INotifyPropertyChanged
    {
        private bool? on;

        [JsonProperty(PropertyName = "on")] //: false,
        public bool? On {
            get
            {
                return on;
            }
            set
            {
                if (value != on)
                {
                    on = value;
                    OnPropertyChanged();
                }
            }
        }
        [JsonProperty(PropertyName = "bri")] //: 1,
        public byte? Bri { get; set; }
        [JsonProperty(PropertyName = "hue")] //: 33761,
        public ushort? Hue { get; set; }
        [JsonProperty(PropertyName = "sat")] //: 254,
        public byte? Sat { get; set; }
        [JsonProperty(PropertyName = "effect")] //: "none",
        public string Effect { get; set; }
        [JsonProperty(PropertyName = "xy")] //: [
        public float[] XY { get; set; }
        [JsonProperty(PropertyName = "ct")] //: 159,
        public ushort? CT { get; set; }
        [JsonProperty(PropertyName = "alert")] //: "none",
        public string Alert { get; set; }
        [JsonProperty(PropertyName = "colormode")] //: "xy",
        public string ColorMode { get; set; }
        [JsonProperty(PropertyName = "mode")] //: "homeautomation",
        public string Mode { get; set; }
        [JsonProperty(PropertyName = "reachable")] //: true
        public bool? Reachable { get; set; }


        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }



    public class SWUpdate
    {
        [JsonProperty(PropertyName = "state")] //: "noupdates",
        public string State { get; set; }
        [JsonProperty(PropertyName = "lastinstall")] //: "2018-01-02T19:24:20"
        public DateTime? LastInstall { get; set; }
    }

    public class LightCapabilitiesControlCt
    {
        [JsonProperty(PropertyName = "min")] //: 153,
        public ushort Min { get; set; }
        [JsonProperty(PropertyName = "max")] //: 500
        public ushort Max { get; set; }
    }

    public class LightCapabilitiesControl
    {
        [JsonProperty(PropertyName = "mindimlevel")] //: 5000,
        public ushort Mindimlevel { get; set; }
        [JsonProperty(PropertyName = "maxlumen")] //: 600,
        public ushort Maxlumen { get; set; }
        [JsonProperty(PropertyName = "colorgamuttype")] //: "B",
        public string Colorgamuttype { get; set; }
        [JsonProperty(PropertyName = "colorgamut")] //: [
        public float[][] Colorgamut { get; set; }
        [JsonProperty(PropertyName = "ct")] //: {
        public LightCapabilitiesControlCt Ct { get; set; }
    }

    public class LightCapabilitiesControlStreaming
    {
        [JsonProperty(PropertyName = "renderer")] //: true,
        public bool Renderer { get; set; }
        [JsonProperty(PropertyName = "proxy")] //: false
        public bool Proxy { get; set; }
    }


    public class LightCapabilities
    {
        [JsonProperty(PropertyName = "certified")] //: true,
        public bool Certified { get; set; }
        [JsonProperty(PropertyName = "control")] //: {
        public LightCapabilitiesControl Control { get; set; }

        [JsonProperty(PropertyName = "streaming")] //: {
        public LightCapabilitiesControlStreaming Streaming { get; set; }

    }

    public class LightConfig
    {
        [JsonProperty(PropertyName = "archetype")]//: "sultanbulb",
        public string Archetype { get; set; }
        [JsonProperty(PropertyName = "function")]//: "mixed",
        public string Function { get; set; }
        [JsonProperty(PropertyName = "direction")]//: "omnidirectional"
        public string Direction { get; set; }
    }
    public class Light
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "state")]
        public LightState State { get; set; }

        [JsonProperty(PropertyName = "swupdate")]
        public SWUpdate SWUpdate { get; set; }

        [JsonProperty(PropertyName = "type")] //: "Extended color light",
        public string Type { get; set; }
        [JsonProperty(PropertyName = "name")] //: "Hue color lamp 7",
        public string Name { get; set; }
        [JsonProperty(PropertyName = "modelid")] //: "LCT007",
        public string Modelid { get; set; }
        [JsonProperty(PropertyName = "manufacturername")] //: "Philips",
        public string Manufacturername { get; set; }
        [JsonProperty(PropertyName = "productname")] //: "Hue color lamp",
        public string Productname { get; set; }
        [JsonProperty(PropertyName = "capabilities")] //: {
        public LightCapabilities Capabilities { get; set; }
        [JsonProperty(PropertyName = "config")]//: {
        public LightConfig Config { get; set; }

        [JsonProperty(PropertyName = "uniqueid")]//: "00:17:88:01:00:bd:c7:b9-0b",
        public string Uniqueid { get; set; }
        [JsonProperty(PropertyName = "swversion")]//: "5.105.0.21169"
        public string SWVersion { get; set; }

        public List<Group> Groups { get; set; }
    }

    public class GroupAction : INotifyPropertyChanged
    {
        private bool? on;

        [JsonProperty(PropertyName = "on")] //: false,
        public bool? On { get => on; set { if (value != on) { on = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "bri")] //: 1,
        public byte? Bri { get; set; }
        [JsonProperty(PropertyName = "hue")] //: 33761,
        public ushort? Hue { get; set; }
        [JsonProperty(PropertyName = "sat")] //: 254,
        public byte? Sat { get; set; }
        [JsonProperty(PropertyName = "xy")] //: [
        public float[] XY { get; set; }
        [JsonProperty(PropertyName = "ct")] //: 159,
        public ushort? CT { get; set; }
        [JsonProperty(PropertyName = "alert")] //: "none",
        public string Alert { get; set; }
        [JsonProperty(PropertyName = "effect")] //: "none",
        public string Effect { get; set; }
        [JsonProperty(PropertyName = "transitiontime")]
        public ushort? Transitiontime { get; set; }
        [JsonProperty(PropertyName = "bri_inc")]
        public short? BriInc { get; set; }
        [JsonProperty(PropertyName = "sat_inc")]
        public short? SatInc { get; set; }
        [JsonProperty(PropertyName = "hue_inc")]
        public int? HueInc { get; set; }
        [JsonProperty(PropertyName = "ct_inc")]
        public int? CtInc { get; set; }
        [JsonProperty(PropertyName = "xy_inc")]
        public float? XyInc { get; set; }
        [JsonProperty(PropertyName = "scene")]
        public string Scene { get; set; }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }


    public class Group
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "lights")]
        public int[] LightIndices { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "action")]
        public GroupAction Action { get; set; }

        public List<Light> Lights { get; set; }
    }

    public class GroupLightList : List<Light>
    {
        public Group Group { get; set; }
        public string GroupName { get { return Group?.Name; } }
    }

    #endregion

    public class BridgeApiClient
    {
        public static BridgeApiClient Current { get; set; }

        public static void Init()
        {
            Current = new BridgeApiClient();
        }

        public HashSet<string> Bridges { get; set; }

        public string IP { get; set; }
        public string Username { get; set; }

        public BridgeApiClient()
        {
            Bridges = new HashSet<string>();
        }

        

        public async Task<string> NewDeveloperAsync()
        {
            if (IP == null)
                throw new Exception("Bridge not set");

            string uri = $"http://{IP}/api";

            using (var httpClient = new HttpClient())
            {
                var postdata = JsonConvert.SerializeObject(new { devicetype = "KronisHue#Kronis" });
                var content = new StringContent(postdata);
                var response = await httpClient.PostAsync(uri, content);
                string json = await JsonFromResponse(response);
                var arr = ParseJsonArray(json);
                return (string)arr?[0]?["success"]?["username"];
            }
        }

        private static async Task<string> JsonFromResponse(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            //return await response.Content.ReadAsStringAsync();
            var stream = await response.Content.ReadAsStreamAsync();
            var sr = new StreamReader(stream);
            return await sr.ReadToEndAsync();

        }

        public async Task<Light[]> GetLightsAsync()
        {
            if (!CheckBridge())
                return new Light[0];

            string uri = $"http://{IP}/api/{Username}/lights";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(uri);
                string json = await JsonFromResponse(response);
                var obj = ParseJsonObject(json);

                List<Light> list = new List<Light>();
                foreach (var prop in obj.Properties())
                {
                    var light = prop.Value.ToObject<Light>();
                    light.Id = prop.Name;
                    list.Add(light);
                }

                return list.ToArray();
            }
        }

        private static JToken ParseJsonToken(string json)
        {
            try
            {
                var token = JToken.Parse(json);
                return token;
            }
            catch 
            {
                Debug.WriteLine(json);
                throw;
            }
        }

        private static JArray ParseJsonArray(string json)
        {
            if (!(ParseJsonToken(json) is JArray arr))
                throw new Exception("Invalid response");
            return arr;
        }

        private static JObject ParseJsonObject(string json)
        {
            if (!(ParseJsonToken(json) is JObject obj))
                throw new Exception("Invalid response");
            return obj;
        }

        public async Task<Group[]> GetGroupsAsync()
        {
            if (!CheckBridge())
                return new Group[0];

            string uri = $"http://{IP}/api/{Username}/groups";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(uri);
                string json = await JsonFromResponse(response);
                var obj = ParseJsonObject(json);

                List<Group> list = new List<Group>();
                foreach (var prop in obj.Properties())
                {
                    var group = prop.Value.ToObject<Group>();
                    group.Id = prop.Name;
                    list.Add(group);
                }

                return list.ToArray();
            }
        }

        public async Task<List<GroupLightList>> GetGroupLightListAsync()
        {
            /*Task<Light[]> lightTask = GetLightsAsync();
            Task<Group[]> groupsTask = GetGroupsAsync();
            await Task.WhenAll(lightTask, groupsTask);
            var lights = lightTask.Result;
            var groups = groupsTask.Result;*/

            var lights = await GetLightsAsync();
            var groups = await GetGroupsAsync();

            List <GroupLightList> result = new List<GroupLightList>();

            foreach (Group g in groups)
            {
                var gll = new GroupLightList
                {
                    Group = g
                };
                gll.AddRange(g.LightIndices.Select(i => lights[i - 1]));

                foreach (var light in gll)
                {
                    if (light.Groups == null)
                        light.Groups = new List<Group>();
                    light.Groups.Add(g);
                }

                g.Lights = gll;

                result.Add(gll);
            }

            return result;
        }

        public async Task SetLightStateAsync(Light light, LightState state)
        {
            CheckBridgeAndThrow();

            string uri = $"http://{IP}/api/{Username}/lights/{light.Id}/state";
            string lightprefix = $"/lights/{light.Id}/state/";

            using (var httpClient = new HttpClient())
            {
                var postdata = JsonConvert.SerializeObject(state,Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var content = new StringContent(postdata);

                var response = await httpClient.PutAsync(uri, content);
                string json = await JsonFromResponse(response);
                var arr = ParseJsonArray(json);

                foreach (var item in arr)
                {
                    if (item is JObject obj &&
                        obj.Property("success")?.Value is JObject statechanges)
                    {
                        foreach (var statechange in statechanges)
                        {
                            if (statechange.Key.StartsWith(lightprefix))
                            {
                                string prop = statechange.Key.Substring(lightprefix.Length);

                                PropertyInfo[] properties = typeof(LightState).GetProperties();
                                foreach (PropertyInfo property in properties)
                                {
                                    JsonPropertyAttribute p = property.GetCustomAttribute<JsonPropertyAttribute>();
                                    string name = p?.PropertyName ?? property.Name;
                                    if (name == prop)
                                    {
                                        property.SetValue(light.State, statechange.Value.ToObject<object>()); 
                                    }
                                }
                            }
                        }
                    }
                }
            }
            UpdateGroups(light);
        }

        public void UpdateGroups(Light light)
        {
            if (light.Groups == null)
                throw new ArgumentException("Light missing groups");

            foreach (Group group in light.Groups)
            {
                bool allon = group.Lights.All(l => l.State.On == true);
                bool alloff = group.Lights.All(l => l.State.On == false);

                if (allon)
                    group.Action.On = true;
                if (alloff)
                    group.Action.On = false;
            }
        }

        public void UpdateLights(Group group)
        {
            foreach (Light light in group.Lights)
            {
                light.State.On = group.Action.On;
            }
        }

        public async Task SetGroupActionAsync(Group group, GroupAction state)
        {
            CheckBridgeAndThrow();

            string uri = $"http://{IP}/api/{Username}/groups/{group.Id}/action";
            string prefix = $"/groups/{group.Id}/action/";

            using (var httpClient = new HttpClient())
            {
                var postdata = JsonConvert.SerializeObject(state, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var content = new StringContent(postdata);

                var response = await httpClient.PutAsync(uri, content);
                string json = await JsonFromResponse(response);
                var arr = ParseJsonArray(json);

                foreach (var item in arr)
                {
                    if (item is JObject obj &&
                        obj.Property("success")?.Value is JObject statechanges)
                    {
                        foreach (var statechange in statechanges)
                        {
                            if (statechange.Key.StartsWith(prefix))
                            {
                                string prop = statechange.Key.Substring(prefix.Length);

                                PropertyInfo[] properties = typeof(GroupAction).GetProperties();
                                foreach (PropertyInfo property in properties)
                                {
                                    JsonPropertyAttribute p = property.GetCustomAttribute<JsonPropertyAttribute>();
                                    string name = p?.PropertyName ?? property.Name;
                                    if (name == prop)
                                    {
                                        property.SetValue(group.Action, statechange.Value.ToObject<object>());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            UpdateLights(group);
        }

        private void CheckBridgeAndThrow()
        {
            if (IP == null)
                throw new Exception("Bridge not set");
            if (Username == null)
                throw new Exception("Username not set");
        }

        private bool CheckBridge()
        {
            if (IP == null)
                return false;
            if (Username == null)
                return false;
            return true;
        }
    }
}
