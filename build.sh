rm -rf build
cd src/Bot && dotnet publish -o ../../build/bot -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
cd ../../
cd src/UI && dotnet publish -o ../../build/ui -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true 