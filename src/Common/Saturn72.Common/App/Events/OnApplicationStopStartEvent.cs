using Saturn72.Core.Services.Events;

namespace Saturn72.Common.App.Events
{
    public class OnApplicationStopStartEvent : EventBase
    {
        public IApp App { get; private set; }

        public OnApplicationStopStartEvent(IApp app)
        {
            App = app;
        }

    }
}