﻿using System;
using System.Threading.Tasks;
using Azure.Health.DataServices.Pipelines;

namespace AzureHealth.DataServices.Channels
{
    /// <summary>
    /// Interface for channels.
    /// </summary>
    public interface IChannel : IDisposable
    {
        /// <summary>
        /// Event signals the close on the channel.
        /// </summary>
        event EventHandler<ChannelCloseEventArgs> OnClose;

        /// <summary>
        /// Event signals an error in the channel.
        /// </summary>
        event EventHandler<ChannelErrorEventArgs> OnError;

        /// <summary>
        /// Event signals the channel is open and connected.
        /// </summary>
        event EventHandler<ChannelOpenEventArgs> OnOpen;

        /// <summary>
        /// Event signals a message received by the channel.
        /// </summary>
        event EventHandler<ChannelReceivedEventArgs> OnReceive;

        /// <summary>
        /// Event signals a change in the state of the channel.
        /// </summary>
        event EventHandler<ChannelStateEventArgs> OnStateChange;

        /// <summary>
        /// Gets a unique id for the channel instance.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets a name for the type of channel.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the requirement for executing the channel.
        /// </summary>
        StatusType ExecutionStatusType { get; }

        /// <summary>
        /// Gets an indicator of whether the identity that received the channel connection is authenticated.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets an indicateor of whether the channel is encrypted.
        /// </summary>
        bool IsEncrypted { get; }

        /// <summary>
        /// Gets the port opened by the channel.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the state of the channel.
        /// </summary>
        ChannelState State { get; }

        /// <summary>
        /// Opens the channel.
        /// </summary>
        /// <returns>Task</returns>
        Task OpenAsync();

        /// <summary>
        /// Sends a message on the channel.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="items">Optional parameters to use when sending.</param>
        /// <returns>Task</returns>
        Task SendAsync(byte[] message, params object[] items);

        /// <summary>
        /// Starts the receive process for the channel.
        /// </summary>
        /// <returns>Task</returns>
        Task ReceiveAsync();

        /// <summary>
        /// Injects a message into the channel.  Useful with connectionless layer-2 protocols.
        /// </summary>
        /// <param name="message">Messages to add when needing auxiliary access to channel.</param>
        /// <returns>Task</returns>
        Task AddMessageAsync(byte[] message);

        /// <summary>
        /// Closes the channel.
        /// </summary>
        /// <returns>Task</returns>
        Task CloseAsync();
    }
}
