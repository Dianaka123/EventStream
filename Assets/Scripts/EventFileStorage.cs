using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    public class EventFileStorage : MonoBehaviour, IEventStorage
    {
        private string filePath;

        private void Awake()
        {
            filePath = Application.dataPath + "/eventsStorage.txt";
        }

        public IEnumerable<EventData> PullEvents()
        {
            if (!File.Exists(filePath))
            {
                return Array.Empty<EventData>();
            }

            var events = JsonConvert.DeserializeObject<IEnumerable<EventData>>(File.ReadAllText(filePath));
            File.Delete(filePath);

            return events;
        }

        public void SaveEvents(IEnumerable<EventData> eventData)
        {
            var listStr = JsonConvert.SerializeObject(eventData); 
            File.WriteAllText(filePath, listStr);
        }
    }
}