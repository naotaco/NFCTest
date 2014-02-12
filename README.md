This is a test app to read NFC data on WP devices.

the parser codes are already merged to [WPPMM](https://github.com/kazyx/WPPMM).  
Please find the latest parser, "SonyNdefUtils" project in WPPMM solution.

### Description
Some of Sony's cameras equips NFC to help Wi-Fi connection.  
In the record, the package name of Sony's app, SSID, and WPA key are stored.  
(Of cource, the package name is not so important for us, WP users.)

Unfortunately, it seems that SSID and key are stored in a NDEF record with Sony's original structure.  
So a library in this app make parsing it easy.

### Dependency
This application depens on no other libraries.

