@echo off 
powershell -command "Set-ExecutionPolicy RemoteSigned" 
powershell .\setServerGC.ps1 2>> err.out