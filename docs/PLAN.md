# Uyuyan Köpekler — Mimari ve Yapım Planı

Bu belge, `CLAUDE.md`'deki onaylı tasarımı koda dökmek için izlenen planı anlatır.
Her bölüm editörde test edilebilir küçük adımlar halinde uygulanır.

---

## 1. Yapılanlar (Adım 0, 1 ve 2)

### Adım 0 — Temizlik ve hata düzeltmeleri
| Sorun | Çözüm |
|---|---|
| Sahnede `Board` nesnesi kayıtlı değildi | Sahneye tek bir `Game` nesnesi (`GameBootstrap`) eklendi; geri kalan her şey koddan kuruluyor |
| Ekran yönü otomatik dönüyordu | Player ayarı **Portrait** yapıldı |
| Kazanınca sayaç sıfırlanmıyordu | Kazanınca kalan köpekler modelde işaretleniyor, sayaç 0'a iniyor |
| `dogCount` çok büyük girilirse oyun bitmiyordu | `BoardConfig` değerleri güvenli aralığa kısıyor |
| "Üç köpek" yazısı sabitti | Mesaj gerçek can sayısını kullanıyor |
| İpucu mesajları birbirini eziyordu | İpuçları tek yerden (`RoundController`) yönetiliyor |
| Telefon döndürülünce kamera bozuluyordu | `BoardCamera` ekran/safe-area değişince yeniden sığdırıyor |
| Mobilde 30 FPS | `Application.targetFrameRate = 60` |

### Adım 1 — Açılış ekranı, iki modun iskeleti ve tasarımlar
- Açılış ekranı: logo (nefes alan uyuyan köpek + Zzz), **Klasik** ve **Macera** düğmeleri, **Koleksiyon** ve **Ayarlar**.
- Klasik mod: sonsuz bahçeler, puan, seri (can kaybetmeden bitirince büyür), en yüksek skor kaydı.
- Macera modu: 12 bölümlük harita, her bölümün hedefi (bahçeyi temizle / hiç can kaybetme / N hamlede bitir), kilit açma ve kayıt.
- Oyun ekranı: üst bar (mola, başlık, mod bilgisi, kalpler, kalan köpek), alt ipucu balonu.
- Diyalog (tek bileşen): mola, sonuç, bölüm girişi, "ilerlemeyi sıfırla" onayı.
- Ayarlar (titreşim, ses, ilerlemeyi sıfırla) ve Koleksiyon (iskelet) ekranları.
- **Tasarımlar:** ikinci turda baştan yapıldı; ayrıntılar `docs/TASARIM.md`. Maketler: `docs/tasarim/ekranlar.png`, `docs/tasarim/kopekler.png`.
- **Animasyonlar:** açılan kareler dalga halinde "pop" yapıyor, köpek uyanınca zıplıyor + tahta
  sarsılıyor, mama kabı pop, düğmeler basınca küçülüyor, ekranlar yumuşak geçiyor,
  menüdeki köpek nefes alıyor.

### Adım 2 — Kare animasyonları, pati izleri, eşyalar, kedi, sesler
- **Köpekler kukla oldu** (`tools/art/rig.py`): 6 ırk biçim olarak da farklı (bacak, kulak, kuyruk, tüy).
  Her kürk için 19 karelik sayfa: yandan duruş/havlama/kuyruk/koşu (4 kare), kıvrılmış uyku (nefes,
  kulak kıpırdatma, uyanma), önden oturma (kuyruk sallama, göz kırpma, sevinç, şaşkınlık). Kedi 11 kare.
- **Animasyon altyapısı:** `Anim/SpriteAnimator` (Image ya da SpriteRenderer üstünde kare oynatır),
  `Anim/Clips` (hangi kareler, hangi hızda), `Anim/ClipLibrary` (sprite dizilerini önbellekler).
- **Pati izleri:** açık karede rakam yerine o kadar pati izi; Ayarlar'dan rakama dönülebilir.
- **Bahçede:** köpekler iki yöne bakar; uyanınca kalkıp havlar ve kuyruk sallar; kaybedince hepsi
  toz bulutu bırakarak dört bir yana kaçar; kazanınca uyuyanlar kare kare nefes alır.
- **Çimin altındaki eşyalar** (Core): kemik, yıldız, inci, altın tasma. Toplanınca kareden sayaca uçar.
  Koleksiyon ekranı gerçek sayıları gösterir.
- **Kedi yardımcısı:** alt bardaki düğme → kare seç → kedi gelir, oturur, miyavlar; 3×3 alandaki
  köpekler kalkıp havlar, üstlerine mama kabı konur; kedinin oturduğu kare güvenliyse kazılır.
- **Sesler:** `Audio/SoundService` (`ISoundPlayer`): kazı, büyük kazı, mama kabı, toplama, nadir eşya,
  can kaybı, kazanma, kaybetme, kaçış, düğme, anahtar, pencere, kedi, havlama, miyav.
- Açılış ekranında üç köpek kare kare uyuyor, yatağın yanında bir köpek kuyruk sallıyor; harita
  rehberi ve pencerelerdeki köpekler de animasyonlu.

---

## 2. Mimari

```
Assets/Scripts/
  Core/        → saf C# (Unity'ye bağımlı DEĞİL, birim testli)   [UyuyanKopekler.Core.asmdef]
    Board/       Cell, BoardConfig, CellState, BoardModel, RevealResult, RevealedCell,
                 ItemType, FoundItem, ItemScatter, CatResult
    Helpers/     CatHelper (kedinin kemik bedeli)
    Session/     GameSession, IRoundState, MoveResult, MoveKind, RoundOutcome, RoundStatus, LossReason
    Goals/       IRoundGoal, GoalStatus, FinishGardenGoal, NoHeartLossGoal, MoveLimitGoal
    Modes/       IGameMode, RoundSetup, RoundReport, RoundAction, ClassicMode, ClassicDifficulty,
                 AdventureMode, LevelDefinition, LevelCatalog, LevelInfo, LevelState
    Save/        ISaveStore, PlayerProgress, GameSettings, SaveKeys, Inventory
  Game/        → Unity tarafı                                     [UyuyanKopekler.Game.asmdef]
    App/         GameBootstrap (sahnedeki tek bileşen), AppController, PlayerPrefsSaveStore,
                 Haptics, BackButton
    Board/       BoardView, TileView, BoardInput, BoardCamera, RoundController
    Art/         IGameArt, IllustratedArt, Palette, DogFrame, CatFrame
    Anim/        SpriteAnimator, Clips, ClipLibrary
    Audio/       ISoundPlayer, SoundService, Sfx
    (görseller tools/art/ betikleriyle üretilir → Assets/Resources/Art, yazı tipi → Assets/Resources/Fonts,
     sesler tools/audio/build.py → Assets/Resources/Audio)
    UI/          UiRoot, UiFactory, Card, UiScreen, SafeAreaFitter, ButtonFeedback, PillToggle,
                 Tween, DialogPanel, DialogContent, HintToast
    UI/Screens/  MainMenuScreen, AdventureMapScreen, GameScreen, SettingsScreen, CollectionScreen
Assets/Tests/EditMode/  → Core için NUnit testleri                [UyuyanKopekler.Tests.asmdef]
```

### SOLID nasıl uygulanıyor
- **S (Tek sorumluluk):** `BoardModel` yalnızca ızgara kurallarını bilir; canları ve hedefleri
  `GameSession` yönetir; puan/seri `ClassicMode`'dadır; çizim `BoardView`, dokunma `BoardInput`,
  kamera `BoardCamera`, ekran geçişleri `AppController`, bir turun akışı `RoundController`'dadır.
- **O (Açık/kapalı):** Yeni hedef = `IRoundGoal` uygulayan yeni sınıf ("5 yıldız topla" gibi).
  Yeni mod = `IGameMode` uygulayan yeni sınıf. Mevcut kod değişmez.
- **L (Liskov):** Bütün hedefler ve modlar aynı sözleşmeyle (`Evaluate`, `CreateRound`,
  `CompleteRound`) birbirinin yerine geçebilir; `RoundController` hangisi olduğunu bilmez.
- **I (Arayüz ayrımı):** Hedefler `GameSession`'ın tamamını değil, yalnızca okuma amaçlı küçük
  `IRoundState`'i görür. Kayıt için tek küçük `ISaveStore` var.
- **D (Bağımlılığın tersine çevrilmesi):** Modlar kaydın nasıl yapıldığını bilmez (`ISaveStore`);
  Core, Unity'yi hiç tanımaz (asmdef `noEngineReferences`). Tüm nesneler `GameBootstrap`'ta
  kurulup birbirine verilir (composition root).

### Bir dokunuşun yolculuğu
```
BoardInput (Pointer) ──CellTapped──▶ RoundController ──Dig──▶ GameSession ──Reveal/Chord──▶ BoardModel
                                          │                         │ (can, hamle, hedef)
                                          │◀──────── MoveResult ────┘
                                          ├─▶ BoardView.PlayReveal (dalga animasyonu)
                                          ├─▶ GameScreen (kalpler, kalan köpek, puan, ipucu)
                                          └─▶ bitince IGameMode.CompleteRound ─▶ RoundReport ─▶ DialogPanel
```

### Oyun kuralları (karar verilenler)
- **Klasik:** Her bahçede 3 can. Bahçe bitince sonraki bahçe gelir ve biraz büyür/zorlaşır.
  Açılan her kare +10 puan; bahçe bonusu `100 + 25 × (bahçe-1)`, seri çarpanıyla (×seri) çarpılır.
  Can kaybetmeden biten her bahçe seriyi 1 artırır, can kaybedilirse seri 0'lanır.
  Bir bahçede tüm canlar giderse koşu biter; puan en yüksek skorla karşılaştırılıp kaydedilir.
- **Macera:** Bölümün hedefi tamamlanınca kazanılır; hedef imkânsızlaşınca (hamle bitti, can gitti)
  ya da canlar bitince kaybedilir. Bölüm bitince bir sonraki açılır.
- **Hamle:** Bir kareyi kazmak ya da rakama dokunup çevreyi açmak 1 hamledir; mama kabı hamle sayılmaz.
- **Eşyalar:** İlk kazıda köpekler yerleşince güvenli karelerin altına rastgele saklanır:
  güvenli karelerin ~%8'i kadar kemik (en az 1), %60 olasılıkla 1 yıldız, %12 inci, %3 altın tasma.
  Kare açılınca eşya toplanır ve hemen kaydedilir (bahçe yarıda bırakılsa da kalır).
  Yeni oyuncu 30 kemikle başlar.
- **Kedi:** 10 kemik. Yalnızca çimli kareye ve ilk kazıdan sonra gönderilebilir. O kare ve 8 komşusundaki
  köpeklerin üstüne mama kabı konur (can gitmez); kare güvenliyse kazılır. 1 hamle sayılır.
  Seçimden vazgeçmek için düğmeye yeniden bas ya da bir kareye basılı tut.

---

## 3. Sonraki adımlar (yeni mimaride nereye oturacakları)

| Adım | Ne eklenecek | Nereye |
|---|---|---|
| ~~2~~ | ~~Eşyalar, kedi, sesler, kare animasyonları~~ | Bitti (yukarıda) |
| 3 | Diğer yardımcılar | Burun (güvenli kare bulur), düdük (3×3 alanı 2 sn gösterir): `CatHelper` gibi Core'da bedel + `BoardModel`'de kural; `BoardView`'da sahne |
| 3 | Kombo | `ComboTracker` (Core): açılan alan büyüklüğüne göre çarpan; `RevealedCell.Depth` zaten dalga için hazır |
| 4 | Macera hedefleri | `CollectStarsGoal`, `FindGoldenBoneGoal` (`IRoundGoal`); `LevelCatalog` büyür |
| 5 | Günlük görevler | `IQuest` + `QuestTracker`, `RoundOutcome`'dan beslenir |
| 6 | Mod temaları | Park/Çiftlik için `tools/art`'a yeni palet, zemin ve ırklar (`rig.BREEDS`) |
| 7 | Kayıt/Reklam/Analitik | `ISaveStore` bulut kaydı; `IAdService` (geçiş reklamı kuralları); `IAnalytics` |

---

## 4. Test
- **Birim testleri:** Unity'de `Window → General → Test Runner → EditMode → Run All`.
  Izgara kuralları, can/hedef mantığı, Klasik puan/seri ve Macera kilitleri test ediliyor.
- **Elle test:** Her adımdan sonra `CLAUDE.md`'deki "Play'e bas ve dene" listesi.
