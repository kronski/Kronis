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
using Xamarin.Forms;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace KronisHue
{
    public class BindingControlAttribute : Attribute
    {
        public Type ControlType { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }

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
                    if(device?.Info?.FriendlyName.StartsWith("Philips hue") == true)
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

    public class NotifyChangeBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LightState : NotifyChangeBase
    {
        private bool? on;
        private long? bri;
        private long? hue;
        private byte? sat;
        private string effect;
        private float[] xy;
        private ushort? ct;
        private string alert;
        private string colormode;
        private string mode;
        private bool? reachable;

        [BindingControl(ControlType = typeof(Switch))]
        [JsonProperty(PropertyName = "on")] //: false,
        public bool? On { get => on; set { if (value != on) { on = value; OnPropertyChanged(); } } }
        [BindingControl(ControlType = typeof(Slider),Max = 255, Min = 0)]
        [JsonProperty(PropertyName = "bri")] //: 1,
        public long? Bri { get => bri; set { if (value != bri) { bri = value; OnPropertyChanged(); } } }
        [BindingControl(ControlType = typeof(Slider), Max = 65535, Min = 0)]
        [JsonProperty(PropertyName = "hue")] //: 33761,
        public long? Hue { get => hue; set { if (value != hue) { hue = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "sat")] //: 254,
        public byte? Sat { get => sat; set { if (value != sat) { sat = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "effect")] //: "none",
        public string Effect { get => effect; set { if (value != effect) { effect = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "xy")] //: [
        public float[] XY { get => xy; set { if (value != xy) { xy = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "ct")] //: 159,
        public ushort? CT { get => ct; set { if (value != ct) { ct = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "alert")] //: "none",
        public string Alert { get => alert; set { if (value != alert) { alert = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "colormode")] //: "xy",
        public string ColorMode { get => colormode; set { if (value != colormode) { colormode = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "mode")] //: "homeautomation",
        public string Mode { get => mode; set { if (value != mode) { mode = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "reachable")] //: true
        public bool? Reachable { get => reachable; set { if (value != reachable) { reachable = value; OnPropertyChanged(); } } }
    }



    public class SWUpdate : NotifyChangeBase
    {
        private string state;
        private DateTime? lastinstall;

           [JsonProperty(PropertyName = "state")] //: "noupdates",
        public string State { get => state; set { if (value != state) { state = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "lastinstall")] //: "2018-01-02T19:24:20"
        public DateTime? LastInstall { get => lastinstall; set { if (value != lastinstall) { lastinstall = value; OnPropertyChanged(); } } }
    }

    public class LightCapabilitiesControlCt : NotifyChangeBase
    {
        private ushort min;
        private ushort max;

        [JsonProperty(PropertyName = "min")] //: 153,
        public ushort Min { get => min; set { if (value != min) { min = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "max")] //: 500
        public ushort Max { get => max; set { if (value != max) { max = value; OnPropertyChanged(); } } }
    }

    public class LightCapabilitiesControl : NotifyChangeBase
    {

        private ushort mindimlevel;
        private ushort maxlumen;
        private string colorgamuttype;
        private float[][] colorgamut;
        private LightCapabilitiesControlCt ct;

        [JsonProperty(PropertyName = "mindimlevel")] //: 5000,
        public ushort Mindimlevel { get => mindimlevel; set { if (value != mindimlevel) { mindimlevel = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "maxlumen")] //: 600,
        public ushort Maxlumen { get => maxlumen; set { if (value != maxlumen) { maxlumen = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "colorgamuttype")] //: "B",
        public string Colorgamuttype { get => colorgamuttype; set { if (value != colorgamuttype) { colorgamuttype = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "colorgamut")] //: [
        public float[][] Colorgamut { get => colorgamut; set { if (value != colorgamut) { colorgamut = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "ct")] //: {
        public LightCapabilitiesControlCt Ct { get => ct; set { if (value != ct) { ct = value; OnPropertyChanged(); } } }
    }

    public class LightCapabilitiesControlStreaming : NotifyChangeBase
    {
        private bool renderer;
        private bool proxy;

        [JsonProperty(PropertyName = "renderer")] //: true,
        public bool Renderer { get => renderer; set { if (value != renderer) { renderer = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "proxy")] //: false
        public bool Proxy { get => proxy; set { if (value != proxy) { proxy = value; OnPropertyChanged(); } } }
    }


    public class LightCapabilities : NotifyChangeBase
    {
        private bool certified;
        private LightCapabilitiesControl control;
        private LightCapabilitiesControlStreaming streaming;

        [JsonProperty(PropertyName = "certified")] //: true,
        public bool Certified { get => certified; set { if (value != certified) { certified = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "control")] //: {
        public LightCapabilitiesControl Control { get => control; set { if (value != control) { control = value; OnPropertyChanged(); } } }

        [JsonProperty(PropertyName = "streaming")] //: {
        public LightCapabilitiesControlStreaming Streaming { get => streaming; set { if (value != streaming) { streaming = value; OnPropertyChanged(); } } }

    }

    public class LightConfig : NotifyChangeBase
    {
        private string archetype;
        private string function;
        private string direction;

        [JsonProperty(PropertyName = "archetype")]//: "sultanbulb",
        public string Archetype { get => archetype; set { if (value != archetype) { archetype = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "function")]//: "mixed",
        public string Function { get => function; set { if (value != function) { function = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "direction")]//: "omnidirectional"
        public string Direction { get => direction; set { if (value != direction) { direction = value; OnPropertyChanged(); } } }
    }
    public class Light : NotifyChangeBase
    {
        private string id;
        private LightState state;
        private SWUpdate swupdate;
        private string _type;
        private string name;
        private string modelid;
        private string manufacturername;
        private string productname;
        private LightCapabilities capabilities;
        private LightConfig config;
        private string uniqueid;
        private string swversion;

        public string Id { get => id; set { if (value != id) { id = value; OnPropertyChanged(); } } }

        [JsonProperty(PropertyName = "state")]
        public LightState State { get => state; set { if (value != state) { state = value; OnPropertyChanged(); } } }

        [JsonProperty(PropertyName = "swupdate")]
        public SWUpdate SWUpdate { get => swupdate; set { if (value != swupdate) { swupdate = value; OnPropertyChanged(); } } }

        [JsonProperty(PropertyName = "type")] //: "Extended color light",
        public string Type { get => _type; set { if (value != _type) { _type = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "name")] //: "Hue color lamp 7",
        public string Name { get => name; set { if (value != name) { name = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "modelid")] //: "LCT007",
        public string Modelid { get => modelid; set { if (value != modelid) { modelid = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "manufacturername")] //: "Philips",
        public string Manufacturername { get => manufacturername; set { if (value != manufacturername) { manufacturername = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "productname")] //: "Hue color lamp",
        public string Productname { get => productname; set { if (value != productname) { productname = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "capabilities")] //: {
        public LightCapabilities Capabilities { get => capabilities; set { if (value != capabilities) { capabilities = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "config")]//: {
        public LightConfig Config { get => config; set { if (value != config) { config = value; OnPropertyChanged(); } } }

        [JsonProperty(PropertyName = "uniqueid")]
        public string Uniqueid { get => uniqueid; set { if (value != uniqueid) { uniqueid = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "swversion")]//: "5.105.0.21169"
        public string SWVersion { get => swversion; set { if (value != swversion) { swversion = value; OnPropertyChanged(); } } }

        [JsonIgnore]
        public List<Group> Groups { get; set; }
    }

    public class GroupAction : NotifyChangeBase
    {
        private bool? on;
        private byte? bri;
        private ushort? hue;
        private byte? sat;
        private float[] xy;
        private ushort? ct;
        private string alert;
        private string effect;
        private ushort? transitiontime;
        private short? bri_inc;
        private short? sat_inc;
        private int? hue_inc;
        private int? ct_inc;
        private float? xy_inc;
        private string scene;

        [JsonProperty(PropertyName = "on")] //: false,
        public bool? On { get => on; set { if (value != on) { on = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "bri")] //: 1,
        public byte? Bri { get => bri; set { if (value != bri) { bri = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "hue")] //: 33761,
        public ushort? Hue { get => hue; set { if (value != hue) { hue = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "sat")] //: 254,
        public byte? Sat { get => sat; set { if (value != sat) { sat = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "xy")] //: [
        public float[] XY { get => xy; set { if (value != xy) { xy = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "ct")] //: 159,
        public ushort? CT { get => ct; set { if (value != ct) { ct = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "alert")] //: "none",
        public string Alert { get => alert; set { if (value != alert) { alert = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "effect")] //: "none",
        public string Effect { get => effect; set { if (value != effect) { effect = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "transitiontime")]
        public ushort? Transitiontime { get => transitiontime; set { if (value != transitiontime) { transitiontime = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "bri_inc")]
        public short? BriInc { get => bri_inc; set { if (value != bri_inc) { bri_inc = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "sat_inc")]
        public short? SatInc { get => sat_inc; set { if (value != sat_inc) { sat_inc = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "hue_inc")]
        public int? HueInc { get => hue_inc; set { if (value != hue_inc) { hue_inc = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "ct_inc")]
        public int? CtInc { get => ct_inc; set { if (value != ct_inc) { ct_inc = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "xy_inc")]
        public float? XyInc { get => xy_inc; set { if (value != xy_inc) { xy_inc = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "scene")]
        public string Scene { get => scene; set { if (value != scene) { scene = value; OnPropertyChanged(); } } }
    }


    public class Group : NotifyChangeBase
    {
        private string id;
        private string name;
        private int[] lightindices;
        private string _type;
        private GroupAction action;
        private List<Light> lights;

        public string Id { get => id; set { if (value != id) { id = value; OnPropertyChanged(); } } }

        [JsonProperty(PropertyName = "name")]
        public string Name { get => name; set { if (value != name) { name = value; OnPropertyChanged(); } } }

        [JsonProperty(PropertyName = "lights")]
        public int[] LightIndices { get => lightindices; set { if (value != lightindices) { lightindices = value; OnPropertyChanged(); } } }

        [JsonProperty(PropertyName = "type")]
        public string Type { get => _type; set { if (value != _type) { _type = value; OnPropertyChanged(); } } }
        [JsonProperty(PropertyName = "action")]
        public GroupAction Action { get => action; set { if (value != action) { action = value; OnPropertyChanged(); } } }

        [JsonIgnore]
        public List<Light> Lights { get => lights; set { if (value != lights) { lights = value; OnPropertyChanged(); } } }
    }

    public class GroupLightList : List<Light>
    {
        public Group Group { get; set; }
        public string GroupName => Group?.Name; 
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
                System.Diagnostics.Debug.WriteLine(json);
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

                UpdateGroups(light);
            }
            
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
