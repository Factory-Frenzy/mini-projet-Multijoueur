using UnityEngine;

public class HunterController : ClassController
{
    public override void Activate()
    {
        gameObject.SetActive(true);
        _camera.transform.SetParent(transform);
        _camera.transform.localPosition = new Vector3(-0.4f, 0.85f, -1.4f);
        ResetAnimator();
    }

    public override void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
