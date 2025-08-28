using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;

[DefaultExecutionOrder(-50)]
public class GameManager : Singleton<GameManager>
{
    [Tooltip("Should the game state start as Playing? (For debugging and testing)")]
    [SerializeField] private bool gameStateStartsAsPlaying;
    [Tooltip("Volume profiles for different quality presets, ordered from Low to Ultra")]
    [SerializeField] private VolumeProfile[] volumeProfiles;
    
    public event Action<GameStates> OnGameStateChanged;
    public event Action<Language> LanguageChanged;
    public GameStates GameState { get; private set; }
    public Language CurrentLanguage { get; private set; }
    public int TargetFrameRate { get; private set; }
    public QualityPreset QualityPreset { get; private set; }
    public float MouseSensitivity { get; private set; }
    private Volume _globalVolume;
    private Coroutine _getSharedValuesCoroutine;
    
    protected override void OnAwake()
    {
        if (gameStateStartsAsPlaying) SetGameState(GameStates.Playing);
        InvokeRepeating(nameof(LazyUpdate), 1f, 1f);
        _globalVolume = FindFirstObjectByType<Volume>();
        _getSharedValuesCoroutine ??= StartCoroutine(GetSharedValues());
        EDebug.Log(StringUtils.AddColorToString("GameManager Awake", CColors.Good));
        EDebug.Log(StringUtils.AddColorToString($"(GameManager) " + Localization.Translate("log.lang"), CColors.Log));
    }

    private void LazyUpdate()
    {
        
    }

    private IEnumerator GetSharedValues()
    {
        // while (Wait until the PlayerPrefs are loaded or some other value is read) yield return null;
        // Fixed values for now
        if (MouseSensitivity <= 0) yield return null;
        MouseSensitivity = 5;
        _getSharedValuesCoroutine = null;
    }

    public void SetLanguage(Language language)
    {
        if (CurrentLanguage == language) return;
        CurrentLanguage = language;
        Localization.LoadLanguage(language);
        EDebug.Log(StringUtils.AddColorToString(
            Localization.Translate("log.lang"), CColors.Log));
        LanguageChanged?.Invoke(CurrentLanguage);
    }
    [ContextMenu("SetLanguageToEnglish")] public void SetLanguageToEnglish() => SetLanguage(Language.En);
    [ContextMenu("SetLanguageToSpanish")] public void SetLanguageToSpanish() => SetLanguage(Language.Es);
    
    public void SetGameState(GameStates state)
    {
        if (GameState == state) return;
        GameState = state;
        OnGameStateChanged?.Invoke(GameState);
    }
    [ContextMenu("SetGameStateToLoading")] public void SetGameStateToGameOver() => SetGameState(GameStates.Loading);
    [ContextMenu("SetGameStateToPlaying")] public void SetGameStateToPlaying() => SetGameState(GameStates.Playing);
    [ContextMenu("SetGameStateToPaused")] public void SetGameStateToPaused() => SetGameState(GameStates.Paused);
    
    
}
