namespace UyuyanKopekler.Core
{
    // Kalıcı kayıt. Şimdilik PlayerPrefs; ileride bulut kaydı aynı arayüzle takılabilir.
    public interface ISaveStore
    {
        // Kayıt yoksa ya da bozuksa yeni (varsayılan) bir nesne döner.
        T Load<T>(string key) where T : class, new();
        void Save<T>(string key, T data) where T : class;
    }
}
