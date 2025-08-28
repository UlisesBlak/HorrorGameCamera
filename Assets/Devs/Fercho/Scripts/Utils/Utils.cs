using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Utils
{
    public enum Language // NOTE: This enum is used for localization...
    {                   // JSON files are named after the enum values and MUST be lowercase (like Minecraft frfr)
        En = 0,
        Es = 1,
    }
    
    public enum SoundType
    {
        Master,
        Bgm,
        Sfx
    }
    
    public enum WindowMode
    {
        Fullscreen = 0,
        Windowed = 1,
        Borderless = 2,
        Maximized = 3,
    }

    public enum QualityPreset
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }
    
    public enum GameStates
    {
        Paused,
        Loading,
        Playing
    }
    
    public static class StringUtils
    {
        public static string AddSizeTagToString(string input, int size) {
            string strSize = size.ToString();
            return $"<size={strSize}> {input} </size>";
        }

        public static string AddColorToString(string input, Color color) {
            string colorStr = ColorUtility.ToHtmlStringRGBA(color);
            return $"<color=#{colorStr}> {input} </color>";
        }
    }
    
    public static class Localization
    {
        private static readonly Dictionary<string, string> Translations = new Dictionary<string, string>();
        private static Language _lang = Language.En;
        private static GameManager _gm = MiscUtils.GetOrCreateGameManager();

        public static void LoadLanguage(Language language)
        { // This method is public. However, it's already called by "Translate" so it shouldn't be necessary... 
            try
            {
                string langFileName = $"Assets/Resources/Lang/{language.ToString().ToLower()}.json";
                TextAsset langFile = Resources.Load<TextAsset>($"Lang/{language.ToString().ToLower()}");
                if (!langFile) {
                    EDebug.LogError($"Couldn't find the language file: {langFileName}");
                    return;
                }
                JsonObject json = JsonObject.Create(langFile.text);
                if (json == null || json.type != JsonObject.Type.Object || json.count == 0) {
                    EDebug.LogError($"This lang file is empty or has invalid translations: {langFileName}");
                    return;
                }
                Translations.Clear();
                for (int i = 0; i < json.keys.Count; i++)
                {
                    string key = json.keys[i];
                    string value = json.list[i].stringValue;
                    Translations[key] = value;
                }
                _lang = language;
                EDebug.LogGood($"(Utils) Language loaded correctly: {language}");
            }
            catch (Exception ex)
            {
                EDebug.LogError($"Error loading lang file: {language}. Details: {ex.Message}");
            }
        }

        public static string Translate(string key)
        {
            if (!Application.isPlaying || Singleton<GameManager>.applicationIsQuitting) {
                EDebug.LogError($"Translation attempted while GameManager is unavailable. Key: {key}");
                return key; // Fallback
            }
            if (!_gm) {
                _gm = Singleton<GameManager>.TryGetInstance();
                if (!_gm) {
                    EDebug.LogError("GameManager is null! Cannot translate.");
                    return key; // Fallback to avoid Null result
                }
            }
            if (Translations.Count == 0 || _lang != _gm.CurrentLanguage)
                LoadLanguage(_gm.CurrentLanguage);
            if (Translations.TryGetValue(key, out string value))
                return value;
            EDebug.LogError($"Translation not found for key: {key}");
            return key; // (Fallback&Knuckles)
        }

        [Serializable] private class SerializableDictionary
        {
            public List<string> keys;
            public List<string> values;

            public Dictionary<string, string> ToDictionary()
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                for (int i = 0; i < keys.Count; i++)
                {
                    dictionary[keys[i]] = values[i];
                }
                return dictionary;
            }
        }
    }

    public static class CColors
    { 
        public static readonly Color Log = new Color(0,0.5f,0.75f,1);  
        public static readonly Color Good = new Color(0.2f,0.75f,0.2f,1);
        public static readonly Color Warning = new Color(0.8f,0.5f,0,1);
        public static readonly Color Error = new Color(0.8f,0.1f,0.1f,1);
    }
    
    public static class MiscUtils
    {
        public static GameManager GetOrCreateGameManager() {
            GameManager gm = GameManager.Instance;
            if (gm) return gm;
            if (!Application.isPlaying || Singleton<GameManager>.applicationIsQuitting) {
                EDebug.LogError("Attempted to create GameManager while the application is not playing or is quitting.");
                return null;
            }
            GameObject newGm = new GameObject("GameManager") { transform = { position = new Vector3(0, 10, 0) } };
            gm = newGm.AddComponent<GameManager>();
            Object.DontDestroyOnLoad(newGm);
            EDebug.Log("GameManager was not found so a new one was created.");
            return gm;
        }
        
        public static void UpdateBarArray(Image[] bars, float percent) {
            int count = bars.Length;
            for (int i = 0; i < count; i++) {
                float value = Mathf.Clamp01(percent * count - i);
                bars[i].fillAmount = value;
            }
        }
        
        public static void ActionToDo([CanBeNull] Animator anim, [CanBeNull] string animTriggerName, 
            [CanBeNull] GameObject objToDoStuff, int actionType, [CanBeNull] ParticleSystem particle/*,
            [CanBeNull] PlayPersistent sound*/)
        {
            if (anim && !string.IsNullOrWhiteSpace(animTriggerName))
                anim.SetTrigger(animTriggerName);
            if (objToDoStuff) {
                switch (actionType) {
                    default: objToDoStuff.SetActive(false);
                        break;
                    case 1: UnityEngine.Object.Destroy(objToDoStuff);
                        break;
                    case 2: objToDoStuff.SetActive(true);
                        break;
                }
            }
            if (particle) {
                Object.Instantiate(particle);
                particle.Play();
            }
            //if (sound != null) sound.enabled = true;
        }
    }
    
    public static class MathUtils
    {
        public static Vector3[] CanonBasis(Transform trans)
        {
            Vector3 camForward = trans.forward;
            Vector3 camRight = trans.right;
            camForward.y = 0;
            camRight.y = 0;
            return new[] { camForward.normalized, camRight.normalized };
        }
        
        public static int SecondsToFrames(float seconds)
        {
            int frameRate = 60;
            if (GameManager.Instance && GameManager.Instance.TargetFrameRate > 0)
                frameRate = GameManager.Instance.TargetFrameRate;
            return Mathf.RoundToInt(seconds * Time.timeScale * frameRate);
        }
        public static Vector3 RandomPosInSphere(float radius) {
            return Random.insideUnitSphere * radius;
        }
        public static Vector3 RandomInRangedSphere(float minRad, float maxRad) {
            var randVal = RandFloat(minRad, maxRad);
            return Random.onUnitSphere * randVal;
        }
        public static Vector3 RandomInRangedSphere(Vector2 minMaxRad) {
            var randVal = RandFloat(minMaxRad);
            return Random.onUnitSphere * randVal;
        }
        public static float RandFromArray(int[] array) {
            if (array.Length == 0) return 0;
            int index = Random.Range(0, array.Length);
            return array[index];
        }
        public static bool RandBool() { return Random.value < 0.5f; }
        public static bool WeightedRandBool(float weight) { return Random.value <= weight; }
        public static float RandFloat(float min, float max)
        { return (min > max)? Random.Range(max, min) : Random.Range(min, max); }
        public static float RandFloat(Vector2 vec)
        { return (vec.x > vec.y)? Random.Range(vec.y, vec.x) : Random.Range(vec.x, vec.y); }
        public static int RandInt(int min, int max) 
        { return (min > max)? Random.Range(max, min) : Random.Range(min, max); }
        public static int RandInt(Vector2 vec)
        { return (vec.x > vec.y)? Random.Range((int)vec.y, (int)vec.x) : Random.Range((int)vec.x, (int)vec.y); }
    }
    
    
    // Lazy way to avoid dictionaries, indulge me a little
    [Serializable] public class MeshRendererArray { public MeshRenderer[] renderers; }
    [Serializable] public class ColliderArray { public Collider[] colliders; }
    [Serializable] public class MaterialArray { public Material[] mats; }
    
}
