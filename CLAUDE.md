# Uyuyan Köpekler

Mayın tarlası mantığında, köpek temalı bir mobil bulmaca oyunu. Çimin altında köpekler uyuyor; oyuncu kareleri kazıyor, rakamlar çevredeki uyuyan köpek sayısını söylüyor. Hedef: Block Blast'ın Tetris'e yaptığını mayın tarlasına yapmak (modlar, toplanabilirler, yardımcılar, kombo, görevler).

Kullanıcı Türkçe konuşuyor. Açıklamaları Türkçe, kısa ve adım adım yap. Kullanıcı Unity'de yeni; editörde yapması gereken bir şey varsa (nesne ekleme, Inspector ayarı) tam olarak nereye tıklayacağını yaz.

## Teknik

- Unity 6.3 LTS (6000.3.27f1), şablon: Universal 2D (URP 2D)
- Hedef: önce Android (Google Play), sonra iOS. Geliştirme Mac (Apple Silicon) üzerinde, test telefonu iPhone.
- Ekran: dikey, 1080×1920 referans.
- Girdi: yeni Input System (`UnityEngine.InputSystem`, `Pointer.current`). Eski `Input.GetMouseButton` kullanma.
- Metin: arayüzde uGUI `Text`, bahçedeki rakamlarda `TextMesh`; yazı tipi **Shantell Sans** (`Assets/Resources/Fonts`, OFL). Arial/LegacyRuntime kullanma.
- Görseller: **tasarım rehberi `docs/TASARIM.md`** (sınırlı palet, mürekkep fırça çizgisi, kâğıt zemin, sert gölge). Bütün çizimler `tools/art/` altındaki Python betikleriyle tarif edilip `python3 tools/art/build.py` ile `Assets/Resources/Art`'a üretiliyor. PNG'leri elle düzenleme; betiği değiştirip yeniden üret. Görünümler yalnızca `IGameArt` arayüzünü tanıyor (`IllustratedArt` onu uyguluyor).
- Kodlar `Assets/Scripts/` altında. Sınıf adı = dosya adı (bir dosyada tek üst düzey tip).
- Ayrıntılı mimari ve plan: `docs/PLAN.md`.

### Mimari (kısa)

- `Assets/Scripts/Core/` → saf C#, Unity'yi tanımaz (`UyuyanKopekler.Core.asmdef`, `noEngineReferences`). Kurallar burada: `BoardModel` (ızgara), `GameSession` (can/hamle/hedef), `IRoundGoal` hedefleri, `IGameMode` modları (`ClassicMode`, `AdventureMode`, `LevelCatalog`), kayıt modelleri (`PlayerProgress`, `GameSettings`, `ISaveStore`).
- `Assets/Scripts/Game/` → Unity tarafı (`UyuyanKopekler.Game.asmdef`):
  - `App/`: `GameBootstrap` (sahnedeki tek bileşen, her şeyi kurar), `AppController` (ekran akışı), `PlayerPrefsSaveStore`, `Haptics`, `BackButton`.
  - `Board/`: `BoardView` + `TileView` (çizim ve animasyon), `BoardInput` (dokunma), `BoardCamera` (sığdırma), `RoundController` (bir turun akışı).
  - `Art/`: `Palette` (renkler), `IGameArt` (görsel sözleşmesi), `IllustratedArt` (Resources'tan yükler).
  - `UI/`: `UiRoot`, `UiFactory` (kart = sert gölge + yüz + kontur), `Card`, `UiScreen`, `DialogPanel`, `HintToast`, `PillToggle`, `ButtonFeedback`... ve `UI/Screens/` altında ekranlar.
- `tools/art/` → çizim hattı: `sketch.py` (fırça/eğri/doku), `style.py` (palet, kürkler), `dogs.py` (köpek pozları), `props.py` (kareler, eşyalar, ikonlar, arayüz, köpek yatağı), `build.py` (üretim + .meta).
- `Assets/Tests/EditMode/` → Core için NUnit testleri (Unity: Window → General → Test Runner → EditMode → Run All).
- Sahne: `SampleScene` içinde yalnızca `Main Camera`, `Global Light 2D` ve `Game` (`GameBootstrap`) var. Arayüz ve bahçe koddan kuruluyor.

## Şu anki durum

Adım 1 bitti (açılış ekranı + iki modun iskeleti) ve görsel tasarım baştan yapıldı (`docs/TASARIM.md`):
- Açılış: el yazısı başlık, köpek yatağında uyuyan üç köpek (nefes alıyorlar, "z" çıkıyor), Klasik / Macera düğmeleri (en iyi skor ve bölüm bilgisiyle), Koleksiyon ve Ayarlar.
- Köpek kadrosu: 6 kürk × 3 poz (uyuyan, kıvrılmış, uyanık-havlayan). Bahçede her kare sabit bir kürk seçer; kazanınca uyanmayanlar kıvrılmış uyurken, kaybedince hepsi havlarken görünür.
- Oynanış (her iki modda): ilk dokunuş güvenli; dokun = kaz (dalga animasyonuyla yayılır), basılı tut (0.35 sn) ya da sağ tık = mama kabı, açık rakama dokun = çevreyi aç; köpek uyanınca "Hav!", sarsıntı, titreşim ve kalp gider; "mama kabı boş karede" uyarısı.
- Üst bar: mola düğmesi, başlık (Bahçe N / Bölüm N), mod satırı (puan + seri ya da hedef ilerlemesi), kalpler, kalan köpek. Altta ipucu balonu.
- Klasik: her bahçede 3 can, bahçeler büyür (`ClassicDifficulty`), kare başına 10 puan + bahçe bonusu × seri; can kaybedince seri sıfırlanır; koşu bitince en yüksek skor kaydedilir.
- Macera: 12 bölümlük harita (kıvrımlı patika, bitenler yeşil, sıradaki turuncu ve nabız gibi atıyor, kilitliler gri); hedefler: bahçeyi temizle / hiç can kaybetmeden bitir / N hamlede bitir; bölüm girişi penceresi; ilerleme kaydediliyor.
- Pencereler: mola, sonuç, bölüm girişi, sıfırlama onayı (tek `DialogPanel`). Android geri tuşu (bilgisayarda Esc) çalışıyor.
- Ayarlar: titreşim, ses (henüz ses yok), ilerlemeyi sıfırla. Koleksiyon: kemik/yıldız/inci/altın tasma kartları (sayılar 2. adımda bağlanacak).
- Kayıt: `PlayerPrefs` içinde JSON (`progress.v1`, `settings.v1`).
- Player ayarı: ekran yönü Portrait.

## Onaylanan tasarım

**Modlar**
- Klasik: sonsuz bahçeler, puan, can kaybetmeden bitirince büyüyen seri, en yüksek skor.
- Macera: harita üzerinde bölümler, her bölümün hedefi var (ör. "5 yıldız topla", "Altın kemiği bul", "Hiç can kaybetmeden bitir", "20 hamlede bitir"). Bölümler giderek büyür, yeni öğeler açılır.

**Çimin altından çıkanlar**
- Kemik: para birimi (yardımcı almak için)
- Yıldız: Macera hedefleri için
- İnci / altın tasma: nadir, koleksiyon için

**Yardımcılar**
- Kedi: kapalı bir kareye konur. Altında köpek varsa köpek uyanır, kediyi kovalar, ikisi animasyonla ekrandan çıkar; köpek bahçeden kaldırılır, can gitmez, komşu rakamlar 1 azalır. Köpek yoksa kedi kareyi güvenle açar.
- Burun: köpek koklayıp güvenli bir kare bulur.
- Düdük: 3×3 alanda köpekler 2 saniyeliğine görünür olur.

**Kombo:** Tek dokunuşla büyük alan açılınca dalga efekti ve puan çarpanı ("Süper kazı! ×3"). Rakama dokunup çevreyi açmak da seri başlatır.

**Görevler:** Günlük 3 görev ("3 bahçe bitir", "Kediyle 2 köpek kovala", "15 kemik topla"), ödülü kemik.

**Canlılık:** Köpekler uyurken nefes alır, Zzz çıkar, kulak kıpırdatır; uyanınca zıplayıp havlar. Açılış ekranında logo, iki mod düğmesi, koleksiyon, ayarlar.

**Mod temaları (ileride):** Bahçe (çit, çiçek, ev köpekleri), Park (kaldırım taşı, bank, sokak köpekleri), Çiftlik (saman, çamur, Kangal / Akbaş / Kars çoban köpeği).

## Yapım sırası

1. ~~Açılış ekranı ve iki modun iskeleti~~ (bitti)
2. Animasyonlar (köpekler: kuyruk, kulak, uyanınca zıplama) ve "yakında" denen şeylerin yapılması (koleksiyon sayıları, ses)  ← kullanıcının istediği sıradaki adım
3. Kemik, yıldız toplama ve kedi yardımcısı (kovalama animasyonu dahil)
4. Kombo sistemi
5. Macera bölümleri ve hedefleri
6. Günlük görevler
7. Sesler, ek çizimler (mod temaları)
8. Ortak altyapı: kayıt, AdMob (ödüllü + geçiş reklamı), Firebase/GameAnalytics olayları, "Reklamları kaldır" satın alımı

## Kurallar

- Sorumlulukları ayrı tut: kurallar Core'da (Unity'siz ve testli), çizim/girdi/arayüz Game'de. Tek seferde dev bir yeniden yazım yapma; her adım editörde test edilebilir kalsın.
- Yeni hedef = `IRoundGoal` uygulayan yeni sınıf; yeni mod = `IGameMode`; yeni görsel = `tools/art`'ta çizim + `IGameArt`'a özellik + `IllustratedArt`'ta yükleme. Mevcut sınıfları büyütmek yerine genişlet.
- Core'a eklenen her kural için `Assets/Tests/EditMode/` altına test yaz.
- Arayüzü ve sahne nesnelerini koddan kur (`UiFactory`, `GameBootstrap`); kullanıcıdan editörde elle nesne kurmasını isteme, gerekmedikçe.
- Tasarımda `docs/TASARIM.md`'ye uy: paletin dışına çıkma, gradyan/parlak efekt kullanma, her kart ve düğme `UiFactory.Card` ile (sert gölge + kontur). Yeni çizimi önce `tools/art`'ta üret, önizle, sonra koda bağla.
- Eski `UnityEngine.Input` sınıfını hiç kullanma (proje yalnızca yeni Input System'e ayarlı; çalışırken hata verir).
- Her adımdan sonra kullanıcıya Play'e basıp neyi denemesi gerektiğini söyle.
- Reklam kuralları (ileride): geçiş reklamı en fazla 2–3 bölümde bir, bölüm ortasında asla, ilk oturumda hiç.
