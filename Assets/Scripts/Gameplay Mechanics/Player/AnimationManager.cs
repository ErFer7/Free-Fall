using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    #region Public Variables
    public Vector2 instancePosition;
    public int numberOfObjects;
    [Header("Colors")]
    public Color extraLifeColor;
    public Color slowMotionColor;
    public Color deathColor;
    #endregion

    #region Private Variables
    private Object pCircularExpansion;
    private GameObject[] circularExpansions;
    private Vector3 playerPositionOnTrigger;
    #endregion

    #region Unity Methods
    private void Start()
    {
        pCircularExpansion = Resources.Load("Circular Expansion", typeof(GameObject));
        circularExpansions = new GameObject[numberOfObjects];

        for (int i = 0; i < numberOfObjects; ++i)
        {
            circularExpansions[i] = Instantiate(pCircularExpansion, instancePosition, Quaternion.identity) as GameObject;
            circularExpansions[i].GetComponent<AnimationEndEvent>().animationManagerScript = this;
            circularExpansions[i].GetComponent<AnimationEndEvent>().isAnimating = false;
        }
    }

    private void FixedUpdate()
    {
        playerPositionOnTrigger = gameObject.transform.position;
    }
    #endregion

    #region Methods
    public void AnimationTrigger(GameObject triggerObject)
    {
        switch (triggerObject.tag)
        {
            case "Death":

                for (int i = 0; i < numberOfObjects; ++i)
                {
                    if (!circularExpansions[i].GetComponent<AnimationEndEvent>().isAnimating)
                    {
                        circularExpansions[i].transform.position = playerPositionOnTrigger;
                        circularExpansions[i].GetComponent<SpriteRenderer>().color = deathColor;
                        circularExpansions[i].GetComponent<Animator>().Play("Circular Expansion", 0, 0F);
                        circularExpansions[i].GetComponent<AnimationEndEvent>().isAnimating = true;
                        break;
                    }
                }
                break;

            case "Life":

                for (int i = 0; i < numberOfObjects; ++i)
                {
                    if (!circularExpansions[i].GetComponent<AnimationEndEvent>().isAnimating)
                    {
                        circularExpansions[i].transform.position = triggerObject.transform.position;
                        circularExpansions[i].GetComponent<SpriteRenderer>().color = extraLifeColor;
                        circularExpansions[i].GetComponent<Animator>().Play("Circular Expansion", 0, 0F);
                        circularExpansions[i].GetComponent<AnimationEndEvent>().isAnimating = true;
                        break;
                    }
                }
                break;

            case "Time":

                for (int i = 0; i < numberOfObjects; ++i)
                {
                    if (!circularExpansions[i].GetComponent<AnimationEndEvent>().isAnimating)
                    {
                        circularExpansions[i].transform.position = triggerObject.transform.position;
                        circularExpansions[i].GetComponent<SpriteRenderer>().color = slowMotionColor;
                        circularExpansions[i].GetComponent<Animator>().Play("Circular Expansion", 0, 0F);
                        circularExpansions[i].GetComponent<AnimationEndEvent>().isAnimating = true;
                        break;
                    }
                }
                break;

            default:
                break;
        }
    }

    public void AnimationEnd(GameObject gameObject)
    {
        for (int i = 0; i < numberOfObjects; ++i)
        {
            if (circularExpansions[i] == gameObject)
            {
                circularExpansions[i].transform.position = instancePosition;
                circularExpansions[i].GetComponent<AnimationEndEvent>().isAnimating = false;
            }
        }
    }
    #endregion
}