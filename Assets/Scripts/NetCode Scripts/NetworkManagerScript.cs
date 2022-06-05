using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UNET;
using TMPro;
using Random = System.Random;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NetworkManagerScript : NetworkBehaviour {
    public string nick;

    private GameObject host_button;
    private GameObject client_button;
    private GameObject nick_input;
    private GameObject join_code_input;

    private GameObject join_code_info;
    private GameObject nick_info;
    private GameObject start_button;
    private GameObject loading_players;

    private string join_code;

    private bool nick_sync = true;
    private bool nick_sync_coroutine = false;

    public void SetJoinCode(string code) { join_code = code; }

    void Start() {
        host_button = GameObject.Find("HostButton");
        client_button = GameObject.Find("ClientButton");
        nick_input = GameObject.Find("NickInput");
        join_code_input = GameObject.Find("JoinCodeInput");

        join_code_info = GameObject.Find("JoinCodeInfo");
        join_code_info.GetComponent<Fading_text>().MakeTextInvisible();;

        nick_info = GameObject.Find("NickInfo");

        start_button = GameObject.Find("StartButton");
        
        start_button.GetComponent<Image>().gameObject.SetActive(false);

        LobbyTexts.ChangeVersion("EN");

        loading_players = GameObject.Find("Loading_players");
        loading_players.SetActive(false);
    }

    private string RandomNick() {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[8];
        var random = new Random();

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    private void PrepareScene(bool host = false) {
        nick_input.SetActive(false);
        join_code_input.SetActive(false);

        join_code_info.GetComponent<Fading_text>().MakeTextVisible();

        if (nick == "" || nick == "Nick...") {
            nick = RandomNick();
        }
    }

    private void HideButtons() {
        host_button.GetComponent<Image>().color = new Color(0,0,0,0);
        host_button.GetComponent<LobbyButtons>().loaded_to_game = true;
        host_button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "";
        
        client_button.GetComponent<Image>().color = new Color(0,0,0,0);
        client_button.GetComponent<LobbyButtons>().loaded_to_game = true;
        client_button.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "";
        
        nick_input.GetComponent<Image>().gameObject.SetActive(false);
        join_code_input.GetComponent<Image>().gameObject.SetActive(false);
    
        LobbyTexts.HideButtons();
    }

    public async void BecomeHost() {
        nick = nick_input.GetComponent<TMP_InputField>().text;

        if (RelayManager.Instance.isRelayEnabled) {
            await RelayManager.Instance.SetupRelay();
        }

        PrepareScene(true);
        NetworkManager.Singleton.StartHost();

        join_code_info.GetComponent<TextMeshProUGUI>().text += "\n" + join_code;
        LobbyTexts.AddText("JoinCodeInfo", "\n" + join_code);

        nick_sync = false;

        start_button.GetComponent<Image>().gameObject.SetActive(true);
        LobbyTexts.Sync();

        HideButtons();
    } 

    public async void BecomeClient() {
        nick = nick_input.GetComponent<TMP_InputField>().text;

        string joinCode = join_code_input.GetComponent<TMP_InputField>().text;
        joinCode = joinCode.ToUpper();

        if (joinCode == "" || joinCode == null) { return; }

        if (RelayManager.Instance.isRelayEnabled) {
            await RelayManager.Instance.JoinRelay(joinCode);
        }

        PrepareScene(true);
        NetworkManager.Singleton.StartClient();

        join_code_info.GetComponent<TextMeshProUGUI>().text += "\n" + joinCode;
        LobbyTexts.AddText("JoinCodeInfo", "\n" + join_code);

        nick_sync = false;
        HideButtons();
    }

    public void ExitLobby() {
        loading_players.SetActive(true);
        loading_players.GetComponent<Image>().sprite = Resources.Load<Sprite>("Other_graphics/" + ChangeSceneClass.language_abbreviation + "/loading");

        SyncLanguageClientRpc(ChangeSceneClass.language_id);
        ExitLobbyStatic();
    }

    [ClientRpc]
    void SyncLanguageClientRpc(int language_id) {
        ChangeSceneClass.DropdownLanguageChange(language_id, true);

        loading_players.SetActive(true);
        loading_players.GetComponent<Image>().sprite = Resources.Load<Sprite>("Other_graphics/" + ChangeSceneClass.language_abbreviation + "/loading");
    }

    static void ExitLobbyStatic() {
        NetworkManager.Singleton.SceneManager.LoadScene("BoardScene", LoadSceneMode.Single);
    }

    void Update() {
        if (nick_sync_coroutine) { return; }

        if (!nick_sync) { nick_sync = nick_info.GetComponent<NicksInfoScript>().nick_sync; }

        if (!nick_sync) {
            nick_info.GetComponent<NicksInfoScript>().AddNick(NetworkManager.Singleton.LocalClientId, nick);

            nick_sync_coroutine = true;
            StartCoroutine(NickSyncCoroutine());
        }
    }


    private IEnumerator NickSyncCoroutine() {
        yield return new WaitForSeconds(2.0f);
        nick_sync_coroutine = false;
    }
}