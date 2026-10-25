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

Pozlar:
- **Uyuyan:** başı ön patilerinde (`sleep_*`)
- **Kıvrılmış:** simit gibi, burnu kuyruğunda (`curl_*`)
- **Uyanık:** dört ayak üstünde, havlıyor (`awake_*`)

Bahçede her kare, konumuna göre sabit bir kürk seçer. Kazanınca uyanmayan köpekler kıvrılmış uyurken görünür; kaybedince hepsi uyanıp havlar.

## 6. Bileşenler

- **Kart** (`UiFactory.Card`): sert gölge + renkli yüz + mürekkep kontur. İçerik `Body`'ye eklenir.
- **Düğme:** Kart + yazı/ikon. Basılınca yüz gölgenin üstüne çöker (`ButtonFeedback`).
- **Üst bar:** Solda yuvarlak geri düğmesi, ortada başlık ve altında hardal fırça darbesi.
- **Diyalog:** Krem kart; tepesinde bir köpek çizimi oturur (kazanınca kıvrılmış, kaybedince havlayan).
- **İpucu balonu:** Altta lacivert kart, krem yazı.
- **Harita:** Noktalı mürekkep patika. Bölüm daireleri renk koduyla ayrılır: bitti = nane + tik, sıradaki = mercan + yanında uyuyan köpek, kilitli = kâğıt koyu + kilit. Hedef yazısı kâğıt etiketin üstündedir.

## 7. Hareket ilkeleri (sonraki oturumda genişletilecek)

- Her şey "canlı ama sakin" olmalı. Köpekler uyurken yalnızca nefes alır (y ekseninde %2–3 şişme), ara sıra "z" çıkar.
- Dokunuşlar hemen karşılık bulur: düğme çöker, kare basılınca hafifçe iner, açılan kareler dalga halinde "pop" yapar.
- Kötü olaylar kısa ve net: sarsıntı, "Hav!", kalp sallanıp solar.
- Planlanan: kuyruk sallama, kulak kıpırdatma, uyanınca zıplayıp yer değiştirme. Çizimler bu yüzden ileride parçalara (gövde, kafa, kuyruk) ayrılacak.

## 8. Çizimleri üretmek

Bütün oyun görselleri kodla tarif edilir ve tek komutla üretilir:

```
pip install cairosvg pillow numpy
python3 tools/art/build.py
```

- `tools/art/sketch.py`: çizim kütüphanesi (yumuşak eğri, fırça çizgisi, kırpılmış doku)
- `tools/art/style.py`: palet ve kürkler
- `tools/art/dogs.py`: köpek pozları
- `tools/art/props.py`: kareler, eşyalar, ikonlar, arayüz parçaları, köpek yatağı
- Çıktı: `Assets/Resources/Art/**.png` (+ `.meta` içe aktarma ayarları), `Assets/Resources/Fonts/`

PNG'ler elle düzenlenmez; değişiklik betikte yapılır ve yeniden üretilir. GUID'ler dosya yolundan türetildiği için yeniden üretmek Unity referanslarını bozmaz.
