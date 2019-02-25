FROM microsoft/dotnet:2.2-sdk
WORKDIR /opt/ffmpeg
RUN apt-get update && \
  apt-get -y --no-install-recommends install \
  autoconf \
  automake \
  build-essential \
  cmake \
  git-core \
  libass-dev \
  libfreetype6-dev \
  libsdl2-dev \
  libtool \
  libva-dev \
  libvdpau-dev \
  libvorbis-dev \
  libxcb1-dev \
  libxcb-shm0-dev \
  libxcb-xfixes0-dev \
  mercurial \
  pkg-config \
  texinfo \
  wget \
  zlib1g-dev && \
  mkdir bin && \
  mkdir build && \
  wget https://www.nasm.us/pub/nasm/releasebuilds/2.13.03/nasm-2.13.03.tar.bz2 && \
  tar xjvf nasm-2.13.03.tar.bz2 && \
  cd nasm-2.13.03 && \
  ./autogen.sh && \
  PATH="/opt/ffmpeg/bin:$PATH" ./configure --prefix="/opt/ffmpeg/build" --bindir="/opt/ffmpeg/bin" && \
  make && \
  make install && \
  cd ../ && \
  wget -O yasm-1.3.0.tar.gz https://www.tortall.net/projects/yasm/releases/yasm-1.3.0.tar.gz && \
  tar xzvf yasm-1.3.0.tar.gz && \
  cd yasm-1.3.0 && \
  ./configure --prefix="/opt/ffmpeg/build" --bindir="/opt/ffmpeg/bin" && \
  make && \
  make install && \
  cd ../ && \
  git -C x264 pull 2> /dev/null || git clone --depth 1 https://git.videolan.org/git/x264 && \
  cd x264 && \
  PATH="/opt/ffmpeg/bin:$PATH" PKG_CONFIG_PATH="/opt/ffmpeg/build/lib/pkgconfig" ./configure --prefix="/opt/ffmpeg/build" --bindir="/opt/ffmpeg/bin" --enable-static --enable-pic && \
  PATH="/opt/ffmpeg/bin:$PATH" make && \
  make install && \
  cd ../ && \
  if cd x265 2> /dev/null; then hg pull && hg update; else hg clone https://bitbucket.org/multicoreware/x265; fi && \
  cd x265/build/linux && \
  PATH="/opt/ffmpeg/bin:$PATH" cmake -G "Unix Makefiles" -DCMAKE_INSTALL_PREFIX="/opt/ffmpeg/build" -DENABLE_SHARED=off ../../source && \
  PATH="/opt/ffmpeg/bin:$PATH" make && \
  make install && \
  cd ../../../ && \
  git -C libvpx pull 2> /dev/null || git clone --depth 1 https://chromium.googlesource.com/webm/libvpx.git && \
  cd libvpx && \
  PATH="/opt/ffmpeg/bin:$PATH" ./configure --prefix="/opt/ffmpeg/build" --disable-examples --disable-unit-tests --enable-vp9-highbitdepth --as=yasm && \
  PATH="/opt/ffmpeg/bin:$PATH" make && \
  make install && \
  cd ../ && \
  wget -O ffmpeg-snapshot.tar.bz2 https://ffmpeg.org/releases/ffmpeg-snapshot.tar.bz2 && \
  tar xjvf ffmpeg-snapshot.tar.bz2 && \
  cd ffmpeg && \
  PATH="/opt/ffmpeg/bin:$PATH" PKG_CONFIG_PATH="/opt/ffmpeg/build/lib/pkgconfig" ./configure \
  --prefix="/opt/ffmpeg/build" \
  --pkg-config-flags="--static" \
  --extra-cflags="-I/opt/ffmpeg/build/include" \
  --extra-ldflags="-L/opt/ffmpeg/build/lib" \
  --extra-libs="-lpthread -lm" \
  --bindir="/opt/ffmpeg/bin" \
  --enable-gpl \
  --enable-libvpx \
  --enable-libx264 \
  --enable-libx265 && \
  PATH="/opt/ffmpeg/bin:$PATH" make && \
  make install && \
  apt-get remove -y autoconf \
  automake \
  build-essential \
  cmake \
  git-core \
  libass-dev \
  libfreetype6-dev \
  libsdl2-dev \
  libtool \
  libva-dev \
  libvdpau-dev \
  libvorbis-dev \
  libxcb1-dev \
  libxcb-shm0-dev \
  libxcb-xfixes0-dev \
  mercurial \
  pkg-config \
  texinfo \
  wget \
  zlib1g-dev && \
  rm -rf /var/lib/apt/lists/*

FROM microsoft/dotnet:2.2-sdk
RUN mkdir -p /opt/ffmpeg/bin && mkdir -p /opt/ffmpeg/build
COPY --from=0 /opt/ffmpeg/bin/ /opt/ffmpeg/bin/
COPY --from=0 /opt/ffmpeg/build/ /opt/ffmpeg/build/
RUN rm -rf /opt/ffmpeg/build/share && \
  apt-get update && \
  apt-get -y --no-install-recommends --allow-unauthenticated install \
  libxcb1 \
  libc6-dev \
  libgdiplus \
  libx11-dev \
  libxcb-shm0 \
  libxcb-shape0 \
  libxcb-xfixes0 \
  libasound2 \
  libSDL2-2.0 \
  libxv1 \
  libva1 \
  libva-drm1 \
  libva-x11-1 \
  libvdpau1 && \
  rm -rf /var/lib/apt/lists/* && \
  chmod a+x /opt/ffmpeg/bin/*
