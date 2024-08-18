@echo off
setlocal

REM Define an array of deployments
set deployments=member-service-depl conference-management-service-deployment conference-service-depl

REM Loop through each deployment and restart it
for %%d in (%deployments%) do (
    kubectl rollout restart deployment %%d
)

echo All specified deployments have been restarted.

endlocal