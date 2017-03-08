cls

::
::	step 1. ½sÄ¶ demoweb
::
rd /s /q WebAPP
rd /s /q ..\..\DemoWeb\obj\
"c:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_compiler.exe" -v / -p ..\..\DemoWeb -u -f -c WebAPP
rd /s /q WebAPP\App_Data
md WebAPP\App_Data


::
:: test build result in IISExpress
::
REM "c:\Program Files\IIS Express\iisexpress.exe" /path:%CD%\WebAPP\

::
::	step 2. BUILD docker image
::
docker rmi andrew/vs20
docker build --force-rm -t=andrew0928/vs20:v1 -t=andrew0928/vs20:latest .

::
::	step 3. PUSH to docker hub
::
docker push andrew0928/vs20:latest
docker push andrew0928/vs20:v1







: docker run --name demo81 -d -p 81:80 andrew/mvcdemo:latest

: docker run --name demo1 -d -p 81:80 andrew/mvcdemo
: docker run --name demo2 -d -p 82:80 andrew/mvcdemo

:: docker run --name demo -d -p 80:80 andrew/mvcdemo

: docker build -t=andrew/revproxy:latest ..\RevProxy(Nginx)
: docker run --name proxy -d -p 80:80 andrew/revproxy