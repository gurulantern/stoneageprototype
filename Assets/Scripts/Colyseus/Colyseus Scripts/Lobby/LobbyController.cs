using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class LobbyController : MonoBehaviour
{
    [SerializeField]
    private GameObject connectingCover = null;
    
    [SerializeField]
    private Toggle collabToggle = null;

    [SerializeField]
    private CreateUserMenu createUserMenu = null;

    //Variables to initialize the room controller
    public string roomName = "my_room";
    public string gameSceneName = "Stone Age";
    protected Dictionary<string, object> roomOptions = null;

    [SerializeField]
    private RoomSelectionMenu selectRoomMenu = null;

    private void Awake()
    {
        createUserMenu.gameObject.SetActive(true);
        selectRoomMenu.gameObject.SetActive(false);
        connectingCover.SetActive(true);
    }

    private IEnumerator Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        while (!ColyseusManager.IsReady)
        {
            yield return new WaitForEndOfFrame();
        }

        ColyseusManager.Instance.Initialize(roomName, roomOptions);
        ColyseusManager.onRoomsReceived += OnRoomsReceived;
        connectingCover.SetActive(false);
    }

    private void OnDestroy()
    {
        ColyseusManager.onRoomsReceived -= OnRoomsReceived;
    }

    public void CreateUser()
    {
        string desiredUserName = createUserMenu.UserName;
        PlayerPrefs.SetString("UserName", desiredUserName);

        ColyseusSettings clonedSettings = ColyseusManager.Instance.CloneSettings();
        clonedSettings.colyseusServerAddress = createUserMenu.ServerURL;
        clonedSettings.colyseusServerPort = createUserMenu.ServerPort;
        clonedSettings.useSecureProtocol= createUserMenu.UseSecure;

        ColyseusManager.Instance.OverrideSettings(clonedSettings);

        ColyseusManager.Instance.InitializeClient();

        ColyseusManager.Instance.UserName = desiredUserName;

        //Do user creation stuff
        createUserMenu.gameObject.SetActive(false);
        selectRoomMenu.gameObject.SetActive(true);
        selectRoomMenu.GetAvailableRooms();
    }

    public void CreateRoom()
    {
        connectingCover.SetActive(true);
        string desiredRoomName = selectRoomMenu.RoomCreationName;
        string gameModeLogic = collabToggle.isOn ? "collaborative" : "competitive";
        roomOptions = new Dictionary<string, object> {{"logic", gameModeLogic}};
        if (!string.IsNullOrEmpty(desiredRoomName))
        {
            LoadMainScene(() => { ColyseusManager.Instance.CreateNewRoom(desiredRoomName, roomOptions); });
        }
    }

    public void JoinOrCreateRoom()
    {
        connectingCover.SetActive(true);
        LoadMainScene(() => { ColyseusManager.Instance.JoinOrCreateRoom(); });
    }

    public void JoinRoom(string id)
    {
        connectingCover.SetActive(true);
        LoadMainScene(() => { ColyseusManager.Instance.JoinExistingRoom(id); });
    }

    public void OnConnectedToServer()
    {
        connectingCover.SetActive(false);
    }

    private void OnRoomsReceived(ColyseusRoomAvailable[] rooms)
    {
        selectRoomMenu.HandRooms(rooms);
    }

    private void LoadMainScene(Action onComplete)
    {
        StartCoroutine(LoadSceneAsync(gameSceneName, onComplete));
    }

    private IEnumerator LoadSceneAsync(string scene, Action onComplete)
    {        
        Scene currScene = SceneManager.GetActiveScene();        
        AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        while (op.progress <= 0.9f)
        {
            //Wait until the scene is loaded
            yield return new WaitForEndOfFrame();
        }

        onComplete.Invoke();
        op.allowSceneActivation = true;
        connectingCover.SetActive(false);
        SceneManager.UnloadSceneAsync(currScene);
    }
}

