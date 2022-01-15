﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Fireworks : MonoBehaviour{
    /* Properties */
    // Game Object
    public Camera mainCamera;
    public MyAudioSource myAudioSource;
    public MyAudioAnalyzer myAudioAnalyzer;
    public Volume postProcessVolume;

    // UI
    public Slider sliderCameraAngle;
    public Slider sliderCameraZoom;
    public Slider sliderCameraHeight;
    public Slider sliderAttackOrBeat;
    public Slider sliderLivelyEffect;
    public Slider sliderShootingWidth;

    // Fireworks Prefabs
    // Rising Prefab
    public GameObject RisingPrefab;
    // Ground Effect Prefabs
    public GameObject ToraStar;
    public GameObject KikuToraStar;
    public GameObject VToraStar;
    public GameObject YashiToraStar;
    public GameObject SazanamiStar;
    public GameObject HiyuStar;
    public GameObject SenrinStar;
    public GameObject RandamaStar;
    // Fireworks Object's Parent
    public GameObject RisingObjects;
    public GameObject GroundEffectObjects;

    // Preferences
    public readonly Prefs presetPrefs01;
    public readonly Prefs presetPrefs02;
    public readonly List<Prefs> prefsList;

    // Public
    public int FireworksSizeMax = 15;
    public float RisingVelocityCoefficient = 17.5f;
    public float SizeToVelocityPowerMultiplier = 0.5f;
    public float shootingWidth = 300.0f;
    public float LowRangeFreqMaxHz = 261.0f;
    public float QuietRMSMax = 0.05f;
    public float GroundStarEffectCoefficient = 1.5f;
    public float SenrinStarEffectCoefficient = 1.0f;
    public int SenrinStarCountMin = 5;
    // Private
    private GameObject obj;
    private List<Tone> toneList;
    private float GroundStarVolumeThreshold = 0.75f;
    private float SenrinStarVolumeThreshold = 0.2f;

    public Fireworks() {
        // Preferences
        // Preset 01
        presetPrefs01 = new Prefs();
        presetPrefs01.cameraPosition = new Vector3(0, 12.5f, -170f);
        presetPrefs01.cameraEulerAngles = new Vector3(-25f, 0, 0);
        presetPrefs01.fireworksShootingWidth = 300f;
        presetPrefs01.ppDepthOfFieldFocusDistance = 11f;
        // Preset 02
        presetPrefs02 = new Prefs();
        presetPrefs02.cameraPosition = new Vector3(0, 55f, -190f);
        presetPrefs02.cameraEulerAngles = new Vector3(-12.5f, 0, 0);
        presetPrefs02.fireworksShootingWidth = 400f;
        presetPrefs02.ppDepthOfFieldFocusDistance = 11f;
        // PrefsList
        prefsList = new List<Prefs>();
        prefsList.Add(presetPrefs01);
        prefsList.Add(presetPrefs02);
    }

    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update(){
        
    }

    // Init
    public void Init() {

    }

    // Rising Fireworks
    public void shootRising(List<Tone> tone_list, float rms = 0.0f) {
        // Tone List check
        if (!tone_list.Any()) {
            return;
        }

        // Copy tone
        toneList = new List<Tone>();
        foreach (Tone tone in tone_list) {
            toneList.Add(tone);
        }

        // Unique Tone List = > Color Blending
        // var uniqueToneList = toneList.Select(x => x.number % 12).Distinct().ToList();
        // float colorBlending = uniqueToneList.Count() / 12;
        float colorBlending = 0.66f;

        // Chord List : Grouped by Chord
        var chordlist = toneList.GroupBy(x => x.chordRef).ToList();

        // Volume Chord List : Grouped by Volume & Chord
        var volumeChordList = chordlist.GroupBy(x => (int)(x.ToList().Select(y => y.volume).Max() * FireworksSizeMax)).ToList();

        // Shoot the Rising
        for (int i = 0; i < volumeChordList.Count(); i++) {
            // Chords in this Volume
            var chords = volumeChordList[i].ToList();

            // Tone Count Max in this Volume
            int volumeKey = volumeChordList[i].Key;
            float toneCountMax = toneList.Where(x => (int)(x.volume * FireworksSizeMax) == volumeKey).Select(x => x.count).Max();

            // Volume : RMS * Count of Tones in this volume
            int countCurrent = chords.Count();
            float volumeCurrent = countCurrent * rms;

            // Fireworks Type
            // Normal : Default Fireworks Type 
            string type = Constant.FIREWORKS_RISING_TYPE_NORMAL;
            // Senrin : Laud Situatiion
            if (volumeCurrent > SenrinStarVolumeThreshold && countCurrent >= SenrinStarCountMin) {
                type = Constant.FIREWORKS_RISING_TYPE_SENRIN;
            }
            // Poka : Quiet Situation
            else if (rms < QuietRMSMax) {
                type = Constant.FIREWORKS_RISING_TYPE_POKA;
            }

            // Rising by Chord
            for (int j = 0; j < chords.Count; j++) {
                var tones = chords[j].ToList();
                // Max Volume in this Chord
                float volumeMax = tones.Select(x => x.volume).Max();

                // Position X
                float posX = ((j + 1) * shootingWidth / (chords.Count + 1)) - (shootingWidth / 2);
                // Position Vector
                Vector3 position = new Vector3(posX, 0, 0);

                // Get Rising Object
                obj = GameObject.FindGameObjectWithTag(Constant.TAG_RISING_STANBY);
                if (obj == null) {
                    obj = Instantiate(RisingPrefab, position, Quaternion.identity, RisingObjects.transform);
                }
                // Change Rising Tag Updating
                obj.tag = Constant.TAG_FIREWORKS_UPDATING;

                var transform = obj.GetComponent<Transform>();
                transform.position = position;
                transform.rotation = Quaternion.identity;

                // Rising Init
                // Get Rising Component
                Rising rising = obj.GetComponent<Rising>();
                // Init
                rising.Init();

                // Set Type
                rising.Type = type;

                // Add ToneList
                foreach (Tone tone in tones) {
                    rising.ToneList.Add(tone);
                }

                // Tone Count Mean
                rising.ToneCountValue = toneCountMax;

                // Color Blending
                rising.ColorBlending = colorBlending;

                // Particle Sysytem Main Module
                var main = obj.GetComponent<ParticleSystem>().main;
                // StartSpeed : from Tone Pitch
                main.startSpeed = volumeToVelocity(volumeMax);

                // Play Particle
                var particleSystem = obj.GetComponent<ParticleSystem>();
                particleSystem.Play();
            }
        }
    }

    // Dynamic Fireworks
    public void shootDynamic(List<Tone> tone_list, float rms = 0.0f, float dynamic_or_echo = 0.5f) {
        // Tone List check
        if (!tone_list.Any()) {
            return;
        }
        // Copy tone
        toneList = new List<Tone>();
        foreach (Tone tone in tone_list) {
            toneList.Add(tone);
        }
        // Color Blending
        float colorBlending = 0.66f;
        
        // Chord List : Grouped by Chord
        var chordlist = toneList.GroupBy(x => x.chordRef).ToList();
        
        // Volume Chord List : Grouped by Volume & Chord
        var volumeChordList = chordlist.GroupBy(x => (int)(x.ToList().Select(y => y.volume).Max() * FireworksSizeMax)).ToList();

        // Shoot Fireworks
        for (int i = 0; i < volumeChordList.Count(); i++) {
            // Chords in this Volume
            var chords = volumeChordList[i].ToList();

            // Tone Count Max in this Volume
            int volumeKey = volumeChordList[i].Key;
            float toneCountMax = toneList.Where(x => (int)(x.volume * FireworksSizeMax) == volumeKey).Select(x => x.count).Max();

            // Volume : RMS * Count of Tones in this volume
            int countCurrent = chords.Count();
            float volumeCurrent = countCurrent * rms;

            // Fireworks Type
            // Normal : Default Fireworks Type 
            string type = Constant.FIREWORKS_RISING_TYPE_NORMAL;
            // Senrin : Laud Situatiion
            if (volumeCurrent > SenrinStarVolumeThreshold && countCurrent >= SenrinStarCountMin) {
                type = Constant.FIREWORKS_RISING_TYPE_SENRIN;
            }
            // Poka : Quiet Situation
            else if (rms < QuietRMSMax) {
                type = Constant.FIREWORKS_RISING_TYPE_POKA;
            }
            // Rising by Chord
            for (int j = 0; j < chords.Count; j++) {
                var tones = chords[j].ToList();
                // Max Volume in this Chord
                float volumeMax = tones.Select(x => x.volume).Max();

                // Position X
                float posX = ((j + 1) * shootingWidth / (chords.Count + 1)) - (shootingWidth / 2);
                posX += UnityEngine.Random.Range(-5f, 5f);
                // Position Y
                float posY = Mathf.Pow(volumeToVelocity(volumeMax), 2.0f) / (2 * Mathf.Abs(Physics.gravity.y));
                posY += UnityEngine.Random.Range(-5f, 5f);
                // Position Vector
                Vector3 position = new Vector3(posX, posY, 0);

                // Get Rising Object
                obj = GameObject.FindGameObjectWithTag(Constant.TAG_RISING_STANBY);
                if (obj == null) {
                    obj = Instantiate(RisingPrefab, position, Quaternion.identity, RisingObjects.transform);
                }
                // Change Rising Tag Updating
                obj.tag = Constant.TAG_FIREWORKS_UPDATING;

                var transform = obj.GetComponent<Transform>();
                transform.position = position;
                transform.rotation = Quaternion.identity;

                // Rising Init
                // Get Rising Component
                Rising rising = obj.GetComponent<Rising>();
                // Init
                rising.Init();

                // Set Type
                rising.Type = type;

                // Add ToneList
                foreach (Tone tone in tones) {
                    rising.ToneList.Add(tone);
                }

                // Tone Count Mean
                rising.ToneCountValue = toneCountMax;

                // Color Blending
                rising.ColorBlending = colorBlending;

                // Particle Sysytem Main Module
                var main = obj.GetComponent<ParticleSystem>().main;
                // StartSpeed : from Tone Pitch
                // main.startSpeed = volumeToVelocity(volumeMax);
                main.startSpeed = 0.0f;
                main.startDelay = UnityEngine.Random.Range(0.0f, 0.25f);

                // Play Particle
                var particleSystem = obj.GetComponent<ParticleSystem>();
                particleSystem.Play();
            }
        }
    }

    // Ground Effect Fireworks
    public void shootGroundEffect(List<Tone> tone_list, float rms = 0.0f) {
        // Tone Volume
        int toneCountCurrent = tone_list.Count();
        float toneVolume = toneCountCurrent * rms;

        // Check Shooting Threshold
        if (toneVolume < GroundStarVolumeThreshold) {
            return;
        }

        // Get Main Chord
        string[] mainChordID = MyAudioAnalyzer.GetMainChords(tone_list);

        // Get Main Tone
        int[] mainToneNumbers = MyAudioAnalyzer.GetMainToneNumbers(tone_list);

        // Chord List : Grouped by Chord
        var chordList = tone_list.GroupBy(x => x.chordRef).ToList();
        // Pitch Chord List : Grouped by Pitch and Chord
        var volumeChordList = chordList.GroupBy(x => (int)(x.ToList().Select(y => y.volume).Max() * FireworksSizeMax)).ToList();

        // Count of Main Pitch Tones
        int[] countArray = new int[volumeChordList.Count];
        for (int i = 0; i < volumeChordList.Count; i++) {
            countArray[i] = volumeChordList[i].ToList().Count();
        }
        int countMax = 0;
        if (countArray.Length > 0) countMax = countArray.Max();

        // Ground Effect
        foreach (string chordID in mainChordID) {
            // Ground Effect
            for (int i = 0; i < countMax; i++) {
                GameObject obj = null;

                // Position X
                float posX = ((i + 1) * shootingWidth / (countMax + 1)) - (shootingWidth / 2);
                // Position Vector
                Vector3 position = new Vector3(posX, 0, 0);

                // Select Effect Star
                // Major => Tora Star
                if (chordID == Constant.CHORD_ID_MAJOR) {
                    // First Search Stanby Star
                    obj = GameObject.FindGameObjectWithTag(Constant.TAG_TORA_STAR_STANBY);
                    if (obj == null) {
                        // Instantiate New Star
                        obj = Instantiate(ToraStar, position, Quaternion.identity, GroundEffectObjects.transform);
                    }
                }
                // Minor => Sazanami Star
                else if (chordID == Constant.CHORD_ID_MINOR) {
                    obj = GameObject.FindGameObjectWithTag(Constant.TAG_SAZANAMI_STAR_STANBY);
                    if (obj == null) {
                        obj = Instantiate(SazanamiStar, position, Quaternion.identity, GroundEffectObjects.transform);
                    }
                }
                // Diminished => KikuTora Star
                else if (chordID == Constant.CHORD_ID_DIMINISHED) {
                    obj = GameObject.FindGameObjectWithTag(Constant.TAG_KIKUTORA_STAR_STANBY);
                    if (obj == null) {
                        obj = Instantiate(KikuToraStar, position, Quaternion.identity, GroundEffectObjects.transform);
                    }
                }
                // Augmented => YashiToraStar
                else if (chordID == Constant.CHORD_ID_AUGMENTED) {
                    obj = GameObject.FindGameObjectWithTag(Constant.TAG_YASHITORA_STAR_STANBY);
                    if (obj == null) {
                        obj = Instantiate(YashiToraStar, position, Quaternion.identity, GroundEffectObjects.transform);
                    }
                }
                // MinorMajor => VTora Star
                else if (chordID == Constant.CHORD_ID_MINORMAJOR) {
                    obj = GameObject.FindGameObjectWithTag(Constant.TAG_VTORA_STAR_STANBY);
                    if (obj == null) {
                        obj = Instantiate(VToraStar, position, Quaternion.identity, GroundEffectObjects.transform);
                    }
                }
                // Half Diminished => YuseiStar
                else if (chordID == Constant.CHORD_ID_HALFDIMINISHED) {
                    obj = GameObject.FindGameObjectWithTag(Constant.TAG_HIYU_STAR_STANBY);
                    if (obj == null) {
                        obj = Instantiate(HiyuStar, position, Quaternion.identity, GroundEffectObjects.transform);
                    }
                }
                // Dominat => Senrin Star
                else if (chordID == Constant.CHORD_ID_DOMINANT) {
                    obj = GameObject.FindGameObjectWithTag(Constant.TAG_SENRIN_STAR_STANBY);
                    if (obj == null) {
                        obj = Instantiate(SenrinStar, position, Quaternion.identity, GroundEffectObjects.transform);
                    }
                }
                // Other(None) => Randama Star
                else {
                    obj = GameObject.FindGameObjectWithTag(Constant.TAG_RANDAMA_STAR_STANBY);
                    if (obj == null) {
                        obj = Instantiate(RandamaStar, position, Quaternion.identity, GroundEffectObjects.transform);
                    }
                }

                // Change Tag to Updating
                obj.tag = Constant.TAG_FIREWORKS_UPDATING;

                // Particle System Transform
                var transform = obj.GetComponent<Transform>();
                transform.position = position;

                // Particle System of Star
                var particleSystem = obj.GetComponent<ParticleSystem>();

                // Particle Sysytem Main Module
                var main = particleSystem.main;

                // Start Color
                Color color = Colors.getToneColor(mainToneNumbers[0]);
                main.startColor = color;

                // Emit
                // float count = particleSystem.emission.GetBurst(0).count.constant;
                // int cycle = particleSystem.emission.GetBurst(0).cycleCount;
                particleSystem.Play();
            }
        }
    }

    // Physics
    // Volume to Velocity
    private float volumeToVelocity(float volume) {
        // Fireworks Size is the product of the tone volume and FireworksSizeMax
        float size = (int)(volume * FireworksSizeMax);
        return RisingVelocityCoefficient * Mathf.Pow(size, SizeToVelocityPowerMultiplier);
    }

    // UI Functions
    // Prefs
    public void ApplyPrefs(Prefs prefs) {
        // Set Camera
        mainCamera.transform.position = prefs.cameraPosition;
        mainCamera.transform.localEulerAngles = prefs.cameraEulerAngles;
        mainCamera.fieldOfView = prefs.cameraFieldOfView;
        // Set VFX Setting
        myAudioSource.AttackOrBeat = prefs.attackOrBeat;
        shootingWidth = prefs.fireworksShootingWidth;
        ChangeLivelyEffectThreshold(prefs.livelyEffect);
        // Post Processing
        DepthOfField depthOfField;
        postProcessVolume.profile.TryGet(out depthOfField);
        depthOfField.focusDistance.value = prefs.ppDepthOfFieldFocusDistance;
        // Set UI Value
        sliderCameraAngle.value = prefs.cameraEulerAngles.x;
        sliderCameraHeight.value = prefs.cameraPosition.y;
        sliderCameraZoom.value = prefs.cameraPosition.z;
        sliderAttackOrBeat.value = prefs.attackOrBeat;
        sliderLivelyEffect.value = prefs.livelyEffect;
        sliderShootingWidth.value = prefs.fireworksShootingWidth;
    }

    // Button Preset Prefs
    public void OnButtonPrestPrefsClicked(int number) {
        ApplyPrefs(prefsList[number]);
    }

    // Camera Angle
    public void OnSliderCameraAngleChanged() {
        // Get Value
        float value = (float)sliderCameraAngle.GetComponent<Slider>().value;
        // Change Camera Angle
        mainCamera.transform.rotation = Quaternion.Euler(value, 0, 0);
    }

    // Change Camera Position
    public void OnCameraPositionChanged() {
        // Get Value
        float height = (float)sliderCameraHeight.GetComponent<Slider>().value;
        float depth = (float)sliderCameraZoom.GetComponent<Slider>().value;
        // Change Camera Position
        mainCamera.transform.position = new Vector3(0, height, depth);
    }

    // Shooting Width Change
    public void OnSliderShootingWidthChanged() {
        // Get Value from slider
        shootingWidth = (int)sliderShootingWidth.GetComponent<Slider>().value;
    }

    // Lively Effect Change
    public void OnSliderLivelyEffectChanged() {
        // Get Value from slider
        float value = (float)sliderLivelyEffect.GetComponent<Slider>().value;
        // Change Lively Effect Threshold
        ChangeLivelyEffectThreshold(value);
    }

    // Change Lively Effect Threshold
    public void ChangeLivelyEffectThreshold(float value) {
        // Threshold
        float threshold = 1 - value;
        // Set Threshold of Effect
        GroundStarVolumeThreshold = GroundStarEffectCoefficient * threshold;
        SenrinStarVolumeThreshold = SenrinStarEffectCoefficient * threshold;
    }
}
