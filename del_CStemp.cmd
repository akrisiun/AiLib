
for /d /r . %%d in (Debug,Release,obj) do @if exist "%%d" rd /s/q "%%d"

@PAUSE