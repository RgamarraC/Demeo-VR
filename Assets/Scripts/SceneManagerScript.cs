using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class SceneManagerScript : MonoBehaviour
{
    public async void LoadScene(string sceneName)
    {
        NetworkRunner[] runners = FindObjectsByType<NetworkRunner>(FindObjectsSortMode.None);
        List<Task> shutdownTasks = new List<Task>();

        foreach (NetworkRunner runner in runners)
        {
            if (runner != null)
            {
                shutdownTasks.Add(runner.Shutdown());
            }
        }

        if (shutdownTasks.Count > 0)
        {
            await Task.WhenAll(shutdownTasks);
            await Task.Yield();
        }

        LobbyGenerator.codigoLobby = null;
        LobbyGenerator.hostName = null;
        JoinLobbyUI.nombreJugador = null;
        JoinLobbyUI.codigoIngresado = null;
        GameplayRoleCache.Clear();

        GameObject tempGo = new GameObject("TempDontDestroyOnLoadHolder");
        DontDestroyOnLoad(tempGo);
        Scene dontDestroyOnLoadScene = tempGo.scene;
        Destroy(tempGo);

        if (dontDestroyOnLoadScene.IsValid())
        {
            GameObject[] rootObjects = dontDestroyOnLoadScene.GetRootGameObjects();
            foreach (GameObject go in rootObjects)
            {
                if (go != null && go != gameObject)
                {
                    bool isFusionOrPhoton = go.name.Contains("Fusion") ||
                                            go.name.Contains("Photon") ||
                                            go.name.Contains("LobbyNetworkRunner") ||
                                            go.name.Contains("ConnectionHandler") ||
                                            go.GetComponentInChildren<NetworkRunner>() != null ||
                                            go.GetComponentInChildren<NetworkObject>() != null;

                    if (isFusionOrPhoton)
                    {
                        Destroy(go);
                    }
                }
            }
        }
        SceneManager.LoadScene(sceneName);
    }
}
