﻿using System.Collections.Generic;

namespace AzureHealth.DataServices.Channels
{
    /// <summary>
    /// An interface for a type of channel collection.
    /// </summary>
    public interface IChannelCollection : IList<IChannel>
    {
    }
}
