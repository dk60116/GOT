using DG.Tweening;
using UnityEngine;

public class TemplePice : MonoBehaviour
{
    public GameObject body;
    [SerializeField]
    private MeshRenderer render;

    [ContextMenu("Init")]
    private void Init()
    {
        body = transform.GetChild(0).gameObject;
        render = body.GetComponent<MeshRenderer>();
    }

    public void AbleOrDisable(bool _on)
    {
        render.DOKill();

        body.SetActive(true);

        if (_on)
        {
            transform.localEulerAngles = new Vector3(0, Random.Range(-5f, 5f), 0);
            render.material.color = new Color(1f, 1f, 1f, 0);
            render.material.DOFade(1f, 0.5f);
        }
        else
        {
            render.material.color = Color.white;
            render.material.DOFade(0, 0.5f).OnComplete(() => body.SetActive(false));
        }
    }
}
