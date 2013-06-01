/*************************************************************
 *       Unity Audio Toolkit (c) by ClockStone 2012          *
 * 
 * Provides useful features for playing audio files in Unity:
 * 
 *  - ease of use: play audio files with a simple static function call, creation 
 *    of required AudioSource objects is handled automatically 
 *  - conveniently define audio assets in categories
 *  - play audios from within the inspector
 *  - set properties such as the volume for the entire category
 *  - change the volume of all playing audio objects within a category at any time
 *  - define alternative audio clips that get played with a specified 
 *    probability or order
 *  - advanced audio pick modes such as "RandomNotSameTwice", "TwoSimultaneously", etc.
 *  - uses audio object pools for optimized performance particularly on mobile devices
 *  - set audio playing parameters conveniently, such as: 
 *      + random pitch & volume
 *      + minimum time difference between play calls
 *      + delay
 *      + looping
 *  - fade out / in 
 *  - special functions for music including cross-fading 
 *  - music track playlist management with shuffle, loop, etc.
 *  - delegate event call if audio was completely played
 *  - audio event log
 *  - audio overview window
 * 
 * 
 * Usage:
 *  - create a unique GameObject named "AudioController" with the 
 *    AudioController script component added
 *  - Create an AudioObject prefab containing the following components: Unity's AudioSource, the AudioObject script, 
 *    and the PoolableObject script (if pooling is wanted). 
 *    Then set your custom AudioSource parameters in this prefab. Next, specify your custom prefab in the AudioController as 
 *    the "audio object".
 *  - create your audio categories in the AudioController using the Inspector, e.g. "Music", "SFX", etc.
 *  - for each audio to be played by a script create an 'audio item' with a unique name. 
 *  - specify any number of audio sub-items (= the AudioClip plus parameters) within an audio item. 
 *  - to play an audio item call the static function 
 *    AudioController.Play( "MyUniqueAudioItemName" )
 *  - Use AudioController.PlayMusic( "MusicAudioItemName" ) to play music. This function assures that only 
 *    one music file is played at a time and handles cross fading automatically according to the configuration
 *    in the AudioController instance
 *  - Note that when you are using pooling and attach an audio object to a parent object then make sure the parent 
 *    object gets deleted using ObjectPoolController.Destroy()
 *   
 ************************************************************/

using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

#pragma warning disable 1591 // undocumented XML code warning

/// <summary>
/// The audio managing class used to define and play audio items and categories.
/// </summary>
/// <remarks>
/// Exactly one instance of an AudioController must exist in each scene using the Audio Toolkit. There a two
/// options when setting up an AudioController. Either you can specify all audio files that are used in your
/// entire project in one single AudioController. Then add this AudioController to your initial scene and set 
/// it persistent from within the inspector, so it will survive when a new scene is loaded. This way all audios
/// are accessible from within your entire application. If you have a lot of audio files though, this may lead 
/// to a lengthy loading time and will have a rather large memory footprint. To avoid this, you can alternatively 
/// set up a specific AudioController for each scene which only contains those audio files needed in the particular 
/// scene.
/// </remarks>
/// <example>
/// Once you have defined your audio categories and items in the Unity inspector you can play music and sound effects 
/// very easily:
/// <code>
/// AudioController.Play( "MySoundEffect1" );
/// AudioController.Play( "MySoundEffect2", new Vector3( posX, posY, posZ ) );
/// AudioController.PlayMusic( "MusicTrack1" );
/// AudioController.SetCategoryVolume( "Music", 0.5f );
/// AudioController.PauseMusic();
/// </code>
/// </example>
/// 

#if AUDIO_TOOLKIT_DEMO
[AddComponentMenu( "ClockStone/Audio/AudioController Demo" )]
public class AudioController : MonoBehaviour // can not make DLL with SingletonMonoBehaviour
{
    static public AudioController Instance 
    {
        get {
            return UnitySingleton<AudioController>.GetSingleton( true, false );
        }
    }
    static public bool DoesInstanceExist()
    {
        return !UnitySingleton<AudioController>.GetSingleton( false, false ).Equals( null );
    }
    static public void SetSingletonType( Type type )
    {
        UnitySingleton<AudioController>._myType = type;
    }
#else
[AddComponentMenu( "ClockStone/Audio/AudioController" )]
public class AudioController : SingletonMonoBehaviour<AudioController>
{
#endif

    /// <summary>
    /// A string containing the version number of the Audio Toolkit
    /// </summary>
    public const string AUDIO_TOOLKIT_VERSION = "4.0";

    /// <summary>
    /// Disables all audio playback.
    /// </summary>
    /// <remarks>
    /// Does not stop currently playing audios. Call <see cref="StopAll()"/> to stop all currently playing.
    /// </remarks>
    public bool DisableAudio
    {
        set
        {
            if ( value != _audioDisabled )
            {
                if ( value == true )
                {
                    // changed in v3.6 - allows to disable Audio without stopping all current audios

                    /*if ( AudioController.DoesInstanceExist() ) // value can be changed by inspector in none-playmode.
                    {
                        StopAll();
                    }*/
                }
                _audioDisabled = value;
            }
        }
        get
        {
            return _audioDisabled;
        }
    }
   
    /// <summary>
    /// The global volume applied to all categories.
    /// You change the volume by script and the change will be apply to all 
    /// playing audios immediately.
    /// </summary>
    public float Volume
    {
        get { return _volume; }
        set { if ( value != _volume ) { _volume = value; _ApplyVolumeChange(); } }
    }

    /// <summary>
    /// You must specify your AudioObject prefab here using the Unity inspector.
    /// <list type="bullet">
    ///     <listheader>
    ///          <description>The prefab must have the following components:</description>
    ///     </listheader>
    ///     <item>
    ///       <term>AudioObject</term>
    ///       <term>AudioSource (Unity built-in)</term>
    ///       <term>PoolableObject</term> <description>only required if pooling is uses</description>
    ///     </item>
    /// </list>
    ///  
    /// </summary>
    public GameObject AudioObjectPrefab;

    /// <summary>
    /// If enabled, the audio controller will survive scene changes
    /// </summary>
    /// <remarks>
    /// For projects with a large number of audio files you may consider having 
    /// separate AudioController version for each scene and only specify those audio items 
    /// that are really required in this scene. This can reduce memory consumption and
    /// speed up loading time for the initial scene.
    /// </remarks>
    public bool Persistent = false;

    /// <summary>
    /// Enables / Disables AudioObject pooling
    /// </summary>
    public bool UsePooledAudioObjects = true;
    
    /// <summary>
    /// If disabled, audios are not played if they have a resulting volume of zero.
    /// </summary>
    public bool PlayWithZeroVolume = false;

    /// <summary>
    /// Gets or sets the musicEnabled.
    /// </summary>
    /// <value>
    ///   <c>true</c> enables music; <c>false</c> disables music
    /// </value>
    public bool musicEnabled
    {
        get { return _musicEnabled; }
        set
        {
            if ( _musicEnabled == value ) return;
            _musicEnabled = value;

            if ( _currentMusic )
            {
                if ( value )
                {
                    if ( _currentMusic.IsPaused() )
                    {
                        _currentMusic.Play();
                    }
                }
                else
                {
                    _currentMusic.Pause();

                }
            }

        }
    }

    /// <summary>
    /// If set to a value > 0 (in seconds) music will automatically be cross-faded with this fading time.
    /// </summary>
    public float musicCrossFadeTime = 0;

    /// <summary>
    /// Specify your audio categories here using the Unity inspector.
    /// </summary>
    public AudioCategory[] AudioCategories;

    /// <summary>
    /// allows to specify a list of audioID that will be played as music one after the other
    /// </summary>
    public string[ ] musicPlaylist;

    /// <summary>
    /// specifies if the music playlist will get looped
    /// </summary>
    public bool loopPlaylist = false;

    /// <summary>
    /// enables / disables shuffling for the music playlist
    /// </summary>
    public bool shufflePlaylist = false;

    /// <summary>
    /// if enabled, the tracks on the playlist will get cross-faded as specified by <see cref="musicCrossFadeTime"/>
    /// </summary>
    public bool crossfadePlaylist = false;

    /// <summary>
    /// Mute time in between two tracks on the playlist.
    /// </summary>
    public float delayBetweenPlaylistTracks = 1;

    /// <summary>
    /// Returns the high precision system time size the application launch
    /// </summary>
    /// <remarks>
    /// For performance reasons the time only gets updated with the frame rate. However,
    /// the time value does not lose precision even if the application is running for a
    /// long time (unlike Unity's 32bit float Time.systemTime
    /// </remarks>
    static public double systemTime
    {
        get
        {
            return _systemTime;
        }
    }


    // **************************************************************************************************/
    //          public functions
    // **************************************************************************************************/

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> as music.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <param name="startTime">The start time [default=0]</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// PlayMusic makes sure that only one music track is played at a time. If music cross fading is enabled in the AudioController
    /// fading is performed automatically.<br/>
    /// If "3D sound" is enabled in the audio import settings of the audio clip the object will be placed right 
    /// in front of the current audio listener which is usually on the main camera. Note that the audio object will not
    /// be parented - so you will hear when the audio listener moves.
    /// </remarks>
    static public AudioObject PlayMusic( string audioID, float volume, float delay, float startTime )
    {
        _isPlaylistPlaying = false;
        return Instance._PlayMusic( audioID, volume, delay, startTime );
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> as music.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <remarks>
    /// Variant of <see cref="PlayMusic( string, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject PlayMusic( string audioID ) 
    { 
        return AudioController.PlayMusic( audioID, 1, 0, 0 ); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> as music.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <remarks>
    /// Variant of <see cref="PlayMusic( string, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject PlayMusic( string audioID, float volume ) 
    { 
        return AudioController.PlayMusic( audioID, volume, 0, 0 ); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> as music.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <remarks>
    /// Variant of <see cref="PlayMusic( string, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject PlayMusic( string audioID, float volume, float delay ) 
    { 
        return AudioController.PlayMusic( audioID, volume, delay, 0 ); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> as music at the specified position.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="worldPosition">The position in world coordinates.</param>
    /// <param name="parentObj">The parent transform or <c>null</c>.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <param name="startTime">The start time [default=0]</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// PlayMusic makes sure that only one music track is played at a time. If music cross fading is enabled in the AudioController
    /// fading is performed automatically.
    /// </remarks>
    static public AudioObject PlayMusic( string audioID, Vector3 worldPosition, Transform parentObj, float volume, float delay, float startTime )
    {
        _isPlaylistPlaying = false;
        return Instance._PlayMusic( audioID, worldPosition, parentObj, volume, delay, startTime );
    }

    /// <summary>
    /// Stops the currently playing music.
    /// </summary>
    /// <returns>
    /// <c>true</c> if any music was stopped, otherwise <c>false</c>
    /// </returns>
    static public bool StopMusic()
    {
        return Instance._StopMusic( 0 );
    }

    /// <summary>
    /// Stops the currently playing music with fade-out.
    /// </summary>
    /// <param name="fadeOut">The fade-out time in seconds.</param>
    /// <returns>
    /// <c>true</c> if any music was stopped, otherwise <c>false</c>
    /// </returns>
    static public bool StopMusic( float fadeOut )
    {
        return Instance._StopMusic( fadeOut );
    }

    /// <summary>
    /// Pauses the currently playing music.
    /// </summary>
    /// <returns>
    /// <c>true</c> if any music was paused, otherwise <c>false</c>
    /// </returns>
    static public bool PauseMusic()
    {
        return Instance._PauseMusic();
    }

    /// <summary>
    /// Uses to test if music is paused
    /// </summary>
    /// <returns>
    /// <c>true</c> if music is paused, otherwise <c>false</c>
    /// </returns>
    static public bool IsMusicPaused()
    {
        if ( _currentMusic != null )
        {
            return _currentMusic.IsPaused();
        }
        return false;
    }

    /// <summary>
    /// Unpauses the current music.
    /// </summary>
    /// <returns>
    /// <c>true</c> if any music was unpaused, otherwise <c>false</c>
    /// </returns>
    static public bool UnpauseMusic()  // un-pauses music
    {
        if ( _currentMusic != null && _currentMusic.IsPaused() )
        {
            _currentMusic.Play();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Enqueues an audio ID to the music playlist queue.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <returns>
    /// The number of music tracks on the playlist.
    /// </returns>
    static public int EnqueueMusic( string audioID )
    {
        return Instance._EnqueueMusic( audioID );
    }

    /// <summary>
    /// Gets a copy of the current playlist audioID array
    /// </summary>
    /// <returns>
    /// The playlist array
    /// </returns>
    static public string[] GetMusicPlaylist()
    {
        string[] playlistCopy = new string[Instance.musicPlaylist != null ? Instance.musicPlaylist.Length : 0 ];

        if ( playlistCopy.Length > 0 )
        {
            Array.Copy( Instance.musicPlaylist, playlistCopy, playlistCopy.Length );
        }
        return playlistCopy;
    }

    /// <summary>
    /// Sets the current playlist to the specified audioID array
    /// </summary>
    /// <param name="playlist">The new playlist array</param>
    static public void SetMusicPlaylist( string[ ] playlist )
    {

        string[ ] playlistCopy = new string[ playlist != null ? playlist.Length : 0 ];

        if ( playlistCopy.Length > 0 )
        {
            Array.Copy( playlist, playlistCopy, playlistCopy.Length );
        }
        Instance.musicPlaylist = playlistCopy;
    }

    /// <summary>
    /// Start playing the music playlist.
    /// </summary>
    /// <returns>
    /// The <c>AudioObject</c> of the current music, or <c>null</c> if no music track could be played.
    /// </returns>
    static public AudioObject PlayMusicPlaylist()
    {
        return Instance._PlayMusicPlaylist();
    }

    /// <summary>
    /// Jumps to the next the music track on the playlist.
    /// </summary>
    /// <remarks>
    /// If shuffling is enabled it will jump to the next randomly chosen track.
    /// </remarks>
    /// <returns>
    /// The <c>AudioObject</c> of the current music, or <c>null</c> if no music track could be played.
    /// </returns>
    static public AudioObject PlayNextMusicOnPlaylist()
    {
        if ( IsPlaylistPlaying() )
        {
            return Instance._PlayNextMusicOnPlaylist( 0 );
        }
        else
            return null;
    }

    /// <summary>
    /// Jumps to the previous music track on the playlist.
    /// </summary>
    /// <remarks>
    /// If shuffling is enabled it will jump to the previously played track.
    /// </remarks>
    /// <returns>
    /// The <c>AudioObject</c> of the current music, or <c>null</c> if no music track could be played.
    /// </returns>
    static public AudioObject PlayPreviousMusicOnPlaylist()
    {
        if ( IsPlaylistPlaying() )
        {
            return Instance._PlayPreviousMusicOnPlaylist( 0 );
        }
        else
            return null;
    }

    /// <summary>
    /// Determines whether the playlist is playing.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if the current music track is from the playlist; otherwise, <c>false</c>.
    /// </returns>
    static public bool IsPlaylistPlaying()
    {
        return _isPlaylistPlaying;
    }

    /// <summary>
    /// Clears the music playlist.
    /// </summary>
    static public void ClearPlaylist()
    {
        Instance.musicPlaylist = null;
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c>.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <param name="startTime">The start time [default=0]</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// If "3D sound" is enabled in the audio import settings of the audio clip the object will be placed right 
    /// in front of the current audio listener which is usually on the main camera. Note that the audio object will not
    /// be parented - so you will hear when the audio listener moves.
    /// </remarks>
    static public AudioObject Play( string audioID, float volume, float delay, float startTime )
    {
        AudioListener al = GetCurrentAudioListener();

        if ( al == null )
        {
            Debug.LogWarning( "No AudioListener found in the scene" );
            return null;
        }

        return Play( audioID, al.transform.position + al.transform.forward, null, volume, delay, startTime );
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c>.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject Play( string audioID ) 
    { 
        return AudioController.Play( audioID, 1, 0, 0 ); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c>.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject Play( string audioID, float volume ) 
    { 
        return AudioController.Play( audioID, volume, 0, 0 ); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c>.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject Play( string audioID, float volume, float delay ) 
    { 
        return AudioController.Play( audioID, volume, delay, 0 ); 
    }
		
    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> parented to a specified transform.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="parentObj">The parent transform.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <param name="startTime">The start time [default=0]</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// If the audio clip is marked as 3D the audio clip will be played at the position of the parent transform. 
    /// As the audio object will get attached to the transform, it is important to destroy the parent object using the
    /// <see cref="ObjectPoolController.Destroy"/> function, even if the parent object is not poolable itself
    /// </remarks>
    static public AudioObject Play( string audioID, Transform parentObj, float volume, float delay, float startTime )
    {
        return Play( audioID, parentObj.position, parentObj, volume, delay, startTime );
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> parented to a specified transform.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="parentObj">The parent transform.</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, Transform, float, float, float )"/> with default parameters.
    /// </remarks>
    static public AudioObject Play( string audioID, Transform parentObj ) 
    { 
        return AudioController.Play( audioID, parentObj, 1, 0, 0 ); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> parented to a specified transform.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="parentObj">The parent transform.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, Transform, float, float, float )"/> with default parameters.
    /// </remarks>
    static public AudioObject Play( string audioID, Transform parentObj, float volume ) 
    { 
        return AudioController.Play( audioID, parentObj, volume, 0, 0 ); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> parented to a specified transform.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="parentObj">The parent transform.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, Transform, float, float, float )"/> with default parameters.
    /// </remarks>
    static public AudioObject Play( string audioID, Transform parentObj, float volume, float delay ) 
    { 
        return AudioController.Play( audioID, parentObj, volume, delay, 0 ); 
    }
	

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> at a specified position.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="position">The position in world coordinates.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <param name="startTime">The start time [default=0]</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// If the audio clip is marked as 3D the audio clip will be played at the specified world position.
    /// </remarks>
    static public AudioObject Play( string audioID, Vector3 position, float volume, float delay, float startTime )
    {
        return Play( audioID, position, null, volume, delay, startTime );
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> at a specified position.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="position">The position in world coordinates.</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, Vector3, float, float, float )"/> with default parameters.
    /// </remarks>
    static public AudioObject Play( string audioID, Vector3 position ) 
    { 
        return AudioController.Play( audioID, position, 1, 0, 0 ); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> at a specified position.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="position">The position in world coordinates.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <remarks>
    /// Variant of <see cref="Play( string, Vector3, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject Play( string audioID, Vector3 position, float volume ) 
    { 
        return AudioController.Play( audioID, position, volume, 0, 0 ); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> at a specified position.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="position">The position in world coordinates.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, Vector3, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject Play( string audioID, Vector3 position, float volume, float delay ) 
    { 
        return AudioController.Play( audioID, position, volume, delay, 0 ); 
    }
	
    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> parented to a specified transform with a world offset.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="worldPosition">The position in world coordinates.</param>
    /// <param name="parentObj">The parent transform.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <param name="startTime">The start time [default=0]</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// If the audio clip is marked as 3D the audio clip will be played at the position of the parent transform. 
    /// As the audio object will get attached to the transform, it is important to destroy the parent object using the
    /// <see cref="ObjectPoolController.Destroy"/> function, even if the parent object is not poolable itself
    /// </remarks>
    static public AudioObject Play( string audioID, Vector3 worldPosition, Transform parentObj, float volume, float delay, float startTime )
    {
        //Debug.Log( "Play: '" + audioID + "'" );
        return Instance._Play( audioID, volume, worldPosition, parentObj, delay, startTime, false );
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> parented to a specified transform with a world offset.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="worldPosition">The position in world coordinates.</param>
    /// <param name="parentObj">The parent transform.</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, Vector3, Transform, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject Play( string audioID, Vector3 worldPosition, Transform parentObj ) 
    { 
        return AudioController.Play( audioID, worldPosition, parentObj, 1, 0, 0); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> parented to a specified transform with a world offset.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="worldPosition">The position in world coordinates.</param>
    /// <param name="parentObj">The parent transform.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, Vector3, Transform, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject Play( string audioID, Vector3 worldPosition, Transform parentObj, float volume ) 
    { 
        return AudioController.Play( audioID, worldPosition, parentObj, volume, 0, 0); 
    }

    /// <summary>
    /// Plays an audio item with the name <c>audioID</c> parented to a specified transform with a world offset.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="worldPosition">The position in world coordinates.</param>
    /// <param name="parentObj">The parent transform.</param>
    /// <param name="volume">The volume [default=1].</param>
    /// <param name="delay">The delay [default=0].</param>
    /// <returns>
    /// Returns the reference of the AudioObject that is used to play the audio item, or <c>null</c> if the audioID does not exist.
    /// </returns>
    /// <remarks>
    /// Variant of <see cref="Play( string, Vector3, Transform, float, float, float )"/> with default parameters.
    /// </remarks>
	static public AudioObject Play( string audioID, Vector3 worldPosition, Transform parentObj, float volume, float delay ) 
    { 
        return AudioController.Play( audioID, worldPosition, parentObj, volume, delay, 0); 
    }

    /// <summary>
    /// Stops all playing audio items with name <c>audioID</c> with a fade-out.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <param name="fadeOutTime">The fade out time.</param>
    /// <returns>Return <c>true</c> if any audio was stopped.</returns>
    static public bool Stop( string audioID, float fadeOutTime )
    {
        AudioItem sndItem = Instance._GetAudioItem( audioID );

        if ( sndItem == null )
        {
            Debug.LogWarning( "Audio item with name '" + audioID + "' does not exist" );
            return false;
        }

        //if ( sndItem.PlayInstead.Length > 0 )
        //{
        //    return Stop( sndItem.PlayInstead, fadeOutTime );
        //}

        AudioObject[ ] audioObjs = GetPlayingAudioObjects( audioID );
        
        foreach( AudioObject  audioObj in audioObjs )
        {
            audioObj.Stop( fadeOutTime );
        }
        return audioObjs.Length > 0;
    }

    /// <summary>
    /// Stops all playing audio items with name <c>audioID</c>.
    /// </summary>
    /// <returns>Return <c>true</c> if any audio was stopped.</returns>
    static public bool Stop( string audioID ) 
    { 
        return AudioController.Stop( audioID, 0 ); 
    }

    /// <summary>
    /// Fades out all playing audio items (including the music).
    /// </summary>
    /// <param name="fadeOutTime">The fade out time.</param>
    static public void StopAll( float fadeOutTime )
    {
        Instance._StopMusic( fadeOutTime );

        AudioObject[ ] objs = GetPlayingAudioObjects();
        
        foreach ( AudioObject o in objs )
        {
            o.Stop( fadeOutTime );
        }
    }

    /// <summary>
    /// Immediately stops playing audio items (including the music).
    /// </summary>
	static public void StopAll() 
    { 
        AudioController.StopAll( 0 ); 
    }

    /// <summary>
    /// Pauses all playing audio items (including the music).
    /// </summary>
    static public void PauseAll()
    {
        Instance._PauseMusic();

        AudioObject[ ] objs = GetPlayingAudioObjects();

        foreach ( AudioObject o in objs )
        {
            o.Pause();
        }
    }

    /// <summary>
    /// Un-pauses all playing audio items (including the music).
    /// </summary>
    static public void UnpauseAll()
    {
        AudioController.UnpauseMusic();

        AudioObject[ ] objs = GetPlayingAudioObjects();

        foreach ( AudioObject o in objs )
        {
            if( o.IsPaused() ) o.Play();
        }
    }

    /// <summary>
    /// Pauses all playing audio items in the specified category (including the music).
    /// </summary>
    /// <param name="categoryName">Name of category.</param>
    static public void PauseCategory( string categoryName )
    {
        if ( _currentMusic != null && _currentMusic.category.Name == categoryName ) AudioController.PauseMusic();

        AudioObject[ ] objs = GetPlayingAudioObjectsInCategory( categoryName );

        foreach ( AudioObject o in objs )
        {
            o.Pause();
        }
    }

    /// <summary>
    /// Un-pauses all playing audio items in the specified category (including the music).
    /// </summary>
    /// <param name="categoryName">Name of category.</param>
    static public void UnpauseCategory( string categoryName )
    {
        if ( _currentMusic != null && _currentMusic.category.Name == categoryName ) AudioController.UnpauseMusic();

        AudioObject[ ] objs = GetPlayingAudioObjectsInCategory( categoryName );

        foreach ( AudioObject o in objs )
        {
            if ( o.IsPaused() ) o.Play();
        }
    }

    /// <summary>
    /// Determines whether the specified audio ID is playing.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <returns>
    ///   <c>true</c> if the specified audio ID is playing; otherwise, <c>false</c>.
    /// </returns>
    static public bool IsPlaying( string audioID )
    {
        return GetPlayingAudioObjects( audioID ).Length > 0;
    }

    /// <summary>
    /// Returns an array of all playing audio objects with the specified <c>audioID</c>.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <returns>
    /// Array of all playing audio objects with the specified <c>audioID</c>.
    /// </returns>
    static public AudioObject[] GetPlayingAudioObjects( string audioID )
    {
        AudioObject[ ] objs = GetPlayingAudioObjects();
        var matchesList = new List<AudioObject>();

        foreach ( AudioObject o in objs )
        {
            if ( o.audioID == audioID )
            {
                if ( o.IsPlaying() )
                {
                    matchesList.Add( o );
                }
            }
        }
        return matchesList.ToArray();
    }

    /// <summary>
    /// Returns an array of all playing audio objects in the category with name <c>categoryName</c>.
    /// </summary>
    /// <param name="categoryName">The category name.</param>
    /// <returns>
    /// Array of all playing audio objects in the specified category.
    /// </returns>
    static public AudioObject[ ] GetPlayingAudioObjectsInCategory( string categoryName )
    {
        AudioObject[ ] objs = GetPlayingAudioObjects();
        var matchesList = new List<AudioObject>();

        foreach ( AudioObject o in objs )
        {
            if ( o.category != null && o.category.Name == categoryName )
            {
                if ( o.IsPlaying() )
                {
                    matchesList.Add( o );
                }
            }
        }
        return matchesList.ToArray();
    }

    /// <summary>
    /// Returns an array of all playing audio objects.
    /// </summary>
    /// <returns>
    /// Array of all playing audio objects.
    /// </returns>
    static public AudioObject[ ] GetPlayingAudioObjects()
    {
#if UNITY_FLASH

       object[] objs = RegisteredComponentController.GetAllOfType( typeof(AudioObject) );
       AudioObject[ ] audioObjList = new AudioObject[ objs.Length ];
       for( int i=0; i<objs.Length; i++ )
       {
           audioObjList[ i ] = (AudioObject) objs[ i ];
       }
       return audioObjList;
#else
        AudioObject[ ] objs = RegisteredComponentController.GetAllOfType<AudioObject>();
        return objs;
#endif
    }

    /// <summary>
    /// Returns the number of all playing audio objects with the specified <c>audioID</c>.
    /// </summary>
    /// <param name="audioID">The audio ID.</param>
    /// <returns>
    /// Number of all playing audio objects with the specified <c>audioID</c>.
    /// </returns>
    static public int GetPlayingAudioObjectsCount( string audioID )
    {
        AudioObject[ ] objs = GetPlayingAudioObjects();
      
        int count = 0;

        foreach ( AudioObject o in objs )
        {
            if ( o.audioID == audioID )
            {
                if ( o.IsPlaying() )
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Enables the music.
    /// </summary>
    /// <param name="b">if set to <c>true</c> [b].</param>
    static public void EnableMusic( bool b )
    {
        AudioController.Instance.musicEnabled = b;
    }

    /// <summary>
    /// Determines whether music is enabled.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if music is enabled; otherwise, <c>false</c>.
    /// </returns>
    static public bool IsMusicEnabled()
    {
        return AudioController.Instance.musicEnabled;
    }

    /// <summary>
    /// Gets the currently active Unity audio listener.
    /// </summary>
    /// <returns>
    /// Reference of the currently active AudioListener object.
    /// </returns>
    static public AudioListener GetCurrentAudioListener()
    {
        AudioController MyInstance = Instance;
        if ( MyInstance._currentAudioListener != null && MyInstance._currentAudioListener.gameObject == null ) // TODO: check if this is necessary and if it really works if object was destroyed
        {
            MyInstance._currentAudioListener = null;
        }

        if ( MyInstance._currentAudioListener == null )
        {
            MyInstance._currentAudioListener = (AudioListener) FindObjectOfType( typeof( AudioListener ) );
        }

        return MyInstance._currentAudioListener;
    }



    /// <summary>
    /// Gets the current music.
    /// </summary>
    /// <returns>
    /// Returns a reference to the AudioObject that is currently playing the music.
    /// </returns>
    static public AudioObject GetCurrentMusic()
    {
        return AudioController._currentMusic;
    }

    /// <summary>
    /// Gets a category.
    /// </summary>
    /// <param name="name">The category's name.</param>
    /// <returns></returns>
    static public AudioCategory GetCategory( string name )
    {
        return AudioController.Instance._GetCategory( name );
    }

    /// <summary>
    /// Changes the category volume. Also effects currently playing audio items.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <param name="volume">The volume.</param>
    static public void SetCategoryVolume( string name, float volume )
    {
        AudioCategory category = GetCategory( name );
        if ( category != null )
        {
            category.Volume = volume;
        }
        else
        {
            Debug.LogWarning( "No audio category with name " + name );
        }
    }

    /// <summary>
    /// Gets the category volume.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <returns></returns>
    static public float GetCategoryVolume( string name )
    {
        AudioCategory category = GetCategory( name );
        if ( category != null )
        {
            return category.Volume;
        }
        else
        {
            Debug.LogWarning( "No audio category with name " + name );
            return 0;
        }
    }

    /// <summary>
    /// Changes the global volume. Effects all currently playing audio items.
    /// </summary>
    /// <param name="volume">The volume.</param>
    static public void SetGlobalVolume(  float volume )
    {
        Instance.Volume = volume;
    }

    /// <summary>
    /// Gets the global volume.
    /// </summary>
    static public float GetGlobalVolume()
    {
        return Instance.Volume;
    }

    /// <summary>
    /// Creates a new audio category
    /// </summary>
    /// <param name="categoryName">Name of the category.</param>
    /// <returns>
    /// Reference to the new category.
    /// </returns>
    static public AudioCategory NewCategory( string categoryName )
    {
        // can not use ArrayHelper at this point because of buggy Flash compiler :(

        int oldCategoryCount = Instance.AudioCategories != null ? Instance.AudioCategories.Length : 0;
        var oldArray = Instance.AudioCategories;
        Instance.AudioCategories = new AudioCategory[ oldCategoryCount + 1 ];
        
        if ( oldCategoryCount > 0)
        {
            oldArray.CopyTo( Instance.AudioCategories, 0 );
        }
        
        var newCategory = new AudioCategory();
        newCategory.Name = categoryName;

        Instance.AudioCategories[ oldCategoryCount ] = newCategory;
        Instance._InvalidateCategories();
        return newCategory;
    }
    
    
    /// <summary>
    /// Removes an audio category.
    /// </summary>
    /// <param name="categoryName">Name of the category to remove.</param>
    static public void RemoveCategory( string categoryName )
    {
        int i, index = -1;
        int oldCategoryCount;

        if ( Instance.AudioCategories != null )
        {
            oldCategoryCount = Instance.AudioCategories.Length;
        }
        else
            oldCategoryCount = 0;

        for ( i = 0; i < oldCategoryCount; i++ )
        {
            if ( Instance.AudioCategories[ i ].Name == categoryName )
            {
                index = i;
                break;
            }
        }

        if ( index == -1 )
        {
            Debug.LogError( "AudioCategory does not exist: " + categoryName );
            return;
        }

        //ArrayHelper.DeleteArrayElement( ref Instance.AudioCategories, index ); // can not use ArrayHelper because of buggy Flash compiler :(
        {
            var newArray = new AudioCategory[ Instance.AudioCategories.Length - 1 ];

            for ( i = 0; i < index; i++ )
            {
                newArray[ i ] = Instance.AudioCategories[ i ];
            }
            for ( i = index + 1; i < Instance.AudioCategories.Length; i++ )
            {
                newArray[ i - 1 ] = Instance.AudioCategories[ i ];
            }
            Instance.AudioCategories = newArray;
        }

        Instance._InvalidateCategories();
    }

    /// <summary>
    /// Adds a custom audio item to a category.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="audioItem">The audio item.</param>
    /// <example>
    /// <code>
    /// var audioItem = new AudioItem();
    /// audioItem.SubItemPickMode = AudioPickSubItemMode.Sequence;
    /// 
    /// audioItem.subItems = new AudioSubItem[ 2 ];
    /// 
    /// audioItem.subItems[ 0 ] = new AudioSubItem();
    /// audioItem.subItems[ 0 ].Clip = audioClip0;
    /// audioItem.subItems[ 0 ].Volume = 0.7f;
    /// 
    /// audioItem.subItems[ 1 ] = new AudioSubItem();
    /// audioItem.subItems[ 1 ].Clip = audioClip1;
    /// audioItem.subItems[ 1 ].Volume = 0.8f;
    /// 
    /// AddToCategory( GetCategory( "CustomSFX" ), audioItem );
    /// </code>
    /// </example>
    /// <seealso cref="AudioController.NewCategory(string)"/>
    /// <seealso cref="AudioController.GetCategory(string)"/>
    static public void AddToCategory( AudioCategory category, AudioItem audioItem )
    {
        // can not use here because of Flash compuler bug: ArrayHelper.AddArrayElement( ref category.AudioItems, audioItem );

        int oldCount = category.AudioItems != null ? category.AudioItems.Length : 0;
        var oldArray = category.AudioItems;
        category.AudioItems = new AudioItem[ oldCount + 1 ];

        if ( oldCount > 0 )
        {
            oldArray.CopyTo( category.AudioItems, 0 );
        }

        category.AudioItems[ oldCount ] = audioItem;
        Instance._InvalidateCategories();
    }

    /// <summary>
    /// Creates an AudioItem with the name <c>audioID</c> containing a single subitem playing the specified 
    /// custom AudioClip. This AudioItem is then added to the specified category.
    /// </summary>
    /// <param name="category">The category.</param>
    /// <param name="audioClip">The custom audio clip.</param>
    /// <param name="audioID">The audioID for the AudioItem to create.</param>
    /// <returns>The <see cref="AudioItem"/> created with the specified <c>audioID</c></returns>
    /// <seealso cref="AudioController.NewCategory(string)"/>
    /// <seealso cref="AudioController.GetCategory(string)"/>
    static public AudioItem AddToCategory( AudioCategory category, AudioClip audioClip, string audioID )
    {
        var audioItem = new AudioItem();
        audioItem.Name = audioID;
        audioItem.subItems = new AudioSubItem[ 1 ];
        
        var audioSubItem = new AudioSubItem();
        audioSubItem.Clip = audioClip;
        audioItem.subItems[ 0 ] = audioSubItem;
        
        AddToCategory( category, audioItem );
        return audioItem;
    }

    /// <summary>
    /// Removes an AudioItem from the AudioController.
    /// </summary>
    /// <param name="audioID">Name of the audio item to remove.</param>
    /// <returns><c>true</c> if the audio item was found and successfully removed, otherwise <c>false</c></returns>
    static public bool RemoveAudioItem( string audioID )
    {
        var audioItem = Instance._GetAudioItem( audioID );
        
        if ( audioItem != null )
        {
            int index = audioItem.category._GetIndexOf( audioItem );
            if ( index < 0 ) return false; // should never be the case!

            var array = audioItem.category.AudioItems;

            //ArrayHelper.DeleteArrayElement( audioItem.category, index ); // Flash export does not currently work!! 
            { 
                var newArray = new AudioItem[ array.Length - 1 ];
                int i;
                for ( i = 0; i < index; i++ )
                {
                    newArray[ i ] = array[ i ];
                }
                for ( i = index + 1; i < array.Length; i++ )
                {
                    newArray[ i - 1 ] = array[ i ];
                }
                audioItem.category.AudioItems = newArray;
            }
            
            if( Instance._categoriesValidated )
            {
                Instance._audioItems.Remove( audioID );
            }
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Tests if a given <c>audioID</c> is valid.
    /// </summary>
    /// <param name="audioID"></param>
    /// <returns><c>true</c> if the <c>audioID</c> is valid</returns>
    static public bool IsValidAudioID( string audioID )
    {
        return Instance._GetAudioItem( audioID ) != null;
    }

    // **************************************************************************************************/
    //          private / protected functions and properties
    // **************************************************************************************************/

#if AUDIO_TOOLKIT_DEMO
    static protected AudioObject _currentMusic;
#else
    static protected PoolableReference<AudioObject> _currentMusicReference = new PoolableReference<AudioObject>();
    static private AudioObject _currentMusic
    {
        set
        {
            _currentMusicReference.Set( value, true  );
        }
        get
        {
            return _currentMusicReference.Get();
        }
    }
#endif
    protected AudioListener _currentAudioListener = null;

    private bool _musicEnabled = true;
    private bool _categoriesValidated = false;

    [SerializeField]
    private bool _audioDisabled = false;

    Dictionary<string, AudioItem> _audioItems;

    static List<int> _playlistPlayed;
    static bool _isPlaylistPlaying = false;

    [SerializeField]
    private float _volume = 1.0f;

    static private double _systemTime;
    

    private void _ApplyVolumeChange()
    {
        AudioObject[ ] objs = GetPlayingAudioObjects();

        foreach ( AudioObject o in objs )
        {
            o._ApplyVolume();
        }
    }

    internal AudioItem _GetAudioItem( string audioID )
    {
        AudioItem sndItem;

        _ValidateCategories();

        if ( _audioItems.TryGetValue( audioID, out sndItem ) )
        {
            return sndItem;
        }

        return null;
    }
    
    protected AudioObject _PlayMusic( string audioID, float volume, float delay, float startTime )
    {
        AudioListener al = GetCurrentAudioListener();
        if ( al == null )
        {
            Debug.LogWarning( "No AudioListener found in the scene" );
            return null;
        }
        return _PlayMusic( audioID, al.transform.position + al.transform.forward, null, volume, delay, startTime );
    }

    protected bool _StopMusic( float fadeOutTime )
    {
        if ( _currentMusic != null )
        {
            _currentMusic.Stop( fadeOutTime );
            _currentMusic = null;
            return true;
        }
        return false;
    }

    protected bool _PauseMusic()
    {
        if ( _currentMusic != null )
        {
            _currentMusic.Pause();
            return true;
        }
        return false;
    }

    protected AudioObject _PlayMusic( string audioID, Vector3 position, Transform parentObj, float volume, float delay, float startTime )
    {
        if ( !IsMusicEnabled() ) return null;

        bool doFadeIn;

        if ( _currentMusic != null )
        {
            doFadeIn = true;
            _currentMusic.Stop( musicCrossFadeTime );
        }
        else
            doFadeIn = false;

        //Debug.Log( "PlayMusic " + audioID );

        _currentMusic = _Play( audioID, volume, position, parentObj, delay, startTime, false );

        if ( _currentMusic  )
        {
            if ( doFadeIn && musicCrossFadeTime > 0 )
            {
                _currentMusic.FadeIn( musicCrossFadeTime );
            }
        }

        return _currentMusic;
    }

    protected int _EnqueueMusic( string audioID )
    {
        int newLength;

        if ( musicPlaylist == null )
        {
            newLength = 1;
        }
        else
            newLength = musicPlaylist.Length + 1;

        string[ ] newPlayList = new string[ newLength ];

        if ( musicPlaylist != null )
        {
            musicPlaylist.CopyTo( newPlayList, 0 );
        }

        newPlayList[ newLength - 1 ] = audioID;
        musicPlaylist = newPlayList;

        return newLength;
    }

    protected AudioObject _PlayMusicPlaylist()
    {
        _ResetLastPlayedList();
        return _PlayNextMusicOnPlaylist( 0 );
    }

    private AudioObject _PlayMusicTrackWithID( int nextTrack, float delay, bool addToPlayedList )
    {
        if ( nextTrack < 0 )
        {
            return null;
        }
        _playlistPlayed.Add( nextTrack );
        _isPlaylistPlaying = true;
        //Debug.Log( "nextTrack: " + nextTrack );
        AudioObject audioObj = _PlayMusic( musicPlaylist[ nextTrack ], 1, delay, 0 );

        if ( audioObj != null )
        {
            audioObj._isCurrentPlaylistTrack = true;
            audioObj.audio.loop = false;
        }
        return audioObj;
    }

    internal AudioObject _PlayNextMusicOnPlaylist( float delay )
    {
        int nextTrack = _GetNextMusicTrack();
        return _PlayMusicTrackWithID( nextTrack, delay, true );
    }

    internal AudioObject _PlayPreviousMusicOnPlaylist( float delay )
    {
        int nextTrack = _GetPreviousMusicTrack();
        return _PlayMusicTrackWithID( nextTrack, delay, false );
    }

    private void _ResetLastPlayedList()
    {
        _playlistPlayed.Clear();
    }

    protected int _GetNextMusicTrack()
    {
        if ( musicPlaylist == null || musicPlaylist.Length == 0 ) return -1;
        if ( musicPlaylist.Length == 1 ) return 0;

        if ( shufflePlaylist )
        {
            return _GetNextMusicTrackShuffled();
        }
        else
        {
            return _GetNextMusicTrackInOrder();

        }
    }

    protected int _GetPreviousMusicTrack()
    {
        if ( musicPlaylist == null || musicPlaylist.Length == 0 ) return -1;
        if ( musicPlaylist.Length == 1 ) return 0;

        if ( shufflePlaylist )
        {
            return _GetPreviousMusicTrackShuffled();
        }
        else
        {
            return _GetPreviousMusicTrackInOrder();

        }
    }

    private int _GetPreviousMusicTrackShuffled()
    {
        if ( _playlistPlayed.Count >= 2 )
        {
            int id = _playlistPlayed[ _playlistPlayed.Count - 2 ];

            _RemoveLastPlayedOnList();
            _RemoveLastPlayedOnList();

            return id;
        }
        else
            return -1;
    }

    private void _RemoveLastPlayedOnList()
    {
        _playlistPlayed.RemoveAt( _playlistPlayed.Count - 1 );
    }

    private int _GetNextMusicTrackShuffled()
    {
        var playedTracksHashed = new HashSet_Flash<int>();

        int disallowTracksCount = _playlistPlayed.Count;

        int randomElementCount;

        if ( loopPlaylist )
        {
            randomElementCount = Mathf.Clamp( musicPlaylist.Length / 4, 2, 10 );

            if ( disallowTracksCount > musicPlaylist.Length - randomElementCount )
            {
                disallowTracksCount = musicPlaylist.Length - randomElementCount;

                if ( disallowTracksCount < 1 ) // the same track must never be played twice in a row
                {
                    disallowTracksCount = 1; // musicPlaylist.Length is always >= 2 
                }
            }
        }
        else
        {
            // do not play the same song twice
            if ( disallowTracksCount >= musicPlaylist.Length ) 
            {
                return -1; // stop playing as soon as all tracks have been played 
            }
        }
        
        
        for ( int i = 0; i < disallowTracksCount; i++ )
        {
            playedTracksHashed.Add( _playlistPlayed[ _playlistPlayed.Count - 1 - i ] );
        }

        var possibleTrackIDs = new List<int>();

        for ( int i = 0; i < musicPlaylist.Length; i++ )
        {
            if ( !playedTracksHashed.Contains( i ) )
            {
                possibleTrackIDs.Add( i );
            }
        }

        return possibleTrackIDs[ UnityEngine.Random.Range( 0, possibleTrackIDs.Count ) ];
    }

    private int _GetNextMusicTrackInOrder()
    {
        if ( _playlistPlayed.Count == 0 )
        {
            return 0;
        }
        int next = _playlistPlayed[ _playlistPlayed.Count - 1 ] + 1;

        if ( next >= musicPlaylist.Length ) // reached the end of the playlist
        {
            if ( loopPlaylist )
            {
                next = 0;
            }
            else
                return -1;
        }
        return next;
    }

    private int _GetPreviousMusicTrackInOrder()
    {
        if ( _playlistPlayed.Count < 2 )
        {
            if ( loopPlaylist )
            {
                return musicPlaylist.Length - 1;
            }
            else
                return -1;
        }

        int next = _playlistPlayed[ _playlistPlayed.Count - 1 ] - 1;

        _RemoveLastPlayedOnList();
        _RemoveLastPlayedOnList();

        if ( next < 0 ) // reached the end of the playlist
        {
            if ( loopPlaylist )
            {
                next = musicPlaylist.Length - 1;
            }
            else
                return -1;
        }
        return next;
    }

    protected AudioObject _Play( string audioID, float volume, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject )
    {
        if ( _audioDisabled ) return null;

        AudioItem sndItem = _GetAudioItem( audioID );
        if ( sndItem == null )
        {
            Debug.LogWarning( "Audio item with name '" + audioID + "' does not exist" );
            return null;
        }

      
        if ( sndItem._lastPlayedTime > 0 )
        {
            if ( AudioController.systemTime < sndItem._lastPlayedTime + sndItem.MinTimeBetweenPlayCalls )
            {
               
                return null;
            }
        }

        if ( sndItem.MaxInstanceCount > 0 )
        {
            var playingAudios = GetPlayingAudioObjects( audioID );

            if ( playingAudios.Length >= sndItem.MaxInstanceCount )
            {
                // search oldest audio and stop it.
                AudioObject oldestAudio = null;

                for ( int i = 0; i < playingAudios.Length; i++ )
                {
                    if ( playingAudios[ i ].isFadingOut )
                    {
                        continue;
                    }
                    if ( oldestAudio == null || playingAudios[ i ].startedPlayingAtTime < oldestAudio.startedPlayingAtTime )
                    {
                        oldestAudio = playingAudios[ i ];
                    }
                }
                //oldestAudio.DestroyAudioObject(); // produces cracking noise

                if ( oldestAudio != null )
                {
                    oldestAudio.Stop( 0.2f );
                }
                
            }
        }

        return PlayAudioItem( sndItem, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject );
    }

    /// <summary>
    /// Plays a specific AudioItem.
    /// </summary>
    /// <remarks>
    /// This function is used by the editor extension and is normally not required for application developers. 
    /// Use <see cref="AudioController.Play(string)"/> instead.
    /// </remarks>
    /// <param name="sndItem">the AudioItem</param>
    /// <param name="volume">the volume</param>
    /// <param name="worldPosition">the world position </param>
    /// <param name="parentObj">the parent object, or <c>null</c></param>
    /// <param name="delay">the delay in seconds</param>
    /// <param name="startTime">the start time seconds</param>
    /// <param name="playWithoutAudioObject">if <c>true</c>plays the audio by using the Unity 
    /// function <c>PlayOneShot</c> without creating an audio game object. Allows playing audios from within the Unity inspector.
    /// </param>
    /// <returns>
    /// The created <see cref="AudioObject"/> or <c>null</c>
    /// </returns>
    public AudioObject PlayAudioItem( AudioItem sndItem, float volume, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject )
    {
        AudioObject audioObj = null;

        //Debug.Log( "_Play '" + audioID + "'" );

        sndItem._lastPlayedTime = AudioController.systemTime;

        AudioSubItem[] sndSubItems = _ChooseSubItems( sndItem );

        if ( sndSubItems == null )
        {
            return null;
        }

        foreach ( var sndSubItem in sndSubItems )
        {
            if ( sndSubItem != null )
            {
                var audioObjRet = PlayAudioSubItem( sndSubItem, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject );

                if ( audioObjRet )
                {
                    audioObj = audioObjRet;
                    audioObj.audioID = sndItem.Name;
                }
            }
        }

        return audioObj;
    }

    protected AudioCategory _GetCategory( string name )
    {
        foreach ( AudioCategory cat in AudioCategories )
        {
            if ( cat.Name == name )
            {
                return cat;
            }
        }
        return null;
    }


    void Update()
    {
        _UpdateSystemTime();
    }

    static private void _UpdateSystemTime()
    {
        _systemTime = SystemTime.timeSinceLaunch;
    }


#if AUDIO_TOOLKIT_DEMO
    protected virtual void Awake()
    {
#else
    protected override void Awake()
    {
        base.Awake();
        // all initialisation must be done in AwakeSingleton()
#endif
    }

    void AwakeSingleton() // is called by singleton, can be called before Awake() 
    {
        _UpdateSystemTime();

        if ( Persistent )
        {
            DontDestroyOnLoad( gameObject );
        }

        if ( AudioObjectPrefab == null )
        {
            Debug.LogError( "No AudioObject prefab specified in AudioController." );
        }
        else
        {
            _ValidateAudioObjectPrefab( AudioObjectPrefab );
        }
        _ValidateCategories();

        if ( _playlistPlayed == null )
        {
            _playlistPlayed = new List<int>();
            _isPlaylistPlaying = false;
        }
    }

    protected void _ValidateCategories()
    {
        if ( !_categoriesValidated )
        {
            InitializeAudioItems();

            _categoriesValidated = true;
        }
    }

    protected void _InvalidateCategories()
    {
        _categoriesValidated = false;
    }

    /// <summary>
    /// Updates the internal <c>audioID</c> dictionary and initializes all registered <see cref="AudioItem"/> objects.
    /// </summary>
    /// <remarks>
    /// There is no need to call this function manually, unless <see cref="AudioItem"/> objects or categories are changed at runtime.
    /// </remarks>
    public void InitializeAudioItems()
    {
        _audioItems = new Dictionary<string, AudioItem>();

        foreach ( AudioCategory category in AudioCategories )
        {
            category._AnalyseAudioItems( _audioItems );

            if ( category.AudioObjectPrefab )
            {
                _ValidateAudioObjectPrefab( category.AudioObjectPrefab );
            }
        }
    }

    protected static AudioSubItem[] _ChooseSubItems( AudioItem audioItem )
    {
        return _ChooseSubItems( audioItem, audioItem.SubItemPickMode );
    }


    protected static AudioSubItem[] _ChooseSubItems( AudioItem audioItem, AudioPickSubItemMode pickMode )
    {
        if ( audioItem.subItems == null ) return null;
        int arraySize = audioItem.subItems.Length;
        if ( arraySize == 0 ) return null;

        int chosen = 0;
        AudioSubItem[] chosenItems;

        if ( arraySize > 1 )
        {
            switch ( pickMode )
            {
            case AudioPickSubItemMode.Disabled:
                return null;

            case AudioPickSubItemMode.Sequence:
                chosen = ( audioItem._lastChosen + 1 ) % arraySize;
                break;

            case AudioPickSubItemMode.SequenceWithRandomStart:
                if ( audioItem._lastChosen == -1 )
                {
                    chosen = UnityEngine.Random.Range( 0, arraySize );
                }
                else
                    chosen = ( audioItem._lastChosen + 1 ) % arraySize;
                break;

            case AudioPickSubItemMode.Random:
                chosen = _ChooseRandomSubitem( audioItem, true );
                break;

            case AudioPickSubItemMode.RandomNotSameTwice:
                chosen = _ChooseRandomSubitem( audioItem, false );
                break;

            case AudioPickSubItemMode.AllSimultaneously:
                chosenItems = new AudioSubItem[ arraySize ];
                Array.Copy( audioItem.subItems, chosenItems, arraySize );
                return chosenItems;

            case AudioPickSubItemMode.TwoSimultaneously:
                chosenItems = new AudioSubItem[ 2 ];
                chosenItems[ 0 ] = _ChooseSubItems( audioItem, AudioPickSubItemMode.RandomNotSameTwice )[ 0 ];
                chosenItems[ 1 ] = _ChooseSubItems( audioItem, AudioPickSubItemMode.RandomNotSameTwice )[ 0 ];
                return chosenItems;
            }
        }

        audioItem._lastChosen = chosen;
        //Debug.Log( "chose:" + chosen );
        chosenItems = new AudioSubItem[ 1 ];
        chosenItems[0] = audioItem.subItems[ chosen ];
        return chosenItems;
    }

    private static int _ChooseRandomSubitem( AudioItem audioItem, bool allowSameElementTwiceInRow )
    {
        int arraySize = audioItem.subItems.Length; // is >= 2 at this point 
        int chosen = 0;

        float probRange;
        float lastProb = 0;

        if ( !allowSameElementTwiceInRow )
        {
            // find out probability of last chosen sub item
            if ( audioItem._lastChosen >= 0 )
            {
                lastProb = audioItem.subItems[ audioItem._lastChosen ]._SummedProbability;
                if ( audioItem._lastChosen >= 1 )
                {
                    lastProb -= audioItem.subItems[ audioItem._lastChosen - 1 ]._SummedProbability;
                }
            }
            else
                lastProb = 0;

            probRange = 1.0f - lastProb;
        }
        else
            probRange = 1.0f;

        float rnd = UnityEngine.Random.Range( 0, probRange );

        int i;
        for ( i = 0; i < arraySize - 1; i++ )
        {
            float prob;

            prob = audioItem.subItems[ i ]._SummedProbability;

            if ( !allowSameElementTwiceInRow )
            {
                if ( i == audioItem._lastChosen ) 
                {
                    continue; // do not play same audio twice
                }

                if ( i > audioItem._lastChosen )
                {
                    prob -= lastProb;
                }
            }

            if ( prob > rnd )
            {
                chosen = i;
                break;
            }
        }
        if ( i == arraySize - 1 )
        {
            chosen = arraySize - 1;
        }

        return chosen;
    }
    
    /// <summary>
    /// Plays a specific AudioSubItem.
    /// </summary>
    /// <remarks>
    /// This function is used by the editor extension and is normally not required for application developers. 
    /// Use <see cref="AudioController.Play(string)"/> instead.
    /// </remarks>
    /// <param name="subItem">the <see cref="AudioSubItem"/></param>
    /// <param name="volume">the volume</param>
    /// <param name="worldPosition">the world position </param>
    /// <param name="parentObj">the parent object, or <c>null</c></param>
    /// <param name="delay">the delay in seconds</param>
    /// <param name="startTime">the start time seconds</param>
    /// <param name="playWithoutAudioObject">if <c>true</c>plays the audio by using the Unity 
    /// function <c>PlayOneShot</c> without creating an audio game object. Allows playing audios from within the Unity inspector.
    /// </param>
    /// <returns>
    /// The created <see cref="AudioObject"/> or <c>null</c>
    /// </returns>
    public AudioObject PlayAudioSubItem( AudioSubItem subItem, float volume, Vector3 worldPosition, Transform parentObj, float delay, float startTime, bool playWithoutAudioObject )
    {
        var audioItem = subItem.item;

        switch( subItem.SubItemType )
        {
        case AudioSubItemType.Item:
            if ( subItem.ItemModeAudioID.Length == 0 )
            {
                Debug.LogWarning( "No item specified in audio sub-item with ITEM mode (audio item: '" + audioItem.Name + "')" );
                return null;
            }
            return _Play( subItem.ItemModeAudioID, volume, worldPosition, parentObj, delay, startTime, playWithoutAudioObject );

        case AudioSubItemType.Clip:
            break;
        }

        if ( subItem.Clip == null ) return null;

        var audioCategory = audioItem.category;

        float volumeWithoutCategory = subItem.Volume * audioItem.Volume * volume;
        
        if ( subItem.RandomVolume != 0 )
        {
            volumeWithoutCategory += UnityEngine.Random.Range( -subItem.RandomVolume, subItem.RandomVolume );
            volumeWithoutCategory = Mathf.Clamp01( volumeWithoutCategory );
        }

        float volumeWithCategory = volumeWithoutCategory * audioCategory.Volume;

        if ( !PlayWithZeroVolume && ( volumeWithCategory <= 0 || Volume <= 0 ) )
        {
            return null;
        }


        GameObject audioObjInstance;

        //Debug.Log( "PlayAudioItem clip:" + subItem.Clip.name );

        GameObject audioPrefab;

        if ( audioCategory.AudioObjectPrefab != null )
        {
            audioPrefab = audioCategory.AudioObjectPrefab;
        }
        else
            audioPrefab = AudioObjectPrefab;

        if ( playWithoutAudioObject )
        {
            audioPrefab.audio.PlayOneShot( subItem.Clip, AudioObject.TransformVolume( volumeWithCategory ) ); // unfortunately produces warning message, but works (tested only with Unity 3.5)

            //AudioSource.PlayClipAtPoint( subItem.Clip, Vector3.zero, AudioObject.TransformVolume( volumeWithCategory ) );
            return null;
        }

        
        if ( audioItem.DestroyOnLoad )
        {
#if AUDIO_TOOLKIT_DEMO
            audioObjInstance = (GameObject) GameObject.Instantiate( audioPrefab, worldPosition, Quaternion.identity );

#else
            if ( UsePooledAudioObjects )
            {
                audioObjInstance = (GameObject)ObjectPoolController.Instantiate( audioPrefab, worldPosition, Quaternion.identity );
            }
            else
            {
                audioObjInstance = (GameObject)ObjectPoolController.InstantiateWithoutPool( audioPrefab, worldPosition, Quaternion.identity );
            }
#endif
        }
        else
        {   // pooling does not work for DontDestroyOnLoad objects
#if AUDIO_TOOLKIT_DEMO
            audioObjInstance = (GameObject) GameObject.Instantiate( audioPrefab, worldPosition, Quaternion.identity );
#else
            audioObjInstance = (GameObject)ObjectPoolController.InstantiateWithoutPool( audioPrefab, worldPosition, Quaternion.identity );
#endif
            DontDestroyOnLoad( audioObjInstance );
        }
        

        if ( parentObj )
        {
            audioObjInstance.transform.parent = parentObj;
        }

        AudioObject sndObj = audioObjInstance.gameObject.GetComponent<AudioObject>();

        sndObj._stopClipAtTime = subItem.ClipStopTime;
        sndObj._startClipAtTime = subItem.ClipStartTime;
        sndObj.audio.clip = subItem.Clip;
        sndObj.audio.pitch = AudioObject.TransformPitch( subItem.PitchShift );
        sndObj.audio.pan = subItem.Pan2D;

        if ( subItem.RandomStartPosition )
        {
            startTime = UnityEngine.Random.Range( 0, sndObj.clipLength );
        }

        sndObj.audio.time = startTime + subItem.ClipStartTime;

        sndObj.audio.loop = audioItem.Loop;
        
        sndObj._volumeExcludingCategory = volumeWithoutCategory;
        sndObj.category = audioCategory;
        sndObj.subItem = subItem;

        sndObj._ApplyVolume();

        if ( subItem.RandomPitch != 0 )
        {
            sndObj.audio.pitch *= AudioObject.TransformPitch( UnityEngine.Random.Range( -subItem.RandomPitch, subItem.RandomPitch ) );
        }

        if ( subItem.RandomDelay > 0 )
        {
            delay += UnityEngine.Random.Range( 0, subItem.RandomDelay );
        }

        audioObjInstance.name = "AudioObject:" + sndObj.audio.clip.name;

        sndObj.Play( delay + subItem.Delay + audioItem.Delay );

        if ( subItem.FadeIn > 0 )
        {
            sndObj.FadeIn( subItem.FadeIn );
        }

#if UNITY_EDITOR && !AUDIO_TOOLKIT_DEMO
        var logData = new AudioLog.LogData_PlayClip();
        logData.audioID = audioItem.Name;
        logData.category = audioCategory.Name;
        logData.clipName = subItem.Clip.name;
        logData.delay = delay;
        logData.parentObject = parentObj != null ? parentObj.name : "";
        logData.position = worldPosition;
        logData.startTime = startTime;
        logData.volume = volumeWithCategory;

        AudioLog.Log_PlayClip( logData );
#endif

        return sndObj;
    }

    internal void _NotifyPlaylistTrackCompleteleyPlayed( AudioObject audioObject )
    {
        audioObject._isCurrentPlaylistTrack = false;
        if ( IsPlaylistPlaying() )
        {
            if ( _currentMusic == audioObject )
            {
                if ( _PlayNextMusicOnPlaylist( delayBetweenPlaylistTracks ) == null )
                {
                    _isPlaylistPlaying = false;
                }
            }
        }
    }

    private void _ValidateAudioObjectPrefab( GameObject audioPrefab )
    {
        if ( UsePooledAudioObjects )
        {
#if AUDIO_TOOLKIT_DEMO
        Debug.LogWarning( "Poolable Audio objects not supported by the Audio Toolkit Demo version" );
#else
            if ( audioPrefab.GetComponent<PoolableObject>() == null )
            {
                Debug.LogWarning( "AudioObject prefab does not have the PoolableObject component. Pooling will not work." );
            }
#endif
        }

        if ( audioPrefab.GetComponent<AudioObject>() == null )
        {
            Debug.LogError( "AudioObject prefab must have the AudioObject script component!" );
        }
    }

    // is public because custom inspector must access it
    public AudioController_CurrentInspectorSelection _currentInspectorSelection = new AudioController_CurrentInspectorSelection();

    public AudioController()
    {
        AudioController.SetSingletonType( typeof( AudioController ) );
    }
}

[Serializable]
public class AudioController_CurrentInspectorSelection
{
    public int currentCategoryIndex = 0;
    public int currentItemIndex = 0;
    public int currentSubitemIndex = 0;
    public int currentPlaylistIndex = 0;
}


