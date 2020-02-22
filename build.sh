RELEASE_DIR="AsperetaClient/bin/Release"
DATA_DIR="/home/hayden/code/GooseClient_BuildData"
OUTPUT_DIR="/home/hayden/code/GooseClient_Build"

rm -r $RELEASE_DIR
rm -r $OUTPUT_DIR

dotnet publish -c Release -r win-x64 --self-contained false
dotnet publish -c Release -r osx-x64 --self-contained false
dotnet publish -c Release -r linux-x64 --self-contained false

rm $RELEASE_DIR/netcoreapp3.1/win-x64/publish/*.pdb

mkdir $OUTPUT_DIR
cp -r $RELEASE_DIR/netcoreapp3.1/win-x64/publish/* $OUTPUT_DIR
cp "$RELEASE_DIR/netcoreapp3.1/linux-x64/publish/AsperetaClient" $OUTPUT_DIR
cp "$RELEASE_DIR/netcoreapp3.1/osx-x64/publish/AsperetaClient" $OUTPUT_DIR/AsperetaClient_osx
cp -r $DATA_DIR/* $OUTPUT_DIR