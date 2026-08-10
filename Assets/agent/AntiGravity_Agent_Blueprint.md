# VERTIGO GAMES CASE - PROJE ANA PLANI VE ANTI-GRAVITY AJAN YÖNERGELERİ

## 1. AJAN KİMLİĞİ VE AMACI
*   **Ajan:** Anti-Gravity (Google AI Ajan Programı)
*   **Görev:** Unity ortamında Artist/Animator'a (Kullanıcıya) 2D Animasyon, VFX, Shader Graph ve UI (Canvas) entegrasyonlarında adım adım rehberlik etmek ve otomasyon/scripting desteği sağlamak.
*   **Ana Hedef:** Vertigo Games "Game Animator & VFX Artist" case çalışmasının iki ana görevini eksiksiz, 60 FPS akıcılığında ve referanslara birebir uygun şekilde 7 gün içinde tamamlamak.

## 2. PROJE ÖZETİ VE GÖREV DAĞILIMI
### Görev 1: Polygun Arena - 2D Animation & Splash Screen
*   **İstenen:** Karakterin 2D riglenmesi, idle animasyonu ve elindeki uzay gemisi için Hologram efekti.
### Görev 2: Critical Strike - Lucky Draw Açılış Efekti
*   **İstenen:** Dissolve (erime) ve Fire (ateş) shader'ları ile Particle System kullanılarak UI üzerinde bir açılış animasyonu.

## 3. ANTI-GRAVITY AJANININ UNITY İÇİNDE YAPABİLECEKLERİ (ADIM ADIM)

### Adım 1: Proje Kurulumu ve Canvas Entegrasyonu
*   **Ajanın Yapacağı İşlem:** Sahnedeki ana Canvas'ı bulur, Render Mode'unu `Screen Space - Camera` olarak ayarlar ve ana kamerayı atar.
*   **UI Hiyerarşisi:** Görsel derinliği sağlamak için Sorting Layer mantığını kurar (`Background`, `Character`, `VFX_Behind`, `VFX_Front`). Canvas Scaler ayarlarını referans çözünürlüğe (örn. 1920x1080) sabitler.

### Adım 2: 2D Rigging ve Animasyon (Görev 1) - [KULLANICI TARAFINDAN YAPILACAKTIR]
*   **Sorumluluk:** Karakterin 2D Riglenmesi (Bone Setup & Weighting) ve Idle animasyonunun yapılması tamamen **Kullanıcı** tarafından gerçekleştirilecektir. Ajan bu adımı doğrudan yapmayacaktır.
*   **Ajan Desteği:** Yalnızca kullanıcı soru sorduğunda veya teknik bir engel yaşadığında rehberlik/rehber döküman sağlayacaktır.
*   **Animator Controller:** İhtiyaç dahilinde Animator Controller geçişleri ve parameter ayarları konusunda yardımcı olunacaktır.

### Adım 3: Shader Graph Oluşturma (Görev 1 ve 2)
Ajan, Shader Graph üzerinden şu yapıları inşa etmekle yükümlüdür:
*   **Hologram Shader (Polygun Arena):** 
    *   *Girdi:* Base Texture.
    *   *İşlem:* `Time` nodunu bir `Sine` noduna bağlayarak dikey kayan bir tarama çizgisi (Scanline) oluşturur. 
    *   *Detay:* Kenar parlaklığı için `Fresnel Effect` ekler ve Alpha kanalını şeffaflık için ayarlar.
*   **Dissolve & Fire Shader (Critical Strike):**
    *   *Girdi:* Balkabağı veya açılış objesi Texture.
    *   *İşlem:* `Simple Noise` (veya `Voronoi`) nodunu `Step` nodu ile birleştirir. `Step`'in Edge değerini bir `Float` parametresine bağlayarak animasyondan kontrol edilebilir hale getirir.
    *   *Ateş Etkisi:* Çözünme sınırlarına HDR turuncu/sarı renkler (`Emission`) atayarak ateşli bir erime efekti sağlar.

### Adım 4: Particle System (Shuriken) Entegrasyonu
*   **Ajanın Yapacağı İşlem:** Efektlerin UI içinde görünmesi için partiküllerin materyalini kontrol eder ve uygun Sorting Layer'a atar.
*   **Lucky Draw Kıvılcımları:** Dissolve shader çalışırken eşzamanlı olarak çevreye yayılan, `Color over Lifetime` ile kırmızıdan sarıya geçip sönen partikül sistemlerini ayarlar. `Shape` modülünü kaynak objenin formuna uydurur.

### Adım 5: Optimizasyon ve Teslimat Hazırlığı
*   **Performans Testi:** Profiler'ı çalıştırarak Particle System ve Shader'ların draw call ve performans metriklerini inceler, hedef 60 FPS'i korur.
*   **Render & Çıktı:** Unity Recorder ayarlarını yapılandırır. Her iki görev için `.mov` formatında, 60 FPS ekran kayıtlarının sorunsuz alınmasını sağlar.
*   **GitHub Reposu:** Proje dosyalarının (gereksiz Library/Temp klasörleri olmadan) public bir GitHub reposuna push edilmeden önceki `.gitignore` kontrolünü yapar.

## 4. AJAN ÇALIŞMA PRENSİBİ VE DURUM KONTROLÜ
*   Ajan sürekli olarak bu dosyayı okur.
*   Kullanıcıdan gelen her "Şu adıma geçelim" veya "Şu efekti test et" komutunda, bu listedeki ilgili adıma giderek gereken parametreleri sunar.
*   Tıkandığı noktada alternatif bir Shader nodu veya Particle parametresi önerir.
