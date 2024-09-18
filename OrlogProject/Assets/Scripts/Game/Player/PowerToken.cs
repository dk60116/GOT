using DG.Tweening;
using UnityEngine;

public class PowerToken : MonoBehaviour
{
    public GameObject body;
    [SerializeField]
    private MeshRenderer render;

    private void Awake()
    {
        body.SetActive(false);
    }

    public void AbleOrDisable(bool _on)
    {
        body.gameObject.SetActive(_on);

        if (_on)
        {
            transform.localEulerAngles = new Vector3(0, Random.Range(-5f, 5f), 0);
            render.material.color = new Color(1f, 1f, 1f, 0);
            render.material.DOFade(1f, 1f);
        }
        else
        {
            render.material.color = Color.white;
            render.material.DOFade(0, 1f);
        }
    }
}
