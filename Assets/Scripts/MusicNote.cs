using System;
using UnityEngine;

public class MusicNote : MonoBehaviour
{
	// Keep a reference of the conductor.
	public Conductor conductor;

	// We keep the start and end positionX to perform interpolation.
	public float startX;
	public float endX;
	public float removeLineX;
	public float beat;

	public Color hitColor;
	public Color missColor;
	private SpriteRenderer spriteRenderer;

	public void Initialize(Conductor conductor, float startX, float endX, float removeLineX, float posY, float beat)
	{
		this.conductor = conductor;
		this.startX = startX;
		this.endX = endX;
		this.beat = beat;
		this.removeLineX = removeLineX;

		// Set to initial position.
		transform.position = new Vector2(startX, posY);
		
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void Update()
	{
		// We update the position of the note according to the position of the song.
		// (Think of this as "resetting" instead of "updating" the position of the note each frame according to the position of the song.)
		// See this image: http://shinerightstudio.com/posts/music-syncing-in-rhythm-games/pic3.png (Note that the direction is reversed.)
		transform.position = new Vector2(startX + (endX - startX) * (1f - (beat - conductor.songposition / conductor.secondsPerBeat) / conductor.BeatsShownOnScreen), transform.position.y);

		// Remove itself when out of the screen (remove line).
		if (transform.position.x > removeLineX)
		{
			Destroy(gameObject);
		}
	}

	// Change the color to indicate whether its a "HIT" or a "MISS".
	public void ChangeColor(bool hit)
	{
		if (hit)
		{
			spriteRenderer.color = hitColor;
		}
		else
		{
			spriteRenderer.color = missColor;
		}
	}
}
