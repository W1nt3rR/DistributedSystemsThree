@echo off
setlocal

REM Set the image name and tag
set IMAGE_NAME=w1nt3rr/conference-service
set IMAGE_TAG=latest

REM Build the Docker image
docker build -t %IMAGE_NAME%:%IMAGE_TAG% .

REM Push the Docker image to Docker Hub
docker push %IMAGE_NAME%:%IMAGE_TAG%

echo Docker image %IMAGE_NAME%:%IMAGE_TAG% has been built and pushed to Docker Hub.

endlocal
