# Uyuyan Köpekler

Mayın tarlası mantığında, köpek temalı bir mobil bulmaca oyunu. Çimin altında köpekler uyuyor; oyuncu kareleri kazıyor, rakamlar çevredeki uyuyan köpek sayısını söylüyor. Hedef: Block Blast'ın Tetris'e yaptığını mayın tarlasına yapmak (modlar, toplanabilirler, yardımcılar, kombo, görevler).

Kullanıcı Türkçe konuşuyor. Açıklamaları Türkçe, kısa ve adım adım yap. Kullanıcı Unity'de yeni; editörde yapması gereken bir şey varsa (nesne ekleme, Inspector ayarı) tam olarak nereye tıklayacağını yaz.

## Teknik

- Unity 6.3 LTS (6000.3.27f1), şablon: Universal 2D (URP 2D)
- Hedef: önce Android (Google Play), sonra iOS. Geliştirme Mac (Apple Silicon) üzerinde, test telefonu iPhone.
- Ekran: dikey, 1080×1920 referans.
- Girdi: yeni Input System (`UnityEngine.InputSystem`, `Pointer.current`). Eski `Input.GetMouseButton` kullanma.
- Metin: şimdilik `TextMesh` + `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`. Arayüz büyüyünce TextMeshPro / uGUI'ye geçilebilir.
- Görseller: henüz çizim yok. Kareler kod içinde üretilen 1×1 beyaz sprite ile renklendiriliyor. Mekanikler oturmadan çizime geçme.
- Kodlar `Assets/Scripts/` altında. Sınıf adı = dosya adı.

## Şu anki durum

`Assets/Scripts/Board.cs` (sahnede `Board` adlı boş nesneye ekli) çalışıyor:
- Izgara (varsayılan 8×10, 12 köpek), ilk dokunuş güvenli
- Dokun: kaz, boş alan yayılarak açılır, rakamlar renkli
- Basılı tut (0.35 sn) ya da sağ tık: mama kabı (işaret)
- Açık rakama dokun: işaret sayısı tutuyorsa komşuları açar
- 3 can; köpeğe denk gelince can gider, 0'da kaybedilir
- Üstte sayaç (kalan köpek, can), bitişte mesaj paneli, dokununca yeni oyun
- "Bir mama kabı boş karede" uyarısı (kazılacak kare kalmadı ama oyun bitmedi)

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

1. Açılış ekranı ve iki modun iskeleti  ← sıradaki adım
2. Kemik, yıldız toplama ve kedi yardımcısı (kovalama animasyonu dahil)
3. Kombo sistemi
4. Macera bölümleri ve hedefleri
5. Günlük görevler
6. Gerçek çizimler, animasyonlar, sesler
7. Ortak altyapı: kayıt, AdMob (ödüllü + geçiş reklamı), Firebase/GameAnalytics olayları, "Reklamları kaldır" satın alımı

## Kurallar

- Board.cs büyüdükçe sorumlulukları ayır (ör. BoardLogic, BoardView, GameManager, Ui) ama tek seferde dev bir yeniden yazım yapma; her adım editörde test edilebilir kalsın.
- Her adımdan sonra kullanıcıya Play'e basıp neyi denemesi gerektiğini söyle.
- Reklam kuralları (ileride): geçiş reklamı en fazla 2–3 bölümde bir, bölüm ortasında asla, ilk oturumda hiç.
