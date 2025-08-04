using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LevelGoal : MonoBehaviour, IFadeObserver
{
    public string nextLevel;
    public List<EnemyBug> enemies;

    private bool over;
    // Start is called before the first frame update
    void Start()
    {
        
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

            if (allDead && !over)
            {
                over = true;
                ScreenFader.Instance.TriggerFade(Color.black, this);
            }
        }
    }

    public void FadeComplete()
    {
        SceneManager.LoadScene(nextLevel);
    }
}
