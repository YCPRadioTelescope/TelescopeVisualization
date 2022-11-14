using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SubtitleClip : PlayableAsset
{
    public string text;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SubtitleBehavior>.Create(graph);

        SubtitleBehavior subtitleBehavior = playable.GetBehaviour();
        subtitleBehavior.subtext = text;

        return playable;
    }
}
