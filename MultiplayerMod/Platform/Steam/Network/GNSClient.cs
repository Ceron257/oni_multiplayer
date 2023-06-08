﻿using System;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Extensions;
using MultiplayerMod.Core.Logging;
using MultiplayerMod.Core.Unity;
using MultiplayerMod.Multiplayer;
using MultiplayerMod.Network;
using MultiplayerMod.Network.Events;
using MultiplayerMod.Platform.Steam.Network.Components;
using MultiplayerMod.Platform.Steam.Network.Messaging;
using GNS.Sockets;

namespace MultiplayerMod.Platform.Steam.Network;

public class GNSClient : BaseClient {

    private readonly Core.Logging.Logger log = LoggerFactory.GetLogger<GNSClient>();

    public static string Identity = "server";
    private readonly Lazy<IPlayer> playerContainer = new(() => new DevPlayer(Identity));
    protected override Lazy<IPlayer> getPlayer() => playerContainer;


    private readonly NetworkMessageProcessor messageProcessor = new();
    private readonly NetworkMessageFactory messageFactory = new();

    private NetworkingSockets devClient;
    private uint devConnection;

    private NetworkingUtils utils;
    private StatusCallback status;
    private DebugCallback debug;

    private int reconnectAttempts = 0;

    private GNSServer getServer()
    {
        try
        {
            return (GNSServer) Container.Get<IMultiplayerServer>();
        } catch
        {
            return null;
        }
    }

    public override void Connect(IMultiplayerEndpoint endpoint) {
        if (endpoint == null)
            Identity = "client";

        devClient = new NetworkingSockets();

        utils = new NetworkingUtils();
        status = (ref StatusInfo info) => 
        {
            var server = getServer();
            if (info.connection != devConnection)
            {
                if (server != null)
                {
                    server.StatusCallback(ref info);
                }
                return;
            }
            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connected:
                    log.Info("Client connected to server - ID: " + info.connection);
                    log.Info($"Sending identity '{Identity}' to server");
                    devClient.SendMessageToConnection(
                        info.connection,
                        System.Text.Encoding.ASCII.GetBytes(Identity),
                        SendFlags.Reliable);
                    SetState(MultiplayerClientState.Connected);
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    devClient.CloseConnection(info.connection);
                    log.Info($"Client disconnected from server {info.connectionInfo.state}");
                    if (info.connection == devConnection && reconnectAttempts < 20)
                    {
                        reconnectAttempts++;
                        log.Info($"Attempting to reconnect {reconnectAttempts} times.");
                        Address address = new Address();
                        address.SetAddress("127.0.0.1", 8081);
                        devConnection = devClient.Connect(ref address);
                        log.Info($"New decConnection {devConnection}");
                    }
                    else
                    {
                        log.Info($"info.connection : {info.connection} != devConnection : {devConnection} ReconnectAttempts: {reconnectAttempts}");
                    }
                    break;
            }
        };

        utils.SetStatusCallback(status);
        debug = (DebugType type, string message) => 
        {
            if (type > DebugType.Message)
                return;
            log.Info($"GNS Debug: {type} - {message}");
        };
        // utils.SetDebugCallback(DebugType.Message, debug);
        
        SetState(MultiplayerClientState.Connecting);
        Address address = new Address();
        address.SetAddress("127.0.0.1", 8081);
        log.Info($"Client connecting... with identity '{Identity}'");
        devConnection = devClient.Connect(ref address);
        log.Info($"devConnection = {devConnection}");
        gameObject = UnityObject.CreateStaticWithComponent<SteamClientComponent>();
        // Run callbacks immediately so that the client on the server reacts to the 
        // connection in a timely manner. Otherwise the connection would fail with
        // problem detected locally:
        devClient.RunCallbacks();
    }

    public override void Tick() {
        devClient.RunCallbacks();

        if (State != MultiplayerClientState.Connected)
            return;
        ReceiveDevCommands();
    }

    public override void Send(IMultiplayerCommand command, MultiplayerCommandOptions options = MultiplayerCommandOptions.None) {
        if (State != MultiplayerClientState.Connected)
            throw new NetworkPlatformException("Client not connected");
        log.Info($"Sending {command} to server.");
        messageFactory.Create(command, options).ForEach(
            handle => {
                var result = devClient.SendMessageToConnection(
                    devConnection,
                    handle.Pointer,
                    handle.Size,
                    SendFlags.Reliable);
                if (result != Result.OK)
                    log.Error($"Failed to send {command}: {result}");
            }
        );
    }

    private void ReceiveDevCommands() {
        const int maxMessages = 20;

        NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
        int netMessagesCount = devClient.ReceiveMessagesOnConnection(devConnection, netMessages, maxMessages);

        if (netMessagesCount > 0)
        {
          log.Info($"netMessagesCount = {netMessagesCount}");
          for (int i = 0; i < netMessagesCount; i++)
          {
            ref NetworkingMessage netMessage = ref netMessages[i];

            log.Info("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
            var message = messageProcessor.Process(
                devConnection,
                new NetworkMessageHandle(netMessage.data, (uint)netMessage.length)
            );
            if (message != null)
            {
                log.Info($"Received message: {message}");
                OnCommandReceived(new CommandReceivedEventArgs(null, message.Command));
            }
            else
            {
                log.Info("messageProcessor.Process returned null!");
            }

            netMessage.Destroy();
          }
        }
    }
}
