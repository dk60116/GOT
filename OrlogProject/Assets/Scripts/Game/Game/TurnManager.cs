using DG.Tweening;
using ExitGames.Client.Photon;
using NUnit.Framework;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum TurnStep { Prepare, Role, Power, Ability, Battle, Final}

public class TurnManager : MonoBehaviourPunCallbacks
{
    static public TurnManager instance;

    public TurnStep step;
    public int firstFaction, nextFaction, crtFaction;

    public float crtTImer;

    public Button roleDiceBtn, selectBtn;

    [SerializeField, ReadOnlyInspector]
    private float timerDuration;
    [SerializeField, ReadOnlyInspector]
    private double startTime;
    [SerializeField]
    private TextMeshProUGUI timerText;
    [SerializeField, ReadOnlyInspector]
    public bool isTimerRunning;

    public int selectStepCount;

    public Rigidbody coin;
    public List<Transform> coinPos;

    [SerializeField]
    private CanvasGroup turnBox;
    [SerializeField]
    private Image turnIcon;
    [SerializeField]
    private TextMeshProUGUI turnText;
    [SerializeField]
    private List<Sprite> turnIconList;

    void Awake()
    {
        instance = this;

        roleDiceBtn.gameObject.SetActive(false);
        timerText.text = 0.ToString();
        turnBox.alpha = 0;
    }

    void Update()
    {
        if (isTimerRunning)
            UpdateTimer();
    }

    public void ChangeStep(TurnStep _step)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("TurnCange" + ": " + _step);

            Hashtable _newProperties = new Hashtable();
            _newProperties.Add("TurnStep", (int)_step);
            
            if (_step != TurnStep.Prepare)
                _newProperties.Add("TurnFaction", firstFaction);

            PhotonNetwork.CurrentRoom.SetCustomProperties(_newProperties);
        }
    }

    public void NextTurn()
    {
        if (crtFaction == firstFaction)
        {
            Hashtable _newProperties = new Hashtable();
            _newProperties.Add("TurnFaction", nextFaction);
            PhotonNetwork.CurrentRoom.SetCustomProperties(_newProperties);
        }
        else if (crtFaction == nextFaction)
        {
            Hashtable _newProperties = new Hashtable();
            _newProperties.Add("TurnFaction", firstFaction);
            PhotonNetwork.CurrentRoom.SetCustomProperties(_newProperties);
        }
    }

    public void ResetTimer(float _duration)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartTimer(_duration);
        }
    }

    public void StartTimer(float _duration)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        startTime = PhotonNetwork.Time;
        timerDuration = _duration;

        photonView.RPC(nameof(RPC_StartTimer), RpcTarget.AllBuffered, startTime, timerDuration);
    }

    [PunRPC]
    void RPC_StartTimer(double _networkStartTime, float _duration)
    {
        startTime = _networkStartTime;
        timerDuration = _duration;
        isTimerRunning = true;
    }

    void UpdateTimer()
    {
        double _elapsedTime = PhotonNetwork.Time - startTime;
        float _remainingTime = timerDuration - (float)_elapsedTime;

        if (_remainingTime <= 0)
        {
            isTimerRunning = false;
            //TimerEnded();
        }
        else
        {
            DisplayTime(_remainingTime);
        }
    }

    private void DisplayTime(float _time)
    {
        int _seconds = Mathf.FloorToInt(_time);

        timerText.text = _seconds.ToString("00");
    }

    private void StepChangeHandler(TurnStep _step)
    {
        Debug.LogError($"Step: {_step}");

        turnBox.alpha = 0;
        turnIcon.sprite = turnIconList[(int)_step];
        turnText.text = $"{_step} Step";

        Sequence _sequence = DOTween.Sequence();
        _sequence.Append(turnBox.DOFade(1f, 1f));
        _sequence.AppendInterval(0.5f);
        _sequence.Append(turnBox.DOFade(0, 1f));

        switch (_step)
        {
            case TurnStep.Prepare:
                if (PhotonNetwork.IsMasterClient)
                    Cointoss();
                break;
            case TurnStep.Role:
                break;
            case TurnStep.Ability:
                break;
            case TurnStep.Final:
                break;
        }
    }

    private void TurnChangeHandler(int _faction)
    {
        Debug.LogError("TurnChangeHandler: " + _faction);

        roleDiceBtn.gameObject.SetActive(false);
        selectBtn.gameObject.SetActive(false);

        bool _syinc = PlayerManager.instance.faction == _faction;
        DiceController.instance.selectMode = false;

        switch (step)
        {
            case TurnStep.Prepare:
                break;
            case TurnStep.Role:
                ResetTimer(30f);
                roleDiceBtn.gameObject.SetActive(_syinc);
                roleDiceBtn.interactable = true;
                if (_faction == firstFaction)
                    selectStepCount++;
                break;
            case TurnStep.Ability:
                break;
            case TurnStep.Final:
                break;
            default:
                break;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            coin.useGravity = false;
            coin.isKinematic = true;

            int _factions = _faction == 0 ? 1 : 0;

            coin.transform.DOJump(coinPos[_factions].position, 0.3f, 1, 0.5f);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (PhotonNetwork.IsMasterClient)
            ChangeStep(TurnStep.Prepare);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("TurnStep"))
        {
            int _step = (int)propertiesThatChanged["TurnStep"];
            Debug.Log("Game State Updated: " + _step);

            step = (TurnStep)_step;
            
            StepChangeHandler(step);
        }
        if (propertiesThatChanged.ContainsKey("TurnFaction"))
        {
            int _faction = (int)propertiesThatChanged["TurnFaction"];
            Debug.Log("Turn Updated: " + _faction);

            crtFaction = _faction;

            TurnChangeHandler(_faction);
        }
    }

    public void Cointoss()
    {
        Debug.LogError("CoinToss");
        StopCoroutine(CointossCoroutine());
        StartCoroutine(CointossCoroutine());
    }

    IEnumerator CointossCoroutine()
    {
        coin.transform.DOKill();

        coin.transform.position = Vector3.up * 1.091847f;
        coin.linearVelocity = Vector3.zero;
        coin.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(2f);

        int _random = Random.Range(0, 2);

        coin.transform.eulerAngles = Vector3.right * (_random == 0 ? 0 : 180f);

        coin.AddForce(Vector3.up * 150f);

        yield return new WaitForSeconds(0.2f);

        coin.AddTorque(Vector3.right * Random.Range(3200f, 6000f));

        yield return new WaitForSeconds(1.2f);

        int _random1 = Random.Range(0, 2);
        coin.AddForce(Vector3.forward * (_random1 == 0 ? -100f : 100f));

        yield return new WaitForSeconds(0.5f);

        if (Tools.Approximation(coin.transform.eulerAngles.z, 0, 3f))
        {
            UpdateFirstFaction(0);
        }
        if (Tools.Approximation(coin.transform.eulerAngles.z, 180f, 3f))
        {
            UpdateFirstFaction(1);
        }

        yield return new WaitForSeconds(1f);

        ChangeStep(TurnStep.Role);
    }

    private void UpdateFirstFaction(int _faction)
    {
        photonView.RPC(nameof(UpdateFirstFactionRPC), RpcTarget.All, _faction);
    }

    [PunRPC]
    private void UpdateFirstFactionRPC(int _faction)
    {
        if (_faction == 0)
        {
            firstFaction = 0;
            nextFaction = 1;
        }
        else
        {
            firstFaction = 1;
            nextFaction = 0;
        }
    }
}
