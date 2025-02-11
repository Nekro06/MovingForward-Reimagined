using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
	public List<WorldItem> worldItems = new List<WorldItem>();
	public int currentWorld = 0;

	void Start()
	{
		InitializeWorlds();

		try
		{
			currentWorld = SceneryManager.instance.sceneryManagerSave.currentSceneryIndex;
		}
		catch
		{
			currentWorld = 0;
		}
		Debug.Log("Current World: " + currentWorld);
	}

	void InitializeWorlds()
	{
		for (int i = 0; i < worldItems.Count; i++)
		{
			worldItems[i].worldIndex = i;
			worldItems[i].worldManager = this;
		}
	}
}
