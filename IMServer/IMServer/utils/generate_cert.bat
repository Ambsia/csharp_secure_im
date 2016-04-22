:: If you installed OpenSSL in non-default directory, you MUST change paths in commands.

@echo off
set OPENSSL_CONF=C:\OpenSSL-Win32\bin\openssl.cfg

"C:\OpenSSL-Win32\bin\openssl" req -x509 -nodes -days 365 -newkey rsa:1024 -keyout private.key -out cert.crt

"C:\OpenSSL-Win32\bin\openssl" pkcs12 -export -in cert.crt -inkey private.key -out ftpserver.pfx -passout pass:Sgf45g@[;gr35

del .rnd
del private.key
del cert.crt