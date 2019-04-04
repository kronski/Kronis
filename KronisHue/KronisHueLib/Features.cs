using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Net;

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

        #region Api Classes

        public class LightState
        {
            [JsonProperty(PropertyName = "on")] //: false,
            public bool On { get; set; }
            [JsonProperty(PropertyName = "bri")] //: 1,
            public byte Bri { get; set; }
            [JsonProperty(PropertyName = "hue")] //: 33761,
            public ushort Hue { get; set; }
            [JsonProperty(PropertyName = "sat")] //: 254,
            public byte Sat { get; set; }
            [JsonProperty(PropertyName = "effect")] //: "none",
            public string Effect  { get; set; }
            [JsonProperty(PropertyName = "xy")] //: [
            public float[] XY { get; set; }
            [JsonProperty(PropertyName = "ct")] //: 159,
            public ushort CT { get; set; }
            [JsonProperty(PropertyName = "alert")] //: "none",
            public string Alert { get; set; }
            [JsonProperty(PropertyName = "colormode")] //: "xy",
            public string ColorMode { get; set; }
            [JsonProperty(PropertyName = "mode")] //: "homeautomation",
            public string Mode { get; set; }
            [JsonProperty(PropertyName = "reachable")] //: true
            public bool Reachable { get; set; }

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

            public Group Group { get; set; }
        }

        public class GroupAction
        {
            [JsonProperty(PropertyName = "on")] //: false,
            public bool On { get; set; }
            [JsonProperty(PropertyName = "bri")] //: 1,
            public byte Bri { get; set; }
            [JsonProperty(PropertyName = "hue")] //: 33761,
            public ushort Hue { get; set; }
            [JsonProperty(PropertyName = "sat")] //: 254,
            public byte Sat { get; set; }
            [JsonProperty(PropertyName = "effect")] //: "none",
            public string Effect { get; set; }
            [JsonProperty(PropertyName = "xy")] //: [
            public float[] XY { get; set; }
            [JsonProperty(PropertyName = "ct")] //: 159,
            public ushort CT { get; set; }
            [JsonProperty(PropertyName = "alert")] //: "none",
            public string Alert { get; set; }
            [JsonProperty(PropertyName = "colormode")] //: "xy",
            public string ColorMode { get; set; }
        }


        public class Group
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "lights")]
            public int[] LightIndices { get; set; }

            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }
            [JsonProperty(PropertyName = "action")]
            public GroupAction Action { get; set; }


            public Light[] Lights { get; set; }
        }

        #endregion

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
            if (Username == null)
                throw new Exception("Username not set");

            if (IP == null)
                throw new Exception("Bridge not set");

            string uri = $"http://{IP}/api/{Username}/lights";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(uri);

                string json = await response.Content.ReadAsStringAsync();
                if (!(JToken.Parse(json) is JObject obj))
                    throw new Exception("Invalid response");

                List<Light> list = new List<Light>();
                foreach (var light in obj.Properties())
                {
                    list.Add(light.Value.ToObject<Light>());
                }

                return list.ToArray();
            }
        }

        public async Task<Group[]> GetGroupsAsync()
        {
            if (IP == null)
                throw new Exception("Bridge not set");
            if (Username == null)
                throw new Exception("Username not set");

            string uri = $"http://{IP}/api/{Username}/groups";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(uri);

                string json = await response.Content.ReadAsStringAsync();
                if (!(JToken.Parse(json) is JObject obj))
                    throw new Exception("Invalid response");

                List<Group> list = new List<Group>();
                foreach (var group in obj.Properties())
                {
                    list.Add(group.Value.ToObject<Group>());
                }

                return list.ToArray();
            }
        }

        public async Task<Light[]> GetLightsWithGroupAsync()
        {
            Task<Light[]> lightTask = GetLightsAsync();
            Task<Group[]> groupsTask = GetGroupsAsync();
            await Task.WhenAll(lightTask, groupsTask);
            

            var lights = lightTask.Result;
            var groups = groupsTask.Result;

            foreach (Group g in groups)
            {
                g.Lights = g.LightIndices.Select(i => lights[i-1]).ToArray();
                foreach (Light l in g.Lights)
                {
                    l.Group = g;
                }
            }

            return lights;
        }
    }
}
