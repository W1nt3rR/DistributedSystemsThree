@echo off
REM This script applies all Kubernetes YAML files in the current directory

REM Loop through all .yaml and .yml files and apply them
for %%f in (*.yaml *.yml) do (
    echo Applying %%f...
    kubectl apply -f %%f
)

echo All files applied.