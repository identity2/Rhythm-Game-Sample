using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Conductor : MonoBehaviour 
{
	// Used to display "HIT" or "MISS".
	public TextMesh statusText;
	public GameObject musicNotePrefab;

	// Some audio file might contain an empty interval at the start. We will substract this empty offset to calculate the actual position of the song.
	public float songOffset;

	// The beat-locations of all music notes in the song should be entered in this array in Editor.
	// See the image: http://shinerightstudio.com/posts/music-syncing-in-rhythm-games/pic1.png
	public float[] track;

	// The start positionX of notes.
	public float startLineX;

	// The positionY of music notes.
	public float posY;

	// The finish line (the positionX where players hit) of the notes.
	public float finishLineX;

	// The positionX where the note should be destroyed.
	public float removeLineX;

	// The position offest of toleration. (If the players hit slightly inaccurate for the music note, we tolerate them and count it as a successful hit.)
	public float tolerationOffset;

	// How many seconds each beat last. This could be calculated by (60 / BPM).
	public float secondsPerBeat;

	// How many beats are contained on the screen. (Imagine this as "how many beats per bar" on music sheets.)
	public float BeatsShownOnScreen = 4f;

	// This plays the song.
	public AudioSource songAudioSource;

	// This plays the beat.
	public AudioSource beatAudioSource;

	// Current song position. (We don't want to show this in Editor, hence the "NonSerialized")
	[NonSerialized] public float songposition;

	// Next index for the array "track".
	private int indexOfNextNote;
	
	// Queue, keep references of the MusicNodes which currently on screen.
	private Queue<MusicNote> notesOnScreen;

	// To record the time passed of the audio engine in the last frame. We use this to calculate the position of the song.
	private float dsptimesong;

	private bool songStarted = false;

	void PlayerInputted()
	{
		// Start the song if it isn't started yet.
		if (!songStarted)
		{
			songStarted = true;
			StartSong();
			statusText.text = "";
			return;
		}

		// Play the beat sound.
		beatAudioSource.Play();
	
		if (notesOnScreen.Count > 0)
		{
			// Get the front note.
			MusicNote frontNote = notesOnScreen.Peek();

			// Distance from the note to the finish line.
			float offset = Mathf.Abs(frontNote.gameObject.transform.position.x - finishLineX);

			// Music note hit.
			if (offset <= tolerationOffset) 
			{
				// Change color to green to indicate a "HIT".
				frontNote.ChangeColor(true);

				statusText.text = "HIT!";
				
				// Remove the reference. (Now the next note moves to the front of the queue.)
				notesOnScreen.Dequeue();
			}
		}
	}

	void Start()
	{
		// Initialize some variables.
		notesOnScreen = new Queue<MusicNote>();
		indexOfNextNote = 0;
	}

	void StartSong()
	{
		// Use AudioSettings.dspTime to get the accurate time passed for the audio engine.
		dsptimesong = (float) AudioSettings.dspTime;

		// Play song.
		songAudioSource.Play();
	}

	void Update()
	{
		// Check key press.
		if (Input.GetKeyDown(KeyCode.Space)) 
		{
			PlayerInputted();
		}

		if (!songStarted) return;

		// Calculate songposition. (Time passed - time passed last frame).
		songposition = (float) (AudioSettings.dspTime - dsptimesong - songOffset);

		// Check if we need to instantiate a new note. (We obtain the current beat of the song by (songposition / secondsPerBeat).)
		// See the image for note spawning (note that the direction is reversed):
		// http://shinerightstudio.com/posts/music-syncing-in-rhythm-games/pic2.png
		float beatToShow = songposition / secondsPerBeat + BeatsShownOnScreen;
		
		// Check if there are still notes in the track, and check if the next note is within the bounds we intend to show on screen.
		if (indexOfNextNote < track.Length && track[indexOfNextNote] < beatToShow) 
		{
			
			// Instantiate a new music note. (Search "Object Pooling" for more information if you wish to minimize the delay when instantiating game objects.)
			// We don't care about the position and rotation because we will set them later in MusicNote.Initialize(...).
			MusicNote musicNote = ((GameObject) Instantiate(musicNotePrefab, Vector2.zero, Quaternion.identity)).GetComponent<MusicNote>();

			musicNote.Initialize(this, startLineX, finishLineX, removeLineX, posY, track[indexOfNextNote]);
				
			// The note is push into the queue for reference.
			notesOnScreen.Enqueue(musicNote);

			// Update the next index.
			indexOfNextNote++;
		}

		// Loop the queue to check if any of them reaches the finish line.
		if (notesOnScreen.Count > 0) 
		{
			MusicNote currNote = notesOnScreen.Peek();

			if (currNote.transform.position.x >= finishLineX + tolerationOffset)
			{
				// Change color to red to indicate a miss.
				currNote.ChangeColor(false);

				notesOnScreen.Dequeue();

				statusText.text = "MISS!";
			}
		}

		// Note that the music note is eventually removed by itself (the Update() function in MusicNote) when it reaches the removeLine.
	}
}
