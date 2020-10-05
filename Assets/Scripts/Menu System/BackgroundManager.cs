using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    #region Public Variables
    public GameObject background;
    public int numberOfElements;
    public float biggestSpriteReferenceSize;
    public Vector2 backgroundElementsSpeed;
    public float backgroundElementsSpeedFactor_MaxValCorrection;
    public float backgroundElementsSpeedFactor_MinVal;
    public float backgroundElementsSpeedFactor_MinValCorrection;
    public float loopPositionOffset;
    public Sprite[] sprites;
    public Vector2 instancePosition;
    #endregion

    #region Private Variables
    private Object pBackgroundElement;
    private GameObject[] backgroundElements;
    private List<float> spriteSizes;
    private float[] backgroundElementsSpeedFactor;
    private Dictionary<GameObject, float> elementHierarchy;
    private List<KeyValuePair<GameObject, float>> sortedElementHierarchy;
    private Coroutine coroutine_BG;
    #endregion

    #region Unity Methods
    private void Start()
    {
        pBackgroundElement = Resources.Load("Background Element", typeof(GameObject));
        backgroundElements = new GameObject[numberOfElements];
        backgroundElementsSpeedFactor = new float[numberOfElements];
        elementHierarchy = new Dictionary<GameObject, float>();

        spriteSizes = new List<float>();

        for (int i = 0; i < sprites.Length; ++i)
        {
            if (!spriteSizes.Contains(sprites[i].rect.width))
            {
                spriteSizes.Add(sprites[i].rect.width);
            }
        }

        spriteSizes.Sort();

        for (int i = 0; i < numberOfElements; ++i)
        {
            backgroundElements[i] = Instantiate(pBackgroundElement, instancePosition, Quaternion.identity) as GameObject;

            if (sprites.Length > 0)
            {
                backgroundElements[i].GetComponent<Image>().sprite = sprites[Random.Range(0, sprites.Length)];
            }

            elementHierarchy.Add(backgroundElements[i], backgroundElements[i].GetComponent<Image>().sprite.rect.width);
        }

        sortedElementHierarchy = (from kv in elementHierarchy orderby kv.Value select kv).ToList();

        for (int i = numberOfElements - 1; i >= 0; --i)
        {
            sortedElementHierarchy[i].Key.transform.SetParent(background.transform);
            sortedElementHierarchy[i].Key.GetComponent<Image>().SetNativeSize();
        }

        for (int i = 0; i < numberOfElements; ++i)
        {
            if (1F - backgroundElements[i].GetComponent<Image>().sprite.rect.width / biggestSpriteReferenceSize >= 1F)
            {
                backgroundElementsSpeedFactor[i] = backgroundElementsSpeedFactor_MaxValCorrection;
            }
            else if (1F - backgroundElements[i].GetComponent<Image>().sprite.rect.width / biggestSpriteReferenceSize <= backgroundElementsSpeedFactor_MinVal)
            {
                backgroundElementsSpeedFactor[i] = backgroundElementsSpeedFactor_MinValCorrection;
            }
            else
            {
                backgroundElementsSpeedFactor[i] = 1F - backgroundElements[i].GetComponent<Image>().sprite.rect.width / biggestSpriteReferenceSize;
            }
        }

        if (numberOfElements > 0)
        {
            coroutine_BG = StartCoroutine(BackgroundGen());
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < backgroundElements.Length; ++i)
        {
            backgroundElements[i].transform.Translate(backgroundElementsSpeed * backgroundElementsSpeedFactor[i]);
        }
    }
    #endregion

    #region Corountines
    private IEnumerator BackgroundGen()
    {
        WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

        for (int i = 0; i < numberOfElements; ++i)
        {
            backgroundElements[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(Random.Range(-background.GetComponent<RectTransform>().rect.width / 2F, background.GetComponent<RectTransform>().rect.width / 2F), Random.Range(-background.GetComponent<RectTransform>().rect.height / 2F, background.GetComponent<RectTransform>().rect.height / 2F));
        }

        for (int i = 0; true; ++i)
        {
            if (i == numberOfElements)
            {
                i = 0;
            }

            if (backgroundElements[i].GetComponent<RectTransform>().anchoredPosition.x + backgroundElements[i].GetComponent<RectTransform>().rect.width / 2F + loopPositionOffset < -background.GetComponent<RectTransform>().rect.width / 2F)
            {
                backgroundElements[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(background.GetComponent<RectTransform>().rect.width / 2F + backgroundElements[i].GetComponent<RectTransform>().rect.width / 2F + loopPositionOffset, Random.Range(-background.GetComponent<RectTransform>().rect.height / 2F, background.GetComponent<RectTransform>().rect.height / 2F));
            }

            yield return fixedWait;
        }
    }
    #endregion
}