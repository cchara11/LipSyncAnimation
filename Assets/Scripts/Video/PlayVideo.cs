using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PlayVideo : MonoBehaviour {

    public Material screenMaterial;
    public GameObject playPause;

    private VideoPlayer player;
    private VideoSource videoSource;
    private AudioSource audioSource;
    private float videoElapsedTimer;
    public GameObject videoTimerIndicator;
    public GameObject dimensionIndicator;
    public GameObject classificationIndicator;

     // file urls
    string videoUrl = "2008.12.05.16.03.15_Operator_AV_lowQ_Poppy.avi";
    string arousalFile = "R5S1TUCPoDA.txt";
    string valenceFile = "R5S1TUCPoDV.txt";

    private bool firstRun = true;
    private bool isPaused = false;
    
    List<BaseEmotion> emotionDimensions;

    // emotion dimensions
    Dictionary<float, float> arousal;
    Dictionary<float, float> valence;
    float currentArousal;
    float currentValence;

    GameObject emotion;

    private void Start()
    {
        videoElapsedTimer = 0.0f;

        // get arousal and valence values
        arousal = ArousalValence.GetArousal(PathManager.getSEMAINPath(arousalFile));
        valence = ArousalValence.GetValence(PathManager.getSEMAINPath(valenceFile));
        currentArousal = 0.0f;
        currentValence = 0.0f;

        emotionDimensions = CircumplexModel.GetEmotionDimensions();

        // spawn emotion to be classified
        emotion = (GameObject)Instantiate(Resources.Load("DimensionPlaceholder"));
        emotion.transform.parent = GameObject.Find("CircumplexModel").transform;
        emotion.transform.localPosition = new Vector3(0, -1f, 0);
        TextMesh emotionText = (TextMesh)emotion.GetComponentInChildren(typeof(TextMesh));
        emotionText.text = "   S";
    }

    IEnumerator StartPlaying()
    {
        playPause.gameObject.transform.GetComponent<Renderer>().enabled = false;
        firstRun = false;
        isPaused = false;

        // add player and audio source
        player = gameObject.AddComponent<VideoPlayer>();
        audioSource = gameObject.AddComponent<AudioSource>();

        player.playOnAwake = false;
        audioSource.playOnAwake = false;
        audioSource.Pause();

        player.source = VideoSource.Url;
        player.url = PathManager.getSEMAINPath(videoUrl);

        player.audioOutputMode = VideoAudioOutputMode.AudioSource;
        player.EnableAudioTrack(0, true);
        player.SetTargetAudioSource(0, audioSource);

        //player.Prepare();

        // wait until video is loaded
        WaitForSeconds time = new WaitForSeconds(1);
        while (!player.isPrepared)
        {
            yield return time;
            break;
        }
        
        screenMaterial.SetTexture("_MainTex", player.texture);

        player.Play();
        audioSource.Play();

    }

    public void PlayPause()
    {
        if (!firstRun && isPaused)
        {
            player.Play();
            audioSource.Play();
            isPaused = false;
            playPause.gameObject.transform.GetComponent<Renderer>().enabled = false;
        }
        else if (!firstRun && !isPaused)
        {
            player.Pause();
            audioSource.Pause();
            isPaused = true;
            playPause.gameObject.transform.GetComponent<Renderer>().enabled = true;
        }
        else
        {
            StartCoroutine(StartPlaying());
        }
    }

    private void FixedUpdate()
    {
        if (player != null)
        {
            if (player.isPlaying)
            {
                videoElapsedTimer += Time.deltaTime;

                TextMesh timerText = (TextMesh)videoTimerIndicator.GetComponent(typeof(TextMesh));
                timerText.text = videoElapsedTimer.ToString("0.000");

                float timeStamp = (float)Math.Truncate(videoElapsedTimer * 100) / 100;

                if (arousal.ContainsKey(timeStamp))
                {
                    currentArousal = arousal[timeStamp];
                }

                if (valence.ContainsKey(timeStamp))
                {
                    currentValence = valence[timeStamp];
                }

                ClassifyEmotion(currentArousal, currentValence);

                TextMesh dimensionText = (TextMesh)dimensionIndicator.GetComponent(typeof(TextMesh));
                dimensionText.text = string.Format("A({0})  V({1})", currentArousal, currentValence);
            }
        }
    }

    public void ClassifyEmotion(float arousal, float valence)
    {
        float minDistance = 1;
        string emotionClass = "NONE";
        Vector2 dimensions = new Vector2(0,0);

        foreach (BaseEmotion be in emotionDimensions)
        {
            float distance = (Mathf.Sqrt((arousal - be.arousal) * (arousal - be.arousal) + (valence - be.valence) * (valence - be.valence)));
            if (distance < minDistance)
            {
                minDistance = distance;
                emotionClass = be.name;
                dimensions.x = be.arousal;
                dimensions.y = be.valence;
            }
        }

        float classifiedDistance = (Mathf.Sqrt((arousal * arousal) + (valence * valence)));
        float baseDistance = (Mathf.Sqrt((dimensions.x * dimensions.x) + (dimensions.y * dimensions.y)));
        float intensity = classifiedDistance / baseDistance;
        if (intensity > 1)
        {
            intensity = 1;
        }

        TextMesh currentEmotion = (TextMesh)classificationIndicator.GetComponent(typeof(TextMesh));
        currentEmotion.text = string.Format("{0} with intensity {1}", emotionClass, intensity);

        // set classified emotions position in realtime
        emotion.transform.localPosition = new Vector3(valence * (-0.5f), -1f, arousal * (-0.5f));

        Debug.DrawLine(emotion.transform.position, GameObject.Find(emotionClass).transform.position, Color.green);
    }

}
