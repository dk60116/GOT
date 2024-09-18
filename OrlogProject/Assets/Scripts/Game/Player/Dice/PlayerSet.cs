using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSet : MonoBehaviour
{
    public List<BattleDice> diceList;
    public GameObject wall;
    public List<Transform> waitZone, battleZone;
    public int waitDiceCount;
    public List<PowerToken> powerTokenList;
    public List<TemplePice> templeList;
    public int powerTokenCount, templeCount;
    public List<GodCard> cardList;

    void Start()
    {
        templeCount = 15;
        ResetSet();
    }

    public void ResetSet()
    {
        wall.gameObject.SetActive(false);

        waitDiceCount = 0;

        foreach (var item in diceList)
        {
            item.seleced = false;
            item.confirmed = false;
            item.battle = false;
            item.crtFace = DiceFace.None;
            item.powerTokenEffect.transform.localPosition = Vector3.zero;
            item.powerTokenEffect.transform.localRotation = Quaternion.identity;

            foreach (var item1 in item.battleEffectList)
                item1.gameObject.SetActive(false);
        }
    }
}
