using Colyseus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    [SerializeField]
    private Text clientCount = null;

    [SerializeField]
    private Button joinButton = null;

    private RoomSelectionMenu menuRef;

    [SerializeField]
    private TextMeshProUGUI roomName = null;
 
    [SerializeField]
    private TextMeshProUGUI gameMode = null;

    [SerializeField]
    private Image backgroundImage = null;

    [SerializeField]
    private Color coopColor = Color.white;
    [SerializeField]
    private Color deathmatchColor = Color.black;

    private StoneAgeRoomAvailable roomRef;

    public void Initialize(StoneAgeRoomAvailable roomReference, RoomSelectionMenu menu)
    {
        menuRef = menu;
        roomRef = roomReference;
        roomName.text = roomReference.roomId;
        string maxClients = roomReference.maxClients > 0 ? roomReference.maxClients.ToString() : "--";
        clientCount.text = $"{roomReference.clients} / {maxClients}";
        //TODO: if we want to lock rooms, will need to do so here
        if (roomReference.maxClients > 0 && roomReference.clients >= roomReference.maxClients)
        {
            joinButton.interactable = false;
        }
        else
        {
            joinButton.interactable = true;
        }
    }

    public void TryJoin()
    {
        menuRef.JoinRoom(roomRef.roomId);
    }

        private void DetermineMode()
    {
        bool isCoop = roomRef.metadata.isCoop;
        
        if (isCoop)
        {
            roomName.text = roomRef.roomId;
            gameMode.text = "Collaborative";
            gameMode.color = coopColor;
            backgroundImage.color = coopColor;
        }
        else
        {
            roomName.text = roomRef.roomId;
            gameMode.text = "Competitive";
            gameMode.color = deathmatchColor;
            backgroundImage.color = deathmatchColor;
        }
    }
}
