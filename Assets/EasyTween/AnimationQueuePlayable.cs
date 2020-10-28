using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


public class AnimationQueuePlayable : PlayableBehaviour
{
    int m_currentClipIndex = -1;
    float m_timeToNextClip;
    AnimationMixerPlayable m_mixerPlayable;

    public void Initialize(AnimationClip[] clipArray, Playable owner, PlayableGraph graph)
    {
        owner.SetInputCount(1);
        m_mixerPlayable = AnimationMixerPlayable.Create(graph, clipArray.Length);
        graph.Connect(m_mixerPlayable, 0, owner, 0);
        owner.SetInputWeight(0, 1);

        //根据clipArray创建AnimationClipPlayable并连接
        for (int clipIndex = 0; clipIndex < m_mixerPlayable.GetInputCount(); ++clipIndex)
            graph.Connect(AnimationClipPlayable.Create(graph, clipArray[clipIndex]), 0, m_mixerPlayable, clipIndex);
    }

    public override void PrepareFrame(Playable owner, FrameData info)
    {
        int ClipCount = m_mixerPlayable.GetInputCount();
        if (ClipCount == 0)
            return;

        m_timeToNextClip -= info.deltaTime;

        if (m_timeToNextClip <= 0.0f)
        {
            m_currentClipIndex++;
            if (m_currentClipIndex >= ClipCount)
                m_currentClipIndex = 0;
            var currentClip = (AnimationClipPlayable) m_mixerPlayable.GetInput(m_currentClipIndex);

            //SetTime(0)，从头开始播放动画
            currentClip.SetTime(0);
            m_timeToNextClip = currentClip.GetAnimationClip().length;
        }

        //利用权重来设置当前播放的Clip
        for (int clipIndex = 0; clipIndex < ClipCount; ++clipIndex)
            m_mixerPlayable.SetInputWeight(clipIndex, clipIndex == m_currentClipIndex ? 1 : 0);
    }

    public override void OnGraphStart(Playable playable)
    {
        Debug.Log("Graph.Play()");
    }

    public override void OnGraphStop(Playable playable)
    {
        Debug.Log("Graph.Stop()");
    }

    public override void OnPlayableCreate(Playable playable)
    {
        Debug.Log("Playable.Create()");
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        Debug.Log("Playable.Destroy()");
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        Debug.Log("Playable.Play()");
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        Debug.Log("Playable.Pause()");
    }

    public override void PrepareData(Playable playable, FrameData info)
    {
        Debug.Log("PrepareData");
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //当连接在ScriptPlayableOutput的时候，会每帧调用
        Debug.Log("ProcessFrame");
    }
}
