using Assets.Scripts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventService : MonoBehaviour, IEventSystem
{

    private class EventsSendModel
    {
        [JsonProperty("events")]
        public List<EventData> Events;
    }

    [SerializeField]
    private TMP_InputField _type;

    [SerializeField]
    private TMP_InputField _data;

    [SerializeField]
    private Button _button;

    [SerializeField]
    private EventFileStorage _fileStorage;

    private List<EventData> _eventsList = new();
    private int _cooldownBeforeSendMs = 2000;
    private string _url = "https://localhost:7074/EventStream";

    private HttpClient client;
    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        _button.onClick.AddListener(() => TrackEvent(_type.text, _data.text));

        client = new HttpClient();
        cancellationTokenSource = new ();

        _ = Task.Run(async () =>
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                List<EventData> eventsCopy;
                lock(_eventsList)
                {
                    eventsCopy = new List<EventData>(_eventsList);
                    _eventsList.Clear();
                }
                
                eventsCopy.AddRange(_fileStorage.PullEvents());
                if (!eventsCopy.Any())
                {
                    continue;
                }

                var result = await SendRequest(eventsCopy);
                if (!result.IsSuccessStatusCode)
                {
                    _fileStorage.SaveEvents(eventsCopy);
                }

                await Task.Delay(_cooldownBeforeSendMs, cancellationTokenSource.Token);
            }
        });
    }

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();

        _button.onClick.RemoveAllListeners();
    }

    public void TrackEvent(string type, string data)
    {
        EventData eventData = new EventData() { EventType = type, Data = data };
        lock (_eventsList)
        {
            _eventsList.Add(eventData);
        }
    }
    
    private Task<HttpResponseMessage> SendRequest(List<EventData> events)
    {
        var model = new EventsSendModel() { Events = events };
        var eventsStr = JsonConvert.SerializeObject(model);

        return client.PostAsync( _url, new StringContent(eventsStr, Encoding.UTF8, "text/json"), cancellationTokenSource.Token);
    }
}
