using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicPanel : MonoBehaviour
{
    [SerializeField]
    protected GameObject background;

    protected virtual void OnEnable()
    {
        background.SetActive(true);
    }

    protected virtual void OnDisable()
    {
        background.SetActive(false);
    }
}
