using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Button loadButton;
    [SerializeField] Button saveButton;

    [SerializeField] GameObject loadScreen;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject inGameUI;

    [SerializeField] World worldScript;
    [SerializeField] GameObject player;


    void Awake()
    {
        player.SetActive(false);
        loadButton.interactable = SaveLoad.LoadWorld();
    }

    public void NewGameButton()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SaveLoad.DeleteTmpChunkData();
        saveButton.interactable = true;
        mainMenu.SetActive(false);
        loadScreen.SetActive(true);
        worldScript.NewGame();
    }

    public void SaveGameButton()
    {
        if (!loadButton.interactable)
        {
            loadButton.interactable = true;
        }
        worldScript.SaveGame();
    }

    public void LoadGameButton()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SaveLoad.DeleteTmpChunkData();
        saveButton.interactable = true;
        mainMenu.SetActive(false);
        loadScreen.SetActive(true);
        worldScript.LoadGame();
    }

    public void QuitButton()
    {
        SaveLoad.DeleteTmpChunkData();
        Application.Quit();
    }

    private void Update()
    {
        if (worldScript.finishedLoading && Input.GetKeyDown(KeyCode.Escape))
        {
            mainMenu.SetActive(!mainMenu.activeSelf);
            player.SetActive(!player.activeSelf);
            inGameUI.SetActive(!inGameUI.activeSelf);
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
