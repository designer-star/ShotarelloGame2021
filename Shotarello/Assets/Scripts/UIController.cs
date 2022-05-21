using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;


public class UIController : MonoBehaviour
{

    //-----------------------------------------------------------------acsess from others scripts-----------------------------------------------------
    public static UIController instance;//very usful thing to acsess in this script from others scripts //static we cant change
    private void Awake()
    {
        instance = this;
    }
    //-----------------------------------------------------------------acsess from others scripts-----------------------------------------------------

    public TMP_Text overheatedMessage;
    public Slider weaponTempSlider;


    public GameObject deathScreen;
    public TMP_Text deathText;

    public Slider healthSlider;

    public TMP_Text killsText, deathsText;


    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;

    public GameObject endScreen;

    public TMP_Text timerText;


    public GameObject optionScreen;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptiond();
        }

        if(optionScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ShowHideOptiond()
    {
        if (!optionScreen.activeInHierarchy)
        {
            optionScreen.gameObject.SetActive(true);
        }
        else
        {
            optionScreen.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
