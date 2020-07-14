################################################################################
# Build
#
################################################################################
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.301 as build-env
WORKDIR /solution

COPY . .
RUN chmod +x ./build.sh
RUN ./build.sh


################################################################################
# Setup runtime image
#
################################################################################
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /solution

# Install some extra packages for the rumbler
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - \
  && apt-get -y install nodejs

RUN apt-get update \
  && apt-get -y install \
    awscli curl \
    gettext-base vim \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

# Copy in the rumble scripts
COPY rumble/docker/rumble.sh .
COPY rumble/docker/entrants entrants

# Copy over the application from the build image
COPY --from=build-env /solution/build ./build

# Copy the runs script
COPY run.sh ./

# Setup environment
ARG GIT_TAG=latest
ENV GIT_TAG=$GIT_TAG
ARG AWS_ACCESS_KEY_ID=DEFAULT
ENV AWS_ACCESS_KEY_ID=$AWS_ACCESS_KEY_ID
ARG AWS_SECRET_ACCESS_KEY=DEFAULT
ENV AWS_SECRET_ACCESS_KEY=$AWS_SECRET_ACCESS_KEY

# RUN find *
ENTRYPOINT ["./rumble.sh"]
