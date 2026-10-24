# Uyuyan Köpekler — Mimari ve Yapım Planı

Bu belge, `CLAUDE.md`'deki onaylı tasarımı koda dökmek için izlenen planı anlatır.
Her bölüm editörde test edilebilir küçük adımlar halinde uygulanır.

---

## 1. Bu turda yapılanlar (Adım 0 + Adım 1)

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
- **Tasarımlar:** tüm görseller kodla çiziliyor (`ArtPainter`, `IGameArt` arayüzünün arkasında). Hazır çizim yok; 6. adımda gerçek
  çizimler aynı yerden takılacak. Kareler yuvarlatılmış, çim dokulu; mama kabı, uyuyan/uyanık
  köpek yüzü, kalp, kemik, yıldız, inci, tasma, pati, dişli, kilit gibi ikonlar var.
- Tasarım maketleri: `docs/tasarim/ekranlar.png` ve `docs/tasarim/ikonlar.png` (Unity dışında, aynı ölçülerle çizilmiş yaklaşık görüntü).
- **Animasyonlar:** açılan kareler dalga halinde "pop" yapıyor, köpek uyanınca zıplıyor + tahta
  sarsılıyor, mama kabı pop, düğmeler basınca küçülüyor, ekranlar yumuşak geçiyor,
  menüdeki köpek nefes alıyor.

---

## 2. Mimari

```
Assets/Scripts/
  Core/        → saf C# (Unity'ye bağımlı DEĞİL, birim testli)   [UyuyanKopekler.Core.asmdef]
    Board/       Cell, BoardConfig, CellState, BoardModel, RevealResult, RevealedCell
    Session/     GameSession, IRoundState, MoveResult, MoveKind, RoundOutcome, RoundStatus, LossReason
    Goals/       IRoundGoal, GoalStatus, FinishGardenGoal, NoHeartLossGoal, MoveLimitGoal
    Modes/       IGameMode, RoundSetup, RoundReport, RoundAction, ClassicMode, ClassicDifficulty,
                 AdventureMode, LevelDefinition, LevelCatalog, LevelInfo, LevelState
    Save/        ISaveStore, PlayerProgress, GameSettings, SaveKeys
  Game/        → Unity tarafı                                     [UyuyanKopekler.Game.asmdef]
    App/         GameBootstrap (sahnedeki tek bileşen), AppController, PlayerPrefsSaveStore,
                 Haptics, BackButton
    Board/       BoardView, TileView, BoardInput, BoardCamera, RoundController
    Art/         IGameArt, ProceduralArt, ArtPainter, PixelCanvas, Sdf, Palette
    UI/          UiRoot, UiFactory, UiScreen, SafeAreaFitter, ButtonFeedback, PillToggle,
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

---

## 3. Sonraki adımlar (yeni mimaride nereye oturacakları)

| Adım | Ne eklenecek | Nereye |
|---|---|---|
| 2 | Kemik, yıldız toplama | `BoardModel`'e "çimin altındaki eşya" katmanı (`ItemType`), `RevealResult`'a toplananlar; `PlayerProgress`'e kemik sayısı |
| 2 | Kedi yardımcısı | `IHelper` arayüzü + `CatHelper`; `BoardModel.RemoveDog(cell)` (komşu rakamlar 1 azalır); `BoardView`'da kovalama animasyonu |
| 3 | Kombo | `ComboTracker` (Core): açılan alan büyüklüğüne göre çarpan; `RevealedCell.Depth` zaten dalga için hazır |
| 4 | Macera hedefleri | `CollectStarsGoal`, `FindGoldenBoneGoal` (`IRoundGoal`); `LevelCatalog` büyür |
| 5 | Günlük görevler | `IQuest` + `QuestTracker`, `RoundOutcome`'dan beslenir |
| 6 | Gerçek çizimler, ses | `IGameArt`'ın içe aktarılan çizimlerle yeni bir uygulaması (`ProceduralArt` yerine); `IAudio` servisi |
| 7 | Kayıt/Reklam/Analitik | `ISaveStore` bulut kaydı; `IAdService` (geçiş reklamı kuralları); `IAnalytics` |

---

## 4. Test
- **Birim testleri:** Unity'de `Window → General → Test Runner → EditMode → Run All`.
  Izgara kuralları, can/hedef mantığı, Klasik puan/seri ve Macera kilitleri test ediliyor.
- **Elle test:** Her adımdan sonra `CLAUDE.md`'deki "Play'e bas ve dene" listesi.
