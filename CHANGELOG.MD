# FNF Overlay — Değişiklik Günlüğü

Bu günlük, **v0.0.1a** temel sürümünden sonra yapılan ve mevcut **v0.0.7** derlemesine ulaşan değişiklikleri özetler.

## v0.0.7

### Eklendi
- **Ayarlar paneli:** Oyun tuşlarını ve temayı uygulama içinden değiştirme; ayarları `%AppData%\FNFOverlay\settings.ini` dosyasına kaydetme.
- **Altı ayrı tuş ayarı:** Sol, Aşağı, Taunt, Yukarı, Sağ ve Dodge. Dodge varsayılan olarak `Space` tuşuna atanır.
- **Alt + S kısayolu:** Ayarlar panelini açıp kapatır.
- **Tema seçenekleri:** BFAli, BFLec, Siyah-Beyaz, Sarı-Kırmızı, Açık Mavi-Kırmızı ve Corruption.
- **Ortak tema paleti:** Overlay, ayarlar, yükleme, bilgi, yapımcı ve veda panellerinde seçilen temanın renkleri kullanılır.
- **Tema animasyonları:** Yeni `AnimationController` ile tüm temalara hareketli ışık bantları, desenler ve parçacık efektleri eklendi. Corruption temasında ayrıca siyah-kırmızı glitch efektleri bulunur.
- **Uzun nota (sustain) göstergesi:** Bir tuş basılı tutulduğunda nota gövdesi uzar. Kısa basışta normal nota gösterilir; uzun nota eşiği sabit 8 oyun karesidir.
- **Desteklenen oyun listesi:** Friday Night Funkin', Psych Engine, Prism Engine, Further Engine, Codename Engine, Kade Engine ve diğer oyun adları `config\supported_games.json` dosyasından okunur.
- **Tek EXE desteği:** Desteklenen oyun listesi ve uygulama ikonu derleme sırasında EXE içine gömülür. Yanında JSON dosyası bulunmasa da gömülü liste kullanılabilir.
- **Bilgi panelinde** kısayollar ve yapılandırılmış oyun tuşlarının gösterilmesi.

### İyileştirildi
- Borderless tasarımlı ayarlar paneli, tema kartları, tuş yakalama kontrolleri ve kapatma düğmesi eklendi.
- Loading, bilgi, yapımcı ve veda panellerinin görsel tasarımı ve tema uyumu geliştirildi.
- Tuş göstergelerine basılı tutma sırasında tekrarlı nota başları üretmek yerine sustain animasyonu eklendi.
- Ayarlar ve tema tercihleri kalıcı hale getirildi; önceki beş tuşlu ayar dosyaları yeni Dodge tuşuna uyumlu şekilde taşınır.
- Tam ekranda katmanlı overlay çizimi için alfa biçimi düzeltildi. Overlay aynı konumdayken gereksiz pencere taşıma çağrıları azaltıldı.
- Ayarlar paneli etkin ekranın kullanılabilir boyutuna göre sığar; küçük ekranlarda içerik kaydırılabilir. Ana FNF overlay'i orijinal 334×320 boyutunda kalır.
- `B.bat` ve `D.bat` derleme listeleri yeni kaynak dosyaları ve gömülü kaynaklarla güncellendi.

### Düzeltildi
- Ayarlar panelindeki tuş adlarının ve satır içeriklerinin görünmemesine neden olan kontrol yerleşimi sorunu giderildi.
- Ayarlar kısayolu ve bilgi metinleri `Alt + S` ile tutarlı hale getirildi.
- Orta yön göstergesi kaldırılarak Taunt tuşu olarak düzenlendi.
- Kısa tuş basışlarının uzun nota olarak görünmesi engellendi.
- Ayarlar paneli kapandığında animasyon zamanlayıcısının durması sağlandı.
