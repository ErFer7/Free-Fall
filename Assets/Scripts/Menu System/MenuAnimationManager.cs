using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuAnimationManager : MonoBehaviour
{
    #region Public Variables
    public Image blackScreenImage;
    public float crossFadeTime;
    #endregion

    #region Private Variables
    private Coroutine coroutine_CFBSA;
    #endregion

    #region Unity Methods
    private void Start()
    {
        coroutine_CFBSA = StartCoroutine(CrossFadeBlackScreenAlpha(0F, crossFadeTime));
    }
    #endregion

    #region Coroutines
    private IEnumerator CrossFadeBlackScreenAlpha(float alpha, float time, float convergenceCriteria = 0.025F)
    {
        blackScreenImage.CrossFadeAlpha(alpha, time, true);

        while (true)
        {
            if (Math.Abs(alpha - blackScreenImage.canvasRenderer.GetColor().a) < convergenceCriteria)
            {
                break;
            }

            yield return null;
        }

        blackScreenImage.gameObject.SetActive(false);

        yield return null;
    }
    #endregion
}