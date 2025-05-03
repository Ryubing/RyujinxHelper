if [ $# != 1 ]; then
   echo "This script requires one argument, representing the version to build."
   read -n1 -r -p "Press any key to exit."
   exit 1
fi

function pub {

echo "Compiling for $1..."
dotnet publish -c release -r $1 --self-contained -p:PublishSingleFile=true -o ../../build/$1/$2 --p:Version="$3"

}

function packClassic {
  if stringContain "win" $1; then
    mv build/$1/classic/RyuBot.exe artifacts/RyuBot-$2_$1.exe
  else
    mv build/$1/classic/RyuBot artifacts/RyuBot-$2_$1
  fi
}

function packUi {
  if stringContain "win" $1; then
    cd build/$1/ui
    7z a ../../../artifacts/RyuBot.UI-$2_$1.7z ./
    cd ../../../
  else
    cd build/$1/ui
    tar -czvf ../../../artifacts/RyuBot.UI-$2_$1.tar.gz ./
    cd ../../../
  fi
}

stringContain() { case $2 in *$1* ) return 0;; *) return 1;; esac ;}

echo "Cleaning previous build & packages..."
rm -rf build
rm -rf artifacts
mkdir artifacts

echo "Building just the bot..."

cd src/Bot

pub linux-arm64 classic $1
pub linux-x64 classic $1
pub win-arm64 classic $1
pub win-x64 classic $1
pub osx-arm64 classic $1
pub osx-x64 classic $1

cd ../../src/UI
echo "Switching to the Avalonia project..."

pub linux-arm64 ui $1
pub linux-x64 ui $1
pub win-arm64 ui $1
pub win-x64 ui $1
pub osx-arm64 ui $1
pub osx-x64 ui $1

cd ../../
echo "Packaging builds..."

packClassic linux-arm64 $1
packClassic linux-x64 $1
packClassic win-arm64 $1
packClassic win-x64 $1
packClassic osx-arm64 $1
packClassic osx-x64 $1

packUi linux-arm64 $1
packUi linux-x64 $1
packUi win-arm64 $1
packUi win-x64 $1
packUi osx-arm64 $1
packUi osx-x64 $1