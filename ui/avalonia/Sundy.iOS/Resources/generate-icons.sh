#!/bin/bash

SOURCE_ICON="/Users/cody/code/codymullins/Sundy/src/Avalonia/Sundy/Assets/logo.png"
OUTPUT_DIR="/Users/cody/code/codymullins/Sundy/src/Avalonia/Sundy.iOS/Resources/Assets.xcassets/AppIcon.appiconset"

# iPhone icons
sips -z 40 40 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-20@2x.png"
sips -z 60 60 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-20@3x.png"
sips -z 58 58 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-29@2x.png"
sips -z 87 87 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-29@3x.png"
sips -z 80 80 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-40@2x.png"
sips -z 120 120 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-40@3x.png"
sips -z 120 120 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-60@2x.png"
sips -z 180 180 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-60@3x.png"

# iPad icons
sips -z 20 20 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-20.png"
sips -z 40 40 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-20@2x-1.png"
sips -z 29 29 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-29.png"
sips -z 58 58 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-29@2x-1.png"
sips -z 40 40 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-40.png"
sips -z 80 80 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-40@2x-1.png"
sips -z 76 76 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-76.png"
sips -z 152 152 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-76@2x.png"
sips -z 167 167 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-83.5@2x.png"

# App Store icon
sips -z 1024 1024 "$SOURCE_ICON" --out "$OUTPUT_DIR/Icon-1024.png"

echo "All icons generated successfully!"

