using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOptions : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _roundTimeInput;
    [SerializeField]
    private TMP_InputField _paintTimeInput;
    [SerializeField]
    private TMP_InputField _voteTimeInput;
    [SerializeField]
    private TMP_InputField tireRateInput;
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

}
