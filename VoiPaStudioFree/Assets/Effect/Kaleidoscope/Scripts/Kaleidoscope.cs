using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class Kaleidoscope : MonoBehaviour {
    // Scene Object
    public GameManager gameManager;
    public Camera mainCamera;
    public Volume postProcessVolume;
    // UI
    public Slider sliderQuality;
    public Slider sliderCameraAngle;
    public Slider sliderCameraFieldOfView;
    public Slider sliderComplexity;

    // My Object
    public GameObject mirroySystem;
    public ReflectionProbe reflectionProbe01;
    public ReflectionProbe reflectionProbe02;

    // Particle System
    // Tone Particle
    public ParticleSystem psParticle;
    public ParticleSystem psSparks;
    public ParticleSystem psSparkTrail;
    public ParticleSystem psTrail;
    // Chord Particle
    public ParticleSystem psNoise;
    public ParticleSystem psRay;
    public ParticleSystem psSpiral;
    public ParticleSystem psStripe;
    public ParticleSystem psRipple;
    public ParticleSystem psRing;
    public ParticleSystem psString;
    public ParticleSystem psGravity;
    // ParticleSystem Temporary Reference
    private ParticleSystem psTemp;

    // Preferences
    public readonly Prefs presetPrefs01;
    public readonly List<Prefs> prefsList;

    // Status
    private bool isMirrorSystemScaling = false;

    // Effect Parameter
    private float mirrorEffectVolumeThreshold = 0.75f;
    private float mirrorTransSpeed = 0.001f;
    private float mirrorTransThreshold = 0.1f; 
    private Vector3 threeMirrorScaleTarget = new Vector3(1.0f, 1.0f, 1.0f);
    private Vector3 threeMirrorScaleNone = new Vector3(1.0f, 1.0f, 1.0f);
    private Vector3 threeMirrorScaleMajor = new Vector3(0.681250039f, 1.467889825f, 1.0f);
    private Vector3 threeMirrorScaleAugmented = new Vector3(0.750184925f, 1.333004658f, 1.0f);
    private Vector3 threeMirrorScaleMinorMajor = new Vector3(0.847017671f, 1.180612913f, 1.0f);
    private Vector3 threeMirrorScaleHalfDiminished = new Vector3(1.180612913f, 0.847017671f, 1.0f);
    private Vector3 threeMirrorScaleDiminished = new Vector3(1.333004658f, 0.750184925f, 1.0f);
    private Vector3 threeMirrorScaleMinor = new Vector3(1.467889825f, 0.681250039f, 1.0f);

    // Constructor
    public Kaleidoscope() {
        //Preferences
        presetPrefs01 = new Prefs();
        presetPrefs01.cameraPosition = new Vector3(0f, 0f, 0f);
        presetPrefs01.cameraEulerAngles = new Vector3(0f, 0f, 0f);
        presetPrefs01.cameraFieldOfView = 90.0f;
        presetPrefs01.kaleidoscopeQuality = 2;
        presetPrefs01.kaleidoscopeComplexity = 0.5f;
        presetPrefs01.ppDepthOfFieldFocusDistance = 10f;
        // Prefs List
        prefsList = new List<Prefs>();
        prefsList.Add(presetPrefs01);
    }

    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update(){
        // Update Mirror System
        UpdateMirrorSystem();
    }

    // Update Mirror Position
    public void UpdateMirrorSystem() {
        // Mirror System Scaling
        MirrorScalingEffect();

        // Change Position as Center
        float posY = -1 * 10 / 2 * Mathf.Sqrt(3) / 3 * mirroySystem.transform.localScale.y;
        mirroySystem.transform.position = new Vector3(0, posY, 0);
    }

    // Mirror Transition Effect
    public void MirrorScalingEffect() {
        if (isMirrorSystemScaling) {
            // Change Scale With Lerp
            Vector3 scale = Vector3.Lerp(mirroySystem.transform.localScale, threeMirrorScaleTarget, mirrorTransSpeed);
            mirroySystem.transform.localScale = scale;
            
            // Transtion End
            float magnitude = (scale - threeMirrorScaleTarget).magnitude;
            if (magnitude < mirrorTransThreshold) {
                isMirrorSystemScaling = false;
            }
        }
    }

    // Attack Effect
    public void AttackEffect(List<Tone> tone_list) {
        // Debug.Log(tone_list.Count);
        // Tone List check
        if (!tone_list.Any()) return;
        // Tone Particle
        foreach (Tone tone in tone_list) {
            // Tone Number
            if (tone.count < 10) {
                psTemp = psParticle;
            }
            else if (tone.count < 20) {
                psTemp = psSparks;
            }
            else if (tone.count < 30) {
                psTemp = psSparkTrail;
            }
            else {
                psTemp = psTrail;
            }
            // Particle Sysytem Main Module
            var main = psTemp.main;
            // Start Color
            Color myColor = KaleidoscopeColors.getToneColor(tone.number);
            Color refColor = KaleidoscopeColors.getToneColor(tone.chordRef);
            main.startColor = (1.0f - 0.33f) * myColor + 0.33f * refColor;
            // Emmit
            // Emit Particle
            var count = psTemp.emission.GetBurst(0).count.constant;
            psTemp.Emit((int)count);
        }
    }

    // Beat Effect
    public void BeatEffect(List<Tone> tone_list, float rms) {
        // Get Tone Volume
        int toneCountCurrent = tone_list.Count();
        float toneVolume = toneCountCurrent * rms;
        // Check Volume Threshold
        if (toneVolume < 0.75f) return;
        // Get Main Chord
        string[] mainChordID = MyAudioAnalyzer.GetMainChords(tone_list);
        if (mainChordID.Length == 0) return;
        // Get Main Tone
        int[] mainToneNumbers = MyAudioAnalyzer.GetMainToneNumbers(tone_list);
        // Beat Chord Particle
        switch (mainChordID[0]) {
            // None -> Noise
            case Constant.CHORD_ID_NONE:
                psTemp = psNoise;
                break;
            // Major -> Ray
            case Constant.CHORD_ID_MAJOR:
                psTemp = psRay;
                break;
            // Augmented -> Spiral
            case Constant.CHORD_ID_AUGMENTED:
                psTemp = psSpiral;
                break;
            // MinorMajor -> Stripe
            case Constant.CHORD_ID_MINORMAJOR:
                psTemp = psStripe;
                break;
            // Half Diminished -> Ripple
            case Constant.CHORD_ID_HALFDIMINISHED:
                psTemp = psRipple;
                break;
            // Diminished -> String
            case Constant.CHORD_ID_DIMINISHED:
                psTemp = psRing;
                break;
            // Minor -> String
            case Constant.CHORD_ID_MINOR:
                psTemp = psString;
                break;
            // Dominant -> Gravity
            case Constant.CHORD_ID_DOMINANT:
                psTemp = psGravity;
                break;
            default:
                break;
        }
        // Particle Sysytem Main Module
        var main = psTemp.main;
        // Start Color
        Color color = Colors.getToneColor(mainToneNumbers[0]);
        main.startColor = color;
        // Emmit
        // Emit Particle
        var count = psTemp.emission.GetBurst(0).count.constant;
        psTemp.Emit((int)count);

        // Tone Particle
        foreach (Tone tone in tone_list) {
            // Tone Number
            if (tone.count < 10) {
                psTemp = psParticle;
            }
            else if (tone.count < 20) {
                psTemp = psSparks;
            }
            else if (tone.count < 30) {
                psTemp = psSparkTrail;
            }
            else {
                psTemp = psTrail;
            }
            // Particle Sysytem Main Module
            main = psTemp.main;
            // Start Color
            Color myColor = KaleidoscopeColors.getToneColor(tone.number);
            Color refColor = KaleidoscopeColors.getToneColor(tone.chordRef);
            main.startColor = (1.0f - 0.33f) * myColor + 0.33f * refColor;
            // Emmit
            // Emit Particle
            count = psTemp.emission.GetBurst(0).count.constant;
            psTemp.Emit((int)count);
        }
    }

    // Mirror Effect
    public void MirrorEffect(List<Tone> tone_list, float rms = 0.0f) {
        // Mirror System in Transition
        if (isMirrorSystemScaling) return;

        // Get Tone Volume
        int toneCountCurrent = tone_list.Count();
        float toneVolume = toneCountCurrent * rms;
        // Check Tone Volume Threshold
        if (toneVolume < mirrorEffectVolumeThreshold) return;
        // Debug.Log(toneVolume);

        // Scale Factor
        // float scaleFactor = 1 / Mathf.Sqrt(Mathf.Sqrt(toneVolume));
        float scaleFactor = 1;

        // Get Main Chord
        string[] mainChordID = MyAudioAnalyzer.GetMainChords(tone_list);
        if (mainChordID.Length == 0) return;
        // Mirror Transition
        switch (mainChordID[0]) {
            // None
            case Constant.CHORD_ID_NONE:
                threeMirrorScaleTarget = GetMirrorScale(threeMirrorScaleNone, scaleFactor);
                isMirrorSystemScaling = true;
                break;
            // Major
            case Constant.CHORD_ID_MAJOR:
                threeMirrorScaleTarget = GetMirrorScale(threeMirrorScaleMajor, scaleFactor);
                isMirrorSystemScaling = true;
                break;
            // Augmented
            case Constant.CHORD_ID_AUGMENTED:
                threeMirrorScaleTarget = GetMirrorScale(threeMirrorScaleAugmented, scaleFactor);
                isMirrorSystemScaling = true;
                break;
            // MinorMajor
            case Constant.CHORD_ID_MINORMAJOR:
                threeMirrorScaleTarget = GetMirrorScale(threeMirrorScaleMinorMajor, scaleFactor);
                isMirrorSystemScaling = true;
                break;
            // Half Diminished
            case Constant.CHORD_ID_HALFDIMINISHED:
                threeMirrorScaleTarget = GetMirrorScale(threeMirrorScaleHalfDiminished, scaleFactor);
                isMirrorSystemScaling = true;
                break;
            // Diminished
            case Constant.CHORD_ID_DIMINISHED:
                threeMirrorScaleTarget = GetMirrorScale(threeMirrorScaleDiminished, scaleFactor);
                isMirrorSystemScaling = true;
                break;
            // Minor
            case Constant.CHORD_ID_MINOR:
                threeMirrorScaleTarget = GetMirrorScale(threeMirrorScaleMinor, scaleFactor);
                isMirrorSystemScaling = true;
                break;
            default:
                break;
        }
    }

    // Mirror Scale
    private Vector3 GetMirrorScale(Vector3 scale, float scale_factor) {
        float scaleX = scale_factor * scale.x;
        float scaleY = scale_factor * scale.y;
        Vector3 newScale = new Vector3(scaleX, scaleY, 1);
        return newScale;
    }

    // Frame Effect
    public void FrameEffect() {

    }

    // UI Functions
    // Button Preset Prefs
    public void OnButtonPrestPrefsClicked(int number) {
        ApplyPrefs(prefsList[number]);
    }

    // Apply Prefs
    public void ApplyPrefs(Prefs prefs) {
        // Camera
        mainCamera.transform.position = prefs.cameraPosition;
        mainCamera.transform.localEulerAngles = prefs.cameraEulerAngles;
        mainCamera.fieldOfView = prefs.cameraFieldOfView;
        // VFX
        ChangeQuality(prefs.kaleidoscopeQuality);
        ChangeComplexity(prefs.kaleidoscopeComplexity);
        // Post Processing
        DepthOfField depthOfField;
        postProcessVolume.profile.TryGet(out depthOfField);
        depthOfField.focusDistance.value = prefs.ppDepthOfFieldFocusDistance;
        // Set UI Value
        sliderQuality.value = prefs.kaleidoscopeQuality;
        sliderCameraAngle.value = prefs.cameraEulerAngles.x;
        sliderCameraFieldOfView.value = prefs.cameraFieldOfView;
        sliderComplexity.value = prefs.kaleidoscopeComplexity;
    }

    // Camera Field of View
    public void OnSliderCameraFieldOfViewValueChanged() {
        // value
        float value = sliderCameraFieldOfView.value;
        // Set Camera Field Of View
        mainCamera.fieldOfView = value;
    }

    // Compplexity
    public void OnSliderComplexityValueChanged() {
        // value
        float value = sliderComplexity.value;
        ChangeComplexity(value);
    }

    // Change Complexity
    public void ChangeComplexity(float value, float min_value = 12.5f, float max_value = 37.5f) {
        // Size
        float sizeZ = min_value + (max_value - min_value) * value;
        // Set Complexity
        var size = reflectionProbe02.size;
        reflectionProbe02.size = new Vector3(size.x, size.y, sizeZ);
    }

    // Camera Angle
    public void OnSliderCameraAngleValueChanged() {
        // value
        float value = sliderCameraAngle.value;
        // Set Camera Angle
        mainCamera.transform.localEulerAngles = new Vector3(value, 0, 0);
    }

    // Quality
    public void OnSliderQualityValueChanged() {
        int value = (int)sliderQuality.value;
        ChangeQuality(value);
    }

    // Change Quality
    public void ChangeQuality(int value) {
        switch (value) {
            case (0):
                reflectionProbe01.resolution = 256;
                reflectionProbe02.resolution = 256;
                break;
            case (1):
                reflectionProbe01.resolution = 512;
                reflectionProbe02.resolution = 512;
                break;
            case (2):
                reflectionProbe01.resolution = 1024;
                reflectionProbe02.resolution = 1024;
                break;
            default:
                break;
        }
    }
}