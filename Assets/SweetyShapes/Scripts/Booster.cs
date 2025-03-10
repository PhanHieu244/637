﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Booster : MonoBehaviour {

    public TimedRewards.RewardType type;

    int currentCount;
    public Text countText;

    public int price; //Price of the booster in the shop

    // Start is called before the first frame update
    void Start() {
        currentCount = PlayerPrefs.GetInt("BOOSTER_" + type, 0);
        countText.text = "x" + currentCount;
    }

    public void AddItem(int num) {
        currentCount += num;
        countText.text = "x" + currentCount;

        PlayerPrefs.SetInt("BOOSTER_" + type, currentCount);
    }

    public void OnClickBooster() {
        if (currentCount < 1) {
            LevelManager._instance.shop.GetComponent<Canvas>().enabled = true;
            LevelManager._instance.shop.GetComponent<ScaleAnim>().Anim();
            return;
        }

        switch (type) {
            case TimedRewards.RewardType.MORE_MOVES:
                AddItem(-1);
                GameManager._instance.DecreaseMoves(-5);
                MessageHandler._instance.ShowMessage("+5 Moves", 2f, Color.green);

                var effect = Instantiate(GameManager._instance.GetRandomExplosion());
                effect.transform.position = Vector3.zero;

                break;
            case TimedRewards.RewardType.ITEM_SHUFFLER:
                AddItem(-1);
                MessageHandler._instance.ShowMessage("Items shuffled", 2f, Color.green);
                StartCoroutine(ShuffleItems());
                break;
        }
    }


    public void OnClickBomb() {
        if (currentCount < 1) {
            LevelManager._instance.shop.GetComponent<Canvas>().enabled = true;
            LevelManager._instance.shop.GetComponent<ScaleAnim>().Anim();
            return;
        }

        GameManager._instance.isBombActivated = !GameManager._instance.isBombActivated;

        if (GameManager._instance.isBombActivated == false) {
            GameManager._instance.bombTopBar.SetActive(false);
        } else {
            GameManager._instance.bombTopBar.SetActive(true);
        }
    }

    /// <summary>
    /// Called from the BoosterShop buy button
    /// </summary>
    public void BuyBooster() {
        if (CurrencyManager._instance.coins < price) {
            MessageHandler._instance.ShowMessage("You do not have enough coins", 2f, Color.red);
            return;
        }

        MessageHandler._instance.ShowMessage("Item Purchased", 2f, Color.green);

        CurrencyManager._instance.AddCoins(-price);

        currentCount = PlayerPrefs.GetInt("BOOSTER_" + type, 0);
        AddItem(1);
    }

    /// <summary>
    /// Shuffle animation for shuffle booster
    /// </summary>
    IEnumerator ShuffleItems() {
        MatchItem[] items = FindObjectsOfType<MatchItem>();
        List<Vector3> positions = new List<Vector3>();

        var effect = Instantiate(GameManager._instance.GetRandomExplosion());
        effect.transform.position = Vector3.zero;

        foreach (MatchItem m in items)
            positions.Add(m.transform.position);

        for (int i = 0; i < items.Length; i++) {
            items[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            var temp = positions[i];

            int random = Random.Range(0, items.Length);
            positions[i] = positions[random];
            positions[random] = temp;
        }

        float t = 0;
        while (t < 1) {
            t += Time.deltaTime / 1.5f;

            for (int i = 0; i < items.Length; i++) {
                items[i].transform.position = Vector3.Lerp(items[i].transform.position, positions[i], t);
            }

            yield return null;
        }

        for (int i = 0; i < items.Length; i++) {
            items[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        var effect2 = Instantiate(GameManager._instance.GetRandomExplosion());
        effect2.transform.position = Vector3.zero;
    }
}
