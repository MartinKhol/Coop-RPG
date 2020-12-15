using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernMusicPlayer : MonoBehaviour
{
	
	public AudioClip[] myClip;
	AudioSource source;
	
	public Animator animator;
	
	int currentClip = 0;
	
	int clipLength;
	
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
		Invoke("ChangeSongs",10f);
		
    }
	
	void ChangeSongs ()
	{
		if (source.clip == null)
		{
			currentClip = Random.Range(0, myClip.Length);
			source.clip = myClip[currentClip];
		}
		else 
		{
			currentClip ++;
			if (currentClip >= myClip.Length)
				currentClip = 0;
			source.clip = myClip[currentClip];
		}
		Invoke("ClipEnd",source.clip.length);
		source.Play();
	}
	
	void ClipEnd ()
	{
		animator.SetTrigger("endsong");
		Invoke("ChangeSongs",10f);
	}
}
