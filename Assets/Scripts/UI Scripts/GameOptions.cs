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
    private TMP_InputField _foodMultiplier;
    [SerializeField]
    private TMP_InputField _observeMultiplier;
    [SerializeField]
    private TMP_InputField _createMultiplier;

    [SerializeField]
    private Toggle _alliances;
    [SerializeField]
    private Toggle _hideTags;
    [SerializeField]
    private Toggle _toggle;

    [SerializeField]
    private List<GameObject> teamObjects;
    [SerializeField]
    private Toggle[] teamToggles;
    [SerializeField]
    private List<Toggle[]> teamsOptions;
    [SerializeField]
    private string[] actions = {"steal", "scare", "create"};

    public void SetOptions()
    {
        Debug.Log("Setting options!");
        Dictionary<string, string> options = GetInput();
        ColyseusManager.NetSend("setOptions",
            new OptionsMessage
            {
                userId = ColyseusManager.Instance.CurrentUser.sessionId,
                attributesToSet = options
            }
        );
    }

    public Dictionary<string, string> GetInput()
    {
        Dictionary<string, string> options = new Dictionary<string, string>();
        options.Add("gatherTime", GatherTime);
        options.Add("paintTime", PaintTime);
        options.Add("voteTime", VoteTime);
        options.Add("tireRate", TireRate);
        options.Add("foodMulti", FoodMultiplier);
        options.Add("observeMulti", ObserveMultiplier);
        options.Add("createMulti", CreateMultiplier);
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
        if (!teamObjects[teamIdx].activeSelf) {
            teamObjects[teamIdx].SetActive(true);
            teamToggles = (teamObjects[teamIdx].GetComponentsInChildren<Toggle>());
            teamsOptions.Add(teamToggles);
        }
    }

    public void RemoveTeamOptions(int teamIdx)
    {
        if (teamObjects[teamIdx].activeSelf) {
            teamObjects[teamIdx].SetActive(false);
            teamToggles = (teamObjects[teamIdx].GetComponentsInChildren<Toggle>());
            teamsOptions.Remove(teamToggles);
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

            return ".5";            
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

    public string ActionToggle
    {
        get
        {
            return _toggle.isOn.ToString();
        }
    }

    public void ChangeValue()
    {
        
    }

}
