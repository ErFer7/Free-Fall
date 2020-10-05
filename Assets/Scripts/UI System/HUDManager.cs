using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    #region Public Variables
    public PlayerControls playerControlsScript;
    public Text livesDisplay;
    #endregion

    void Start()
    {
        playerControlsScript.OnLifeValueChange += LifeValueChangeHandler;
    }

    private void LifeValueChangeHandler(int val)
    {
        livesDisplay.text = playerControlsScript.lives.ToString();
    }
}
