#!/usr/bin/env bash

if [[ -z "$VERSION" ]]; then
    echo "Provide the version as environment variable VERSION"
    exit 1
fi

if [[ -z "$RUNTIME" ]]; then
    echo "Provide the runtime as environment variable RUNTIME"
    exit 1
fi

# Copy the application
mkdir -p BuildFolder/opt/screenpresent/
cp -r ./publish/$RUNTIME/* BuildFolder/opt/screenpresent/

# Create control file
mkdir -p BuildFolder/DEBIAN
echo "Package: screenpresent" > BuildFolder/DEBIAN/control
echo "Version: $VERSION" >> BuildFolder/DEBIAN/control
echo "Section: utils" >> BuildFolder/DEBIAN/control
echo "Priority: optional" >> BuildFolder/DEBIAN/control
if [[ "$RUNTIME" == "linux-x64" ]]; then
  echo "Architecture: amd64" >> BuildFolder/DEBIAN/control
else
  echo "Architecture: arm64" >> BuildFolder/DEBIAN/control
fi
echo "Maintainer: ScreenPresent <info@screenpresent.de>" >> BuildFolder/DEBIAN/control
echo "Depends: libvlc-dev, libvlccore-dev" >> BuildFolder/DEBIAN/control
echo "Description: ScreenPresent - A presentation application" >> BuildFolder/DEBIAN/control

# Create the desktop shortcut
mkdir -p BuildFolder/usr/share/applications
echo "[Desktop Entry]" > BuildFolder/usr/share/applications/screenpresent.desktop
echo "Version=$VERSION" >> BuildFolder/usr/share/applications/screenpresent.desktop
echo "Name=ScreenPresent" >> BuildFolder/usr/share/applications/screenpresent.desktop
echo "Comment=ScreenPresent - A presentation application" >> BuildFolder/usr/share/applications/screenpresent.desktop
echo "Exec=/opt/screenpresent/ScreenPresent" >> BuildFolder/usr/share/applications/screenpresent.desktop
echo "Icon=screenpresent" >> BuildFolder/usr/share/applications/screenpresent.desktop
echo "Terminal=false" >> BuildFolder/usr/share/applications/screenpresent.desktop
echo "Type=Application" >> BuildFolder/usr/share/applications/screenpresent.desktop
echo "Categories=Utility;Presentation;" >> BuildFolder/usr/share/applications/screenpresent.desktop

# Copy the icons
mkdir -p BuildFolder/usr/share/icons/hicolor/64x64/apps
cp ./ScreenPresent/Assets/icon.png BuildFolder/usr/share/icons/hicolor/64x64/apps/screenpresent.png

# Build the application
dpkg-deb --build BuildFolder $RUNTIME.deb
rm -rf BuildFolder