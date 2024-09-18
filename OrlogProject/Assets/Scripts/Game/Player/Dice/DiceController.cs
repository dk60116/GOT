using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiceController : MonoBehaviourPunCallbacks
{
    static public DiceController instance;

    public PlayerSet playerSetA, playerSetB;
    public List<Sprite> diceFaceIcons;

    [ReadOnlyInspector]
    public bool selectMode;

    [ReadOnlyInspector]
    public List<BattleDice> confirmedDices, battleZoneBList;
    [SerializeField]
    public List<BattleDice> battleDiceListA, battleDiceListB;

    [ReadOnlyInspector]
    public List<int> confirmedDiceA, confirmedDiceB;

    void Awake()
    {
        instance = this;

        confirmedDiceA = new List<int>();
        confirmedDiceB = new List<int>();

        for (int i = 0; i < 6; i++)
        {
            confirmedDiceA.Add(-1);
            confirmedDiceB.Add(-1);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Time.timeScale = 1f;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            Time.timeScale = 2f;
        else if (Input.GetKeyDown(KeyCode.Alpha0))
            Time.timeScale = 0.2f;
    }

    public void ResetController()
    {
        confirmedDiceA.Clear();
        battleDiceListA.Clear();
        battleDiceListB.Clear();
        battleZoneBList.Clear();

        for (int i = 0; i < 6; i++)
        {
            confirmedDiceA.Add(-1);
            confirmedDiceB.Add(-1);
        }
    }

    public void RollDice()
    {
        PlayerManager.instance.myDice.wall.SetActive(true);

        List<BattleDice> _dice = PlayerManager.instance.myDice.diceList.Where(x => !x.confirmed).ToList();
        List<Vector3> _pos = GetRollSortPos(_dice.Count);

        for (int i = 0; i < _dice.Count; i++)
        {
            _dice[i].transform.localPosition = _pos[i];
            _dice[i].RollDice();
        }

        TurnManager.instance.roleDiceBtn.interactable = false;
        Invoke(nameof(RollCompleteHandler), 3f);
    }

    public void SortAll()
    {
        StopAllCoroutines();
        StartCoroutine(SortAllCoroutine());
    }

    IEnumerator SortAllCoroutine()
    {
        TurnManager.instance.selectStepCount = 3;

        List<BattleDice> _diceA = playerSetA.diceList.Where(x => !x.confirmed).ToList();
        List<Vector3> _posA = GetRollSortPos(_diceA.Count);

        List<BattleDice> _diceB = playerSetB.diceList.Where(x => !x.confirmed).ToList();
        List<Vector3> _posB = GetRollSortPos(_diceB.Count);

        playerSetA.ResetSet();
        playerSetB.ResetSet();

        for (int i = 0; i < _diceA.Count; i++)
        {
            _diceA[i].confirmed = false;
            _diceA[i].battle = false;
            _diceA[i].transform.localPosition = _posA[i];
            _diceA[i].RollDice();
            _diceA[i].seleced = true;
        }

        for (int i = 0; i < _diceA.Count; i++)
        {
            _diceB[i].confirmed = false;
            _diceB[i].battle = false;
            _diceB[i].transform.localPosition = _posB[i];
            _diceB[i].RollDice();
            _diceB[i].seleced = true;
        }

        yield return new WaitForSeconds(3f);

        StartCoroutine(SelectDiceCoroutine(playerSetA, false));
        StartCoroutine(SelectDiceCoroutine(playerSetB, false));

        yield return new WaitForSeconds(3.5f);

        SortToDiceBattleRPC();
    }

    private void RollCompleteHandler()
    {
        TurnManager.instance.roleDiceBtn.interactable = true;
        TurnManager.instance.roleDiceBtn.gameObject.SetActive(false);
        TurnManager.instance.selectBtn.gameObject.SetActive(true);
        
        PlayerManager.instance.myDice.wall.SetActive(false);

        selectMode = true;
        TurnManager.instance.selectBtn.interactable = true;

        foreach (var item in PlayerManager.instance.myDice.diceList)
            item.outline.enabled = !item.confirmed;

        if (TurnManager.instance.selectStepCount == 3)
        {
            selectMode = false;

            List<BattleDice> _dice = PlayerManager.instance.myDice.diceList.Where(x => !x.confirmed).ToList();


            foreach (var item in _dice)
            {
                item.outline.enabled = false;
                item.seleced = true;
            }

            SelectDice();
        }
    }

    public void SelectDice()
    {
        StopCoroutine(SelectDiceCoroutine(PlayerManager.instance.myDice));
        StartCoroutine(SelectDiceCoroutine(PlayerManager.instance.myDice));
    }

    IEnumerator SelectDiceCoroutine(PlayerSet _diceSet, bool _rpc = true)
    {
        selectMode = false;

        TurnManager.instance.selectBtn.interactable = false;

        foreach (var item in _diceSet.diceList)
        {
            if (item.seleced)
            {
                item.ChangeConfirmValue(true);
                confirmedDices.Add(item);
            }

            item.OnOffRigid(false);
        }

        if (_rpc)
            UpdateConfirmFace();

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < _diceSet.diceList.Count; i++)
        {
            BattleDice _dice = _diceSet.diceList[i];

            if (_dice.seleced)
            {
                _dice.transform.DOJump(_diceSet.waitZone[_diceSet.waitDiceCount].position, 0.3f, 1, 0.5f);
                _dice.transform.DORotate(_diceSet.diceList[i].SnapRotationWithCorrection(_diceSet.diceList[i].transform.eulerAngles, true), 0.5f);
                _diceSet.waitDiceCount++;
                _dice.seleced = false;
                _dice.outline.enabled = false;
                _dice.glow.SetActive(false);

                yield return new WaitForSeconds(0.25f);
            }
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var item in _diceSet.diceList)
        {
            item.seleced = false;
            item.outline.enabled = false;
        }

        if (!_rpc)
            yield break;

        if (TurnManager.instance.selectStepCount < 3 || PlayerManager.instance.faction == 0)
        {
            TurnManager.instance.NextTurn();
        }
        else
        {
            photonView.RPC(nameof(SortToDiceBattleRPC), RpcTarget.All);
        }
    }

    [PunRPC]
    private void SortToDiceBattleRPC()
    {
        battleDiceListA = playerSetA.diceList.OrderBy(x => x.crtFace).ToList();
        battleDiceListB = playerSetB.diceList.OrderBy(x => x.crtFace).ToList();

        battleZoneBList = new List<BattleDice>();

        battleZoneBList = Enumerable.Repeat<BattleDice>(null, 12).ToList();

        MachDice(DiceFace.Sword, DiceFace.Armor);
        MachDice(DiceFace.Arrow, DiceFace.Shield);
        MachDice(DiceFace.Armor, DiceFace.Sword);
        MachDice(DiceFace.Shield, DiceFace.Arrow);

        List<BattleDice> _otherDices = battleDiceListB.Where(x => !x.battle).ToList();

        for (int i = 0; i < _otherDices.Count; i++)
        {
            battleZoneBList[i + 6] = _otherDices[i];
            _otherDices[i].battle = true;
        }

        Sequence _sequence = DOTween.Sequence();

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            for (int i = 0; i < battleZoneBList.Count; i++)
            {
                if (battleZoneBList[i] != null)
                    _sequence.Join(battleZoneBList[i].transform.DOJump(playerSetB.battleZone[i].position, 0.3f, 1, 0.5f));
            }
        }

        if (PlayerManager.instance.faction == 0)
        {
            for (int i = 0; i < battleDiceListA.Count; i++)
                _sequence.Join(battleDiceListA[i].transform.DOJump(playerSetA.battleZone[i].position, 0.3f, 1, 0.5f));
        }
        else
        {
            for (int i = 0; i < battleZoneBList.Count; i++)
            {
                if (battleZoneBList[i] != null)
                    _sequence.Join(battleZoneBList[i].transform.DOJump(playerSetB.battleZone[i].position, 0.3f, 1, 0.5f));
            }
        }

        if (PhotonNetwork.IsMasterClient)
            TurnManager.instance.ChangeStep(TurnStep.Power);

        _sequence.AppendInterval(1f);
        _sequence.AppendCallback(() => ExtractGodPowerA());
        _sequence.AppendCallback(() => ExtractGodPowerB());
    }

    private void MachDice(DiceFace _faceA, DiceFace _faceB)
    {
        for (int i = 0; i < battleDiceListA.Count; i++)
        {
            if (battleDiceListA[i].crtFace == _faceA)
            {
                for (int j = 0; j < battleDiceListB.Count; j++)
                {
                    if (!battleDiceListB[j].battle)
                    {
                        if (battleDiceListB[j].crtFace == _faceB)
                        {
                            battleDiceListB[j].battle = true;
                            battleZoneBList[i] = battleDiceListB[j];
                            break;
                        }
                    }
                }
            }
        }
    }

    public void ExtractGodPowerA()
    {
        Sequence _sequence = DOTween.Sequence();

        for (int i = 0; i < battleDiceListA.Count; i++)
        {
            if (battleDiceListA[i].facePower)
            {
                Transform _tokenEffect = battleDiceListA[i].powerTokenEffect;

                _tokenEffect.localPosition = Vector3.zero;
                _tokenEffect.eulerAngles = new Vector3(-90f, 0, -180f);
                _sequence.Join(_tokenEffect.DOMoveY(1.2f, 0.5f));
            }
        }

        _sequence.AppendInterval(1f);

        for (int i = 0; i < battleDiceListA.Count; i++)
        {
            if (battleDiceListA[i].facePower)
            {
                Transform _tokenEffect = battleDiceListA[i].powerTokenEffect;

                _sequence.Join(_tokenEffect.transform.DOMove(playerSetA.powerTokenList[playerSetA.powerTokenCount].transform.position, 0.5f).OnComplete(() =>
                {
                    playerSetA.powerTokenList[playerSetA.powerTokenCount].AbleOrDisable(true);
                    playerSetA.powerTokenCount++;
                }));

                _sequence.AppendCallback(() => _tokenEffect.localPosition = Vector3.zero);
            }
        }

        _sequence.AppendInterval(1f);
        _sequence.AppendCallback(()=> BattleDice());
    }

    public void ExtractGodPowerB()
    {
        Sequence _sequence = DOTween.Sequence();

        for (int i = 0; i < battleDiceListB.Count; i++)
        {
            if (battleDiceListB[i].facePower)
            {
                Transform _tokenEffect = battleDiceListB[i].powerTokenEffect;

                _tokenEffect.localPosition = Vector3.zero;
                _tokenEffect.eulerAngles = new Vector3(-90f, 0, -180f);
                _sequence.Join(_tokenEffect.DOMoveY(1.2f, 0.5f));
            }
        }

        _sequence.AppendInterval(1f);

        for (int i = 0; i < battleDiceListB.Count; i++)
        {
            if (battleDiceListB[i].facePower)
            {
                Transform _tokenEffect = battleDiceListB[i].powerTokenEffect;

                _sequence.Join(_tokenEffect.transform.DOMove(playerSetB.powerTokenList[playerSetB.powerTokenCount].transform.position, 0.5f).OnComplete(() =>
                {
                    playerSetB.powerTokenList[playerSetB.powerTokenCount].AbleOrDisable(true);
                    playerSetB.powerTokenCount++;
                }));
            }
        }
    }


    public void BattleDice()
    {
        if (PhotonNetwork.IsMasterClient)
            TurnManager.instance.ChangeStep(TurnStep.Battle);

        StopCoroutine(BattleDiceCoroutine());
        StartCoroutine(BattleDiceCoroutine());
    }

    IEnumerator BattleDiceCoroutine()
    {
        Debug.LogError("Start Battle");

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < battleDiceListA.Count; i++)
        {
            BattleElement_A(i, DiceFace.Sword, DiceFace.Armor);
            BattleElement_A(i, DiceFace.Arrow, DiceFace.Shield);
            BattleElement_A(i, DiceFace.Armor, DiceFace.Sword);
            BattleElement_A(i, DiceFace.Shield, DiceFace.Arrow);
            BattleElement_A(i, DiceFace.Curse, DiceFace.None);

            yield return new WaitForSeconds(2f);
        }

        for (int i = battleDiceListA.Count; i < battleZoneBList.Count; i++)
        {
            BattleElement_B(i, DiceFace.Sword);
            BattleElement_B(i, DiceFace.Arrow);
            BattleElement_B(i, DiceFace.Armor);
            BattleElement_B(i, DiceFace.Shield);
            BattleElement_B(i, DiceFace.Curse);
            
            yield return new WaitForSeconds(2f);
        }
    }

    private void BattleElement_A(int _i, DiceFace _faceA, DiceFace _faceB)
    {
        Sequence _sequence = DOTween.Sequence();

        _sequence.AppendInterval(1f);

        if (battleDiceListA[_i].crtFace == _faceA)
        {
            if (battleZoneBList[_i] != null)
            {
                if (battleZoneBList[_i].crtFace == _faceB)
                {
                    SpawnBattleEffect(battleDiceListA[_i], (int)_faceA, true);
                    SpawnBattleEffect(battleZoneBList[_i], (int)_faceB, true);
                }
            }
            else
            {
                SpawnBattleEffect(battleDiceListA[_i], (int)_faceA, false);
            }
        }
    }

    private void BattleElement_B(int _i, DiceFace _faceB)
    {
        if (battleZoneBList[_i] != null && battleZoneBList[_i].crtFace == _faceB)
        {
            SpawnBattleEffect(battleZoneBList[_i], (int)_faceB, false);
        }
    }


    private void SpawnBattleEffect(BattleDice _dice, int _battleEffect, bool _block)
    {
        Sequence _sequence = DOTween.Sequence();

        _dice.battleEffectList[_battleEffect].transform.localScale = Vector3.zero;
        _dice.battleEffectList[_battleEffect].transform.DOScale(Vector3.one, 0.3f);

        _sequence.AppendCallback(() =>
        {
            _dice.battleEffectList[_battleEffect].transform.position = _dice.transform.position + Vector3.up * 0.1f;
            _dice.battleEffectList[_battleEffect].transform.eulerAngles = _dice.faction == 0 ? Vector3.zero : Vector3.up * 180f;
            _dice.battleEffectList[_battleEffect].gameObject.SetActive(true);
        });

        PlayerSet _mySet = _dice.faction == 0 ? playerSetA : playerSetB;
        PlayerSet _targetSet = _dice.faction == 0 ? playerSetB : playerSetA;

        switch (_dice.crtFace)
        {
            case DiceFace.Sword:
                if (_block)
                {
                    _sequence.AppendInterval(0.5f);
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DORotate(Vector3.up * (_dice.faction == 0 ? -45f : 225f), 0.4f));
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DORotate(Vector3.up * (_dice.faction == 0 ? 45f : -225f), 0.1f));
                    _sequence.AppendInterval(0.5f);
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOScale(0, 0.3f));
                }
                else
                {
                    _targetSet = _dice.faction == 0 ? playerSetB : playerSetA;

                    _targetSet.templeCount--;

                    Vector3 _target = _dice.faction == 0 ? playerSetB.templeList[playerSetB.templeCount].transform.position : playerSetA.templeList[playerSetA.templeCount].transform.position;

                    _sequence.AppendInterval(0.25f);

                    _sequence.Append(_dice.battleEffectList[_battleEffect].DOLookAt(_target, 0.2f));
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOMove(_target, 0.3f));
                    _sequence.AppendCallback(() =>
                    {
                        _targetSet.templeList[_targetSet.templeCount].AbleOrDisable(false);
                    });
                    _sequence.AppendInterval(1f);
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOScale(0, 0.3f));
                }
                break;
            case DiceFace.Arrow:
                if (_block)
                {
                    _sequence.AppendInterval(0.5f);
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOMoveZ(_dice.battleEffectList[_battleEffect].transform.position.z + (_dice.faction == 0 ? -0.2f : 0.2f), 0.4f));
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOMoveZ(_dice.battleEffectList[_battleEffect].transform.position.z + (_dice.faction == 0 ? 0.05f : -0.05f), 0.1f));
                    _sequence.AppendInterval(0.2f);
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOScale(0, 0.3f));
                }
                else
                {
                    _targetSet = _dice.faction == 0 ? playerSetB : playerSetA;

                    _targetSet.templeCount--;

                    Vector3 _target = _dice.faction == 0 ? playerSetB.templeList[playerSetB.templeCount].transform.position : playerSetA.templeList[playerSetA.templeCount].transform.position;

                    _sequence.AppendInterval(0.25f);

                    _sequence.Append(_dice.battleEffectList[_battleEffect].DOLookAt(_target, 0.2f));
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOMove(_target, 0.3f));
                    _sequence.AppendCallback(() =>
                    {
                        _targetSet.templeList[_targetSet.templeCount].AbleOrDisable(false);
                    });
                    _sequence.AppendInterval(1f);
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOScale(0, 0.3f));
                }
                break;
            case DiceFace.Armor:
                _sequence.AppendInterval(1.5f);
                _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOScale(0, 0.3f));
                break;
            case DiceFace.Shield:
                _sequence.AppendInterval(1.5f);
                _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOScale(0, 0.3f));
                break;
            case DiceFace.Curse:
                if (_targetSet.powerTokenCount <= 0)
                {
                    _sequence.AppendInterval(1.5f);
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOScale(0, 0.3f));
                }
                else
                {
                    _sequence.AppendInterval(0.3f);

                    Vector3 _targetTokenPos = _targetSet.powerTokenList[_targetSet.powerTokenCount - 1].transform.position;
                    _targetTokenPos.y = 1.2f;
                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOJump(_targetTokenPos, 0.3f, 1, 0.5f));

                    _sequence.AppendInterval(0.3f);

                    _sequence.Append(_dice.battleEffectList[_battleEffect].transform.DOScale(0, 0.5f));
                    _sequence.AppendCallback(() =>
                    {
                        _targetSet.powerTokenList[_targetSet.powerTokenCount - 1].AbleOrDisable(false);
                        _mySet.powerTokenList[_mySet.powerTokenCount].AbleOrDisable(true);
                        _targetSet.powerTokenCount--;
                        _mySet.powerTokenCount++;
                    });
                }
                break;
            case DiceFace.None:
                break;
        }
    }

    public void UpdateConfirmFace(bool _rpc = true)
    {
        List<int> _temp = new List<int>();

        for (int i = 0; i < confirmedDices.Count; i++)
            _temp.Add(confirmedDices[i].transform.GetSiblingIndex());

        if (_rpc)
            photonView.RPC(nameof(UpdateConfirmFaceRPC), RpcTarget.All, PlayerManager.instance.faction, _temp.ToArray());
    }

    [PunRPC]
    private void UpdateConfirmFaceRPC(int _faction, int[] _index)
    {
        if (_faction == 0)
        {
            for (int i = 0; i < _index.Length; i++)
                confirmedDiceA[i] = _index[i];
        }
        else
        {
            for (int i = 0; i < _index.Length; i++)
                confirmedDiceB[i] = _index[i];
        }
    }

    public List<Vector3> GetRollSortPos(int _count)
    {
        List<Vector3> _temp = new List<Vector3>();

        switch (_count)
        {
            case 6:
                _temp.Add(new Vector3(-0.15f, 0, 0.075f));
                _temp.Add(new Vector3(0, 0, 0.075f));
                _temp.Add(new Vector3(0.15f, 0, 0.075f));
                _temp.Add(new Vector3(-0.15f, 0, -0.075f));
                _temp.Add(new Vector3(0, 0, -0.075f));
                _temp.Add(new Vector3(0.15f, 0, -0.075f));
                break;
            case 5:
                _temp.Add(new Vector3(-0.075f, 0, 0.075f));
                _temp.Add(new Vector3(0.075f, 0, 0.075f));
                _temp.Add(new Vector3(-0.15f, 0, -0.075f));
                _temp.Add(new Vector3(0, 0, -0.075f));
                _temp.Add(new Vector3(0.15f, 0, -0.075f));
                break;
            case 4:
                _temp.Add(new Vector3(-0.075f, 0, 0.075f));
                _temp.Add(new Vector3(0.075f, 0, 0.075f));
                _temp.Add(new Vector3(-0.075f, 0, -0.075f));
                _temp.Add(new Vector3(0.075f, 0, -0.075f));
                break;
            case 3:
                _temp.Add(new Vector3(-0.15f, 0, 0));
                _temp.Add(new Vector3(0, 0, 0));
                _temp.Add(new Vector3(0.15f, 0, 0));
                break;
            case 2:
                _temp.Add(new Vector3(-0.075f, 0, 0));
                _temp.Add(new Vector3(0.075f, 0, 0));
                break;
            case 1:
                _temp.Add(Vector3.zero);
                break;
        }

        return _temp;
    }
}
