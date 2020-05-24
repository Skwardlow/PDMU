using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    Text lapCounterText;
    Text countDownText;
    Text timer;
    Text controlText;
    Player player;
    public Opponent[] opponents;
    public GameObject[] checkPoints;
    Button btnPlayAgain;
    Button btnExitGame;

    int countDown = 3;

    public bool raceStarted;

    public AudioClip[] beeps;
    public AudioClip winSound;
    public AudioClip loseSound;

    int tinySeconds;
    int seconds;
    int minutes;

	// инициализация игры
	void Awake () {

        //инстанс конструктора
        //чтобы точно все было хорошо
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            DestroyObject(gameObject);
        }

        //обнаруживаем инстансы всех объектов
        lapCounterText = GameObject.Find("LapCounter").GetComponent<Text>();
        countDownText = GameObject.Find("CountDownText").GetComponent<Text>();
        timer = GameObject.Find("Timer").GetComponent<Text>();
        controlText = GameObject.Find("Controls").GetComponent<Text>();
        player = GameObject.Find("PlayerCar").GetComponent<Player>();
        btnPlayAgain = GameObject.Find("PlayAgain").GetComponent<Button>();
        btnExitGame = GameObject.Find("Exit").GetComponent<Button>();
        checkPoints = new GameObject[10];
        for(int i = 0; i < checkPoints.Length; i++)
        {
            checkPoints[i] = GameObject.Find("CheckPoint" + i);
        }
        timer.text = "Time: 0 : 0 : 0";

        btnPlayAgain.gameObject.SetActive(false);
        btnExitGame.gameObject.SetActive(false);

        StartCoroutine(CountDown());
    }

    IEnumerator CountDown() // стартовая сценка с бипами, чтобы все было как в настоящей гонке :)
    {
        yield return new WaitForSeconds(1);
        for(int i = 0; i < 3; i++)
        {
            SoundManager.instance.PlaySound(SoundManager.instance.generalSoundSource, beeps[i], false);
            countDownText.text = countDown.ToString();
            countDown--;
            yield return new WaitForSeconds(1);
        }
        SoundManager.instance.PlaySound(SoundManager.instance.generalSoundSource, beeps[3], false);
        countDownText.text = "GO!";
        raceStarted = true;
        yield return new WaitForSeconds(2);
        countDownText.enabled = false;
        controlText.enabled = false;
        
    }
	
	// Апдейт раз в секунду
	void Update () {
       

        if (raceStarted)
        {
            tinySeconds++;
            if (tinySeconds > 10)
            {
                seconds++;
                tinySeconds = 0;
            }
            if (seconds > 60)
            {
                minutes++;
                seconds = 0;
            }
            timer.text = "Time: " + minutes + " : " + seconds + " : " + tinySeconds;

            if (!player.onRoad) // если игрок не на трассе, то сообщим ему об этом
            {
                if (!countDownText.enabled)
                {
                    countDownText.enabled = true;
                    countDownText.text = "Off track!";
                }
                
            }
            else
            {
                countDownText.enabled = false;
            }

            lapCounterText.text = "Lap: " + player.lapCounter.ToString();

            if (player.lapCounter > 3) // закончить гонку, если игрок проехал больше трех кругов
            {
                RaceOver(true);
            }

        
        }
       
	}

    void SetPlaces()
    {
        int playerLap = player.lapCounter;
        int playerCheckPoint = player.checkPointCounter;
        float playerPositionToNextCheckPoint;
        if(playerCheckPoint == 0) // большущая проверка чекпоинтов и настройка связей между ними
        {
            playerPositionToNextCheckPoint = GetDistanceToNextCheckPoint(player.transform.position, checkPoints[9].transform.position, checkPoints[playerCheckPoint].transform.position);
        }
        else if( playerCheckPoint == 10)
        {
            playerPositionToNextCheckPoint = GetDistanceToNextCheckPoint(player.transform.position, checkPoints[playerCheckPoint - 1].transform.position, checkPoints[0].transform.position);
        }
        else
        {
            playerPositionToNextCheckPoint = GetDistanceToNextCheckPoint(player.transform.position, checkPoints[playerCheckPoint - 1].transform.position, checkPoints[playerCheckPoint].transform.position);
        }
      
        int[] opponentsLap = new int [3];
        int[] opponentsCheckPoint = new int[3];
        float[] opponentsPositionToNextCheckPoint = new float[3];

        int playerPlace = 4;
        // считаем место игрока по чекам
        for (int i = 0; i < opponents.Length; i++)
        {
            opponentsLap[i] = opponents[i].lapCounter;
            opponentsCheckPoint[i] = opponents[i].checkPointCounter;
            if (opponentsCheckPoint[i] == 0 || opponentsCheckPoint[i] == 10)
            {
                opponentsPositionToNextCheckPoint[i] = GetDistanceToNextCheckPoint(opponents[i].transform.position, checkPoints[9].transform.position, checkPoints[opponentsCheckPoint[0]].transform.position);
            }
            else
            {
                opponentsPositionToNextCheckPoint[i] = GetDistanceToNextCheckPoint(opponents[i].transform.position, checkPoints[opponentsCheckPoint[i] - 1].transform.position, checkPoints[opponentsCheckPoint[i]].transform.position);
            }

            if(playerLap > opponentsLap[i] || playerCheckPoint > opponentsCheckPoint[i] || playerPositionToNextCheckPoint > opponentsPositionToNextCheckPoint[i])
            {
                playerPlace--;
            }
        }
        player.place = playerPlace;
    }

    public float GetDistanceToNextCheckPoint(Vector3 position, Vector3 lastNodeReached, Vector3 nextNode) // считаем расстояние до следующего чекпоинта
    {
        Vector3 displacementFromCurrentNode = position - lastNodeReached;
        Vector3 currentSegmentVector = nextNode - lastNodeReached;
        float distance = Vector3.Dot(displacementFromCurrentNode, currentSegmentVector) /
            currentSegmentVector.sqrMagnitude;
        return distance;
    }

    public void RaceOver(bool playerWon) // окончание гонки, проверка, победил игрок или нет
    {
        SoundManager.instance.PauseOrPlayMusic(true);
        player.engineSource.enabled = false;
        countDownText.enabled = true;
        if (playerWon)
        {
            countDownText.text = "You Won!";
            SoundManager.instance.PlaySound(SoundManager.instance.generalSoundSource, winSound, false);
        }
        else
        {
            countDownText.text = "You Lost!";
            SoundManager.instance.PlaySound(SoundManager.instance.generalSoundSource, loseSound, false);
        }
        btnPlayAgain.gameObject.SetActive(true);
        btnExitGame.gameObject.SetActive(true);

        raceStarted = false;
    }

    public void Restart() // перезапуск
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
    public void ExitGame() // выход из игры
    {
        Application.Quit();
    }
}
