using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UNET;
using TMPro;
using Unity.Collections;
using System.Text.RegularExpressions;
using System; 
using System.Linq;

public class NicksInfoScript : NetworkBehaviour
{
    public string nicks;
    public bool nick_sync = false;

    void Awake() {
        nicks = "Nicki: ";
    }

    public override void OnNetworkSpawn() {
        nicks = "Nicki: ";
    }

    public void AddNick(ulong clientId, string nick) {
        AddNickServerRpc(clientId, nick);
    }

    [ServerRpc(RequireOwnership = false)]
    void AddNickServerRpc(ulong clientId, string nick) {
        nicks += "\n" + nick;
        AddNickClientRpc(nicks);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        ChangeSceneClass.clients_ids[ChangeSceneClass.clients_number] = clientId;
        ChangeSceneClass.clients_nicks[ChangeSceneClass.clients_number] = nick;
        ChangeSceneClass.clients_number++;

        NickSyncClientRpc(clientRpcParams);
    }

    [ClientRpc]
    void AddNickClientRpc(string synced_nicks) {
        nicks = synced_nicks;
        gameObject.GetComponent<TextMeshProUGUI>().text = nicks.ToString();

        var lines = Regex.Split(nicks.ToString(), "\r\n|\r|\n").Skip(1);

        LobbyTexts.ReplaceText("NickInfo", "\n" + string.Join(Environment.NewLine, lines.ToArray()));
    }

    [ClientRpc]
    void NickSyncClientRpc(ClientRpcParams clientRpcParams = default) {
        nick_sync = true;
    }
}