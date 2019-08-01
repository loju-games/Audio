using UnityEngine;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// iOS: https://developer.apple.com/reference/avfoundation/avaudiosession/1616610-otheraudioplaying?language=objc
/// </summary>
public static class BackgroundAudioDetector
{

    public static bool IsOtherAudioPlaying()
    {
#if UNITY_EDITOR
        return false;
#elif UNITY_IOS
		return _IsOtherAudioPlaying();
#elif UNITY_ANDROID
		if (_audioFocusListener == null){
			_audioFocusListener = new AudioFocusListener();      

			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject audioManager = activity.Call<AndroidJavaObject>("getSystemService", "audio");

			audioManager.Call<Int32>("requestAudioFocus", _audioFocusListener, 3, 1);
		}

		return !_audioFocusListener.HasAudioFocus;
#else
        return false;
#endif
    }

#if UNITY_IOS

    [DllImport("__Internal")]
    private static extern bool _IsOtherAudioPlaying();

#elif UNITY_ANDROID

	private static AudioFocusListener _audioFocusListener;
	private sealed class AudioFocusListener : AndroidJavaProxy
	{
		public AudioFocusListener() : base("android.media.AudioManager$OnAudioFocusChangeListener") { }

		private bool _HasAudioFocus = true;      
		public bool HasAudioFocus { get { return _HasAudioFocus; }}

		public void onAudioFocusChange(int focus)
		{
			_HasAudioFocus = (focus >= 0);
		}

		public override string toString()
		{
			return "AudioFocusListener";
		}
	}
	
#endif

}
