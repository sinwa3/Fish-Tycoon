using System.Collections.Generic;
using UnityEngine;

/// <summary> KDH : Resources 에셋의 로드, 캐싱 및 인스턴스 생성을 관리한다. </summary>
public class ResourceManager
{
    private readonly Dictionary<System.Type, Dictionary<string, Object>> _loadedObjectMap = new();
    /// <summary> KDH : Resources.LoadAll로 로드한 스프라이트 시트 캐싱 </summary>
    private readonly Dictionary<string, Sprite[]> _loadedSpriteMap = new();

    #region ===== 로드 =====

    /// <summary> KDH : 지정한 Resources 경로의 에셋을 로드하고 캐시에 저장한다. </summary>
    public T Load<T>(string path) where T : Object
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            CPrint.Error("[ResourceManager] Resource path가 비어 있습니다.");
            return null;
        }

        Dictionary<string, Object> cache = GetCache<T>();

        if (cache.TryGetValue(path, out Object cachedObject))
        {
            return cachedObject as T;
        }

        T loadedObject = Resources.Load<T>(path);

        if (loadedObject == null)
        {
            CPrint.Error($"[ResourceManager] Resource를 로드하지 못했습니다. Path: {path}, Type: {typeof(T).Name}");
            return null;
        }

        cache[path] = loadedObject;

        return loadedObject;
    }
    
    /// <summary> KDH : 지정한 타입의 Resources 캐시를 반환하고 없으면 생성한다. </summary>
    private Dictionary<string, Object> GetCache<T>() where T : Object
    {
        System.Type objectType = typeof(T);

        if (_loadedObjectMap.TryGetValue(objectType, out Dictionary<string, Object> cache))
        {
            return cache;
        }

        cache = new Dictionary<string, Object>();
        _loadedObjectMap.Add(objectType, cache);

        return cache;
    }

    /// <summary> KDH : 지정한 Resources 경로의 에셋을 캐싱하지 않고 로드한다. </summary>
    public T LoadWithoutCache<T>(string path) where T : Object
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            CPrint.Error("[ResourceManager] Resource path가 비어 있습니다.");
            return null;
        }

        T loadedObject = Resources.Load<T>(path);

        if (loadedObject == null)
        {
            CPrint.Error($"[ResourceManager] Resource를 로드하지 못했습니다. Path: {path}, Type: {typeof(T).Name}");
            return null;
        }

        return loadedObject;
    }
    
    /// <summary> KDH : 지정한 Resources 경로의 스프라이트 시트를 로드하고 캐싱한다. </summary>
    private Sprite[] LoadAllSprites(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            CPrint.Error("[ResourceManager] SpriteSheet path가 비어 있습니다.");
            return null;
        }

        if (_loadedSpriteMap.TryGetValue(path, out Sprite[] cachedSprites))
        {
            return cachedSprites;
        }

        Sprite[] loadedSprites = Resources.LoadAll<Sprite>(path);

        if (loadedSprites == null || loadedSprites.Length == 0)
        {
            CPrint.Error($"[ResourceManager] SpriteSheet를 로드하지 못했습니다. Path: {path}");
            return null;
        }

        _loadedSpriteMap.Add(path, loadedSprites);

        return loadedSprites;
    }

    /// <summary> KDH : 지정한 Resources 경로의 스프라이트 시트에서 이름에 해당하는 스프라이트를 반환한다. </summary>
    public Sprite LoadSprite(string path, string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName))
        {
            CPrint.Error("[ResourceManager] Sprite name이 비어 있습니다.");
            return null;
        }

        Sprite[] sprites = LoadAllSprites(path);

        if (sprites == null)
        {
            return null;
        }

        foreach (Sprite sprite in sprites)
        {
            if (sprite.name == spriteName)
            {
                return sprite;
            }
        }

        CPrint.Error($"[ResourceManager] Sprite를 찾지 못했습니다. Path: {path}, Name: {spriteName}");

        return null;
    }
    
    #endregion ===== 로드 =====

    #region ===== 생성 =====

    /// <summary> KDH : 지정한 프리팹을 인스턴스화한다. </summary>
    public GameObject Spawn(GameObject prefab, Transform parent = null)
    {
        if (prefab == null)
        {
            CPrint.Error("[ResourceManager] Spawn prefab이 null입니다.");
            return null;
        }

        GameObject instance = Object.Instantiate(prefab, parent);
        instance.name = prefab.name;

        return instance;
    }

    /// <summary> KDH : Resources 경로의 프리팹을 로드하고 생성한다. </summary>
    public GameObject Spawn(string path, Transform parent = null)
    {
        GameObject prefab = Load<GameObject>(path);

        return prefab != null ? Spawn(prefab, parent) : null;
    }

    /// <summary> KDH : Resources 경로의 프리팹을 로드하고 지정한 위치와 회전으로 생성한다. </summary>
    public GameObject Spawn(string path, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject instance = Spawn(path, parent);

        if (instance == null)
        {
            return null;
        }

        instance.transform.SetPositionAndRotation(position, rotation);

        return instance;
    }

    #endregion ===== 생성 =====

    #region ===== 정리 =====

    /// <summary> KDH : Resources 캐시를 초기화하고 더 이상 참조되지 않는 에셋 정리를 요청한다. </summary>
    public void Clear()
    {
        _loadedObjectMap.Clear();
        _loadedSpriteMap.Clear();

        Resources.UnloadUnusedAssets();
    }

    #endregion ===== 정리 =====
}