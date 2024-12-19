# Instagram Imaging

## Single **exe** deployment

```
dotnet publish -c Release -p:PublishSingleFile=true -p:SelfContained=true -p:RuntimeIdentifier=win-x64 -p:PublishReadyToRun=true -o deploy
```

## Usage example

```
igimg.exe alt -w 2048 -h 2048 -p 50 -f "0032 (Z6A_3397).jpg"
```

```
find . -type f -iname "*.jpg" | xargs -I {} igimg alt -w 2048 -h 2048 -p 50 -f {}
```

