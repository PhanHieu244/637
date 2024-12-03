using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

    public static LevelManager _instance;

    private void Awake() {
        _instance = this;
    }

    public GameObject[] levels; //Levels on the map
    public LevelPrefab[] levelPrefabs; //Level prefabs for the levels

    public GameObject levelWindow;
    public Text levelText, targetText;

    int selectedLevel;

    public GameObject[] hideInGame; //The gameobjects to hide during gameplay
    public GameObject gameUi;

    public GameObject energyWindow;

    public GameObject indicator; //Indicator gameobject on the map which shows the current level

    public GameObject shop;

    private void Start() {
        SetMap();
    }

    public void SetMap() {
        int highestLevel = PlayerPrefs.GetInt("HIGHEST_LEVEL", 1);

        for (int i = 0; i < levels.Length; i++) {
            if (highestLevel > i) {
                levels[i].GetComponentInChildren<Text>().text = i + 1 + "";
                levels[i].GetComponent<Button>().interactable = true;

                levels[i].GetComponent<Button>().onClick.RemoveAllListeners();

                int n = i;
                levels[i].GetComponent<Button>().onClick.AddListener(() => OnClickLevel(n));
            } else {
                levels[i].GetComponentInChildren<Text>().text = "";
                levels[i].GetComponent<Button>().interactable = false;
            }
        }

        indicator.GetComponent<RectTransform>().anchoredPosition =
            levels[highestLevel - 1].GetComponent<RectTransform>().anchoredPosition + Vector2.up * 100;

    }

    void OnClickLevel(int num) {
        OnClickFade(levelWindow);

        selectedLevel = num;

        levelText.text = "LEVEL " + (selectedLevel + 1).ToString();

        if (levelPrefabs[selectedLevel].gameType == GameManager.GameType.Score)
            targetText.text = "Reach " + levelPrefabs[selectedLevel].scoreToReach +
                " points";

        if (levelPrefabs[selectedLevel].gameType == GameManager.GameType.CollectTarget)
            targetText.text = "Collect the targets";

        if (levelPrefabs[selectedLevel].gameType == GameManager.GameType.Escape)
            targetText.text = "Rescue all the animals";
    }

    IEnumerator Fade(GameObject g) {
        float t = 0;
        CanvasGroup cg = g.GetComponent<CanvasGroup>();

        while (t < 1) {
            t += Time.deltaTime * 2;
            cg.alpha = t;

            yield return null;
        }

        g.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void OnClickFade(GameObject g) {
        StartCoroutine(Fade(g));
    }

    public void Play() {
        if (EnergySystem._instance.currentEnergy < 1) {
            energyWindow.GetComponent<Canvas>().enabled = true;
            energyWindow.GetComponent<ScaleAnim>().Anim();

            return;
        }

        foreach (GameObject g in hideInGame)
            g.SetActive(false);

        gameUi.SetActive(true);

        Instantiate(levelPrefabs[selectedLevel]);

        if (EnergySystem._instance.alwaysDecreaseEnergy)
            EnergySystem._instance.AddEnergy(-1);

        levelWindow.GetComponent<CanvasGroup>().blocksRaycasts = false;
        levelWindow.GetComponent<CanvasGroup>().alpha = 0;
    }

    public int GetCurrentLevel() {
        return selectedLevel;
    }
}
