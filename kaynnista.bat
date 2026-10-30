@echo off
rem Kaynnistaa Blackjack-pelin. Vaatii .NET 8 SDK:n.
rem Ensimmaisella kerralla dotnet lataa Jypeli-kirjaston ja kaantaa pelin.
cd /d "%~dp0ht1"
dotnet run
if errorlevel 1 pause
