FROM nice/ld-publisher-base

MAINTAINER James Kirk <james.kirk@nice.org.uk>

ENV STARDOG_VERSION=4.0.1

RUN mkdir /compiler

# Keep package management separate from code
ADD paket.dependencies paket.lock .paket/ /compiler/
ADD .paket/ /compiler/.paket/

WORKDIR /compiler

RUN mono .paket/paket.bootstrapper.exe && mono .paket/paket.exe install

ADD RELEASE_NOTES.md build.sh build.fsx compiler.sln /compiler/
ADD src /compiler/src
ADD tests /compiler/tests

RUN /compiler/build.sh &&\
    cd /compiler &&\
    find . -maxdepth 1 -not -name "bin" -not -name "." | xargs -i rm -rf {}

#make the contents of /tools available globally
ADD tools/ /tools/
RUN cd /bin && \
    ln -s /tools/* .

EXPOSE 8081

ADD populateAppConfig.sh /compiler/

CMD ./populateAppConfig.sh && mono /compiler/bin/compiler.api.exe
