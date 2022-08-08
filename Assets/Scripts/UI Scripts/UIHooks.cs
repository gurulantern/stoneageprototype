using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class UIHooks : MonoBehaviour
{
    [SerializeField] Slider _staminaSlider;
    [SerializeField] Slider _observeSlider;
    [SerializeField] GameObject createControl;
    [SerializeField] TextMeshProUGUI _personalFruitCount;
    [SerializeField] TextMeshProUGUI _personalMeatCount;
    [SerializeField] TextMeshProUGUI _personalWoodCount;
    [SerializeField] TextMeshProUGUI _personalSeedsCount;
    [SerializeField] TextMeshProUGUI _personalFishCount;
    [SerializeField] CreateMenu _createMenu;

    [SerializeField]
    private ProgressContainer progressPrefab;
    private Dictionary<Scorable, ProgressContainer> progressCounters;
    public RectTransform progressRoot;


    
    public CharControllerMulti character;
    // Start is called before the first frame update

    void Awake()
    {
        progressCounters = new Dictionary<Scorable, ProgressContainer>();
 
    }

    public void Initialize(CharControllerMulti player) {
        character = this.gameObject.GetComponent<CharControllerMulti>();
        _staminaSlider.value = character.maxStamina;

        character.ChangedResource += UpdateText;
    }
    
    void Update()
    {
        UpdateProgressCounters();
    }

    void FixedUpdate()
    {
        if (character.IsMine && character != null) //&& GameController.Instance.gamePlaying)
        {
            _staminaSlider.value = character.currentStamina;
        }
    }

    void OnDestroy() 
    {
        if(character != null)
        {
            character.ChangedResource -= UpdateText;
        }
    }
    void OnEnable() 
    {
        BlueprintScript.createObject += AddProgress;
        Scorable.finish += DeleteProgress;
        CharControllerMulti.initProgresses += AddProgresses;
        EnvironmentController.initProgresses += AddNewProgress;
        GameController.onUnlock += UnlockCreateUI;
        //character.ChangedResource += UpdateText;
    }

    void OnDisable()
    {
        BlueprintScript.createObject -= AddProgress;
        Scorable.finish -= DeleteProgress;
        CharControllerMulti.initProgresses -= AddProgresses;
        EnvironmentController.initProgresses -= AddNewProgress;
        GameController.onUnlock -= UnlockCreateUI;
    }

    void UpdateText(int icon) 
    {
        _personalFruitCount.text = character.fruit.ToString();
        _personalMeatCount.text = character.meat.ToString();
        _personalWoodCount.text = character.wood.ToString();
        _personalSeedsCount.text = character.seeds.ToString();
        //_personalFishCount.text = character.fish.ToString();
    }

    public void ToggleCreate()
    {
        if (GameController.Instance.gamePlaying) {
            _createMenu.ToggleMenu();
        }
    }

    public void ToggleMenuButton(int button, bool active)
    {
        _createMenu.SetButtonActive(button, active);
    }

    public void AddProgress(int type, float cost, Scorable scorable)
    {
        AddNewProgress(scorable);
    }

    public void AddProgresses(CharControllerMulti player) {
        foreach (Scorable s in EnvironmentController.Instance.scorables) {
            OnInitObject(s, player);
        }
    }

    public void AddNewProgress(Scorable scorable)
    {
        Debug.Log("Spawning a Progress Counter");
        if (progressCounters.ContainsKey(scorable) == false && scorable.ownerTeam == GameController.Instance.GetTeamIndex(ColyseusManager.Instance.CurrentUser.sessionId)
            && scorable.finished == false) 
        {
            ProgressContainer newProgress = Instantiate(progressPrefab);
            newProgress.transform.SetParent(progressRoot);
            scorable.progressContainer = newProgress;
            scorable.SetProgress();

            progressCounters.Add(scorable, newProgress);
        }
    }

    public void DeleteProgress(Scorable scorable)
    { 
        if (progressCounters.ContainsKey(scorable) == true) { 
            progressCounters.Remove(scorable);   
        }
    }

    private void OnInitObject(Scorable scorable, CharControllerMulti player)
    {
        if (progressCounters.ContainsKey(scorable) == false && scorable.State.finished == false 
            && scorable.ownerTeam == player.TeamIndex) 
        {
            ProgressContainer newProgress = Instantiate(progressPrefab);
            newProgress.transform.SetParent(progressRoot);
            scorable.progressContainer = newProgress;
            scorable.SetProgress();

            progressCounters.Add(scorable, newProgress);
        }
    }

    private void UpdateProgressCounters()
    {
        foreach (KeyValuePair<Scorable, ProgressContainer> pair in progressCounters)
        {
            pair.Value.UpdatePosition(Camera.main.WorldToScreenPoint(pair.Key.transform.position));
        }
    }

    private void UnlockCreateUI(string mostObserved)
    {
        createControl.SetActive(true);
    }
}
