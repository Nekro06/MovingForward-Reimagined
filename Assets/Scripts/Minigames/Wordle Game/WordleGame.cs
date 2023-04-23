using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordleGame : MonoBehaviour
{
	public MovingForwardWordleWordsObject movingForwardWordleWordsObject;
	public WordleLetterRows[] wordleLetterRows;
	public KeyboardInput keyboardInput;
	public GameObject keyboardTogglerGameObject;
	public KeyboardToggler keyboardToggler;
	public GameObject wordleWinLosePanelGameObject;

	private WordleWinLosePanel wordleWinLosePanel;
	private MovingForwardWordleWordsObject.Word wordToGuess;
	private int currentRow = 0;
	private char[] wordToGuessCharArray = new char[5];
	private char[] currentWord = new char[5] { '\0', '\0', '\0', '\0', '\0' };
	private int[] letterBoxStates = new int[5];

	private Dictionary<string, int> allLetterStates = new Dictionary<string, int>();
	void PickWordToGuess()
	{
		int randomWordIndex = UnityEngine.Random.Range(0, movingForwardWordleWordsObject.Words.Count);
		wordToGuess = movingForwardWordleWordsObject.Words[randomWordIndex];

		Debug.Log("Word to guess: " + wordToGuess.word);
	}

	void Start()
	{
		wordleWinLosePanelGameObject.SetActive(false);
		wordleWinLosePanel = wordleWinLosePanelGameObject.GetComponent<WordleWinLosePanel>();
		keyboardInput.wordleGame = this;
		PickWordToGuess();
	}

	public void KeyboardInputHandler(string key)
	{
		if (key == "Backspace")
		{
			RemoveLetterFromRow();
			wordleLetterRows[currentRow].RemoveLetterFromRow();
		}
		else if (key == "Enter")
		{
			StartCoroutine(Enter());
		}
		else
		{
			char letter = key[0];
			letter = char.ToUpper(letter);
			AddLetterToCurrentWord(letter);
			wordleLetterRows[currentRow].AddLetterToRow(letter);
		}
	}

	IEnumerator Enter()
	{
		int[] state = CheckRow();
		wordleLetterRows[currentRow].SetLetterBoxState(state);
		Dictionary<string, int> letterStates = new Dictionary<string, int>();
		// add allLettersStates to letterStates
		foreach (KeyValuePair<string, int> entry in allLetterStates)
		{
			letterStates.Add(entry.Key, entry.Value);
		}

		for (int i = 0; i < currentWord.Length; i++)
		{
			if (currentWord[i] != '\0')
			{
				// add to the letterStates but if it already exists, then replace it
				if (letterStates.ContainsKey(currentWord[i].ToString().ToUpper()))
				{
					letterStates[currentWord[i].ToString().ToUpper()] = state[i];
				}
				else
				{
					letterStates.Add(currentWord[i].ToString().ToUpper(), state[i]);
				}

				// add to the allLetterStates but if it already exists, then replace it
				if (allLetterStates.ContainsKey(currentWord[i].ToString().ToUpper()))
				{
					allLetterStates[currentWord[i].ToString().ToUpper()] = letterBoxStates[i];
				}
				else
				{
					allLetterStates.Add(currentWord[i].ToString().ToUpper(), letterBoxStates[i]);
				}
			}
		}

		keyboardToggler.ToggleKeyboard();
		yield return new WaitForSeconds(0.01f);
		keyboardTogglerGameObject.SetActive(false);
		yield return new WaitForSeconds(1.5f);

		if (state[0] == 3 && state[1] == 3 && state[2] == 3 && state[3] == 3 && state[4] == 3)
		{
			// The word is correct
			StartCoroutine(TotallyWin());
			yield break;
		}

		keyboardTogglerGameObject.SetActive(true);

		if (currentRow == 5)
		{
			wordleWinLosePanel.isWin = false;
			wordleWinLosePanel.wordToGuess = wordToGuess;
			wordleWinLosePanel.score = 0;
			wordleWinLosePanelGameObject.SetActive(true);
			yield break;
		}

		currentRow++;
		currentWord = new char[5];
		Debug.Log("Entering New Line");
	}

	IEnumerator TotallyLose()
	{
		wordleWinLosePanel.isWin = false;
		wordleWinLosePanel.wordToGuess = wordToGuess;
		wordleWinLosePanel.score = 0;
		wordleWinLosePanelGameObject.SetActive(true);
		yield break;
	}

	IEnumerator TotallyWin()
	{
		wordleLetterRows[currentRow].Win();
		yield return new WaitForSeconds(1.5f);
		UpdateStatistics();
		CreateNewLifeCycle();
		wordleWinLosePanel.isWin = true;
		wordleWinLosePanel.wordToGuess = wordToGuess;
		wordleWinLosePanel.score = 50;
		wordleWinLosePanelGameObject.SetActive(true);
		
	}

	void CreateNewLifeCycle()
    {
        LifeCycleItem lifeCycleItem = new LifeCycleItem();
        lifeCycleItem.name = "Wordle";
        lifeCycleItem.startTime = System.DateTime.Now + System.TimeSpan.FromHours(1.5f);
        lifeCycleItem.maxRepeatCount = -1;
        lifeCycleItem.repeatType = LifeCycleRepeatType.Custom;
        lifeCycleItem.customRepeatTime = System.TimeSpan.FromHours(1.5f).Seconds;

        LifeCycleManager.instance.AddLifeCycleItem(lifeCycleItem);
    }

	void UpdateStatistics()
	{
		float points = 50;

		ExperienceManager.instance.AddExperience(points);

		WordleCompletedEvent wordleCompletedEvent = new WordleCompletedEvent(
			"Wordle Completed",
			currentRow,
			wordToGuess,
			points
		);

		Aggregator.instance.Publish(wordleCompletedEvent);
	}

	public Dictionary<string, int> GetAllLetterStates()
	{
		return allLetterStates;
	}

	void AddLetterToCurrentWord(char letter)
	{
		for (int i = 0; i < currentWord.Length; i++)
		{
			if (currentWord[i] == '\0')
			{
				Debug.Log("Adding letter to current word");
				currentWord[i] = letter;
				break;
			}
		}
	}

	void RemoveLetterFromRow()
	{
		for (int i = currentWord.Length - 1; i >= 0; i--)
		{
			if (currentWord[i] != '\0')
			{
				currentWord[i] = '\0';
				break;
			}
		}
	}

	int[] CheckRow()
	{
		// 0 = normal
		// 1 = letter not in word
		// 2 = right letter wrong position
		// 3 = right letter right position
		int[] _letterBoxStates = new int[5];

		// Check if the word is correct
		// regardless of the upper/lower case
		// and the order of the letters
		// and if there are two or more of the same letter in the word
		string currentWordString = new string(currentWord);
		string wordToGuessString = wordToGuess.word;

		if (currentWordString.ToLower() == wordToGuessString.ToLower())
		{
			// The word is correct
			// Check if the letters are in the right position
			for (int i = 0; i < currentWord.Length; i++)
			{
				if (currentWordString[i].ToString().ToLower() == wordToGuessString[i].ToString().ToLower())
				{
					_letterBoxStates[i] = 3;
				}
				else
				{
					_letterBoxStates[i] = 2;
				}
			}
		}
		else
		{
			// The word is not correct
			// Check if the letters are in the word
			for (int i = 0; i < currentWord.Length; i++)
			{
				if (wordToGuessString.ToLower().Contains(currentWord[i].ToString().ToLower()))
				{
					// check if the letter is in the right position
					if (currentWordString[i].ToString().ToLower() == wordToGuessString[i].ToString().ToLower())
					{
						_letterBoxStates[i] = 3;
					}
					else
					{
						_letterBoxStates[i] = 2;
					}
				}
				else
				{
					_letterBoxStates[i] = 1;
				}
			}
		}

		letterBoxStates = _letterBoxStates;
		return _letterBoxStates;
	}
}


