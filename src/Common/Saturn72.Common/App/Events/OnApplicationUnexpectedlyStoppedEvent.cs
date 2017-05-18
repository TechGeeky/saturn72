﻿using Saturn72.Core.Services.Events;

namespace Saturn72.Common.App.Events
{
    public class OnApplicationUnexpectedlyStoppedEvent:EventBase
    {
        public IApp App { get; private set; }

        public OnApplicationUnexpectedlyStoppedEvent(IApp app)
        {
            App = app;
        }

    }
}