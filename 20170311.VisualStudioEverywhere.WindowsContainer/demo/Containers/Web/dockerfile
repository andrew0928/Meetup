FROM microsoft/aspnet
MAINTAINER Andrew Wu, https://www.facebook.com/andrewwu.blog.0928
LABEL Description="Visual Studio Everywhere DEMO!" Vendor="Andrew Studio" Version="1.0.2017.0311"

# enable feature: ASP.NET 4.5
#RUN dism /online /enable-feature /all /featurename:IIS-ASPNET45

# copy demoweb
ADD webapp c:/inetpub/wwwroot/

#CMD [ "ping", "localhost", "-t" ]
