using UnityEngine;

public class Prefs{
    // Camera Preferences
    public Vector3 cameraPosition;
    public Vector3 cameraEulerAngles;
    public float cameraFieldOfView = 60f;
    // VFX Preferences
    // VFX Common
    public float attackOrBeat = 0.5f;
    public float livelyEffect = 0.5f;
    // VFX Fireworks
    public float fireworksShootingWidth = 300f;
    // VFX Kaleidoscope
    // Quality : Low = 0, Middle = 1, High = 2;
    public int kaleidoscopeQuality = 1;
    // Complexity
    public float kaleidoscopeComplexity = 0.5f;
    // Post Processing Preferences
    public float ppDepthOfFieldFocusDistance = 10f;

    // Constructor
    public Prefs() { 
        cameraPosition = new Vector3(0, 0, 0);
        cameraEulerAngles = new Vector3(0, 0, 0);
    }
}
