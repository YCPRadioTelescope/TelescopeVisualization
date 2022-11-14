using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class SubtitleBehavior : PlayableBehaviour
{
    public string subtext;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        TextMeshProUGUI text = playerData as TextMeshProUGUI;
        text.text = subtext;
        text.color = new Color(1, 1, 1, info.weight);
    }
}
