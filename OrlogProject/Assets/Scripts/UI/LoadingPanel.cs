using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour
{
    [SerializeField]
    Image bg;

    void OnEnable()
    {
        bg.color = new Color(0.0039f, 0.070f, 0.078f, 0);

        bg.DOFade(1f, 0.5f);
    }
}
