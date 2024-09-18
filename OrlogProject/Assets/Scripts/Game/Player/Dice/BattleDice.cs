using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DiceFace { Sword, Arrow, Armor, Shield, Curse, None}

public class BattleDice : MonoBehaviourPunCallbacks
{
    public int faction;

    [SerializeField]
    private List<SpriteRenderer> icons, blesses;
    public Collider col;
    public Rigidbody rig;

    public List<DiceFace> faceList;
    public List<bool> blessList;

    [SerializeField, ReadOnlyInspector]
    public DiceFace crtFace;
    [SerializeField, ReadOnlyInspector]
    public bool facePower;

    [SerializeField, ReadOnlyInspector]
    private Vector3 orgPos;

    public Outline outline;
    public GameObject glow;

    [ReadOnlyInspector]
    public bool seleced, confirmed, battle;

    public List<Transform> battleEffectList;
    public Transform powerTokenEffect;

    void Awake()
    {
        orgPos = transform.position;
        outline.enabled = false;
        glow.SetActive(false);
    }

    [ContextMenu("SetFace")]
    private void SetFace()
    {
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].sprite = DiceController.instance.diceFaceIcons[(int)faceList[i]];
            blesses[i].enabled = blessList[i];
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        if (faction == 0)
        {
            if (PhotonNetwork.IsMasterClient)
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient)
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
    }

    public void RollDice()
    {
        if (!gameObject.activeSelf)
            return;
        if (confirmed)
            return;

        rig.linearVelocity = Vector3.zero;

        StopAllCoroutines();
        StartCoroutine(RollDiceCoroutine());
    }

    IEnumerator RollDiceCoroutine()
    {
        OnOffRigid(true);

        outline.enabled = false;
        glow.gameObject.SetActive(false);

        transform.DOKill();

        yield return new WaitForSeconds(0.1f);

        rig.angularVelocity = Vector3.zero;

        Vector3 _randomRotation = new Vector3(
            Random.Range(360f, 720f),
            Random.Range(360f, 720f),
            Random.Range(360f, 720f)
        );

        rig.AddForce(Vector3.up * 3f, ForceMode.Impulse);

        yield return new WaitForSeconds(0.3f);

        rig.AddTorque(_randomRotation * 2f);

        yield return new WaitForSeconds(1.2f);

        Quaternion _currentRotation = transform.rotation;

        Vector3 _euler = _currentRotation.eulerAngles;
        _euler = SnapRotationWithCorrection(_euler);

        transform.DORotate(_euler, 0.2f).SetEase(Ease.OutBounce);

        yield return new WaitForSeconds(1f);

        ChangeFaceValue(faceList[GetTopFaceValue()], blessList[GetTopFaceValue()]);
    }

    public Vector3 SnapRotationWithCorrection(Vector3 _euler, bool _y = false)
    {
        _euler.x = SnapToNearest90WithThreshold(_euler.x);
        _euler.z = SnapToNearest90WithThreshold(_euler.z);

        if (_y)
            _euler.y = Random.Range(-5f, 5f);

        return _euler;
    }

    private float SnapToNearest90WithThreshold(float _angle)
    {
        float _snappedAngle = Mathf.Round(_angle / 90f) * 90f;
        float _angleDiff = Mathf.Abs(_snappedAngle - _angle);

        if (_angleDiff > 2.5f)
            return _snappedAngle;

        return _snappedAngle;
    }

    private int GetTopFaceValue()
    {
        float _x = transform.eulerAngles.x;
        float _y = transform.eulerAngles.y;
        float _z = transform.eulerAngles.z;

        if (Tools.Approximation(_x, 0, 3f) && Tools.Approximation(_z, 0, 3f))
            return 0;
        else if (Tools.Approximation(_x, 0, 3f) && Tools.Approximation(_z, 180f, 3f))
            return 1;
        else if (Tools.Approximation(_x, -180f, 3f) && Tools.Approximation(_z, 0, 3f))
            return 1;
        else if (Tools.Approximation(_x, 0, 3f) && Tools.Approximation(_z, 270f, 3f))
            return 2;
        else if (Tools.Approximation(_x, -180f, 3f) && Tools.Approximation(_z, 0, 3f))
            return 2;
        else if (Tools.Approximation(_x, 0, 3f) && Tools.Approximation(_z, -90, 3f))
            return 3;
        else if (Tools.Approximation(_x, 0, 3f) && Tools.Approximation(_z, 90, 3f))
            return 3;
        else if (Tools.Approximation(_x, 90f, 3f) && Tools.Approximation(_z, 0, 3f))
            return 4;
        else
            return 5;
    }

    void OnMouseEnter()
    {
    }

    void OnMouseOver()
    {
        if (!photonView.IsMine)
            return;

        if (DiceController.instance.selectMode)
            outline.OutlineColor = new Color(0.45f, 1f, 0.58f);
    }

    void OnMouseExit()
    {
        if (!photonView.IsMine)
            return;

        if (!seleced)
        {
            glow.gameObject.SetActive(false);
            outline.OutlineColor = Color.white;
        }
    }

    void OnMouseDown()
    {
        if (!photonView.IsMine)
            return;
        if (!DiceController.instance.selectMode)
            return;

        seleced = !seleced;

        outline.enabled = !seleced;
        glow.SetActive(seleced);
    }

    public void OnOffRigid(bool _on)
    {
        photonView.RPC(nameof(OnOffRigiRPC), RpcTarget.All, _on);
    }

    [PunRPC]
    private void OnOffRigiRPC(bool _on)
    {
        col.enabled = _on;
        rig.isKinematic = !_on;
        rig.useGravity = _on;
    }

    public void ChangeFaceValue(DiceFace _face, bool _power)
    {
        photonView.RPC(nameof(ChangeFaceValueRPC), RpcTarget.All, (int)_face, _power);
    }

    [PunRPC]
    private void ChangeFaceValueRPC(int _value, bool _power)
    {
        crtFace = (DiceFace)_value;
        facePower = _power;
    }

    public void ChangeConfirmValue(bool _value)
    {
        photonView.RPC(nameof(ChangeConfirmValueRPC), RpcTarget.All, _value);
    }

    [PunRPC]
    private void ChangeConfirmValueRPC(bool _value)
    {
        confirmed = _value;
    }
}
