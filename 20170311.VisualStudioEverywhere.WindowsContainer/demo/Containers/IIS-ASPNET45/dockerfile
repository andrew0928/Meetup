FROM microsoft/iis
MAINTAINER Andrew Wu, https://www.facebook.com/andrewwu.blog.0928
LABEL Description="Course Of Microservice Demo - IIS with ASP.NET 45 enabled" Vendor="Andrew Studio" Version="12.15.0.0"

# enable feature: ASP.NET 4.5
RUN dism /online /enable-feature /all /featurename:IIS-ASPNET45

CMD [ "ping", "localhost", "-t" ]