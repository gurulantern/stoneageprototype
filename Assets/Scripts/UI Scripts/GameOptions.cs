using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOptions : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _gatherTimeInput;
    [SerializeField]
    private TMP_InputField _paintTimeInput;
    [SerializeField]
    private TMP_InputField _voteTimeInput;
    [SerializeField]
    private TMP_InputField _tireRateInput;
    [SerializeField]
    private TMP_InputField _restRateInput;
    [SerializeField]
    private TMP_InputField _aurochsInput;
    [SerializeField]
    private TMP_InputField _observeReqInput;
    [SerializeField]
    private TMP_InputField _foodMultiplier;
    [SerializeField]
    private TMP_InputField _observeMultiplier;
    [SerializeField]
    private TMP_InputField _createMultiplier;
    [SerializeField]
    private TMP_InputField _night;

    [SerializeField]
    private Toggle _alliances;
    [SerializeField]
    private Toggle _hideTags;
    [SerializeField]
    private Toggle _deadAurochs;

    [SerializeField]
    private Toggle _toggle;

    [SerializeField]
    private List<GameObject> teamObjects;
    [SerializeField]
    private List<TMP_InputField> inputFields;

    [SerializeField]
    private Toggle[] teamToggles;

    [SerializeField]
    private string[] actions = {"steal", "scare", "create"};

    public int InputSelected = -1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKeyDown(KeyCode.LeftShift)) {
            InputSelected--;
            if (InputSelected < 0) InputSelected = 10;
            SelectInputField(InputSelected);
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            InputSelected ++;
            if (InputSelected > 10) InputSelected = 0;
            SelectInputField(InputSelected);
        }
    }

    private void SelectInputField(int selected) {
        if (selected >= 0 && selected <= 10) {
            inputFields[selected].Select();
        } 
    }

    public void GatherSelected() => InputSelected = 0;
    public void PaintSelected() => InputSelected = 1;
    public void VoteSelected() => InputSelected = 2;
    public void TireSelected() => InputSelected = 3;
    public void RestSelected() => InputSelected = 4;
    public void AurochsSelected() => InputSelected = 5;
    public void ObsReqSelected() => InputSelected = 6;
    public void NightSelected() => InputSelected = 7;
    public void FoodXSelected() => InputSelected = 8;
    public void ObsXSelected() => InputSelected = 9;
    public void CreateXSelected() => InputSelected = 10;



    public void SetOptions()
    {
        Debug.Log("Setting options!");
        Dictionary<string, object> options = GetInput();
        ColyseusManager.NetSend("setOptions",
            new OptionsMessage
            {
                userId = ColyseusManager.Instance.CurrentUser.sessionId,
                optionsToSet = options
            }
        );
    }

    public Dictionary<string, object> GetInput()
    {
        Dictionary<string, object> options = new Dictionary<string, object>();
        options.Add("logic", "Competitive");
        options.Add("gatherTime", GatherTime);
        options.Add("paintTime", PaintTime);
        options.Add("voteTime", VoteTime);
        options.Add("foodMulti", FoodMultiplier);
        options.Add("observeMulti", ObserveMultiplier);
        options.Add("createMulti", CreateMultiplier);
        options.Add("aurochs", AurochsInput);
        options.Add("night", Night);
        options.Add("deadAurochs", DeadAurochs);

        options.Add("observeReq", ObserveReqInput);
        options.Add("tireRate", TireRate);
        options.Add("restRate", RestRate);
        options.Add("alliances", Alliances);
        options.Add("hideTags", Tags);
        
        for (int i = 0; i < teamObjects.Count; i ++)
        {
            if (teamObjects[i].activeSelf) {
                teamToggles = teamObjects[i].GetComponentsInChildren<Toggle>();
                for (int j = 0; j < actions.Length; j ++)
                {
                    _toggle = teamToggles[j];
                    options.Add($"team{i}{actions[j]}", ActionToggle);
                }
            }
        }
        return options;
    }

    public void AddTeamOptions(int teamIdx)
    {
        if (teamObjects[teamIdx].activeSelf == false) {
            teamObjects[teamIdx].SetActive(true);
        }
    }

    public void RemoveTeamOptions(int teamIdx)
    {
        if (teamObjects[teamIdx].activeSelf) {
            teamObjects[teamIdx].SetActive(false);
        }
    }

    public string GatherTime
    {
        get
        {
            if (string.IsNullOrEmpty(_gatherTimeInput.text) == false)
            {
                return _gatherTimeInput.text;
            }

            return "60";
        }
    }

    public string PaintTime
    {
        get
        {
            if (string.IsNullOrEmpty(_paintTimeInput.text) == false)
            {
                return _paintTimeInput.text;
            }

            return "60";            
        }
    }

    public string VoteTime
    {
        get
        {
            if (string.IsNullOrEmpty(_voteTimeInput.text) == false)
            {
                return _voteTimeInput.text;
            }

            return "60";            
        }
    }

    public string TireRate
    {
        get
        {
            if (string.IsNullOrEmpty(_tireRateInput.text) == false)
            {
                return _tireRateInput.text;
            }

            return ".01";            
        }
    }

    public string RestRate
    {
        get
        {
            if (string.IsNullOrEmpty(_restRateInput.text) == false)
            {
                return _restRateInput.text;
            }

            return ".05";            
        }
    }

    public string AurochsInput
    {
        get
        {
            if (string.IsNullOrEmpty(_aurochsInput.text) == false)
            {
                return _aurochsInput.text;
            }

            return "8";            
        }
    }

    public string ObserveReqInput
    {
        get
        {
            if (string.IsNullOrEmpty(_observeReqInput.text) == false)
            {
                return _observeReqInput.text;
            }

            return "100";            
        }
    }

    public string FoodMultiplier
    {
        get
        {
            if (string.IsNullOrEmpty(_foodMultiplier.text) == false)
            {
                return _foodMultiplier.text;
            }

            return "3";            
        }
    }

    public string ObserveMultiplier
    {
        get
        {
            if (string.IsNullOrEmpty(_observeMultiplier.text) == false)
            {
                return _observeMultiplier.text;
            }

            return "1";            
        }
    }

    public string CreateMultiplier
    {
        get
        {
            if (string.IsNullOrEmpty(_createMultiplier.text) == false)
            {
                return _createMultiplier.text;
            }

            return "2";            
        }
    }

    public string Alliances
    {
        get
        {
            return _alliances.isOn.ToString();
        }
    }

    public string Tags
    {
        get
        {
            return _hideTags.isOn.ToString();
        }
    }

    public string DeadAurochs
    {
        get
        {
            return _deadAurochs.isOn.ToString();
        }
    }

    public string Night
    {
        get
        {
            if (string.IsNullOrEmpty(_night.text) == false)
            {
                return _night.text;
            }

            return "0";            
        }
    }

    public string ActionToggle
    {
        get
        {
            return _toggle.isOn.ToString();
        }
    }


}
