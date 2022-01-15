using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>{
    // FPS
    public int targetFPS = 60;

    // Camera
    public GameObject MainCamera;
    public GameObject UICamera;

    // UI
    public GameObject SettingUI;
    public GameObject AudioSpectrumUI;
    public GameObject fireworksSettingUI;
    public GameObject kaleidoscopeSettingUI;
    public GameObject VFXUI;
    public GameObject QuitUI;
    private GameObject[] allUI;
    public Dropdown dropdownDisplayMode;
    public Dropdown dropdownVFX;
    public Dropdown dropdownSkybox;
    
    // SkyBox
    public bool skyboxRotation = true;
    public Material skyboxColdNight;
    public Material skyboxDeepDusk;
    public Material skyboxNightMoonBurst;
    public Material skyboxBlack;
    private Material[] skyboxes;
    private Material skybox;
    public float skyRotSpeed = 0.001f;
    private float skyRotVal;

    // Game Objects
    public MyAudioSource myAudioSource;
    private GameObject[] allVFX;
    public GameObject VFXFireworks;
    public GameObject VFXKaleidoscope;

    // Status
    public string currentVFXID;
    public bool isFireworks = true;
    private bool isVFX = false;

    // Start is called before the first frame update
    void Start(){
        // FPS
        Application.targetFrameRate = targetFPS;

        // VFX Setting
        allVFX = new GameObject[] { VFXFireworks, VFXKaleidoscope };
        // Default VFX
        currentVFXID = Constant.VFX_ID_FIREWORKS;
        // Change VFX
        OnDropdownVFXValueChanged();

        // All UI
        allUI = new GameObject[] { SettingUI, AudioSpectrumUI, fireworksSettingUI, kaleidoscopeSettingUI, VFXUI, QuitUI };

        // Skybox Material
        skyboxes = new Material[] { skyboxNightMoonBurst, skyboxColdNight, skyboxDeepDusk, skyboxBlack };
        // Set DropdownSkybox
        dropdownSkybox.AddOptions(skyboxes.Select(x => x.name).ToList());
        // Set Skybox
        OnDropdownSkyboxChanged();

        // Show SettingUI
        ShowSettingUI();
    }

    // Update is called once per frame
    void Update(){
        // Skybox Rotation
        if (skyboxRotation) {
            skyRotVal = Mathf.Repeat(skybox.GetFloat("_Rotation") + skyRotSpeed, 360f);
            skybox.SetFloat("_Rotation", skyRotVal);
        }

        // Key Event
        // Escape -> SettingUI
        if(Input.GetKeyDown(KeyCode.Escape)) { 
            // Stop Fireworks
            if (isVFX) {
                ShowSettingUI();
                isVFX = false;
            }
            //  Show Quit UI
            else {
                ShowUI(QuitUI);
            }
        }
    }

    // Show FireWorks UI
    public void ShowVFXUI() {
        // Deactivate All UI
        foreach (GameObject UI in allUI) {
            UI.SetActive(false);
        }
        // Active Fireworks UI
        VFXUI.SetActive(true);
        isVFX = true;
    }

    // Show Advanced Setting UI
    public void ShowVFXSettingUI() {
        // Deactivate All UI
        foreach (GameObject UI in allUI) {
            UI.SetActive(false);
        }
        // Switch by VFX
        switch (currentVFXID) {
            case Constant.VFX_ID_FIREWORKS:
                // Active Fireworks UI
                fireworksSettingUI.SetActive(true);
                AudioSpectrumUI.SetActive(true);
                isVFX = false;
                break;
            case Constant.VFX_ID_KALEIDOSCOPE:
                kaleidoscopeSettingUI.SetActive(true);
                AudioSpectrumUI.SetActive(true);
                isVFX = false;
                break;
        }
    }

    // Show Setting UI
    public void ShowSettingUI() {
        // Deactivate All UI
        foreach (GameObject UI in allUI) {
            UI.SetActive(false);
        }
        // Active Setting UI
        SettingUI.SetActive(true);
        AudioSpectrumUI.SetActive(true);
        isVFX = false;
    }

    // Show New UI
    void ShowUI(GameObject newUI) {
        // Deactivate All UI
        foreach(GameObject UI in allUI) {
            UI.SetActive(false);
        }
        // Activate New UI
        newUI.SetActive(true);
    }

    // Change Display Mode
    public void OnDropdownDisplayModeValueChanged() {
        string displayMode = dropdownDisplayMode.options[dropdownDisplayMode.value].text;
        switch (displayMode) {
            // Single Display
            case Constant.UI_DISPLAY_MODE_SINGLE:
                MainCamera.GetComponent<Camera>().targetDisplay = 0;
                UICamera.SetActive(false);
                break;
            // Dual Display
            case Constant.UI_DISPLAY_MODE_DUAL:
                if (Display.displays.Length > 1) {
                    Display.displays[1].Activate();
                }
                MainCamera.GetComponent<Camera>().targetDisplay = 1;
                UICamera.SetActive(true);
                break;
            default:
                break;
        }
    }

    // Change VFX
    public void OnDropdownVFXValueChanged() {
        // All Disable
        foreach (GameObject VFX in allVFX) {
            VFX.SetActive(false);
        }
        // Get Value of Dropdown
        string VFXID = dropdownVFX.options[dropdownVFX.value].text;
        // Init
        switch (VFXID) {
            // Fireworks
            case Constant.VFX_ID_FIREWORKS:
                // Get VFX Component
                var fireworks = VFXFireworks.GetComponent<Fireworks>();
                // Apply Preset Preferences
                fireworks.ApplyPrefs(fireworks.presetPrefs01);
                // Show VFX
                VFXFireworks.SetActive(true);
                // ID
                currentVFXID = Constant.VFX_ID_FIREWORKS;
                break;
            // Kaleidoscope
            case Constant.VFX_ID_KALEIDOSCOPE:
                // Get VFX Component
                var kaleidoscope = VFXKaleidoscope.GetComponent<Kaleidoscope>();
                // Apply Preset Preferences
                kaleidoscope.ApplyPrefs(kaleidoscope.presetPrefs01);
                // Show VFX
                VFXKaleidoscope.SetActive(true);
                // ID
                currentVFXID = Constant.VFX_ID_KALEIDOSCOPE;
                break;
            default:
                break;
        }
        // Change VFX of MyAudioSource
        myAudioSource.currentVFXID = VFXID;
    }

    // Change Skybox Material
    public void OnDropdownSkyboxChanged() {
        // get new skybox from Dropdown
        skybox = skyboxes[dropdownSkybox.value];
        // Set new skybox
        RenderSettings.skybox = skybox;
    }

    public void OnQuitUIButtonOKPressed() {
        Application.Quit();
    }
}
