using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UNET;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class Restart_game_script : NetworkBehaviour
{
    public void OnMouseDown () {
        ChangeSceneClass.Shuffle();
        NetworkManager.Singleton.SceneManager.LoadScene("BoardScene", LoadSceneMode.Single);
    }
}
