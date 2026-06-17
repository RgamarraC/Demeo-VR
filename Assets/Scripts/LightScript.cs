using UnityEngine;

public class SceneStateManager : MonoBehaviour
{
    [Header("Lights")]
    public Light[] directionalLights;

    [Header("Window Renderer")]
    public Renderer windowRenderer;

    [Header("Materials")]
    public Material materialMain;
    public Material materialLobby;

    void Start()
    {
        SetInitialState();
    }

    // 🔴 Estado inicial
    public void SetInitialState()
    {
        foreach (Light l in directionalLights)
            l.enabled = false;

        SetWindowMaterial(materialMain);
    }

    // 🟢 Click en botones
    public void EnterLobbyState()
    {
        foreach (Light l in directionalLights)
            l.enabled = true;

        SetWindowMaterial(materialLobby);
    }

    // 🔙 volver
    public void BackToMain()
    {
        SetInitialState();
    }

    // 🎯 CAMBIAR SOLO MATERIAL 0
    void SetWindowMaterial(Material newMat)
    {
        Material[] mats = windowRenderer.materials;

        mats[0] = newMat;   // 👈 SOLO el slot 0

        windowRenderer.materials = mats;
    }
}