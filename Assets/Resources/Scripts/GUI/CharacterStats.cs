using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class CharacterStats : MonoBehaviour
{
	public RawImage healthBar;
	public RawImage manaBar;
	public RawImage potion;
	public static List<Texture> potionImages = null;
	public Text score;

	public void Start()
	{
		if(potionImages == null)
		{
			potionImages = new List<Texture>();
			foreach(String pt in Enum.GetNames(typeof(PotionType)))
			{
				Debug.LogWarning("Textures/GUI/Potions/" + pt.ToString().ToLower());
				Texture tex = Resources.Load("Textures/GUI/Potions/" + pt.ToString().ToLower()) as Texture;
				if(!tex)
				{
					int w = (int) potion.rectTransform.rect.width;
					int h = (int) potion.rectTransform.rect.height;
					tex = new Texture2D(w, h);
				}
				potionImages.Add(tex);
			}
		}
	}

	public void ResizeHealthBar(float percent)
	{
	}

	public void ResizeManaBar(float percent)
	{
	}

	public void DisplayPotion(PotionType type)
	{
	}

	public void UpdateScore(int score)
	{
	}
}
