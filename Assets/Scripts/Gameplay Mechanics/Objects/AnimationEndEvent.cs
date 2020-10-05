using UnityEngine;

public class AnimationEndEvent : MonoBehaviour
{
    #region Public Variables
    #region Editor Acessible
    public AnimationManager animationManagerScript;
    #endregion

    public bool isAnimating;
    #endregion

    #region Methods
    public void AnimationEndTrigger()
    {
        animationManagerScript.AnimationEnd(gameObject);
    }
    #endregion
}
