@echo off
setlocal

REM Configuration
set DB_USER=sa
set DB_PASSWORD=Password123!
set SCRIPT_DIR=%~dp0
set INIT_SCRIPT=Init.sql
set SEED_SCRIPT=SeedData.sql
set INIT_SCRIPT_PATH="%SCRIPT_DIR%%INIT_SCRIPT%"
set SEED_SCRIPT_PATH="%SCRIPT_DIR%%SEED_SCRIPT%"

REM Check if SQL scripts exist
if not exist %INIT_SCRIPT_PATH% (
    echo ERROR: %INIT_SCRIPT% not found in %SCRIPT_DIR%
    goto :eof
)
if not exist %SEED_SCRIPT_PATH% (
    echo ERROR: %SEED_SCRIPT% not found in %SCRIPT_DIR%
    goto :eof
)

REM Clear caches
echo Clearing SQL Server caches...
sqlcmd -U %DB_USER% -P %DB_PASSWORD% -Q "DBCC DROPCLEANBUFFERS;" -C -N
sqlcmd -U %DB_USER% -P %DB_PASSWORD% -Q "DBCC FREEPROCCACHE;" -C -N

REM Execute scripts
echo Executing initialization script: %INIT_SCRIPT%
sqlcmd -U %DB_USER% -P %DB_PASSWORD% -i %INIT_SCRIPT_PATH% -C -N

if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to execute %INIT_SCRIPT%
    goto :eof
)

echo Executing data seeding script: %SEED_SCRIPT%
sqlcmd -U %DB_USER% -P %DB_PASSWORD% -i %SEED_SCRIPT_PATH% -C -N

if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to execute %SEED_SCRIPT%
    goto :eof
)

echo Database reseeding complete.

:eof
endlocal
pause
