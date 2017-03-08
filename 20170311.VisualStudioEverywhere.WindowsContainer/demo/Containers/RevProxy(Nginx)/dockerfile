# This dockerfile utilizes components licensed by their respective owners/authors.
# Prior to utilizing this file or resulting images please review the respective licenses at: http://nginx.org/LICENSE

# microsoft github reference: https://github.com/Microsoft/Virtualization-Documentation/tree/master/windows-container-samples/windowsservercore/nginx

# docker build -t=andrew/revproxy:latest .
# docker run --rm -t -i andrew/revproxy cmd.exe

FROM microsoft/windowsservercore:latest

LABEL Description="Nginx" Vendor=Nginx" Version="1.1.13"

COPY nginx /nginx
COPY nginx.conf /nginx/conf/nginx.conf

CMD [ "/nginx/nginx.exe" ]
