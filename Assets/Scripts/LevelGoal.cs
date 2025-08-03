using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelGoal : MonoBehaviour
{
    public string nextLevel;
    public List<EnemyBug> enemies;

    public Image fadeToBlack;

    private float timer = 0;
    bool levelEnd;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        fadeToBlack.color = Vector4.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerController>() != null)
        {
            bool allDead = true;
            for(int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].gameObject.activeSelf)
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {

                levelEnd = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (levelEnd)
        {
            timer += Time.deltaTime;

            fadeToBlack.color = new Vector4(0, 0, 0, timer);
            if (timer > 1)
            {
                SceneManager.LoadScene(nextLevel);
            }
        }
    }
}
