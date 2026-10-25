# Uyuyan Köpekler — Tasarım Rehberi

Görsel yön: **"pazar sabahı eskiz defteri"**. Sınırlı renk paleti, gevşek mürekkep çizgisi, kâğıt zemin.
Parlak gradyanlar, cam efektleri ve "parlak mobil oyun" görünümü kullanılmaz.

Maketler: `docs/tasarim/ekranlar.png`, köpek kadrosu: `docs/tasarim/kopekler.png`.

## 1. Esinlenilen kaynaklar

- **Kullanıcının referansı:** düz renkli, tam boy, farklı ırklarda köpekler. Palet hardal, mercan, lacivert,
  pembe, nane ve krem. Çizgi gevşek ve siyah; bacaklar bazen yalnızca çizgiyle çizilmiş; gövdede ilmek ve çizgi dokuları var.
- **Two Dots:** sade ve düz arayüz, ekran üç sıraya bölünür (üstte hedef/bilgi, ortada oyun, altta boşluk).
  "Kuyrukta beklerken oynanır: hızlı, temiz, eğlenceli olmalı."
- **Elle çizilmiş arayüz akımı:** kusurlu konturlar, kâğıt dokusu, el yazısı etiketler, "insan eli" hissi.
- **Baskı (risograf) görünümü:** dolgu ile çizgi birbirine tam oturmaz, biraz kayık durur. Bu, çizimleri "basılmış" gibi gösterir.

## 2. Palet

| Ad | Renk | Kullanım |
|---|---|---|
| Kâğıt | `#F8F1E4` | Bütün zeminler (dokulu) |
| Kâğıt koyu | `#EDE2CC` | Kilitli/pasif şeyler, kapalı anahtar |
| Mürekkep | `#211D1A` | Bütün çizgiler, sert gölgeler, yazılar |
| Mürekkep soluk | `#6B5F55` | Açıklama yazıları |
| Mercan | `#EE6A4C` | Birincil eylem (Klasik, Başla, Yeniden), kalpler, yatak |
| Hardal | `#DDB65A` | İkincil eylem (Macera), vurgu fırça darbesi |
| Nane | `#A7D8CE` | Olumlu (Devam, bitmiş bölüm, açık anahtar) |
| Lacivert | `#2E3C5A` | İpucu balonu, koyu köpek |
| Pembe | `#F3C3CD` | Yumuşak vurgu |
| Krem | `#F7E3C3` | Kartlar |
| Çim | `#A9C98F` / `#B4D49C` | Bahçe kareleri (dama tahtası) |
| Toprak | `#EEDDB8` | Kazılmış kare |

Kural: Bir ekranda en fazla iki ana renk öne çıkar. Yeni renk eklemeden önce palete bak.
Renkler `tools/art/style.py` ile `Assets/Scripts/Game/Art/Palette.cs` arasında aynı tutulur.

## 3. Çizgi ve şekil

- Konturlar **fırça çizgisi**dir: uçları incelir, kalınlığı hafif dalgalanır.
- Kontur, dolgunun 4 px sağ-üstüne kayık düşer ve her yeri kapatmaz (açık uçlar serbest).
- Kutu ve daireler hafif titrek çizilir. Köşeler yuvarlak, ama kusursuz değil.
- **Sert gölge:** Her kart ve düğmenin altında, yumuşak gölge yerine 8–14 px aşağı kaymış düz mürekkep gölgesi vardır.
- Doku: gövdelerde ilmek (e-e-e), kısa tüy çizgileri, şeritler; açık bir ton, kırpılmış.

## 4. Yazı

- **Shantell Sans** (OFL lisanslı; Türkçe karakterlerin hepsi var): ExtraBold başlıklar ve rakamlar için, Bold düğmeler ve açıklamalar için.
- Başlık boyları 66–150, düğme yazısı 46–74, açıklama 30–38 (1080×1920 referansında).
- Yazı rengi neredeyse her zaman mürekkep. Lacivert zemin üstünde krem kullanılır.

## 5. Köpek kadrosu

Altı kürk var. Her kürkün kulağı sabittir, böylece aynı renk köpek her pozda aynı köpek gibi görünür:

| Kürk | Kulak | Not |
|---|---|---|
| Hardal | Sarkık | İlmek dokulu |
| Lacivert | Sarkık, mercan rengi | Referanstaki lacivert köpek |
| Mercan | Dik | Şeritli |
| Pembe | Sarkık | Beyaz ilmekli |
| Nane | Dik | Pembe tüylü |
| Krem | Dik | Hardal lekeli |

Her köpek bir **kukla** (`tools/art/rig.py`): gövde, kafa, kulak, kuyruk ve dört bacak ayrı ayrı çizilir, poz da eklem açılarıyla tarif edilir. Irklar yalnızca renk değil, biçim olarak da ayrışır (bacak boyu, kulak tipi, kuyruk tipi, tüylülük). Her kürk için tek bir **kare sayfası** üretilir (`Dogs/dog_<kürk>.png`, 320 px kareler, 5 sütun):

| Grup | Kareler | Nerede |
|---|---|---|
| Yandan | `Stand`, `Alert`, `Bark`, `Wag0–1`, `Run0–3` | Bahçede uyanan köpek, kediye havlama, kaybedince kaçış |
| Kıvrılmış | `Sleep0–1` (nefes), `SleepTwitch` (kulak kıpırdar), `Wake` (göz açık) | Kazanınca uyuyanlar, açılış ekranı, mola penceresi |
| Önden | `Sit0–2` (kuyruk sallar), `Blink`, `Happy`, `Gasp` | Harita rehberi, bölüm girişi, kazanma ve onay pencereleri |

Kedi (siyah, mercan tasmalı) aynı yöntemle çizilir: `Walk0–3`, `Startle`, `Run0–3`, `Sit0–1` (`Dogs/cat.png`, 4 sütun).

Yan görünüm kareleri sağa bakar; sola bakmak için aynalanır. Bahçede her kare, konumuna göre sabit bir kürk ve yön seçer, böylece köpekler iki yöne de bakar.

## 5.1 İpucu: pati izleri

Açık karelerde rakam yerine **o kadar pati izi** çizilir (`Board/hint_1..8`): zar yüzü gibi dizilir, her sayının kendi rengi vardır (1 lacivert-mavi, 2 yeşil, 3 mercan, 4 mor...). Ayarlardan kapatılırsa eski rakamlar gelir.

## 6. Bileşenler

- **Kart** (`UiFactory.Card`): sert gölge + renkli yüz + mürekkep kontur. İçerik `Body`'ye eklenir.
- **Düğme:** Kart + yazı/ikon. Basılınca yüz gölgenin üstüne çöker (`ButtonFeedback`).
- **Üst bar:** Solda yuvarlak geri düğmesi, ortada başlık ve altında hardal fırça darbesi.
- **Diyalog:** Krem kart; tepesinde oynayan bir köpek oturur (kazanınca sevinçli, kaybedince havlayan, molada uyuyan, bölüm girişinde kuyruk sallayan, sıfırlamada şaşkın).
- **İpucu balonu:** Alt barın üstünde lacivert kart, krem yazı.
- **Alt bar (oyun):** Solda nane yeşili **kedi düğmesi** (kedi yüzü + "Kedi" + kemik fiyatı; seçimdeyken mercan olur, nabız gibi atar, "Kare seç" yazar), sağda krem **kemik sayacı**.
- **Harita:** Noktalı mürekkep patika. Bölüm daireleri renk koduyla ayrılır: bitti = nane + tik, sıradaki = mercan + yanında kuyruk sallayan köpek, kilitli = kâğıt koyu + kilit. Hedef yazısı kâğıt etiketin üstündedir.

## 7. Hareket ilkeleri

- Her şey "canlı ama sakin" olmalı. Uyuyan köpekler kare kare nefes alır, arada kulak kıpırdatır, "z" çıkar. Hareketler gerçek kare animasyonudur (`SpriteAnimator` + `Clips`), çizimi esnetmek yerine.
- Dokunuşlar hemen karşılık bulur: düğme çöker (ve "tık" sesi), kare basılınca hafifçe iner, açılan kareler dalga halinde "pop" yapar.
- Kötü olaylar kısa ve net: köpek gözünü açar, kalkar, havlar (`Wake` klibi), sarsıntı, "Hav!", kalp sallanıp solar.
- **Kaybedince:** bütün köpekler toz bulutu bırakarak bahçenin ortasından dört bir yana koşarak kaçar.
- **Kedi sahnesi:** kedi kenardan yürüyerek gelir, kareye oturup miyavlar; çevredeki köpekler kalkıp ona havlar, kedi irkilip koşarak kaçar, köpeklerin üstüne mama kabı konur.
- **Eşyalar:** kareden fırlayıp kavis çizerek kemik sayacına uçar; nadir eşyalar ekranın ortasında büyüyüp kaybolur.

## 7.1 Sesler

Kısa, yumuşak, "kâğıt ve ahşap" hissi veren sesler (`Assets/Resources/Audio`). Arayüz sesleri CC0 lisanslı uisfx paketinden seçilir; havlama ve miyav `tools/audio/build.py` içinde sentezlenir. Aynı ses art arda çalınca perdesi hafifçe değişir. Ayarlardan kapatılabilir.

## 8. Çizimleri üretmek

Bütün oyun görselleri kodla tarif edilir ve tek komutla üretilir:

```
pip install cairosvg pillow numpy
python3 tools/art/build.py
```

- `tools/art/sketch.py`: çizim kütüphanesi (yumuşak eğri, fırça çizgisi, kırpılmış doku)
- `tools/art/style.py`: palet ve kürkler
- `tools/art/rig.py`: köpek ve kedi kuklaları, pozlar, kare sayfaları, pati izleri, toz bulutu
- `tools/art/props.py`: kareler, eşyalar, ikonlar, arayüz parçaları, köpek yatağı
- Çıktı: `Assets/Resources/Art/**.png` (+ `.meta` içe aktarma ayarları), `Assets/Resources/Fonts/`

Sesler: `python3 tools/audio/build.py --uisfx <uisfx paketinin klasörü>` (paket: `npm pack uisfx@0.4.0`) (`Assets/Resources/Audio`).

PNG'ler elle düzenlenmez; değişiklik betikte yapılır ve yeniden üretilir. GUID'ler dosya yolundan türetildiği için yeniden üretmek Unity referanslarını bozmaz.
