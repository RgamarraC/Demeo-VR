using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;

public class LobbyCallbacks : INetworkRunnerCallbacks
{
    private LobbyGenerator lobby;

    public LobbyCallbacks(LobbyGenerator lobby)
    {
        this.lobby = lobby;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        lobby.HandlePlayerJoined(runner, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        lobby.HandlePlayerLeft(runner, player);
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        UnityEngine.Debug.Log("HOST MIGRATION DETECTADA EN CALLBACKS");

        lobby.HandleHostMigration(runner, hostMigrationToken);
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        UnityEngine.Debug.Log("CACHE GAMEPLAY: Guardando datos antes de cambiar de escena.");

        if (lobby != null)
        {
            lobby.GuardarDatosParaGameplay();
        }
    }
}