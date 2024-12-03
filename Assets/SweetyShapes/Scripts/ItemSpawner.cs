using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {

    public static ItemSpawner _instance;

    private void Awake() {
        _instance = this;
    }

    [HideInInspector]
    public int currentItemNumber;

    public int itemNumber; // The maximum number of the items
    public int itemVariations; // How many different types of items can be spawned

    // Start is called before the first frame update
    void Start() {
        StartCoroutine(Spawn(itemNumber));
    }

    IEnumerator Spawn(int num) {
        GameManager._instance.canClickItem = false;

        for (int i = 0; i < num; i++) {
            CreateItem();

            yield return new WaitForSeconds(.05f);
        }

        GameManager._instance.canClickItem = true;

        while (gameObject != null) {
            if (itemNumber != currentItemNumber) {
                while (itemNumber != currentItemNumber) {
                    CreateItem();

                    yield return new WaitForSeconds(.05f);
                }
            }

            yield return new WaitForSeconds(.05f);
        }
    }

    void CreateItem() {
        GameObject g;

        if (GameManager._instance.GetGameType() == GameManager.GameType.Escape && Random.Range(0, 100) < GameManager._instance.percentToSpawnEscapeTarget) {
            int num = Random.Range(0, Mathf.Min(GameManager._instance.animals.Length, GameManager._instance.animalNum));
            g = Instantiate(GameManager._instance.animals[num]);
            g.GetComponent<TargetAnimal>().id = num;

        } else {
            int num = Random.Range(0, Mathf.Min(GameManager._instance.items.Length, itemVariations));

            g = Instantiate(GameManager._instance.items[num]);
            g.GetComponent<MatchItem>().id = num;
        }

        g.transform.position = new Vector3(Random.Range(-2f, 2f), Random.Range(5f, 6f), 0);
        g.transform.SetParent(transform);

        currentItemNumber++;

    }
}
