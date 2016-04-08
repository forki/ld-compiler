FROM nice/alpine-fsharp:2f00052c29ce34a5ce8e765b287b6e5072c1b22e	

MAINTAINER James Kirk <james.kirk@nice.org.uk>

ADD . /usr/share/ld-compiler/

ARG PUBLISH_NUGET=no
ENV PUBLISH_NUGET ${PUBLISH_NUGET}

RUN /usr/share/ld-viewer/build.sh
