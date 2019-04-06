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

    public class LightState
    {
        [JsonProperty(PropertyName = "on")] //: false,
        public bool? On { get; set; }
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
    }

    public class GroupAction
    {
        [JsonProperty(PropertyName = "on")] //: false,
        public bool? On { get; set; }
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
    }

    public class GroupLightList : List<Light>
    {
        public Group Group { get; set; }
        public string GroupName { get { return Group?.Name; } }
        public List<Light> Lights => this;
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

                string json = await response.Content.ReadAsStringAsync();
                JArray arr = JArray.Parse(json);
                return (string)arr?[0]?["success"]?["username"];
            }
        }

        public async Task<Light[]> GetLightsAsync()
        {
            CheckBridge();

            string uri = $"http://{IP}/api/{Username}/lights";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(uri);

                string json = await response.Content.ReadAsStringAsync();
                if (!(JToken.Parse(json) is JObject obj))
                    throw new Exception("Invalid response");

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

        public async Task<Group[]> GetGroupsAsync()
        {
            CheckBridge();

            string uri = $"http://{IP}/api/{Username}/groups";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(uri);

                string json = await response.Content.ReadAsStringAsync();
                if (!(JToken.Parse(json) is JObject obj))
                    throw new Exception("Invalid response");

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
            Task<Light[]> lightTask = GetLightsAsync();
            Task<Group[]> groupsTask = GetGroupsAsync();
            await Task.WhenAll(lightTask, groupsTask);
            

            var lights = lightTask.Result;
            var groups = groupsTask.Result;

            List<GroupLightList> result = new List<GroupLightList>();

            foreach (Group g in groups)
            {
                var gll = new GroupLightList
                {
                    Group = g
                };
                gll.AddRange(g.LightIndices.Select(i => lights[i - 1]));
                result.Add(gll);
            }

            return result;
        }

        public async Task SetLightStateAsync(Light light, LightState state)
        {
            CheckBridge();

            string uri = $"http://{IP}/api/{Username}/lights/{light.Id}/state";
            string lightprefix = $"/lights/{light.Id}/state/";

            using (var httpClient = new HttpClient())
            {
                var postdata = JsonConvert.SerializeObject(state,Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var content = new StringContent(postdata);

                var response = await httpClient.PutAsync(uri, content);

                string json = await response.Content.ReadAsStringAsync();
                if (!(JToken.Parse(json) is JArray arr))
                    throw new Exception("Invalid response");

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
        }

        public async Task SetGroupActionAsync(Group group, GroupAction state)
        {
            CheckBridge();

            string uri = $"http://{IP}/api/{Username}/groups/{group.Id}/action";
            string prefix = $"/groups/{group.Id}/action/";

            using (var httpClient = new HttpClient())
            {
                var postdata = JsonConvert.SerializeObject(state, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var content = new StringContent(postdata);

                var response = await httpClient.PutAsync(uri, content);

                string json = await response.Content.ReadAsStringAsync();
                if (!(JToken.Parse(json) is JArray arr))
                    throw new Exception("Invalid response");

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
        }

        private void CheckBridge()
        {
            if (IP == null)
                throw new Exception("Bridge not set");
            if (Username == null)
                throw new Exception("Username not set");
        }
    }
}
