using System;
using System.Threading.Tasks;
using Godot;

public class Asset<T> where T : Resource
{
    public string ResourcePath { get; protected set; }

    protected T Object;
    public bool IsLoaded => Object != null;

    protected Asset(string path)
    {
        ResourcePath = path;

        if (OS.IsDebugBuild() && !ResourceLoader.Exists(path))
        {
            GD.Print($"*** ERROR: Broken AssetRef: {path} ***");
        }
    }
    public static implicit operator Asset<T>(string path) => new Asset<T>(path);

    public T Load()
    {
        if (Object == null)
        {
            Object = GD.Load<T>(ResourcePath);
        }
        
        return Object;
    }

    public async Task<T> LoadAsync()
    {
        if (Object == null)
        {
            if (ResourceLoader.HasCached(ResourcePath))
            {
                return Load();
            }
            
            await Task.Run(() => 
            {
                lock(typeof(Asset<T>))
                {
                    // Don't use the cache for threading reasons!
                    Object = GD.Load<T>(ResourcePath);
                }
            });
        }        
        
        return Object;
    }

    public void Unload()
    {
        Object = null;
    }

    public void Reload()
    {
        Unload();
        Load();
    }
}

public class Scene<T> : Asset<PackedScene> where T : Node
{
    private Scene(string scenePath) : base(scenePath) 
    {

    }
    public static implicit operator Scene<T>(string path) => new Scene<T>(path);
    
    public T Instance()
    {
        if (Object == null) 
        {
            Load();
        }
         
        // Loaded and held - just instance
        return (T)Object.Instance();        
    } 
     
    public async Task<T> InstanceAsync()
    {
        if (Object == null) 
        {
            await LoadAsync();
        }
         
        // Loaded and held - just instance
        return (T)Object.Instance();        
    } 
}