using System;
using UnityEngine;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Game
{
    // Kayıtları JSON olarak PlayerPrefs'e yazar.
    public sealed class PlayerPrefsSaveStore : ISaveStore
    {
        public T Load<T>(string key) where T : class, new()
        {
            string json = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json)) return new T();
            try
            {
                // Önce varsayılanlarla kur, sonra kayıttakileri üstüne yaz: eski kayıtlarda
                // olmayan yeni alanlar (ör. kemik, pati izi ayarı) varsayılan değerini korur.
                var data = new T();
                JsonUtility.FromJsonOverwrite(json, data);
                return data;
            }
            catch (ArgumentException e)
            {
                Debug.LogWarning($"Kayıt okunamadı ({key}), sıfırdan başlanıyor: {e.Message}");
                return new T();
            }
        }

        public void Save<T>(string key, T data) where T : class
        {
            PlayerPrefs.SetString(key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
