using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LobbyButtons : MonoBehaviour {
    NetworkManagerScript network_manager_script;
    private bool clicked = false;
    private int coroutine_timer = 1;
    public bool loaded_to_game = false;

    void Start() {
        network_manager_script = GameObject.Find("NetworkHelper").GetComponent<NetworkManagerScript>();        
    }

    public void Host() {
        if (clicked || loaded_to_game) {
            return;
        }

        clicked = true;
        network_manager_script.BecomeHost();
        StartCoroutine(ButtonCoroutine());
    }

    public void Client() {
        if (clicked || loaded_to_game) {
            return;
        }

        network_manager_script.BecomeClient();
        clicked = true;
        StartCoroutine(ButtonCoroutine());
    }

    public void StartGame() {
        if (NetworkManager.Singleton.IsServer) {
            ChangeSceneClass.Shuffle();
            network_manager_script.ExitLobby();
        }
    }

    private IEnumerator ButtonCoroutine() {
        yield return new WaitForSeconds(coroutine_timer);
        clicked = false;
    }
}