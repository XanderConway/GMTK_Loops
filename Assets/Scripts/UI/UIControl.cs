using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIControl : MonoBehaviour, MenuControls.IMenuActions
{

    public GameObject pauseMenu;
    MenuControls menuControls;
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable()
    {
        if (menuControls == null)
        {
            menuControls = new MenuControls();
        }

        menuControls.Menu.SetCallbacks(this);
        menuControls.Menu.Enable();

    }


    public void UnPause()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);
            }
            Time.timeScale = 0;
        }
    }


}
