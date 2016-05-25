FROM frolvlad/alpine-mono:latest
RUN mkdir /home/app
COPY . /home/app
CMD mono /home/app/bin/Debug/docker01.exe

