using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuOptionManager : MonoBehaviour
{
    public Slider soundSlider;
    public void SoundSliderOnValueChanged()
    {
        GameManager_.Instance.SoundPlayer.SetVolume(soundSlider.value);
    }
}
