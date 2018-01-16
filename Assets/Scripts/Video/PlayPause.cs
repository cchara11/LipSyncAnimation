using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPause : MonoBehaviour {
    
    private void OnMouseDown()
    {
        PlayVideo pv = GameObject.Find("Monitor").GetComponent<PlayVideo>();
        pv.PlayPause();
    }
}
