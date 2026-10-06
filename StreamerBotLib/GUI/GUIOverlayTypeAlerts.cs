using StreamerBotLib.Static;
using StreamerBotLib.Systems.Overlay.Enums;

namespace StreamerBotLib.GUI
{
    public class GUIOverlayTypeAlerts
    {
        private Dictionary<string, List<string>> OverlayTypeActionPairs { get; set; } = [];

        public GUIOverlayTypeAlerts()
        {
            foreach (OverlayTypes overlay in Enum.GetValues<OverlayTypes>())
            {
                // ignore OverlayTypes.None - can't remove from enum due to EF Core datamodel uses as a type resolved to enum number index; requires intact enum definition
                if (overlay == OverlayTypes.None) continue;
                OverlayTypeActionPairs.Add(overlay.ToString(), []);
            }
        }

        public void AddData(OverlayTypes type, IEnumerable<string> data)
        {
            lock (OverlayTypeActionPairs)
            {
                if (type != OverlayTypes.None)
                {
                    OverlayTypeActionPairs[type.ToString()].UniqueAddRange(data);
                    OverlayTypeActionPairs[type.ToString()].Sort();
                }
            }
        }

        public List<string> TypeNames
        {
            get
            {
                lock (OverlayTypeActionPairs)
                {
                    return [.. OverlayTypeActionPairs.Keys];
                }
            }
        }

        public List<string> GetValue(OverlayTypes type)
        {
            lock (OverlayTypeActionPairs)
            {
                return 
                    type == OverlayTypes.None 
                    ? [] // send empty for "None" records 
                    : [.. OverlayTypeActionPairs[type.ToString()]];
            }
        }

        public Dictionary<string, List<string>> GetValues()
        {
            lock (OverlayTypeActionPairs)
            {
                return new(OverlayTypeActionPairs);
            }
        }
    }
}
