using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class LocalPersistencyLayer : ILocalPersistencyLayer
{
    private string fileName;
    public LocalPersistencyLayer(string fileName)
    {
        this.fileName = fileName;

#if DEBUG_INITIALIZATION
        Debug.Log(fileName);  
#endif
    }

    public void Load<T>(Action<T, bool> onLoad, T _default) where T : class
    {
        Task.Run(() =>
        {
#if DEBUG_INITIALIZATION
            Debug.Log($"Started loading {fileName}");
#endif
            if (!File.Exists(fileName))
            {
#if DEBUG_INITIALIZATION
                Debug.Log($"File not exists{fileName}");
#endif
                onLoad?.Invoke(_default, false);
                Thread.Sleep(0);
                return;
            }
            
            TextReader reader = null;
            try
            {
                reader = new StreamReader(fileName);
#if DEBUG_INITIALIZATION
                Debug.Log($"Started reading file {fileName}");
#endif
                var fileContents = reader.ReadToEnd();
                var unmarshalled = JsonConvert.DeserializeObject<T>(fileContents);
                onLoad?.Invoke(unmarshalled, true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                onLoad?.Invoke(null, false);
            }
            finally
            {
#if DEBUG_INITIALIZATION
                Debug.Log($"Finished loading{fileName}");
#endif
                if (reader != null)
                {
                    reader.Dispose();
                    reader.Close();
                }
                Thread.Sleep(0);
            }
        });
    }
    

    public void Save<T>(T blob, Action<bool> saved)
    {
        try
        {
            var copy = blob.Copy();
            Task.Run(() =>
            {
                var json = JsonConvert.SerializeObject(copy);
                File.WriteAllText(fileName, json);
                saved?.Invoke(true); 
                Thread.Sleep(0);
            });
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            saved?.Invoke(false);
            throw;
        }
    }
    
}