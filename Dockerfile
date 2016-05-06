FROM nice/ld-publisher-base

MAINTAINER James Kirk <james.kirk@nice.org.uk>

ENV STARDOG_VERSION=4.0.1

ADD . /compiler

RUN /compiler/build.sh &&\
    cd /compiler &&\
    find . -maxdepth 1 -not -name "bin" -not -name "." | xargs -i rm -rf {}

#make the contents of /tools available globally
ADD tools/ /tools/
RUN cd /bin && \
    ln -s /tools/* .

EXPOSE 8081

CMD mono /compiler/bin/compiler.api.exe
