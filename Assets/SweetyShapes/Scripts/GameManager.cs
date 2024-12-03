using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager _instance;

    public enum GameType { CollectTarget, Score, Escape }
    public enum BoosterType { ITEM_SHUFFLER, MORE_MOVES, BOMB }

    public GameObject[] items; //The items for the gamefield
    public GameObject[] animals; //for the ESCAPE game type

    public GameObject[] explosionEffects;

    public Text movesText;
    int currentMoves = 10; //how many moves the player has

    LevelPrefab level; //the current level

    [HideInInspector]
    public int[] targets;

    public TargetUI[] targetUIs;

    public Booster[] boosters;

    public GameObject winPanel, loosePanel;

    public bool canClickItem = true; //if it is false, the player cannot click on the items

    [HideInInspector]
    public bool isBombActivated;
    public GameObject bombTopBar;

    [Header("Score Target")]
    public GameObject scoreBar;
    public Image fillImage;
    public Text percentText;
    int currentScore;

    //variables for the ESCAPE game type
    [HideInInspector]
    public int percentToSpawnEscapeTarget;
    [HideInInspector]
    public int animalNum;


    private void Awake() {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        targets = new int[3];
    }

    // Update is called once per frame
    void Update() {

    }

    /// <summary>
    /// Gets the random explosion effect from a public array.
    /// </summary>
    /// <returns>The random explosion.</returns>
    public GameObject GetRandomExplosion() {
        return explosionEffects[Random.Range(0, explosionEffects.Length)];
    }

    public void DecreaseMoves(int n = 1) {
        currentMoves -= n;
        movesText.text = currentMoves + "";

        canClickItem = false;
        StartCoroutine(DelayedGameOverCheck());
    }

    IEnumerator DelayedGameOverCheck() {
        yield return new WaitForSeconds(.25f);

        if (IsTargetCompleted()) {
            winPanel.SetActive(true);

            if (PlayerPrefs.GetInt("HIGHEST_LEVEL", 1) == LevelManager._instance.GetCurrentLevel() + 1) {
                PlayerPrefs.SetInt("HIGHEST_LEVEL", PlayerPrefs.GetInt("HIGHEST_LEVEL", 1) + 1);
                CurrencyManager._instance.AddCoins(25);
            }

            yield break;
        }

        if (currentMoves == 0) { // GAME OVER
            loosePanel.SetActive(true);
            canClickItem = false;

            if (!EnergySystem._instance.alwaysDecreaseEnergy)
                EnergySystem._instance.AddEnergy(-1);

            yield break;
        }

        canClickItem = true;
    }

    /// <summary>
    /// Called on the winPanel or loosePanel if you click the NEXT button
    /// </summary>
    public void OnClickNext() {
        Destroy(level.gameObject);

        LevelManager._instance.SetMap();

        winPanel.SetActive(false);
        loosePanel.SetActive(false);

        foreach (GameObject g in LevelManager._instance.hideInGame)
            g.SetActive(true);

        LevelManager._instance.gameUi.SetActive(false);
    }

    public void SetLevel(LevelPrefab l) {
        level = l;

        currentMoves = l.allMoves;
        movesText.text = currentMoves + "";

        switch (level.gameType) {
            case GameType.CollectTarget:
                for (int i = 0; i < targetUIs.Length; i++) {
                    scoreBar.SetActive(false);

                    if (level.targetCount > i) {
                        targetUIs[i].gameObject.SetActive(true);

                        targetUIs[i].remainingItemNum = level.targetNums[i];
                        targetUIs[i].amountText.text = level.targetNums[i] + "";

                        targetUIs[i].picture.sprite = items[i].GetComponent<SpriteRenderer>().sprite;
                    } else
                        targetUIs[i].gameObject.SetActive(false);
                }
                break;

            case GameType.Score:
                foreach (TargetUI t in targetUIs) {
                    t.gameObject.SetActive(false);
                }

                scoreBar.SetActive(true);
                fillImage.fillAmount = 0;
                percentText.text = "0%";
                break;

            case GameType.Escape:
                for (int i = 0; i < targetUIs.Length; i++) {
                    scoreBar.SetActive(false);

                    if (level.targetCount > i) {
                        targetUIs[i].gameObject.SetActive(true);

                        targetUIs[i].remainingItemNum = level.targetNums[i];
                        targetUIs[i].amountText.text = level.targetNums[i] + "";

                        targetUIs[i].picture.sprite = animals[i].GetComponent<SpriteRenderer>().sprite;
                    } else
                        targetUIs[i].gameObject.SetActive(false);
                }
                break;
        }

        percentToSpawnEscapeTarget = level.percentToSpawnAnimal;
        animalNum = level.targetCount;

        currentScore = 0;
    }

    /// <summary>
    /// Called from TargetAnimal.cs
    /// </summary>
    public void CheckTargetDone() {
        if (IsTargetCompleted()) {
            winPanel.SetActive(true);

            if (PlayerPrefs.GetInt("HIGHEST_LEVEL", 1) == LevelManager._instance.GetCurrentLevel() + 1) {
                PlayerPrefs.SetInt("HIGHEST_LEVEL", PlayerPrefs.GetInt("HIGHEST_LEVEL", 1) + 1);
                CurrencyManager._instance.AddCoins(25);
            }
        }
    }

    /// <summary>
    /// Checking if the target is completed or not
    /// </summary>
    bool IsTargetCompleted() {
        if (level.gameType == GameType.CollectTarget || level.gameType == GameType.Escape) {
            for (int i = 0; i < level.targetCount; i++)
                if (!targetUIs[i].IsCompleted()) return false;

            return true;
        }

        if (level.gameType == GameType.Score) {
            if (level.scoreToReach <= currentScore)
                return true;
        }

        return false;
    }

    public void DecreaseTargetCollection(int id) {
        if (level.gameType != GameType.CollectTarget && level.gameType != GameType.Escape) return;

        for (int i = 0; i < level.targetCount; i++) {
            if (i == id)
                targetUIs[i].DecreaseItemNum();
        }
    }

    public void IncreaseScore() {
        if (level.gameType != GameType.Score) return;

        currentScore += 10;
    }

    /// <summary>
    /// Fill animation for the fillBar
    /// </summary>
    IEnumerator FillUp(int prevProgression) {
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime * 1.5f;
            fillImage.fillAmount = Mathf.Lerp(prevProgression, currentScore, t) / level.scoreToReach;
            percentText.text = (int)(fillImage.fillAmount * 100) + "%";

            yield return null;
        }

        fillImage.fillAmount = currentScore / (float)level.scoreToReach;
        percentText.text = (int)(fillImage.fillAmount * 100) + "%";
    }

    public void DoFillUp() {
        if (level.gameType == GameType.Score) {
            StartCoroutine(FillUp(currentScore));
        }
    }

    public GameType GetGameType() {
        return level.gameType;
    }
}
