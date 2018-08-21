FROM microsoft/dotnet:2.1-sdk AS build

# Install mono
ENV MONO_VERSION 5.4.1.6
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF

RUN echo "deb http://download.mono-project.com/repo/debian stretch/snapshots/$MONO_VERSION main" > /etc/apt/sources.list.d/mono-official.list \
  && apt-get update \
  && apt-get install -y locales mono-runtime \
  && rm -rf /var/lib/apt/lists/* /tmp/*

RUN apt-get update \
  && apt-get install -y binutils curl mono-devel ca-certificates-mono fsharp mono-vbnc nuget referenceassemblies-pcl \
  && rm -rf /var/lib/apt/lists/* /tmp/*

# Set locale
RUN echo 'en_US.UTF-8 UTF-8' > /etc/locale.gen && /usr/sbin/locale-gen

# Copy project(s) and restore
WORKDIR /app
COPY paket.lock paket.dependencies makewise-core.sln ./
COPY .paket .paket
COPY src/Clinics/*.fsproj ./src/Clinics/
COPY src/GiraffeWeb/*.fsproj ./src/GiraffeWeb/
RUN dotnet restore

# Copy and publish app
WORKDIR /app/
COPY src/Clinics/. ./src/Clinics/
COPY src/GiraffeWeb/. ./src/GiraffeWeb/
RUN dotnet publish -c Release -o out

# Test app -- TODO
# FROM build AS testrunner
# WORKDIR /app/src/Clinics/tests
# COPY src/Clinics/tests/. .
# ENTRYPOINT ["dotnet", "test", "--logger:trx"]

# Build runtime
FROM microsoft/dotnet:2.1-runtime-alpine AS runtime
WORKDIR /app
COPY --from=build /app/src/GiraffeWeb/out ./
COPY --from=build /app/.paket .paket
EXPOSE 80
ENTRYPOINT ["dotnet", "GiraffeWeb.dll"]
