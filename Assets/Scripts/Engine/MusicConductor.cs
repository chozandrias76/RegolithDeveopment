using UnityEngine;
using System.Collections;

public class MusicConductor : MonoBehaviour {
	AudioClip	currentTrack;
	AudioSource audioSource;
	int 		curTrackID=666;
	float 		trackStartTime;
	float 		trackEndTime;
	// Use this for initialization
	string[]	titleTrackList={
		"A Mind of Its Own",
		"Blurred Atmospheres",
		"Creepy MusicBox",
		"Hard as Nails",
		"Sacrifice - Close",
		"Walking Into A Trap"
	};
	
	void playRandomTitle(){
		//return;
		curTrackID=(curTrackID+Random.Range(1,titleTrackList.Length-1))%titleTrackList.Length;
		currentTrack = (AudioClip)Resources.Load("Music/"+titleTrackList[curTrackID]);
		//currentTrack.length
        audioSource.clip = currentTrack;
		float pitchMul=1.0f+(0.25f*Random.Range (-1,1));
		audioSource.pitch=pitchMul;
		audioSource.volume=0.05f;
		audioSource.Play();
		trackStartTime=Time.time;
		trackEndTime=trackStartTime+(currentTrack.length/pitchMul)+10.0f;
	}
	
	void Start () {
		audioSource = (AudioSource)gameObject.AddComponent("AudioSource");
		playRandomTitle();
	}
	
	// Update is called once per frame
	void Update () {
	 	if(Input.GetKeyDown(KeyCode.M) || (Time.time>trackEndTime && audioSource.isPlaying==false)){
			playRandomTitle();
			
		}
	}
}
