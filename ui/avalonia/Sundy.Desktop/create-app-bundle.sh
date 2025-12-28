#!/bin/bash

# Script to create a macOS .app bundle for Sundy

set -e

echo "Building Sundy.Desktop..."
dotnet build -c Debug

APP_NAME="Sundy"
BUILD_DIR="bin/Debug/net10.0/osx-arm64"
APP_BUNDLE="$BUILD_DIR/$APP_NAME.app"
CONTENTS_DIR="$APP_BUNDLE/Contents"
MACOS_DIR="$CONTENTS_DIR/MacOS"
RESOURCES_DIR="$CONTENTS_DIR/Resources"

echo "Creating app bundle structure..."
rm -rf "$APP_BUNDLE"
mkdir -p "$MACOS_DIR"
mkdir -p "$RESOURCES_DIR"

echo "Copying executable and dependencies..."
# Copy all files except the .app bundle itself
for item in "$BUILD_DIR"/*; do
    if [[ "$(basename "$item")" != "$APP_NAME.app" ]] && [[ "$(basename "$item")" != "publish" ]]; then
        cp -r "$item" "$MACOS_DIR/"
    fi
done

# Verify the executable exists
if [ ! -f "$MACOS_DIR/Sundy.Desktop" ]; then
    echo "Error: Sundy.Desktop executable not found in $BUILD_DIR"
    echo "Available files:"
    ls -la "$BUILD_DIR" | head -20
    exit 1
fi

echo "Copying icon..."
ICON_SOURCE="../Sundy/Assets/AppIcon.icns"
if [ ! -f "$ICON_SOURCE" ]; then
    echo "Warning: Icon file not found at $ICON_SOURCE, trying absolute path..."
    ICON_SOURCE="/Users/cody/code/codymullins/Sundy/src/Avalonia/Sundy/Assets/AppIcon.icns"
fi
cp "$ICON_SOURCE" "$RESOURCES_DIR/"

echo "Creating Info.plist..."
cat > "$CONTENTS_DIR/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>Sundy</string>
    <key>CFBundleDisplayName</key>
    <string>Sundy</string>
    <key>CFBundleIdentifier</key>
    <string>com.codymullins.sundy</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleSignature</key>
    <string>????</string>
    <key>CFBundleExecutable</key>
    <string>Sundy.Desktop</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon.icns</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSHumanReadableCopyright</key>
    <string>Copyright © 2025 Cody Mullins</string>
</dict>
</plist>
EOF

echo "Making executable..."
chmod +x "$MACOS_DIR/Sundy.Desktop"

echo ""
echo "✅ App bundle created at: $APP_BUNDLE"
echo "To run: open $APP_BUNDLE"

