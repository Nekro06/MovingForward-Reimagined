using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberLocationLivesManager : MonoBehaviour
{
	public GameObject heartPrefab;
    public GameObject deadHeartPrefab;
	public int lives = 3;
    public int maxLives = 3;
	private int cachedLives = 0;

	void Update()
	{
        if (lives != cachedLives)
        {
            StartCoroutine(UpdateLives());
            cachedLives = lives;
        }
	}

	IEnumerator UpdateLives()
	{

		// remove all hearts
		foreach (Transform child in transform)
		{
			GameObject.Destroy(child.gameObject);
		}

		// add hearts
		for (int i = 0; i < lives; i++)
		{
			GameObject heart = Instantiate(heartPrefab, transform);
		}

        // add dead hearts
        for (int i = 0; i < maxLives - lives; i++)
        {
            GameObject deadHeart = Instantiate(deadHeartPrefab, transform);
        }

		yield return null;
	}
}
